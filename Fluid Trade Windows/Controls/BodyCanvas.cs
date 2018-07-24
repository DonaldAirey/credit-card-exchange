namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Shapes;
    using Utilities;

	/// <summary>
	/// A canvas used for data area of a report.
	/// </summary>
	internal class BodyCanvas : ReportCanvas
	{
		public delegate void ResetCursorDelegate(bool isBusy);

		// Private Instance Fields
		private Point anchorPoint;
		private Cursor cross;
		private ReportCell currentSelectedCell = null;

		public ReportCell CurrentSelectedCell
		{
			get { return currentSelectedCell; }
		}
		private BodyCanvas.MouseState mouseState;
		private Path selectedOutline;
		private List<Rect> selectedRanges;

		// Private Enumerations
		private enum MouseState { ButtonUp, ButtonDown, Selecting };

		/// <summary>
		/// Create a canvas for the data area of a report.
		/// </summary>
		internal BodyCanvas()
		{

			// The cursor used for the body of the report is the same one used by Excel for cell selection.
			this.cross = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/Cross.cur", UriKind.Relative)).Stream);
			this.Cursor = this.cross;

			// This list of rectangles is used to manage the selection of cells.
			this.selectedRanges = new List<Rect>();

			// This path is used to outline the selected cells on the canvas.  Note that it will always be on top of the other children.
			this.selectedOutline = new Path();
			this.selectedOutline.RenderTransform = new TranslateTransform(-1.0, -1.0);
			this.selectedOutline.SnapsToDevicePixels = true;
			this.selectedOutline.Stroke = new SolidColorBrush(Color.FromArgb(0xA7, 0x0, 0x0, 0x0));
			this.selectedOutline.StrokeThickness = 3.0;
			Canvas.SetZIndex(this.selectedOutline, int.MaxValue);
			this.Children.Add(this.selectedOutline);

		}

		public void UpdateCursor(bool isBusy)
		{

			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new ResetCursorDelegate(ResetCursor), isBusy);

		}

		public void ResetCursor(bool isBusy)
		{

			if (isBusy)
			{

				this.Cursor = Cursors.Wait;
				Mouse.UpdateCursor();

			}
			else
			{

				Mouse.OverrideCursor = null;
				Mouse.UpdateCursor();
				this.Cursor = this.cross;

			}
		}

		/// <summary>
		/// Overrides the default arrangement of a Canvas.
		/// </summary>
		/// <param name="arrangeSize">The largest possible size for the canvas.</param>
		/// <returns>The actual size used by this canvas.</returns>
		protected override Size MeasureOverride(Size constraint)
		{

			// The base class does the hard work of building the visible part of the report on the canvas.
			Size size = base.MeasureOverride(constraint);

			// This creates a group of geometries around all the selected cells.
			GeometryGroup geometryGroup = new GeometryGroup();
			foreach (UIElement uiElement in this.Children)
				if (uiElement is FrameworkElement)
				{
					FrameworkElement frameworkElement = uiElement as FrameworkElement;
					ReportCell reportCell = DynamicReport.GetCell(frameworkElement);
					if (reportCell != null)
						if (reportCell.IsSelected)
							geometryGroup.Children.Add(new RectangleGeometry(reportCell.Rect));

				}

			// An path around all the selected cells is created from the aggregation of all the selected cells.  This path was
			// roughly backward engineered arond the one used for Microsoft Excel.
			this.selectedOutline.Data = geometryGroup.GetOutlinedPathGeometry();

			// This is the size of the canvas as measured by the base class.
			return size;

		}

		/// <summary>
		/// Handles the mouse button being pressed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			// This state variable will control how the 'Mouse Move' and 'Mouse Up' event handlers interpret the user action.  The
			// 'selectedColumn' field is used as the starting point for any drag-and-drop type of operation.
			this.mouseState = MouseState.ButtonDown;

			// Evaluate the state of the keyboard.  Key combinations involving the shift and control keys will alter the areas that
			// are selected.
			bool isShiftKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
			bool isControlKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

			// The mouse selected with the mouse button is used as an anchor point unless the shift key is pressed.  The anchor
			// point allows for ranges of columns to be selected.  Everything between the anchor and the currently selected column
			// will be selected when the shift key is down.  This is modeled after the Excel extended range selection keys.
			Point mouseDownLocation = e.GetPosition(this);

			// Do not clear the select cells in the Report Grid if the right mouse button is pressed.
			if (e.RightButton != MouseButtonState.Pressed)
			{
				//Since the user is selecting a cell, it will invalidate any row selection that there may be.
				foreach (List<ReportRow> block in this.ReportGrid.SelectedRowBlocks)
					foreach (ReportRow row in block)
						foreach (ReportCell cell in row.Cells)
							cell.IsSelected = false;
				this.ReportGrid.SelectedRowBlocks.Clear();
				this.ReportGrid.SelectedRowHeaderBlocks.Clear();
			}

			
            if (!isShiftKeyPressed)
			{
				IInputElement iInputElement = this.InputHitTest(mouseDownLocation);
				DependencyObject dependencyObject = iInputElement as DependencyObject;
				while (dependencyObject != null)
				{
					ReportCell reportCell = DynamicReport.GetCell(dependencyObject);
					if (reportCell != null)
					{
						Keyboard.Focus(dependencyObject as IInputElement);
                        currentSelectedCell = reportCell;
						break;
					}
					dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				}
				this.anchorPoint = mouseDownLocation;
			}

			// The shift and control key extend the selection operation in the same way as Microsoft Excel.
			if (isShiftKeyPressed || isControlKeyPressed)
			{

				// When the shift key is pressed during column selection, every column between the last column selected 
				// and the current column is selected.
				if (isShiftKeyPressed)
				{

					// This is an ordered rectangle that encompasses all the selected cells.
					Rect selectedRectangle = GetSelectionRectangle(mouseDownLocation);
					SetSelectedRectangle(selectedRectangle);
				}

				// When the control key is pressed a single column is added to the range of columns selected.
				if (isControlKeyPressed)
				{

					// A point marks the range when the control key is pressed.  This is almost identical to pressing the mouse
					// button except the previous ranges are not cleared.  This is part of the extended selection algorithm that is
					// modeled after Excel.
					Rect selectedRange = new Rect(mouseDownLocation, new Size(0, 0));

					// This removes any previous instance of this column in the selection.
					foreach (Rect existingRange in this.selectedRanges)
						if (selectedRange == existingRange)
						{
							this.selectedRanges.Remove(existingRange);
							break;
						}

					// The new range becomes the starting point for any further extended selection operations.
					this.selectedRanges.Add(selectedRange);

				}

				// This instructs the event handlers how the mouse movement is to be interpreted.
				this.mouseState = MouseState.Selecting;

			}
			else
			{
				// Evaluate if we need to be able to determine the when we right click within the currently selected range then do not clear the selectedRanges.
				if (!((e.RightButton == MouseButtonState.Pressed) && (CurrentSelectedCell != null) && (CurrentSelectedCell.IsSelected))) //MIGHT try this --> if (reportCell.Rect.IntersectsWith(selectedRange))
				{

					// Clear the selected ranges as we do not have a right mouse button pressed with the currently Selected ranges.

					// A simple selection that doesn't involve the modifier keys will clear out any previously selected ranges.
					this.selectedRanges.Clear();
					// The column is added at the start of a new range of selected columns.
					this.selectedRanges.Add(new Rect(mouseDownLocation, new Size(0, 0)));

				}
			}

			
			// Evaluate if we need to be able to determine the when we right click within the currently selected range then do not clear the selectedRanges.
			if (!((e.RightButton == MouseButtonState.Pressed) && (CurrentSelectedCell != null) && (CurrentSelectedCell.IsSelected))) //MIGHT try this --> if (reportCell.Rect.IntersectsWith(selectedRange))
			{
				// This will select all the columns in the selected ranges of columns and remove the selection from all the rest of the cells.
				SelectCells();
			}
		}

		/// <summary>
		/// Handles the movement of the mouse in the column header.
		/// </summary>
		/// <param name="e">The mouse movement event arguments.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{

			// This gets the location of the mouse in document coordinates.
			Point mouseLocation = e.GetPosition(this);

			// The action taken by a mouse movement in the column heading of a viewer is driven by a set of states.  These states
			// are driven, in turn, by where the mouse started and what buttons are pressed.  They can instruct the viewer to
			// resize the columns, move the column, delete the column or sort the column.
			switch (this.mouseState)
			{

			case MouseState.Selecting:

				// This is an ordered rectangle that encompasses all the selected cells.
                Rect currentRange = GetSelectionRectangle(mouseLocation);

                // When dragging the mouse around during a selection, the last range is replaced with the current range of cells.
				if (this.selectedRanges[this.selectedRanges.Count - 1] != currentRange)
				{
					this.selectedRanges[this.selectedRanges.Count - 1] = currentRange;
					SelectCells();
				}

				break;

			}

		}

		/// <summary>
		/// Handles the mouse button being released over a column header.
		/// </summary>
		/// <param name="state">The thread initialization parameter.</param>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{

			// This resets the state of the mouse for the next operation.
			this.mouseState = MouseState.ButtonUp;

		}

        /// <summary>
        /// Handle selection with keyboard
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
			base.OnPreviewKeyDown(e);

            bool isShiftKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
            switch (e.Key)
            {
            case Key.Delete:

                if (this.selectedRanges.Count != 0)
                {
					// Send a delete event to all the selected cells. Comment this out for the time being to avoid issues.
                    //SendKbDeleteEvent();
                   //e.Handled = true;
                }
                break;

            case Key.Right:
			case Key.Left:

                if (isShiftKeyPressed)
                {
                    int direction = (e.Key == Key.Right) ? 1 : -1;
                    SelectNextHorizantalCell(direction);
                    SelectCells();
                    e.Handled = true;
                }
                else if (ReportGrid.IsChildInFocus == false)
                {
                    FocusCellChanged();
					// Place a border around the focused cell
					this.selectedRanges.Clear();
					this.selectedRanges.Add(Rect.Inflate(this.CurrentSelectedCell.Rect, -1.0, -1.0));
					SelectCells();

					// Highlight the row and column header
					this.ReportGrid.SelectHeadersOfCurrentCell(this.CurrentSelectedCell);

                    e.Handled = true;
                }                
                break;
            case Key.Up:
            case Key.Down:

                if (isShiftKeyPressed)
                {
                    int direction = (e.Key == Key.Down) ? 1 : -1;
                    SelectNextVerticalCell(direction);
                    SelectCells();
                    e.Handled = true;
                }
                else if (ReportGrid.IsChildInFocus == false)
                {
                    FocusCellChanged();
					// Place a border around the focused cell
					this.selectedRanges.Clear();
					this.selectedRanges.Add(Rect.Inflate(this.CurrentSelectedCell.Rect, -1.0, -1.0));
					SelectCells();

					// Highlight the row and column header
					this.ReportGrid.SelectHeadersOfCurrentCell(this.CurrentSelectedCell);

                    e.Handled = true;
                }
                
                break;
			case Key.Enter:

					FocusCellChanged();
					// Place a border around the focused cell
					this.selectedRanges.Clear();
					this.selectedRanges.Add(Rect.Inflate(this.CurrentSelectedCell.Rect, -1.0, -1.0));
					SelectCells();

					// Highlight the row and column header
					this.ReportGrid.SelectHeadersOfCurrentCell(this.CurrentSelectedCell);

					e.Handled = true;

				break;
			case Key.Tab:

				FocusCellChanged();
				// Place a border around the focused cell
				this.selectedRanges.Clear();
				this.selectedRanges.Add(Rect.Inflate(this.CurrentSelectedCell.Rect, -1.0, -1.0));
				SelectCells();

				// Highlight the row and column header
				this.ReportGrid.SelectHeadersOfCurrentCell(this.CurrentSelectedCell);

				e.Handled = true;
				break;

            } // end switch on Key
 
        }

		/// <summary>
		/// Set the focus to a cell and highlights the column header and row header and also puts a border around it.
		/// </summary>
		/// <param name="reportCell"></param>
		public void SetFocusedCell(ReportCell reportCell)
		{
			FocusCellChanged();
			// Place a border around the focused cell
			this.selectedRanges.Clear();
			this.selectedRanges.Add(Rect.Inflate(this.CurrentSelectedCell.Rect, -1.0, -1.0));
			SelectCells();

			// Highlight the row and column header
			this.ReportGrid.SelectHeadersOfCurrentCell(this.CurrentSelectedCell);
		}


        /// <summary>
        /// Sends delete to all the selected cells
        /// </summary>
        /// <returns></returns>
        private void SendKbDeleteEvent()
        {
            List<FrameworkElement> selectedCells = GetSelectedCells();
            ICommand deleteCommand = ApplicationCommands.Delete;
            deleteCommand.Execute("Delete");
            foreach (var selectedCell in selectedCells)
            {
                //TODO:  Replace this with general RoutedCommand so that is not specific to TextBoxes
                TextBox tb = selectedCell as TextBox;
                if (tb != null)
                {
                    tb.Clear();
                }
            }
        }

        /// <summary>
        /// Returns List of currently selectedCells
        /// </summary>
        /// <returns></returns>
        public List<FrameworkElement> GetSelectedCells()
        {
            List<FrameworkElement> selectedCells = new List<FrameworkElement>();
            foreach (UIElement uiElement in this.Children)
            {
                if (uiElement is FrameworkElement)
                {
                    FrameworkElement frameworkElement = uiElement as FrameworkElement;
					ReportCell reportCell = DynamicReport.GetCell(frameworkElement);
                    if (reportCell != null)
                    {
                        if (reportCell.IsSelected)
                        {
                            selectedCells.Add(frameworkElement);
                        }
                    }
                }
            }

            return selectedCells;
        }    
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        private void SelectNextHorizantalCell(int direction)
        {
            ReportCell lastSelected = currentSelectedCell ?? this.ReportGrid.CurrentReportCell;
            ReportCell toBeSelectedCell = this.ReportGrid.FindClosestHorizontalCell(lastSelected, 0.0, direction);
            if (toBeSelectedCell != null)
            {
                Rect selectedRectangle = GetSelectionRectangle(toBeSelectedCell.Rect.Center());
                SetSelectedRectangle(selectedRectangle);
                currentSelectedCell = toBeSelectedCell;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        private void SelectNextVerticalCell(int direction)
        {
            ReportCell lastSelected = currentSelectedCell ?? this.ReportGrid.CurrentReportCell;
            ReportCell toBeSelectedCell = this.ReportGrid.FindClosestVerticalCell(lastSelected, 0.0, direction);
            if (toBeSelectedCell != null)
            {
                Rect selectedRectangle = GetSelectionRectangle(toBeSelectedCell.Rect.Center());
                SetSelectedRectangle(selectedRectangle);
                currentSelectedCell = toBeSelectedCell;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedRectangle"></param>
        private void SetSelectedRectangle(Rect selectedRectangle)
        {
            // In the unlikely event that the shift key was down during the setting of the anchor point, this will
            // create a dummy entry in the list of selected column ranges.
            if (this.selectedRanges.Count == 0)
                this.selectedRanges.Add(selectedRectangle);
            else
                this.selectedRanges[this.selectedRanges.Count - 1] = selectedRectangle;
        }

        /// <summary>
        /// This is in response to focused cell change because of Keyboard navigation.
        /// </summary>
        private void FocusCellChanged()
        {
            this.selectedRanges.Clear();         
            this.anchorPoint = this.ReportGrid.CurrentReportCell.Rect.Center();
            this.currentSelectedCell = this.ReportGrid.CurrentReportCell;         
        }

        private Point? FindClosestHorizontalCellPoint(int direction)
        {
            ReportCell lastSelected = currentSelectedCell ?? this.ReportGrid.CurrentReportCell;
            ReportCell targetCell = this.ReportGrid.FindClosestHorizontalCell(lastSelected, 0.0, direction);

            if (targetCell != null)
            {
                return targetCell.Rect.Center();
            }

            return null;

        }

        /// <summary>
        /// Calculates the rectangle to be selected : given the final point.
        /// </summary>
        /// <param name="selectToLocation">Select to this point from the current anchor point</param>
        /// <returns></returns>
        private Rect GetSelectionRectangle(Point selectToLocation)
        {
           
            double xPos = (selectToLocation.X < this.anchorPoint.X ? selectToLocation.X : this.anchorPoint.X);
            double yPos = (selectToLocation.Y < this.anchorPoint.Y ? selectToLocation.Y : this.anchorPoint.Y);

            return new Rect(xPos, yPos,
                            Math.Abs(selectToLocation.X - this.anchorPoint.X),
                            Math.Abs(selectToLocation.Y - this.anchorPoint.Y));
        }

		/// <summary>
		/// Invoked when an unhandled PreviewGotKeyboardFocus attached event reaches an element in the route derived from this class.
		/// This will place a border around the cell.
		/// </summary>
		/// <param name="e">The arguments describing the focus change.</param>
		//protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		//{

		//    // If the visual element that received the focus is associated with a virtual cell, then a distinctive border is drawn
		//    // around it to indicate that it is selected.  This is roughly engineered around the Excel user interface.
		//    ReportCell reportCell = DynamicReport.GetCell(e.NewFocus as DependencyObject);
		//    bool isShiftKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
		//    if (reportCell != null)
		//    {
		//        //if ((!isShiftKeyPressed) || (e.RightButton != MouseButtonState.Pressed))
		//        if (!isShiftKeyPressed)
		//        {
		//            this.selectedRanges.Clear();
		//        }
		//        this.selectedRanges.Add(Rect.Inflate(reportCell.Rect, -1.0, -1.0));
		//        SelectCells();
		//    }

		//    // Allow the base class to handle the rest of the keyboard focus event.
		//    base.OnPreviewGotKeyboardFocus(e);

		//}

		/// <summary>
		/// Invoked when an unhandled PreviewLostKeyboardFocus attached event reaches an element in the route derived from this class.
		/// </summary>
		/// <param name="e">The arguments describing the focus change.</param>
		protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{

			// If the visual element that lost the focus was associated with a virtual cell, then all the distinctive borders are
			// removed from the report.
			DependencyObject dependencyObject = Keyboard.FocusedElement as DependencyObject;
			Boolean isChildOfCell = false;
			while (dependencyObject != null)
			{
				if (DynamicReport.GetCell(dependencyObject) != null)
				{
					isChildOfCell = true;
					break;
				}
				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
			}
			if (!isChildOfCell)
			{
				this.selectedRanges.Clear();
				SelectCells();
			}

			// Allow the base class to handle the rest of the keyboard focus event.
			base.OnPreviewLostKeyboardFocus(e);

		}

		/// <summary>
		/// Selects all the cells in the selected columns, unselects the rest.
		/// </summary>
		/// <param name="anchorColumn">The anchor column.</param>
		private void SelectCells()
		{

			// This flag indicates that the selection has changed and is used to force the canvas redraw the selected area.
			bool hasSelectionChanged = false;
			Boolean rowIsSelected;
			List<ReportRow> selectedRows = new List<ReportRow>();

			this.ReportGrid.SelectedRowBlocks.Clear();
			this.ReportGrid.SelectedRowHeaderBlocks.Clear();

			// This will select all the cells that fall within the rectangles selected by the user.
			foreach (ReportRow reportRow in this.ReportGrid.Rows)
			{

				rowIsSelected = false;

				foreach (ReportCell reportCell in reportRow.Cells)
				{

					// Each cell in the report is tested against a set of rectangles.  If the cell intersects with any of the
					// selected areas, then it is selected.
					bool found = false;

					foreach (Rect selectedRange in this.selectedRanges)
						if (reportCell.Rect.IntersectsWith(selectedRange))
						{
							found = true;
							break;
						}

					// This will select the cells that belong to the selected ranges and clear the selection from any cells that
					// are no longer selected.  If the selected state of any cell changes, then a flag is set that indicates the
					// canvas should be measured and arranged.
					if (found)
					{
						if (!reportCell.IsSelected)
						{
							hasSelectionChanged = true;
							reportCell.IsSelected = true;
							rowIsSelected = true;
						}
					}
					else
					{
						if (reportCell.IsSelected)
						{
							hasSelectionChanged = true;
							reportCell.IsSelected = false;
						}
					}

				}

				if (rowIsSelected)
					selectedRows.Add(reportRow);

			}

			// When the selected state of any of the cells has changed, the canvas is measure and arranged.  This will force an
			// evaluation of the shape of the selected border.
			if (hasSelectionChanged)
			{
				this.ReportGrid.SelectedRowHeaderBlocks.Add(selectedRows);
				this.ReportGrid.SelectedRowBlocks.Add(selectedRows);
				this.ReportGrid.DynamicReport.ChangeSelection();
				InvalidateMeasure();
			}

		}

	}

}

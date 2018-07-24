namespace FluidTrade.Core.Windows.Controls
{

	using System;
    using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
	/// A canvas used for the row header area of a report.
	/// </summary>
	internal class RowHeaderCanvas : ReportCanvas
	{

		// Public Static Fields
		public static readonly System.Windows.DependencyProperty IsHeaderFrozenProperty;
		public static readonly System.Windows.Input.RoutedUICommand FreezeRowHeaders;

		// Private Instance Fields
		private HeaderPopup headerPopup;
		private ReportRow destinationRow;
		private RowHeaderCanvas.DropAction destinationState;
		private RowHeaderCanvas.MouseState mouseState;
		private RowHeightPopup rowHeightPopup;
		private DestinationPopup destinationPopup;
		private System.Collections.Generic.List<List<ReportRow>> selectedRanges;
		private System.Collections.Generic.List<ReportCell> headerCells;
		private System.Double resizeStart;
		private System.Windows.Point anchorPoint;
		private System.Windows.Point mouseDownLocation;
		private System.Collections.Generic.List<SortItem> sortOrder;
		private System.Windows.Input.Cursor selectRow;
		private System.Windows.Input.Cursor horizontalSplit;
		private System.Windows.Input.Cursor bigEx;

		// Private Enumerations
		private enum MouseState { ButtonUp, ButtonDown, DraggingRow, DraggingTile, ResizingRow, Selecting };
		private enum DropAction { NoAction, Select, Delete };

		// Public Events
		internal event ResizeRowHandler ResizeMouseMove;

		/// <summary>
		/// Creates the static resources used by this class.
		/// </summary>
		static RowHeaderCanvas()
		{

			// This value is used to prevent modifications to the colum header.  Once frozen, selecting a part of the header will 
			// select the entire row.  When not frozen, dragging rows around will change the order and size of the rows.
			RowHeaderCanvas.IsHeaderFrozenProperty = DependencyProperty.Register("IsHeaderFrozen", typeof(object),
				typeof(RowHeaderCanvas), new FrameworkPropertyMetadata(true));

			// Select Row Command
			RowHeaderCanvas.FreezeRowHeaders = new RoutedUICommand("FreezeRowHeaders", "Freeze Row Headers", typeof(RowHeaderCanvas));

		}

		/// <summary>
		/// Creates a manager for the drawing operations of a document viewer.
		/// </summary>
		internal RowHeaderCanvas()
		{

			// Indicates that the row can be selected.
			this.selectRow = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/SelectRow.cur", UriKind.Relative)).Stream);

			// Indicates that a row is being resized.
			this.horizontalSplit = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/HorizontalSplit.cur", UriKind.Relative)).Stream);

			// This cursor indicates that a row will be deleted if dropped.
			this.bigEx = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/BigEx.cur", UriKind.Relative)).Stream);

			// The Row Popup is used for drag and drop operations that move the row from one place to another or removes it
			// from the report.
			this.headerPopup = new HeaderPopup();
			this.headerPopup.PlacementTarget = this;
			this.headerPopup.Orientation = Orientation.Horizontal;

			// The Row Height Popup is used when resizing a row.  It provides feedback about the proposed new size of the
			// row.
			this.rowHeightPopup = new RowHeightPopup();
			this.rowHeightPopup.PlacementTarget = this;
			this.rowHeightPopup.HorizontalOffset = 2.0;

			// The Destination Popup displays two red arrows to indicate the destination location of a drag-and-drop operation for
			// a row heading.
			this.destinationPopup = new DestinationPopup();
			this.destinationPopup.PlacementTarget = this;
			this.destinationPopup.Orientation = Orientation.Horizontal;
			this.destinationPopup.HorizontalOffset = 0.0;

			// These lists are used to manage the selection of rows.
			this.selectedRanges = new List<List<ReportRow>>();
			this.headerCells = new List<ReportCell>();

			// Command Bindings
			this.CommandBindings.Add(new CommandBinding(RowHeaderCanvas.FreezeRowHeaders, HandleFrozenHeader));

		}

		/// <summary>
		/// Gets or sets an indicator of whether the panes are frozen or allowed to move.
		/// </summary>
		public bool IsHeaderFrozen
		{
			get { return (bool)this.GetValue(RowHeaderCanvas.IsHeaderFrozenProperty); }
			set { this.SetValue(RowHeaderCanvas.IsHeaderFrozenProperty, value); }
		}

        /// <summary>
        /// Returns the selected rows
        /// </summary>
        public List<List<ReportRow>> SelectedRowBlocks
        {
            get { return this.selectedRanges; }
        }


        /// <summary>
        /// Allow the report grid to set the selected rows from a CTRL+A
        /// </summary>
        /// <param name="reportRowList"></param>
        internal void SetSelectedRowBlocks(List<List<ReportRow>> reportRowList)
        {

			// Alternative Method:
			this.selectedRanges.Clear();

			if (reportRowList != null)
				this.selectedRanges.AddRange(reportRowList);
			else
				// Clear the Header Row if it is selected.
				foreach (ReportCell cell in this.ReportGrid.Rows.FindRowAt(0).Cells)
					cell.IsSelected = false;


			
			// Original Method:
			/*
			this.selectedRanges.Clear();
			
			if (reportRowList != null)
			{
				this.selectedRanges.AddRange(reportRowList);
			}
			else
			{
				// Clear the Header Row if it is selected.
				ReportRow selectedRow = this.ReportGrid.Rows.FindRowAt(0);
				foreach (ReportColumn reportColumn in this.ReportGrid.Columns)
				{
					ReportCell reportCell = selectedRow[reportColumn];
						reportCell.IsSelected = false;
				}
			}
			*/


		}


		/// <summary>
		/// Handles a change to the frozen state of the header.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		private void HandleFrozenHeader(object sender, RoutedEventArgs routedEventArgs)
		{

			// Just toggle the state.
			this.IsHeaderFrozen = !this.IsHeaderFrozen;

		}

		/// <summary>
		/// Selects all the cells in the selected rows, unselects the rest.
		/// </summary>
		private void SelectRows()
		{
			foreach (List<ReportRow> block in this.ReportGrid.SelectedRowBlocks)
				foreach (ReportRow row in block)
					foreach (ReportCell cell in row.Cells)
						cell.IsSelected = false;

			this.ReportGrid.SelectedRowBlocks.Clear();

			foreach (List<ReportRow> block in this.selectedRanges)
			{

				this.ReportGrid.SelectedRowBlocks.Add(block);
				//selected.AddRange(block);

				foreach (ReportRow row in block)
					foreach (ReportCell cell in row.Cells)
						cell.IsSelected = true;

			}
			// This will redraw the newly selected cells with the highlighting.
			//this.ReportGrid.InvalidateMeasure();
			this.ReportGrid.InvalidateVisual();

		}

		/// <summary>
		/// Handles the mouse button being pressed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{

			// The sort order is maintained in this structure.
			this.sortOrder = new List<SortItem>();

			// This gets the location of the mouse in document coordinates.
			Mouse.Capture(this);

			// This state variable will control how the 'Mouse Move' and 'Mouse Up' event handlers interpret the user action.  The
			// 'selectedRow' field is used as the starting point for any drag-and-drop type of operation.
			this.mouseState = MouseState.ButtonDown;

			// Evaluate the state of the keyboard.  Key combinations involving the shift and control keys will alter the areas that
			// are selected.
			bool isShiftKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
			bool isControlKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

			// This will use the current position for an anchor unless the shift key is pressed.
			this.mouseDownLocation = e.GetPosition(this);
			if (!isShiftKeyPressed)
				this.anchorPoint = this.mouseDownLocation;

			// The mouse indicates which column has been selected and the anchor indicates the starting point of the selection in 
			// an extended selection operation.
			ReportRow selectedRow = this.ReportGrid.Rows.FindRowAt(this.mouseDownLocation.Y);
			ReportRow anchorRow = this.ReportGrid.Rows.FindRowAt(this.anchorPoint.Y);

			Point mouseDownLocation = e.GetPosition(this);
			if (!isShiftKeyPressed)
			{
				IInputElement iInputElement = this.InputHitTest(mouseDownLocation);
				DependencyObject dependencyObject = iInputElement as DependencyObject;
				while (dependencyObject != null)
				{
					if (DynamicReport.GetCell(dependencyObject) != null)
					{
						Keyboard.Focus(dependencyObject as IInputElement);
						break;
					}
					dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				}
			}

			// Every cell that appears in the header canvas is considered part of the selectable header.  This will collect all the
			// selected cells in a list while creating a rectangle that is the union of all those selected cells.
			this.headerCells.Clear();
			foreach (ReportColumn reportColumn in this.ReportGrid.Columns)
			{
				ReportCell reportCell = selectedRow[reportColumn];
				if (reportColumn.Left < this.ActualWidth)
					this.headerCells.Add(reportCell);
			}

			// If a row is selected then the position and movement of the mouse will suggest one of several gestures that need
			// to be interpreted: is a row being moved, is it being resized, is it being deleted or selected?  The code below
			// will begin to interpret the input gesture.
			if (selectedRow != null)
			{

				// The header has two modes: when the headers are frozen, only selection operations are enabled with the mouse.
				// When not frozen, the rows can be moved, resized, resorted and removed.
				if (this.IsHeaderFrozen)
				{

					// The shift and control key extend the selection operation in the same way as Microsoft Excel.
					if (isShiftKeyPressed || isControlKeyPressed)
					{

						// When the shift key is pressed during row selection, every row between the last row selected 
						// and the current row is selected.
						if (isShiftKeyPressed)
						{

							// In the unlikely event that the shift key was down during the setting of the anchor point, this will
							// create a dummy entry in the list of selected row ranges.
							if (this.selectedRanges.Count == 0)
							{
								List<ReportRow> reportRows = new List<ReportRow>();
								reportRows.Add(selectedRow);
								this.selectedRanges.Add(reportRows);
							}

							// The most recent range will be replaced with a new range when the mouse is dragged around the row 
							// headers.  This has the effect of clearing the rows that are no longer selected and selecting only
							// the rows between the anchor and the currently selected row.
							List<ReportRow> lastRange = this.selectedRanges[this.selectedRanges.Count - 1];

							// This will select each row between the last selected row and the one just selected.  Note that the
							// list of rows is distinct, so duplicate rows are ignored.
							ReportRow firstRow = anchorRow.Top < selectedRow.Top ? anchorRow : selectedRow;
							ReportRow secondRow = anchorRow.Top < selectedRow.Top ? selectedRow : anchorRow;
							lastRange.Clear();
							foreach (ReportRow reportRow in this.ReportGrid.Rows)
								if (firstRow.Top <= reportRow.Top && reportRow.Top <= secondRow.Top)
									lastRange.Add(reportRow);

						}

						// When the control key is pressed a single row is added to the range of rows selected.
						if (isControlKeyPressed)
						{

							// This removes any previous instance of this row in the selection.
							foreach (List<ReportRow> rowRange in this.selectedRanges)
								if (rowRange.Contains(selectedRow))
									rowRange.Remove(selectedRow);

							// The row is added (or re-added) at the start of the range of selected rows.
							List<ReportRow> reportRows = new List<ReportRow>();
							reportRows.Add(selectedRow);
							this.selectedRanges.Add(reportRows);

						}

					}
					else
					{
                        // The row is added at the start of a new range of selected rows.
                        List<ReportRow> reportRows = new List<ReportRow>();

                        // The if condition below fixes issue that allows the right click to not clear the selected rows.
                        // Hence it allows every other condition to clear the state.
						if (e.RightButton != MouseButtonState.Pressed)
						{
							// A simple selection that doesn't involve the modifier keys will clear out any previously selected ranges.
							this.selectedRanges.Clear();
						}
						else
						{
							// This removes any previous instance of this row in the selection.
							foreach (List<ReportRow> rowRange in this.selectedRanges)
								if (rowRange.Contains(selectedRow))
									rowRange.Remove(selectedRow);
						}

						reportRows.Add(selectedRow);
						this.selectedRanges.Add(reportRows);

					}

					// This will select all the rows in the selected ranges of rows and remove the selection from all the 
					// rest of the cells.
					SelectRows();

					// This instructs the event handlers how the mouse movement is to be interpreted.
					this.mouseState = MouseState.Selecting;

				}
				else
				{

					// The top mouse button can either select the row or begin a resizing operation.  This will perform a 'Hit
					// Test' to see which operation should be performed.
					if (e.LeftButton == MouseButtonState.Pressed)
					{

						// This is a 'Hit Test' for the bottom edge of the row header tile to see if the user is trying to change
						// the size of the row.  If the mouse is close to the bottom edge, then the drag operation to change the
						// size of the tile is begun.
						if (selectedRow.Bottom - DynamicReport.splitBorder <= this.mouseDownLocation.Y &&
							this.mouseDownLocation.Y < selectedRow.Bottom)
						{
							this.resizeStart = selectedRow.Bottom;
							this.mouseState = MouseState.ResizingRow;
							this.destinationRow = null;
						}
						else
						{

							// This is a 'Hit Test' for the top edge of the row header tile to see if the user is trying to change
							// the size of the row.  Note that because the top edge really belongs to the previous row header when
							// resizing, that the previous row is selected for the operation.
							if (selectedRow.Top <= this.mouseDownLocation.Y &&
								this.mouseDownLocation.Y < selectedRow.Top + DynamicReport.splitBorder)
							{
								this.resizeStart = selectedRow.Top;
								this.mouseState = MouseState.ResizingRow;
								foreach (ReportRow reportRow in this.ReportGrid.Rows)
									if (reportRow.Bottom == selectedRow.Top)
										selectedRow = reportRow;
								this.destinationRow = null;
							}

						}

					}

					// At this point, a resizing operation has been selected from the input gesture of the mouse.
					if (this.mouseState == MouseState.ResizingRow)
					{

						// The parent window will watch for this event to tell it how to draw the row width indicator lines. The
						// dimension and location of those lines are outside of this window and must be handled by the parent.
						if (this.ResizeMouseMove != null)
							this.ResizeMouseMove(this, new ResizeRowEventArgs(selectedRow, selectedRow.Height,
														   false));

						// This window provides quantitative feedback for the new width of the row.  The offsets were arrived at
						// empirically from reverse engineering Excel.
						this.rowHeightPopup.VerticalOffset = this.mouseDownLocation.Y - 2.0;
						this.rowHeightPopup.Content = selectedRow.Height;
						this.rowHeightPopup.IsOpen = true;

					}
					else
					{

						// This will select the button momentarily for drag-and-drop and sorting operations.
						foreach (ReportCell reportCell in this.headerCells)
							reportCell.IsSelected = true;

					}

				}

			}

		}

		/// <summary>
		/// Handles the movement of the mouse in the row header.
		/// </summary>
		/// <param name="e">The mouse movement event arguments.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{

			// This gets the location of the mouse in document coordinates.
			Point mouseLocation = e.GetPosition(this);

			// The anchorRow describes the starting row for an extended selection.  The selectedRow is the one over which 
			// the mouse is currently.
			ReportRow selectedRow = this.ReportGrid.Rows.FindRowAt(mouseLocation.Y);
			ReportRow anchorRow = this.ReportGrid.Rows.FindRowAt(this.anchorPoint.Y);

			// The action taken by a mouse movement in the row heading of a viewer is driven by a set of states.  These states
			// are driven, in turn, by where the mouse started and what buttons are pressed.  They can instruct the viewer to
			// resize the rows, move the row, delete the row or sort the row.
			switch (this.mouseState)
			{

			case MouseState.Selecting:

				// When dragging the mouse around the row headings, the most recent range will be replaced.  The new range consist
				// of every row between the anchor row and the row over which the mouse is currently.

				// HACK - Fix this when the selected range is guaranteed to have rows in it.
				if (this.selectedRanges.Count > 0)
				{
					List<ReportRow> lastRange = this.selectedRanges[this.selectedRanges.Count - 1];
					if ((lastRange[lastRange.Count - 1] != selectedRow) && (selectedRow !=null))
					{

						// This will select each row between the last selected row and the one just selected.  Note that the
						// list of rows is distinct, so duplicate rows are ignored.
						ReportRow firstRow = anchorRow.Top < selectedRow.Top ? anchorRow : selectedRow;
						ReportRow secondRow = anchorRow.Top < selectedRow.Top ? selectedRow : anchorRow;
						lastRange.Clear();
						foreach (ReportRow reportRow in this.ReportGrid.Rows)
							if (firstRow.Top <= reportRow.Top && reportRow.Top <= secondRow.Top)
								lastRange.Add(reportRow);

						// Add the row Header to the range also so the it will be highlight too.
						lastRange.Add(ReportGrid.reportRowCollection.FindRowAt(0));

						// This will select all the cells in the range of rows that have been selected.  Everything else will be
						// cleared.  The active cell -- the one that has the input focus -- is the first cell of the anchor row.
						SelectRows();

					}
				}

				break;

			case MouseState.ResizingRow:

				// This will calculate the current row width based on the difference between the anchor point and the current mouse
				// location.
				double rowHeight = selectedRow.Height + (mouseLocation.Y - this.anchorPoint.Y);

				// The owner of the header canvas is called to draw the row boundary line so that it follows the mouse when the
				// user is resizing the rows.  This operation can't be done from this canvas because the size of the scrolling
				// canvas -- where the row boundary cursor line is displayed -- isn't known here.
				if (this.ResizeMouseMove != null)
					this.ResizeMouseMove(this, new ResizeRowEventArgs(selectedRow, rowHeight, false));

				// As the size of the row is dragged around, the Popup widow displays a human readable form of the width.
				this.rowHeightPopup.Content = rowHeight;

				break;

			case MouseState.ButtonDown:

				// When the user presses the top mouse button, they initiate some drag operation and the mouse activity is
				// captured by the row header window.  If the user is simply moving the mouse over the window, then feedback is
				// given in the shape of the cursor. This formula determins if the mouse has moved an absolute distance of four
				// pixels from the original location. If it has, the user has selected a movement operation for the row.
				// Otherwise, the mouse operation will be interpreted as a request for a new sort order when the top mouse button
				// is lifted.
				if (Math.Sqrt(Math.Pow(mouseLocation.Y - this.mouseDownLocation.Y, 2.0) +
					Math.Pow(mouseLocation.Y - this.mouseDownLocation.Y, 2.0)) > DynamicReport.headerDragTrigger)
				{

					// At this point the mouse movements are intepreted as drag-and-drop operations for the row headers. The
					// drop states determines what happens when the mouse button is released.  It can either be moved, deleted or
					// have no action taken.
					this.mouseState = MouseState.DraggingRow;
					this.destinationState = DropAction.NoAction;

					// When dragging a row, the proposed destination appears as a set of two red arrows marking where the row
					// will reside if dropped.  The scale of the destination arrows must match the scale of the report.
					this.destinationPopup.Scale = this.ReportGrid.DynamicReport.Scale;
					this.destinationPopup.Visibility = Visibility.Hidden;
					this.destinationPopup.IsOpen = true;

					// This sets up the dragging operation by creating a destination cursor (the red arrows that point to where the
					// row will 'snap' into place), a row cursor (it looks like the row header was ripped out of the page)
					// and positions the row cursor at the tip of the current mouse location.
					this.headerPopup.Resources = this.ReportGrid.Resources;
					this.headerPopup.Content = this.headerCells;
					this.headerPopup.Scale = this.ReportGrid.DynamicReport.Scale;
					this.headerPopup.Height = selectedRow.Height;
					this.headerPopup.Width = this.ActualWidth;
					this.headerPopup.Location = mouseLocation;
					this.headerPopup.IsOpen = true;

				}

				break;

			case MouseState.DraggingRow:

				// If the window that contains the row headings contains the cursor, then it's possible that a destination is
				// selected for the row drag-and-drop operation.  If the cursor is outside of the header quadrant, the row
				// will be deleted when the mouse button is released.
				if (this.Viewport.Contains(mouseLocation))
				{

					// Any operation inside the visible header gets the basic pointing arrow for a cursor.
					this.Cursor = this.selectRow;

					// When the mouse is inside the header quadrant but there is no destination selected, then nothing will happen
					// when the mouse button is release.
					this.destinationState = DropAction.NoAction;

					// This attempts to find a destination for the row operation.
					if (selectedRow != null)
					{

						// A row can't be its own destination.
						if (anchorRow != selectedRow)
						{

							// A destination is selected if the left edge of the target column is entirely visible in the header
							// quadrant and the left half of the column header contains the current mouse location.
							Rect testAreaTop = new Rect(this.Viewport.Left, selectedRow.Top, this.Viewport.Width,
								selectedRow.Height / 2.0);
							if (testAreaTop.Contains(mouseLocation) && anchorRow.Bottom != selectedRow.Top)
							{
								this.destinationState = DropAction.Select;
								this.destinationRow = selectedRow;
							}

							// This will test the right half of each of the colum headers.  If the cursor is over the right half
							// and the rightmost part of the destination is entirely visible in the header, then it can be a
							// destination.
							Rect testAreaBottom = new Rect(this.Viewport.Left, selectedRow.Top + selectedRow.Height / 2.0,
								this.Viewport.Width, selectedRow.Height / 2.0);
							if (testAreaBottom.Contains(mouseLocation) && selectedRow.Bottom != anchorRow.Top)
							{
								this.destinationState = DropAction.Select;
								this.destinationRow = null;
								foreach (ReportRow nextRow in this.ReportGrid.Rows)
									if (nextRow.Top == selectedRow.Bottom)
										this.destinationRow = nextRow;
							}

						}

					}

					// If a valid destination was found in the search above, move the set of red arrows (the destination cursor)
					// over the exact spot where the row will be moved.
					if (this.destinationState == DropAction.Select)
					{
						this.destinationPopup.VerticalOffset = this.destinationRow == null ?
							this.ReportGrid.ExtentHeight : this.destinationRow.Top;
						this.destinationPopup.Visibility = Visibility.Visible;
					}
					else
					{
						this.destinationPopup.Visibility = Visibility.Hidden;
					}

				}
				else
				{

					// If the mouse isn't over the row header quadrant, a big 'X' instead of the destination cursor give the
					// user feedback that the row will be dropped from the viewer if they release the mouse button.
					this.destinationPopup.Visibility = Visibility.Hidden;
					this.Cursor = this.bigEx;

					// This will instruct the 'mouse up' action to delete the currently selected row.
					this.destinationState = DropAction.Delete;

				}

				// The cursor row is really a floating window, not a cursor.  It needs to be moved to match the location of the
				// mouse.  Note that the floating window doesn't have a parent, so the coordinates are in screen units.
				this.headerPopup.Location = mouseLocation;

				break;

			case MouseState.ButtonUp:

				// This will determine which cursor should be used when the button isn't pressed while moving the mouse: a horizontal
				// size cursor or a regular arrow cursor.  If the mouse is over the bottom or top edge of the row, then the
				// horizontal resizing cursor is used.
				bool isResizingRow = false;

				// This will attempt to hit test the current row to see if it is a candidate for resizing.
				if (selectedRow != null)
				{

					// This is a 'Hit Test' for the bottom edge of the row header tile to see if the user is trying to change the
					// size of the row.
					if (selectedRow.Top + selectedRow.Height - DynamicReport.splitBorder <= mouseLocation.Y &&
						mouseLocation.Y < selectedRow.Top + selectedRow.Height)
						isResizingRow = true;

					// This is a 'Hit Test' for the top edge of the row header tile to see if the user is trying to change the
					// size of the row.
					if (selectedRow.Top <= mouseLocation.Y &&
						mouseLocation.Y < selectedRow.Top + DynamicReport.splitBorder)
						isResizingRow = true;

				}

				// Only the selection cursor is used when the header is frozen.  When unfrozen, select the resizing cursor when
				// the mouse is over the edge of the column header.
				this.Cursor = this.IsHeaderFrozen || !isResizingRow ? this.selectRow : this.horizontalSplit;

				break;

			}

		}

		/// <summary>
		/// Handles the mouse button being released over a row header.
		/// </summary>
		/// <param name="state">The thread initialization parameter.</param>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{

			// This gets the location of the mouse in document coordinates.
			Point mouseLocation = e.GetPosition(this);

			// Evaluate the state of the keyboard.  Key combinations involving the shift and control keys will alter the areas that
			// are selected.
			bool isShiftKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
			bool isControlKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

			// The target row for any movement operations is the last row selected.
			ReportRow anchorRow = this.ReportGrid.Rows.FindRowAt(this.anchorPoint.Y);

			// The mouse state indications what action should be taken when the mouse button is released.
			switch (this.mouseState)
			{

			case MouseState.ResizingRow:

				// At the conclusion of the resizing operation a decision is made to remove or change the width of a row.  If 
				// the size is reduced to zero (or less), this gesture is taken as a command to remove the row.  Any positive
				// value for a width will result in a row change command.
				double rowHeight = anchorRow.Height + (mouseLocation.Y - this.anchorPoint.Y);
				rowHeight = rowHeight < 0.0 ? 0.0 : rowHeight;
				if (this.ResizeMouseMove != null)
					this.ResizeMouseMove(this, new ResizeRowEventArgs(anchorRow, rowHeight, true));

				// Note - this is where the logic should go for removing or resizing a row.  It should probably bubble up through 
				// the Report and be handled by some higher logic that determins if removing or resizing a row is a legal action.
				// Currently it is not supported.

				// This will hide the popup until it is needed again.
				this.rowHeightPopup.IsOpen = false;

				// This is a momentary button: the column heading will loose the selection state when the button is released.
				foreach (ReportCell reportCell in this.headerCells)
					reportCell.IsSelected = false;

				break;

			case MouseState.DraggingRow:

				// The action taken when the dragging operation is complete depends on whether a valid destination is selected or
				// whether the row is meant to be deleted.
				switch (this.destinationState)
				{

				case DropAction.Select:

					// This will move the row from its current location to the desired location.
					if (this.destinationPopup.Visibility == Visibility.Visible)
					{

						// TODO - This should generate an event to move a row from one place to another.

					}

					break;

				case DropAction.Delete:

					// This will delete the row from the view.
					this.ReportGrid.Rows.Remove(anchorRow);

					break;

				}

				// This is a momentary button: the column heading will loose the selection state when the button is released.
				foreach (ReportCell reportCell in this.headerCells)
					reportCell.IsSelected = false;

				break;

			}

			// This resets the state of the mouse for the next operation.
			this.mouseState = MouseState.ButtonUp;

			if (this.headerPopup.IsOpen)
				this.headerPopup.IsOpen = false;
			
			// Hide the destination cursor when the mouse is released.
			if (this.destinationPopup.IsOpen)
				this.destinationPopup.IsOpen = false;

			// Release the Hounds, errr... mouse.
			Mouse.Capture(null);

		}

		/// <summary>
		/// Handles a change to the size of the header window.
		/// </summary>
		/// <param name="sizeInfo">Information about the size change.</param>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{

			// Changing the height of the control changes the height of the cursor that indicates the destination for a row
			// movement operation.
			if (sizeInfo.WidthChanged)
				this.destinationPopup.TargetDistance = sizeInfo.NewSize.Width;

			// Allow the base class to handle the remainder of the event.
			base.OnRenderSizeChanged(sizeInfo);

		}

	}

}

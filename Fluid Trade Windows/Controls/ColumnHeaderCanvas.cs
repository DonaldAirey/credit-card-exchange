namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;

    /// <summary>
	/// A canvas used for the column header area of a report.
	/// </summary>
	public class ColumnHeaderCanvas : ReportCanvas
	{

		// Public Static Fields
		public static readonly DependencyProperty IsHeaderFrozenProperty;

		// Private Instance Fields
		private ReportColumn anchorColumn;
		private Point anchorPoint;
		private Cursor bigEx;
		private ColumnWidthPopup columnWidthPopup;
		private ReportColumn destinationColumn;
		private DestinationPopup destinationPopup;
		private ColumnHeaderCanvas.DropAction destinationState;
		private List<ReportCell> headerCells;
		private HeaderPopup headerPopup;
		private Point mouseDownLocation;
		private ColumnHeaderCanvas.MouseState mouseState;
		private Double resizeStart;
		private Cursor selectColumn;
		private List<List<ReportColumn>> selectedRanges;
		private List<SortItem> sortOrder;
		private Cursor verticalSplit;

		// Private Enumerations
		private enum MouseState { ButtonUp, ButtonDown, DraggingColumn, DraggingTile, ResizingColumn, Selecting };
		private enum DropAction { NoAction, Select, Delete };

		// Public Events
		internal event ResizeColumnHandler ResizeMouseMove;

		/// <summary>
		/// Creates the static resources used by this class.
		/// </summary>
		static ColumnHeaderCanvas()
		{

			// This value is used to prevent modifications to the colum header.  Once frozen, selecting a part of the header will
			// select the entire column.  When not frozen, dragging columns around will change the order and size of the columns.
			ColumnHeaderCanvas.IsHeaderFrozenProperty = DependencyProperty.Register(
				"IsHeaderFrozen",
				typeof(object),
				typeof(ColumnHeaderCanvas),
				new FrameworkPropertyMetadata(true));

		}

		/// <summary>
		/// Creates a manager for the drawing operations of a document viewer.
		/// </summary>
		internal ColumnHeaderCanvas()
		{

			// Indicates that the column can be selected.
			this.selectColumn = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/SelectColumn.cur", UriKind.Relative)).Stream);

			// Indicates that a column is being resized.
			this.verticalSplit = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/VerticalSplit.cur", UriKind.Relative)).Stream);

			// This cursor indicates that a column will be deleted if dropped.
			this.bigEx = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/BigEx.cur", UriKind.Relative)).Stream);

			this.sortOrder = new List<SortItem>();

			// The Column Popup is used for drag and drop operations that move the column from one place to another or removes it
			// from the report.
			this.headerPopup = new HeaderPopup();
			this.headerPopup.PlacementTarget = this;
			this.headerPopup.Orientation = Orientation.Vertical;

			// The Column Width Popup is used when resizing a column.  It provides feedback about the proposed new size of the
			// column.
			this.columnWidthPopup = new ColumnWidthPopup();
			this.columnWidthPopup.PlacementTarget = this;
			this.columnWidthPopup.VerticalOffset = 2.0;

			// The Destination Popup displays two red arrows to indicate the destination location of a drag-and-drop operation for
			// a column heading.
			this.destinationPopup = new DestinationPopup();
			this.destinationPopup.PlacementTarget = this;
			this.destinationPopup.Orientation = Orientation.Vertical;
			this.destinationPopup.VerticalOffset = 0.0;

			// These lists are used to manage the selection of columns.
			this.selectedRanges = new List<List<ReportColumn>>();
			this.headerCells = new List<ReportCell>();

		}

		/// <summary>
		/// Gets or sets an indicator of whether the panes are frozen or allowed to move.
		/// </summary>
		public bool IsHeaderFrozen
		{
			// FB 191 - Remove PadLock to do this we are 
			//  Setting the default getter always return IsHeaderFrozen=false
			//  Disable the ability to set IsHeaderFrozen as this not needed anymore. 
			//	TODO: In the future we should remove the setter if are not going to all setting the IsHeaderFrozen property.
			//get { return (bool)this.GetValue(ColumnHeaderCanvas.IsHeaderFrozenProperty); }
			get { return false; }
			set { this.SetValue(ColumnHeaderCanvas.IsHeaderFrozenProperty, value); }
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
		/// Selects all the cells in the selected columns, unselects the rest.
		/// </summary>
		/// <param name="anchorColumn">The anchor column.</param>
		private void SelectCells()
		{

			// This rectangle is used to find the first active row after the header.
			Rect selectedRect = this.headerCells[0].Rect;
			for (int index = 1; index < this.headerCells.Count; index++)
				selectedRect = Rect.Union(selectedRect, this.headerCells[index].Rect);

			// This will select the cells based on the columns that have been selected.  Note that the cell in the anchor column 
			// closest to the header is made the input cell.
			foreach (ReportRow reportRow in this.ReportGrid.Rows)
				foreach (ReportColumn reportColumn in this.ReportGrid.Columns)
				{

					// This cell will be examined to see if it is selected or not.
					ReportCell reportCell = reportRow[reportColumn];

					// Search through the range of selected columns to see if the cell should be selected or not.
					bool isColumnSelected = false;
					foreach (List<ReportColumn> columnRange in this.selectedRanges)
						if (columnRange.Contains(reportColumn))
						{
							isColumnSelected = true;
							break;
						}

					// This will select the column if it is part of the selected range of columns or clear it if it isn't.  Note
					// that the cell in the anchor column that is closest to the header gets the input focus.
					if (isColumnSelected)
					{
						reportCell.IsSelected = true;
					}
					else
					{
						reportCell.IsSelected = false;
					}

				}

			// This will redraw the newly selected cells with the highlighting.
			this.ReportGrid.InvalidateMeasure();

		}

		/// <summary>
		/// Handles the mouse button being pressed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{

			// This gets the location of the mouse in document coordinates.
			Mouse.Capture(this);

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
			this.mouseDownLocation = e.GetPosition(this);
			if (!isShiftKeyPressed)
				this.anchorPoint = this.mouseDownLocation;

			// The mouse indicates which column has been selected and the anchor indicates the starting point of the selection in 
			// an extended selection operation.
			ReportColumn selectedColumn = this.ReportGrid.Columns.FindColumnAt(this.mouseDownLocation.X);
			this.anchorColumn = this.ReportGrid.Columns.FindColumnAt(this.anchorPoint.X);

			// Every cell that appears in the header canvas is considered part of the selectable header.  This will collect all the
			// selected cells in a list while creating a rectangle that is the union of all those selected cells.
			this.headerCells.Clear();
			foreach (ReportRow reportRow in this.ReportGrid.Rows)
			{
				ReportCell reportCell = reportRow[this.anchorColumn];
				if (reportRow.Top < this.ActualHeight)
					this.headerCells.Add(reportCell);
			}

			// Every cell that appears in the header canvas is considered part of the selectable header.  This will collect all the
			// selected cells in a list while creating a rectangle that is the union of all those selected cells.
			Rect selectedRect = this.headerCells[0].Rect;
			for (int index = 1; index < this.headerCells.Count; index++)
				selectedRect = Rect.Union(selectedRect, this.headerCells[index].Rect);

			// If a column is selected then the position and movement of the mouse will suggest one of several gestures that need
			// to be interpreted: is a column being moved, is it being resized, is it being deleted or selected?  The code below
			// will begin to interpret the input gesture.
			if (selectedColumn != null)
			{

				// The header has two modes: when the headers are frozen, only selection operations are enabled with the mouse.  
				// When not frozen, the columns can be moved, resized, resorted and removed.
				if (this.IsHeaderFrozen)
				{

					// The shift and control key extend the selection operation in the same way as Microsoft Excel.
					if (isShiftKeyPressed || isControlKeyPressed)
					{

						// When the shift key is pressed during column selection, every column between the last column selected 
						// and the current column is selected.
						if (isShiftKeyPressed)
						{

							// In the unlikely event that the shift key was down during the setting of the anchor point, this will
							// create a dummy entry in the list of selected column ranges.
							if (this.selectedRanges.Count == 0)
							{
								List<ReportColumn> reportColumns = new List<ReportColumn>();
								reportColumns.Add(selectedColumn);
								this.selectedRanges.Add(reportColumns);
							}

							// The most recent range will be replaced with a new range when the mouse is dragged around the column 
							// headers.  This has the effect of clearing the columns that are no longer selected and selecting only
							// the columns between the anchor and the currently selected column.
							List<ReportColumn> lastRange = this.selectedRanges[this.selectedRanges.Count - 1];
							int firstIndex = this.ReportGrid.Columns.IndexOf(this.anchorColumn);
							int secondIndex = this.ReportGrid.Columns.IndexOf(selectedColumn);
							int startIndex = firstIndex < secondIndex ? firstIndex : secondIndex;
							int endIndex = firstIndex > secondIndex ? firstIndex : secondIndex;

							// This will replace the last range in the list with the new range selected from the current position
							// and the anchor position.
							lastRange.Clear();
							for (int index = startIndex; index <= endIndex; index++)
								lastRange.Add(this.ReportGrid.Columns[index] as ReportColumn);

						}

						// When the control key is pressed a single column is added to the range of columns selected.
						if (isControlKeyPressed)
						{

							// This removes any previous instance of this column in the selection.
							foreach (List<ReportColumn> columnRange in this.selectedRanges)
								if (columnRange.Contains(selectedColumn))
									columnRange.Remove(selectedColumn);

							// The column is added (or re-added) at the start of the range of selected columns.
							List<ReportColumn> reportColumns = new List<ReportColumn>();
							reportColumns.Add(selectedColumn);
							this.selectedRanges.Add(reportColumns);

						}

					}
					else
					{

						// A simple selection that doesn't involve the modifier keys will clear out any previously selected ranges.
						selectedRanges.Clear();

						// The column is added at the start of a new range of selected columns.
						List<ReportColumn> reportColumns = new List<ReportColumn>();
						reportColumns.Add(selectedColumn);
						this.selectedRanges.Add(reportColumns);

					}

					// This will select all the columns in the selected ranges of columns and remove the selection from all the 
					// rest of the cells.
					SelectCells();

					// This instructs the event handlers how the mouse movement is to be interpreted.
					this.mouseState = MouseState.Selecting;

				}
				else
				{

					// The left mouse button can either select the column or begin a resizing operation.  This will perform a 'Hit
					// Test' to see which operation should be performed.
					if (e.LeftButton == MouseButtonState.Pressed)
					{

						// This is a 'Hit Test' for the right edge of the column header tile to see if the user is trying to 
						// change the size of the column.  If the mouse is close to the right edge, then the drag operation to
						// change the size of the tile is begun.
						if (selectedColumn.Right - DynamicReport.splitBorder <= this.mouseDownLocation.X &&
							this.mouseDownLocation.X < selectedColumn.Right)
						{
							this.resizeStart = selectedColumn.Right;
							this.mouseState = MouseState.ResizingColumn;
							this.destinationColumn = null;
						}
						else
						{

							// This is a 'Hit Test' for the left edge of the column header tile to see if the user is trying to 
							// change the size of the column.  Note that because the left edge really belongs to the previous
							// column header when resizing, that the previous column is selected for the operation.
							if (selectedColumn.Left <= this.mouseDownLocation.X &&
								this.mouseDownLocation.X < selectedColumn.Left + DynamicReport.splitBorder)
							{

								this.resizeStart = selectedColumn.Left;
								this.mouseState = MouseState.ResizingColumn;
								int index = this.ReportGrid.Columns.IndexOf(selectedColumn);
								if (index != 0)
								{
									this.anchorColumn = this.ReportGrid.Columns[index - 1] as ReportColumn;
									this.destinationColumn = null;
								}

							}

						}

					}

					// At this point, a resizing operation has been selected from the input gesture of the mouse.
					if (this.mouseState == MouseState.ResizingColumn)
					{

						// The parent window will watch for this event to tell it how to draw the column width indicator lines. The
						// dimension and location of those lines are outside of this window and must be handled by the parent.
						if (this.ResizeMouseMove != null)
							this.ResizeMouseMove(this, new ResizeColumnEventArgs(this.anchorColumn, this.anchorColumn.Width,
														   false));

						// This window provides quantitative feedback for the new width of the column.  The offsets were arrived at
						// empirically from reverse engineering Excel.
						this.columnWidthPopup.HorizontalOffset = this.mouseDownLocation.X - 2.0;
						this.columnWidthPopup.Content = selectedColumn.Width;
						this.columnWidthPopup.IsOpen = true;

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
		/// Handles the movement of the mouse in the column header.
		/// </summary>
		/// <param name="e">The mouse movement event arguments.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{

			// This gets the location of the mouse in document coordinates.
			Point mouseLocation = e.GetPosition(this);

            // FogBugz Case #75 - (NP) 20090319 - if we past the end of the columns on the left most column
            // then set the x location of the mouse to zero.
            // Now this solves the problem for this case but might cause other problems.
            // To resolve if there are other problems then and allow selectedColumn to be negative 
            // and set selectedColumn for each "case" conditon below. 
            // Otherwise selectedColumn will be null and cause this issue.
            // Current testing appear this fix will work.
            if (mouseLocation.X < 0)
                mouseLocation.X = 0.0;

			// The selectedColumn is the one over which the mouse is currently.
			ReportColumn selectedColumn = this.ReportGrid.Columns.FindColumnAt(mouseLocation.X);

			// The action taken by a mouse movement in the column heading of a viewer is driven by a set of states.  These states
			// are driven, in turn, by where the mouse started and what buttons are pressed.  They can instruct the viewer to
			// resize the columns, move the column, delete the column or sort the column.
			switch (this.mouseState)
			{

			case MouseState.Selecting:

				// When dragging the mouse around the column headings, the most recent range will be replaced.  The new range
				// consist of every column between the anchor column and the column over which the mouse is currently.
				List<ReportColumn> lastRange = this.selectedRanges[this.selectedRanges.Count - 1];
				if (lastRange[lastRange.Count - 1] != selectedColumn)
				{

					// The idea here is to select every column between the anchor column and the currently selected column with the idea that
					// this selection will replace the previous range.
					int firstIndex = this.ReportGrid.Columns.IndexOf(this.anchorColumn);
					int secondIndex = this.ReportGrid.Columns.IndexOf(selectedColumn);
					int startIndex = firstIndex < secondIndex ? firstIndex : secondIndex;
					int endIndex = firstIndex > secondIndex ? firstIndex : secondIndex;

					// This will select each column between the anchor column and the one just selected and replace the last 
					// selected range with this newly evaluated one.
					lastRange.Clear();
					for (int index = startIndex; index <= endIndex; index++)
					{
						ReportColumn reportColumn = this.ReportGrid.Columns[index] as ReportColumn;
						if (!lastRange.Contains(reportColumn))
							lastRange.Add(reportColumn);
					}

					// This will select all the cells in the range of columns that have been selected.  Everything else will be 
					// cleared.  The active cell -- the one that has the input focus -- is the first cell of the anchor column.
					SelectCells();

				}

				break;

			case MouseState.ResizingColumn:

				// This will calculate the current column width based on the difference between the anchor ponit and the current
				// mouse location.
				double columnWidth = this.anchorColumn.Width + (mouseLocation.X - this.anchorPoint.X);

				// The owner of the header canvas is called to draw the column boundary line so that it follows the mouse when the
				// user is resizing the columns.  This operation can't be done from this canvas because the size of the scrolling
				// canvas -- where the column boundary cursor line is displayed -- isn't known here.
				if (this.ResizeMouseMove != null)
					this.ResizeMouseMove(this, new ResizeColumnEventArgs(this.anchorColumn, columnWidth, false));

				// As the size of the column is dragged around, the Popup widow displays a human readable form of the width.
				this.columnWidthPopup.Content = columnWidth;

				break;

			case MouseState.ButtonDown:

				// When the user presses the left mouse button, they initiate some drag operation and the mouse activity is
				// captured by the column header window.  If the user is simply moving the mouse over the window, then feedback is
				// given in the shape of the cursor. This formula determins if the mouse has moved an absolute distance of four
				// pixels from the original location. If it has, the user has selected a movement operation for the column.
				// Otherwise, the mouse operation will be interpreted as a request for a new sort order when the left mouse button
				// is lifted.
				if (Math.Sqrt(Math.Pow(mouseLocation.X - this.mouseDownLocation.X, 2.0) +
					Math.Pow(mouseLocation.Y - this.mouseDownLocation.Y, 2.0)) > DynamicReport.headerDragTrigger)
				{

					// At this point the mouse movements are intepreted as drag-and-drop operations for the column headers. The
					// drop states determines what happens when the mouse button is released.  It can either be moved, deleted or
					// have no action taken.
					this.mouseState = MouseState.DraggingColumn;
					this.destinationState = DropAction.NoAction;

					// When dragging a column, the proposed destination appears as a set of two red arrows marking where the column
					// will reside if dropped.  The scale of the destination arrows must match the scale of the report.
					this.destinationPopup.Scale = this.ReportGrid.DynamicReport.Scale;
					this.destinationPopup.Visibility = Visibility.Hidden;
					this.destinationPopup.IsOpen = true;

					// This sets up the dragging operation by creating a destination cursor (the red arrows that point to where the
					// column will 'snap' into place), a column cursor (it looks like the column header was ripped out of the page)
					// and positions the column cursor at the tip of the current mouse location.
					this.headerPopup.Resources = this.ReportGrid.Resources;
					this.headerPopup.Content = this.headerCells;
					this.headerPopup.Scale = this.ReportGrid.DynamicReport.Scale;
					this.headerPopup.Width = selectedColumn.Width;
					this.headerPopup.Height = this.ActualHeight;
					this.headerPopup.Location = mouseLocation;
					this.headerPopup.IsOpen = true;

				}

				break;

			case MouseState.DraggingColumn:

				// If the window that contains the column headings contains the cursor, then it's possible that a destination is
				// selected for the column drag-and-drop operation.  If the cursor is outside of the header quadrant, the column
				// will be deleted when the mouse button is released.
				if (this.Viewport.Contains(mouseLocation))
				{

					// Any operation inside the visible header gets the basic pointing arrow for a cursor.
					this.Cursor = this.selectColumn;

					// When the mouse is inside the header quadrant but there is no destination selected, then nothing will happen
					// when the mouse button is release.
					this.destinationState = DropAction.NoAction;

					// This attempts to find a destination for the column operation.
					if (selectedColumn != null)
					{

						// A column can't be its own destination.
						if (this.anchorColumn != selectedColumn)
						{

							// A destination is selected if the left edge of the target column is entirely visible in the header
							// quadrant and the left half of the column header contains the current mouse location.
							Rect testAreaLeft = new Rect(selectedColumn.Left, this.Viewport.Top, selectedColumn.Width / 2.0,
								this.Viewport.Height);
							if (testAreaLeft.Contains(mouseLocation) && this.anchorColumn.Right != selectedColumn.Left)
							{
								this.destinationState = DropAction.Select;
								this.destinationColumn = selectedColumn;
							}

							// This will test the right half of each of the colum headers.  If the cursor is over the right half
							// and the rightmost part of the destination is entirely visible in the header, then it can be a
							// destination.
							Rect testAreaRight = new Rect(selectedColumn.Left + selectedColumn.Width / 2.0, this.Viewport.Top,
								selectedColumn.Width / 2.0, this.Viewport.Height);
							if (testAreaRight.Contains(mouseLocation) && selectedColumn.Right != this.anchorColumn.Left)
							{
								this.destinationState = DropAction.Select;
								int index = this.ReportGrid.Columns.IndexOf(selectedColumn);
								this.destinationColumn = index + 1 == this.ReportGrid.Columns.Count ? null :
									this.ReportGrid.Columns[index + 1] as ReportColumn;
							}

						}

					}

					// If a valid destination was found in the search above, move the set of red arrows (the destination cursor)
					// over the exact spot where the column will be moved.
					if (this.destinationState == DropAction.Select)
					{
						this.destinationPopup.HorizontalOffset = this.destinationColumn == null ?
							this.ReportGrid.ExtentWidth : this.destinationColumn.Left;
						this.destinationPopup.Visibility = Visibility.Visible;
					}
					else
					{
						this.destinationPopup.Visibility = Visibility.Hidden;
					}

				}
				else
				{

					// If the mouse isn't over the column header quadrant, a big 'X' instead of the destination cursor give the
					// user feedback that the column will be dropped from the viewer if they release the mouse button.
					this.destinationPopup.Visibility = Visibility.Hidden;
					this.Cursor = this.bigEx;

					// This will instruct the 'mouse up' action to delete the currently selected column.
					this.destinationState = DropAction.Delete;

				}

				// The cursor column is really a floating window, not a cursor.  It needs to be moved to match the location of the
				// mouse.  Note that the floating window doesn't have a parent, so the coordinates are in screen units.
				this.headerPopup.Location = mouseLocation;

				break;

			case MouseState.ButtonUp:

				// This will determine which cursor should be used when the button isn't pressed while moving the mouse: a 
				// vertical size cursor or a regular arrow cursor.  If the mouse is over the right or left edge of the column,
				// then the vertical resizing cursor is used.
				bool isResizingColumn = false;

				// This will attempt to hit test the current column to see if it is a candidate for resizing.
				if (selectedColumn != null)
				{

					// This is a 'Hit Test' for the right edge of the column header tile to see if the user is trying to 
					// change the size of the column.
					if (selectedColumn.Left + selectedColumn.Width - DynamicReport.splitBorder <= mouseLocation.X &&
						mouseLocation.X < selectedColumn.Left + selectedColumn.Width)
						isResizingColumn = true;

					// This is a 'Hit Test' for the left edge of the column header tile to see if the user is trying to change 
					// the size of the column.
					if (selectedColumn.Left <= mouseLocation.X &&
						mouseLocation.X < selectedColumn.Left + DynamicReport.splitBorder)
						isResizingColumn = true;

				}

				// Only the selection cursor is used when the header is frozen.  When unfrozen, select the resizing cursor when 
				// the mouse is over the edge of the column header.
				this.Cursor = this.IsHeaderFrozen || !isResizingColumn ? this.selectColumn : this.verticalSplit;

				break;

			}

		}

		/// <summary>
		/// Handles the mouse button being released over a column header.
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

			// The mouse state indications what action should be taken when the mouse button is released.
			switch (this.mouseState)
			{

			case MouseState.ResizingColumn:

				// At the conclusion of the resizing operation a decision is made to remove or change the width of a column.  If 
				// the size is reduced to zero (or less), this gesture is taken as a command to remove the column.  Any positive
				// value for a width will result in a column change command.
				double columnWidth = this.anchorColumn.Width + (mouseLocation.X - this.anchorPoint.X);
				columnWidth = columnWidth < 0.0 ? 0.0 : columnWidth;
				if (this.ResizeMouseMove != null)
					this.ResizeMouseMove(this, new ResizeColumnEventArgs(this.anchorColumn, columnWidth, true));
				if (columnWidth <= 0)
					this.ReportGrid.Columns.Remove(this.anchorColumn);
				else
					this.ReportGrid.Columns.SetColumnWidth(this.anchorColumn, columnWidth);

				// This will hide the popup until it is needed again.
				this.columnWidthPopup.IsOpen = false;

				// This is a momentary button: the column heading will loose the selection state when the button is released.
				foreach (ReportCell reportCell in this.headerCells)
					reportCell.IsSelected = false;

				break;

			case MouseState.ButtonDown:

				// This will sort the document by one or more selected columns.  The control key is used to select multiple columns
				// similar to the selection mode of Microsoft Office.  The first step to sorting is to find out which column in the
				// view has been selected for sorting.
				ReportColumn reportColumn = this.ReportGrid.Columns.FindColumnAt(mouseLocation.X);
				ReportField fieldDefinition = this.ReportGrid.Fields[reportColumn.ColumnId];

				// The 'SortTemplate' attribute is used to distinguish columns that can be selected (usually columns that have user
				// input can be selected) versus columns that can be sorted.
				if (!fieldDefinition.IsSelectable)
				{

					// If the control key is kept pressed, then the selected sort columns will be combined.  Otherwise, any
					// previously selected sort column is discarded.
					if (!isControlKeyPressed)
						while (this.sortOrder.Count > 1 && this.sortOrder[this.sortOrder.Count - 1].Column != fieldDefinition)
							this.sortOrder.Remove(this.sortOrder[0]);

					// The direction of the sorting in a column can be toggled each time it is selected.  This requires a test to
					// make sure that the same column has been selected.  Of course, if there is nothing in the list of sorting
					// columns, then the list is initialize with an ascending sort.
					if (this.sortOrder.Count != 0)
					{

						// Find the last sort column in the sort specification.  If it matches the last column selected, then the
						// order of the sorting is toggled.  If it doesn't match, then the column is added to the list of columns
						// when the control key is pressed or used to initialize the list when the control key is not pressed.
						SortItem sortItem = this.sortOrder[this.sortOrder.Count - 1];
						if (sortItem.Column == fieldDefinition)
						{
							sortItem.SortOrder = sortItem.SortOrder == SortOrder.Ascending ? SortOrder.Descending :
								SortOrder.Ascending;
						}
						else
						{
							if (!isControlKeyPressed)
								this.sortOrder.Remove(sortItem);
							this.sortOrder.Add(new SortItem(fieldDefinition, SortOrder.Ascending));
						}

					}
					else
					{

						// This will initialize the list of sort columns when the list is empty.
						this.sortOrder.Add(new SortItem(fieldDefinition, SortOrder.Ascending));

					}

					FluidTradeCommands.SortReport.Execute(new SortEventArgs(this.sortOrder), this);

				}

				// This is a momentary button: the column heading will loose the selection state when the button is released.
				foreach (ReportCell reportCell in this.headerCells)
					reportCell.IsSelected = false;

				break;

			case MouseState.DraggingColumn:

				// The action taken when the dragging operation is complete depends on whether a valid destination is selected or
				// whether the column is meant to be deleted.
				switch (this.destinationState)
				{

				case DropAction.Select:

					// This will move the column from its current location to the desired location.
					if (this.destinationPopup.Visibility == Visibility.Visible)
					{

						// When the column is removed from the list, all the columns that appeared after the removed column will be shifted to
						// the left.  This will adjust the target index so that the column will be re-inserted into the list at the desired
						// index after being removed.
						int newIndex = this.destinationColumn == null ? this.ReportGrid.Columns.Count :
							this.ReportGrid.Columns.IndexOf(this.destinationColumn);
						int oldIndex = this.ReportGrid.Columns.IndexOf(this.anchorColumn);
						newIndex = oldIndex < newIndex ? newIndex - 1 : newIndex;
						this.ReportGrid.Columns.Move(oldIndex, newIndex);
					}

					break;

				case DropAction.Delete:

					// This will delete the column from the view.
					this.ReportGrid.Columns.Remove(this.anchorColumn);

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

			// Changing the height of the control changes the height of the cursor that indicates the destination for a column
			// movement operation.
			if (sizeInfo.HeightChanged)
				this.destinationPopup.TargetDistance = sizeInfo.NewSize.Height;

			// Allow the base class to handle the remainder of the event.
			base.OnRenderSizeChanged(sizeInfo);

		}

	}

}

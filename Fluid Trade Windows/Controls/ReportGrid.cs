namespace FluidTrade.Core.Windows.Controls
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Shapes;
	using System.Windows.Threading;


    enum ReferencePoint { TopLeft, BottomLeft, TopRight, BottomRight };

    /// <summary>
    /// Provides different views of the same report in a screen split into four quadrants.
    /// </summary>
    public class ReportGrid : Grid, IScrollInfo
    {

        // Enumerations
        private enum MouseState { ButtonUp, ButtonDown, DraggingColumn, DraggingTile, ResizingColumn };
        private enum DropAction { NoAction, Select, Delete };

        // Private Constants
        private const System.Int32 defaultDuration = 0;
		private const int reportToolTipShowMSecs = 7777;
		//time to let search spin (for now 2 minutes)
		private const int searchTimeout = 120000;
        // Private Read Only Fields
        private readonly System.Double splitterLength = 5.0;
        private readonly System.Windows.Media.Color splitColor = Color.FromArgb(0xFF, 0xD7, 0xE6, 0xF7);

        // Public Static Fields
        public static readonly RoutedEvent CollectionChangedEvent;
        public static readonly RoutedEvent ColumnChangedEvent;
        public static readonly RoutedEvent DeleteEvent;
        public static readonly RoutedEvent InsertEvent;
		public static readonly RoutedEvent ChildFocusEvent;
		public static readonly RoutedEvent ShowToolTipEvent;

		ReportCell previousSearchCell = null;
		string searchString = null;

		// Volatile is used as hint to the compiler that this data
		// member will be accessed by multiple threads.
		private volatile bool _shouldStop;

		// Delegate for when the search is completed.
		Action searchCompletedAction;

		/// <summary>
		/// tick count the last time the mouse moved
		/// </summary>
		private int lastMouseMoveTick;

		/// <summary>
		/// hover rect that moust must move outside of to close tooltip
		/// </summary>
		private System.Windows.Rect toolTipHoverRect;

		/// <summary>
		/// tick count when the tool tip was shown
		/// </summary>
		private int toolTipShowTick;

		/// <summary>
		/// 0.5 sec timer that is used to manage tool tip show/hide
		/// </summary>
		private System.Timers.Timer toolTipTimer;

		/// <summary>
		/// tooltip object for grid
		/// </summary>
		private ToolTip reportGridToolTip;

        /// <summary>
        /// Identifies the MarkThree.Windows.Controls.ReportGrid.Scale dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleProperty;

        /// <summary>
        /// Identifies the MarkThree.Windows.Controls.ReportGrid.Split dependency property.
        /// </summary>
        public static readonly DependencyProperty SplitProperty;


        /// <summary>
        /// Gets a value that represents the SetScale command
        /// </summary>
        public static readonly RoutedCommand SetScale;

        /// <summary>
        /// Gets a value that represents the SetSplit command.
        /// </summary>
        public static readonly RoutedCommand SetSplit;

        // Internal Instance Fields
        internal ReportCanvas[] reportCanvases;
        private ReportCell focusedCell;

        // Private Instance Fields
        private BodyCanvas bodyCanvas;
        private ColumnHeaderCanvas columnHeaderCanvas;
        private RowHeaderCanvas rowHeaderCanvas;
        //private ReportCanvas contextMenuCanvas;
        private Boolean canHorizontallyScroll;
        private Boolean canVerticallyScroll;
        private Boolean isLayoutFrozen;
        private Double scaleFactor;
        private GridSplitter horizontalGridSplitter;
        private GridSplitter verticalGridSplitter;
        private ScrollViewer scrollOwner;
        private Cursor horizontalSplit;
        private Cursor selectColumn;
        private Cursor selectRow;
        private Cursor verticalSplit;
        private ScaleTransform scaleTransform;
        private Point offset;
        private Size extent;
        private Size viewport;
        private Line resizeColumnStart;
        private Line resizeColumnEnd;

        internal ReportFieldCollection reportFieldCollection;
        internal ReportColumnCollection reportColumnCollection;
        internal ReportRowCollection reportRowCollection;
        private RowTemplateCollection rowTemplateCollection;

        private System.Collections.Generic.List<List<ReportRow>> selectedRanges;
		private bool isColumnHeaderClicked = false;
        private ReportCell currentSelectedCell = null;

		public bool IsColumnHeaderClicked
		{
			get { return isColumnHeaderClicked; }
			set { isColumnHeaderClicked = value; }
		}

        public ReportCell CurrentSelectedCell
        {
            get { return currentSelectedCell; }
        }

        internal UndoManager undoManager;
        internal Duration duration;
        internal Dictionary<ICommand, CommandBinding> commandBingingMap;

        static ReportGrid()
        {
            // Scale
            ReportGrid.ScaleProperty = DependencyProperty.Register(
                "Scale",
                typeof(Double),
                typeof(ReportGrid),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnScaleChanged)));

            // Split
            ReportGrid.SplitProperty = DependencyProperty.Register(
                "Split",
                typeof(Size),
                typeof(ReportGrid),
                new FrameworkPropertyMetadata(new Size(), FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnSplitChanged)));

            //// SetScale
            ReportGrid.SetScale = new RoutedCommand("SetScale", typeof(ReportGrid));

            // SetSplit
            ReportGrid.SetSplit = new RoutedCommand("SetSplit", typeof(ReportGrid));

            // Insert Routed Event
            ReportGrid.InsertEvent = EventManager.RegisterRoutedEvent(
                "Insert",
                RoutingStrategy.Bubble,
                typeof(ColumnDefinitionsEventHandler),
                typeof(ReportGrid));

            // Delete Routed Event
            ReportGrid.DeleteEvent = EventManager.RegisterRoutedEvent(
                "Delete",
                RoutingStrategy.Bubble,
                typeof(ColumnDefinitionsEventHandler),
                typeof(ReportGrid));

            // CollectionChanged Routed Event
            ReportGrid.CollectionChangedEvent = EventManager.RegisterRoutedEvent(
                "CollectionChanged",
                RoutingStrategy.Bubble,
                typeof(CollectionChangedEventHandler),
                typeof(ReportGrid));

            // ColumnWidthChanged Routed Event
            ReportGrid.ColumnChangedEvent = EventManager.RegisterRoutedEvent(
                "ColumnChanged",
                RoutingStrategy.Bubble,
                typeof(ColumnChangedEventHandler),
                typeof(ReportGrid));

            ReportGrid.ChildFocusEvent = EventManager.RegisterRoutedEvent(
                "ChildFocus",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ReportGrid));

			ReportGrid.ShowToolTipEvent = EventManager.RegisterRoutedEvent(
				"ShowToolTipEvent",
				RoutingStrategy.Bubble,
				typeof(ReportGridtToolTipEventHandler),
				typeof(ReportGrid));

        }

        /// <summary>
        /// Creates a manager for the drawing operations of a document viewer.
        /// </summary>
        public ReportGrid()
        {

            // Grids normally aren't allowed to get the focus.  This grid is allowed to get the focus long enough to figure out 
            // which of the visual elements within should be given the focus.
            FocusManager.SetIsFocusScope(this, true);
            this.Focusable = true;

            // This is the default duration of the animations that are performed when the collection changes.
            this.duration = new Duration(TimeSpan.FromMilliseconds(ReportGrid.defaultDuration));

            this.CommandBindings.Add(new CommandBinding(ReportGrid.SetSplit, OnSetSplit));
            this.CommandBindings.Add(new CommandBinding(ReportGrid.SetScale, OnSetScale));

            // A grid is used to divide the screen into four viewports.  Each having an independent view of the same underlying
            // report.  An additional row and column is used for the screen splitter.
            base.RowDefinitions.Add(new RowDefinition());
            base.RowDefinitions.Add(new RowDefinition());
            base.RowDefinitions.Add(new RowDefinition());
            base.ColumnDefinitions.Add(new ColumnDefinition());
            base.ColumnDefinitions.Add(new ColumnDefinition());
            base.ColumnDefinitions.Add(new ColumnDefinition());

            // The Undo Manager provides the ability to Undo and Redo operations within its scope.  Specialized Undo classes that
            // handle the commands particular to a given class are mapped to that class.  This mapping provides a mechanism whereby
            // any control that is added as a child anywhere in the logical hierarchy is given a chance to listen in on the routed
            // events as they happen.  When some event modifies a given control, it stores the proper command handler for undoing
            // that event onto the shared 'Undo' stack.
            this.undoManager = new UndoManager();
            this.undoManager.TypeUndoMap.Add(typeof(ReportGrid), new ReportUndo(this.undoManager));
            UndoManager.SetUndoScope(this, this.undoManager);

            // The report is organized into columns and rows.  This collection describes the fields that are the non-volatile 
            // description of the columns that can appear in a report.  While the selection and order of the columns can change,
            // the fields are relatively static and provide a way for a user to add columns to the report that are no longer
            // visible.
            this.reportFieldCollection = new ReportFieldCollection();

            // This control has a sophisticated method of displaying the focus of the child elements and doesn't need the default
            // visual style.
            this.FocusVisualStyle = null;

            this.RequestBringIntoView += new RequestBringIntoViewEventHandler(OnRequestBringIntoView);
            AddHandler(ReportGrid.ChildFocusEvent, new RoutedEventHandler(OnChildFocusHandler));

            // This collection describe the selection and order of the columns.
            this.reportColumnCollection = new ReportColumnCollection(this);
            this.AddLogicalChild(this.reportColumnCollection);

            // This collection describes the hierarchical organization of the report.
            this.rowTemplateCollection = new RowTemplateCollection();

            // The collection of rows in the report are found here.
            this.reportRowCollection = new ReportRowCollection(this);

            // Each canvas represents a part of the split screen view.
            this.reportCanvases = new ReportCanvas[4];

            // This panel is fixed in the upper, left hand corner and does not scroll.
            this.reportCanvases[0] = new ReportCanvas();
            this.reportCanvases[0].Name = "Q0";
            Grid.SetRow(this.reportCanvases[0], 0);
            Grid.SetColumn(this.reportCanvases[0], 0);
            this.Children.Add(this.reportCanvases[0]);

            // This panel contains the column headings and can scroll horizontally.
            this.columnHeaderCanvas = new ColumnHeaderCanvas();
            this.columnHeaderCanvas.ResizeMouseMove += new ResizeColumnHandler(columnHeaderCanvas_ResizeMouseMove);
            this.reportCanvases[1] = this.columnHeaderCanvas;
            this.reportCanvases[1].Name = "Q1";
            Grid.SetRow(this.reportCanvases[1], 0);
            Grid.SetColumn(this.reportCanvases[1], 2);
            this.Children.Add(this.reportCanvases[1]);

            // This panel contains the row headings and can scroll vertically.
            this.rowHeaderCanvas = new RowHeaderCanvas();
            this.reportCanvases[2] = this.rowHeaderCanvas;
            this.reportCanvases[2].Name = "Q2";
            Grid.SetRow(this.reportCanvases[2], 2);
            Grid.SetColumn(this.reportCanvases[2], 0);
            this.Children.Add(this.reportCanvases[2]);

            // This panel contains the content of the report and will scroll horizontally or vertically.
            this.bodyCanvas = new BodyCanvas();
            this.reportCanvases[3] = this.bodyCanvas;
            this.reportCanvases[3].Name = "Q3";
            Grid.SetRow(this.reportCanvases[3], 2);
            Grid.SetColumn(this.reportCanvases[3], 2);
            this.Children.Add(this.reportCanvases[3]);

            // This splitter is used to change the width of the row header.
            this.verticalGridSplitter = new GridSplitter();
            this.verticalGridSplitter.Focusable = false;
            this.verticalGridSplitter.Background = new SolidColorBrush(this.splitColor);
            this.verticalGridSplitter.HorizontalAlignment = HorizontalAlignment.Left;
            this.verticalGridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
            this.verticalGridSplitter.Width = this.splitterLength;
            this.verticalGridSplitter.ShowsPreview = true;
            this.verticalGridSplitter.DragCompleted += new DragCompletedEventHandler(VerticalSplitterDragCompleted);
            Grid.SetRowSpan(verticalGridSplitter, 3);
            Grid.SetColumn(verticalGridSplitter, 1);

            // This splitter is used to change the height of the column header.
            this.horizontalGridSplitter = new GridSplitter();
            this.horizontalGridSplitter.Focusable = false;
            this.horizontalGridSplitter.Background = new SolidColorBrush(this.splitColor);
            this.horizontalGridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.horizontalGridSplitter.VerticalAlignment = VerticalAlignment.Top;
            this.horizontalGridSplitter.Height = this.splitterLength;
            this.horizontalGridSplitter.ShowsPreview = true;
            this.horizontalGridSplitter.DragCompleted += new DragCompletedEventHandler(HorizontalSplitterDragCompleted);
            Grid.SetRow(horizontalGridSplitter, 1);
            Grid.SetColumnSpan(horizontalGridSplitter, 3);

            // This is the cursor used to mark the starting point for a column resizing operation.
            this.resizeColumnStart = new Line();
            Grid.SetRowSpan(this.resizeColumnStart, 3);
            Grid.SetColumnSpan(this.resizeColumnStart, 3);
            Grid.SetZIndex(this.resizeColumnStart, 1);
            this.resizeColumnStart.Stroke = Brushes.Black;
            this.resizeColumnStart.StrokeThickness = 1.0;
            this.resizeColumnStart.StrokeDashArray.Add(1.0);
            this.resizeColumnStart.StrokeDashArray.Add(1.0);

            // This is the cursor used to mark the end point for a column resizing operation.
            this.resizeColumnEnd = new Line();
            Grid.SetRowSpan(this.resizeColumnEnd, 3);
            Grid.SetColumnSpan(this.resizeColumnEnd, 3);
            Grid.SetZIndex(this.resizeColumnEnd, 1);
            this.resizeColumnEnd.Stroke = Brushes.Black;
            this.resizeColumnEnd.StrokeThickness = 1.0;
            this.resizeColumnEnd.StrokeDashArray.Add(1.0);
            this.resizeColumnEnd.StrokeDashArray.Add(1.0);

            // These resources are used to draw and manipulate the report.
            this.selectColumn = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/SelectColumn.cur", UriKind.Relative)).Stream);
            this.selectRow = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/SelectRow.cur", UriKind.Relative)).Stream);
            this.verticalSplit = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/VerticalSplit.cur", UriKind.Relative)).Stream);
            this.horizontalSplit = new Cursor(Application.GetResourceStream(new Uri("/FluidTrade.FluidTradeWindows;component/Controls/Resources/HorizontalSplit.cur", UriKind.Relative)).Stream);

            // These values support the scrolling interface.  The grid has no ability to scroll on its own.  Each of the four 
            // canvases use a TranslateTransform to move the content within the viewport that is presented on screen.
            this.canHorizontallyScroll = false;
            this.canVerticallyScroll = false;
            this.scrollOwner = null;
            this.viewport = new Size();

            // The split screens are frozen by default.
            this.isLayoutFrozen = true;
            base.RowDefinitions[1].Height = new GridLength(0.0);
            base.ColumnDefinitions[1].Width = new GridLength(0.0);

            // The split size determines where each of the four quadrants will be placed within the grid.  The offset is used to
            // move the virtual space of each of the quadrants to make them appear to scroll in the finite pane in which they are
            // presented.
            this.offset = new Point();

            // This transform is used to set the magnification factor.  The aspect ratio is fixed at 1:1.
            this.scaleFactor = DefaultDocument.ScaleFactor;
            this.scaleTransform = new ScaleTransform(this.scaleFactor, this.scaleFactor);
            this.LayoutTransform = this.scaleTransform;

            // This creates a fast lookup table for the command bindings so they can be quickly added and removed.  The need to 
            // reference the bindings quickly comes in handling the Undo/Redo logic where the command handlers need to be inhibited
            // temporarly while restoring the values.
            this.commandBingingMap = new Dictionary<ICommand, CommandBinding>();
            foreach (CommandBinding commandBinding in this.CommandBindings)
                this.commandBingingMap.Add(commandBinding.Command, commandBinding);

			this.selectedRanges = new List<List<ReportRow>>();

			//create tooltip
			this.reportGridToolTip = new ToolTip();
			this.reportGridToolTip.Closed += new RoutedEventHandler(reportGridToolTip_Closed);
			
			this.ToolTip = this.reportGridToolTip;
	
			//create tooltip timer that is used to determine hover time
			//and close time on the grid. 
			//the grid is different than other controls because normally you move out of the
			//control and you can get another tooltip, but for the grid you want
			//to be able to get a different tooltip for each cell, eventhough it 
			//is just one control
			
			//timer will tick each 500 ms and will check member vars to 
			//see if hover or close time has elapsed. 
			this.toolTipTimer = new System.Timers.Timer(500);
			this.toolTipTimer.AutoReset = true;
			this.toolTipTimer.Elapsed += new System.Timers.ElapsedEventHandler(toolTipTimer_ElapsedWorkerThread);
			((ToolTip)this.ToolTip).IsOpen = false;

			//cutom handle tooltip subscribe to event and just mark as handled
			this.ToolTipOpening += new ToolTipEventHandler(ReportGrid_ToolTipOpening);
			this.ToolTipClosing += new ToolTipEventHandler(ReportGrid_ToolTipClosing);

			this.columnHeaderCanvas.MouseDown += new MouseButtonEventHandler(columnHeaderCanvas_MouseDown);
		}

		void columnHeaderCanvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			isColumnHeaderClicked = true;
		}


		public void UpdateBodyCanvasCursor(bool isBusy)
		{
			this.bodyCanvas.UpdateCursor(isBusy);
		}

		/// <summary>
		/// handler for tooltip close.. will clear the content
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void reportGridToolTip_Closed(object sender, RoutedEventArgs e)
		{
			this.reportGridToolTip.Content = null;
		}


		/// <summary>
		/// handler to tooltip timer. This will not fire on the ui thread so need to marshal calls
		/// back to ui thread
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void toolTipTimer_ElapsedWorkerThread(object sender, System.Timers.ElapsedEventArgs e)
		{
			//handle the tick on the UI thread
			this.Dispatcher.BeginInvoke(new System.Threading.WaitCallback(toolTipTimer_ElapsedGridUIThread), new object[1]);
		}

		/// <summary>
		/// handler for tooltip timer event on the UI thread
		/// if tooltip is open will check if close time has elapsed, if so will close tooltip
		/// if tooltip is closed will check mouse hover time has elapsed, if so will open tooltip
		/// </summary>
		/// <param name="state">not used</param>
		private void toolTipTimer_ElapsedGridUIThread(object state)
		{
			//if timer is disabled nothing we need to do
			if(this.toolTipTimer.Enabled == false)
				return;

			if(this.reportGridToolTip.IsOpen == true)
			{
				//if tooltip open if show time has elapsed close it
				if(Environment.TickCount - this.toolTipShowTick > reportToolTipShowMSecs)
				{
					this.OnCloseTooltip();
				}
			}
			else
			{
				//tooltip closed check if hover time has elapsed
				if(this.lastMouseMoveTick >= this.toolTipShowTick &&
					Environment.TickCount - this.lastMouseMoveTick > SystemParameters.MouseHoverTime.Milliseconds)
				{
					this.OnShowToolTip();
				}
			}
		}

		/// <summary>
		/// method to close tooltip
		/// when tooltip closes the tooltip content is cleared
		/// </summary>
		protected virtual void OnCloseTooltip()
		{
			if(this.reportGridToolTip == null)
				return;

			if(this.reportGridToolTip.IsOpen == true)
				this.reportGridToolTip.IsOpen = false;

			this.toolTipHoverRect = Rect.Empty;
		}

		/// <summary>
		/// method to show tooltip will raise ShowToolTipEvent event to allow
		/// subscriber to set the tooltip content 
		/// if content is not set tooltip is not shown
		/// </summary>
		protected virtual void OnShowToolTip()
		{
			//get mouse postion
			Point p = Mouse.GetPosition(this);
			
			//get the report cell at the mouse point
			IInputElement inputElementAtPoint;
			ReportCell reportCell = GetReportCellFromMouseLocation(p, out inputElementAtPoint);
			if(reportCell == null)
				return;

			this.reportGridToolTip.Content = null;

			try
			{
				//fire event to subscribers allowing them to set the tooltip content
				this.RaiseEvent(new ReportGridtToolTipEventArgs(ReportGrid.ShowToolTipEvent, this, reportCell, inputElementAtPoint, this.reportGridToolTip));
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.Assert(false, ex.Message, ex.ToString());
				// Make sure any errors trying to show a tooltip are logged.
				EventLog.Error("{0}, {1}", ex.ToString(), ex.StackTrace);

				return;
			}
			//if content is set show it
			if(this.reportGridToolTip.Content != null)
			{
				//compute rectangle that mouse needs to move out of to close the toolip
				System.Windows.Rect tmpHoverRect = new Rect(p, new Size(SystemParameters.MouseHoverWidth, SystemParameters.MouseHoverHeight));
				tmpHoverRect.Offset(-(SystemParameters.MouseHoverWidth / 2F), -(SystemParameters.MouseHoverHeight / 2F));
				this.toolTipHoverRect = tmpHoverRect;
				((ToolTip)this.ToolTip).IsOpen = true;
				this.toolTipShowTick = Environment.TickCount+10; //just make sure this does not == the mouseMoveTick
			}
		}

		/// <summary>
		/// mouse leave override
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			//turn off toolTip timer
			this.toolTipTimer.Enabled = false;
			//close tooltip
			this.OnCloseTooltip();
			
			base.OnMouseLeave(e);
		}

		/// <summary>
		/// mouse move override
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseMove(MouseEventArgs e)
		{			
			base.OnMouseMove(e);

			//record the last time the mouse moved
			this.lastMouseMoveTick = Environment.TickCount;

			//if tooltip is open check if mouse has moved outside of hover rect
			if(this.reportGridToolTip.IsOpen == true &&
				this.toolTipHoverRect.Contains(e.GetPosition(this)) == false)
			{
				this.OnCloseTooltip();
			}
			
			//if tooltip timer is disabled start it. Timer should 
			//always be disabled it mouse is outisde of grid
			if(this.toolTipTimer.Enabled == false)
				this.toolTipTimer.Enabled = true;
		}

		/// <summary>
		/// hander for default tooltip show event, mark as handled because grid needs to
		/// show and hide tooltip in a custom manner
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReportGrid_ToolTipClosing(object sender, ToolTipEventArgs e)
		{
			e.Handled = true;
		}

		/// <summary>
		/// hander for default tooltip show event, mark as handled because grid needs to
		/// show and hide tooltip in a custom manner
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReportGrid_ToolTipOpening(object sender, ToolTipEventArgs e)
		{
			e.Handled = true;
		}

		/// <summary>
		/// Get a reportCell from a mouse position
		/// </summary>
		/// <param name="mouseLocation"></param>
		/// <param name="inputElementAtPoint"></param>
		/// <returns></returns>
		public ReportCell GetReportCellFromMouseLocation(Point mouseLocation, out IInputElement inputElementAtPoint)
		{
			inputElementAtPoint = this.InputHitTest(mouseLocation);
			DependencyObject dependencyObject = inputElementAtPoint as DependencyObject;
			while(dependencyObject != null)
			{
				ReportCell reportCell = DynamicReport.GetCell(dependencyObject);

				if(reportCell != null)
				{
					return reportCell;
				}
				if(dependencyObject is Visual)
				{
					// Search up the tree until we have a valid dependency object.
					dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				}
				else
				{
					break;
				}
			}

			return null;
		}

        /// <summary>
        /// Property to determine if the focus is within a control on around the control.
        /// </summary>
        public bool IsChildInFocus
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ReportCell CurrentReportCell
        {
            get { return this.focusedCell; }
        }

        public ReportRow CurrentReportRow
        {
            get 
            {
                if (this.focusedCell == null)
                    return null;

                return this.focusedCell.ReportRow; 
            }
        }

        internal void ClearFocusedCell()
        {

            this.focusedCell.IsFocused = false;
            this.focusedCell = null;

			// Clear the header row also.
			this.rowHeaderCanvas.SetSelectedRowBlocks(null);

        }

        /// <summary>
        /// Invoked when an unhandled PreviewGotKeyboardFocus attached event reaches an element in the route derived from this class.
        /// </summary>
        /// <param name="e">The arguments describing the focus change.</param>
        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {

            if (e.NewFocus is ReportGrid)
            {

                if (this.focusedCell == null)
                {
                    FocusManager.SetFocusedElement(this, this.FindFirstElement());
                    e.Handled = true;
                }

            }
            else
            {


                // This moves the logical focus from the current virtual cell to the one associated with the user interface element.
				ReportCell reportCell = DynamicReport.GetCell(e.NewFocus as DependencyObject);
                if (reportCell != null)
                {
                    SetFocusedCell(reportCell);
					//e.Handled = true;
                }
            }

            // Allow the base class to handle the rest of the keyboard focus event.
            base.OnPreviewGotKeyboardFocus(e);

        }

        /// <summary>
        /// Assign the focusedcell to new report cell 
        /// </summary>
        /// <param name="reportCell"></param>
        private void SetFocusedCell(ReportCell reportCell)
        {
            if (this.focusedCell != null)
                this.focusedCell.IsFocused = false;
            this.focusedCell = reportCell;
            this.focusedCell.IsFocused = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {

			// The base class handles any keys not handled above.
			base.OnPreviewKeyDown(e);

			if (this.focusedCell != null)
            {
                bool isShiftKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
                switch (e.Key)
                {

                    case Key.Down:

                        if (!IsChildInFocus)
                        {
                            this.MoveFocusByRow(1);
                        }

                        break;

					case Key.Enter:

                            this.MoveFocusByRow(1);
                            //e.Handled = true;

                        break;

                    case Key.Escape:

                        IsChildInFocus = false;
                        this.ExitElement();
                        break;

                    case Key.F2:

                        if (!IsChildInFocus)
                        {
                            FrameworkElement frameworkElement = FocusManager.GetFocusedElement(this) as FrameworkElement;
                            if (frameworkElement != null && this.EnterElement(frameworkElement))
                            {
                                frameworkElement.LostFocus += new RoutedEventHandler(OnElementLostFocus);
                                IsChildInFocus = true;
                            }
                        }
                        else
                        {
                            IsChildInFocus = false;
                            this.ExitElement();
                        }
                        break;

                    case Key.Left:

                        if (!IsChildInFocus && !(isShiftKeyPressed))
                        {
                            this.MoveFocusByColumn(-1);
                        }

                        break;

                    case Key.PageDown:

                        if (!IsChildInFocus && !(isShiftKeyPressed))
                        {
                            this.MoveFocusByPage(1);
                            e.Handled = true;
                        }

                        break;

                    case Key.PageUp:

                        if (!IsChildInFocus && !(isShiftKeyPressed))
                        {
                            this.MoveFocusByPage(-1);
                            e.Handled = true;
                        }

                        break;

                    case Key.Tab:

                        int direction = isShiftKeyPressed ? -1 : 1;
                        this.MoveFocusByColumn(direction);
						//e.Handled = true;
						
                        break;

                    case Key.Right:

                        if (!IsChildInFocus && !(isShiftKeyPressed))
                        {
                            this.MoveFocusByColumn(1);
                        }
                        break;

                    case Key.Up:

                        if (!IsChildInFocus && !(isShiftKeyPressed))
                        {
                            this.MoveFocusByRow(-1);
                        }

                        break;

					case Key.Delete:

						// When more than one row is selected do not allow this event to goto the individual cell.
						// I.E. Delete operation to the FocusableTextBox other wise it will delete the contents of the single.
						// We agreed that delete here should not empty the contents of all the cells of the rows selected.
						if (this.selectedRanges.Count > 0)
							if (this.selectedRanges[0].Count > 0)
							{
								e.Handled = true;
							}
						break;

                }

            }

        }

		/// <summary>
		/// This method will be called when the thread is started to perform the search on another thread.
		/// </summary>
		public void DoSearchWork()
		{
			#region Search code
			//Console.WriteLine("worker thread: working...");
			ReportCell locatedReportCell = null;
			ReportCell currentSearchCell = null;
			ReportCell reportCell = null;

			// Figure out where the current cell is to begin the search if no cell is focused then start at the top.
			if (this.CurrentReportCell == null)
			{
				//currentSearchCell = this.Rows.FindRowAt(0)[this.Columns.FindColumnAt(0)];
				this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
				{
					currentSearchCell = this.Rows.FindRowAt(0)[this.Columns.FindColumnAt(0)];
				}));
			}
			else
			{

				currentSearchCell = this.CurrentReportCell;
				if (previousSearchCell == currentSearchCell)
				{
					//currentSearchCell = FindClosestHorizontalCell(currentSearchCell, 0.0, 1);
					this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
					{
						currentSearchCell = FindClosestHorizontalCell(currentSearchCell, 0.0, 1);
					}));

				}
			}

			bool matchFound = false;
			bool outerBreakLoop = false;
			bool innerBreakLoop = false;
			int startTime = Environment.TickCount;
			while (FluidTrade.Core.Utilities.ApplicationHelper.IsAppExiting == false &&
				(!outerBreakLoop || !_shouldStop) && Environment.TickCount - startTime < searchTimeout)
			{

				while (FluidTrade.Core.Utilities.ApplicationHelper.IsAppExiting == false &&
						!innerBreakLoop && Environment.TickCount - startTime < searchTimeout)
				{
					reportCell = currentSearchCell;

					if (reportCell.Content.Key is DataTableCoordinate)
					{
						DataTableCoordinate dtc = reportCell.Content.Key as DataTableCoordinate;
						System.Data.DataRow dr = dtc.DataRow;

						if (dr != null)
						{

							#region String type search
							// String type search.
							if (dr[dtc.DataColumn.ColumnName] is string)
							{
								string extractedString = dr[dtc.DataColumn.ColumnName] as string;

								if (dtc.DataColumn.ColumnName.CompareTo("SocialSecurityNumber") == 0)
								{
									// Compare without the dashes.
									// This was done to handle situation where you might or might not have dashes in the SS#
									string noDashesExtractedString = extractedString.Replace("-", "");
									string noDashesSearchString = searchString.Replace("-", "");
									if (noDashesExtractedString.CompareTo(noDashesSearchString) == 0)
									{
										locatedReportCell = reportCell;
										matchFound = true;
										break;
									}
								}
								else if (extractedString.CompareTo(searchString) == 0 || System.Text.RegularExpressions.Regex.IsMatch(extractedString, searchString, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
								{
									locatedReportCell = reportCell;
									matchFound = true;
									break;
								}

							}
							#endregion String type search

							#region Decimal type search
							// Decimal type search.
							if (dr[dtc.DataColumn.ColumnName] is decimal)
							{

								Decimal d = 0.0m;
								Decimal? extractedDecimal = (Decimal)dr[dtc.DataColumn.ColumnName];

								// if this Decimal has a comma we need a way to remove it then do the compare.
								string searchStringCommaRemoved = searchString.Replace(",", "");

								if (extractedDecimal != null)
									d = (Decimal)extractedDecimal;
								if (extractedDecimal != null &&
									d.ToString("G29").CompareTo(searchStringCommaRemoved) == 0 ||
									System.Text.RegularExpressions.Regex.IsMatch(d.ToString("G29"), searchStringCommaRemoved, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
								{
									locatedReportCell = reportCell;
									matchFound = true;
									break;
								}
							}
							#endregion Decimal type search

							#region Date Time type search
							// Date Time type search.
							if ((dr[dtc.DataColumn.ColumnName] is System.DateTime) && dtc.DataColumn.ColumnName.CompareTo("ModifiedTime") == 0 || dtc.DataColumn.ColumnName.CompareTo("CreatedTime") == 0)
							{

								System.DateTime parsedDateTime;
								System.DateTime dbDateTime = System.DateTime.MinValue;
								System.DateTime comparableDateTime = System.DateTime.MinValue;
								string formattedDateTimeString;

								if (DateTime.TryParse(searchString, out parsedDateTime))
								{

									// Remove the seconds to prepare for Compare for the passed in string.
									formattedDateTimeString = parsedDateTime.ToString("MM/dd/yyyy hh:mm tt");
									DateTime.TryParse(formattedDateTimeString, out comparableDateTime);

									// Get the Date Time from the database.
									System.DateTime dt = System.DateTime.MinValue;
									System.DateTime? extractedSystemDateTime = (System.DateTime)dr[dtc.DataColumn.ColumnName];
									if (extractedSystemDateTime != null)
									{
										dt = (System.DateTime)extractedSystemDateTime;

										// Remove the seconds to prepare for Compare for the date time from the database.
										formattedDateTimeString = dt.ToString("MM/dd/yyyy hh:mm tt");
										DateTime.TryParse(formattedDateTimeString, out dbDateTime);
										DateTime localDateTime = dbDateTime.ToLocalTime();

										if (extractedSystemDateTime != null &&
											DateTime.Compare(localDateTime, comparableDateTime) == 0 ||
											System.Text.RegularExpressions.Regex.IsMatch(localDateTime.ToString(), comparableDateTime.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase))
										{
											locatedReportCell = reportCell;
											matchFound = true;
											break;
										}

									}

								}
								else
								{
									//Partial search inside a date.

									// Get the Date Time from the database.
									System.DateTime dt = System.DateTime.MinValue;
									System.DateTime? extractedSystemDateTime = (System.DateTime)dr[dtc.DataColumn.ColumnName];
									if (extractedSystemDateTime != null)
									{
										dt = (System.DateTime)extractedSystemDateTime;

										// Remove the seconds to prepare for Compare for the date time from the database.
										formattedDateTimeString = dt.ToString("MM/dd/yyyy hh:mm tt");

										if (System.Text.RegularExpressions.Regex.IsMatch(formattedDateTimeString, searchString, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
										{
											locatedReportCell = reportCell;
											matchFound = true;
											break;
										}
									}


								}
							}
							else if (dr[dtc.DataColumn.ColumnName] is System.DateTime)
							{
								System.DateTime parsedDateTime;
								if (DateTime.TryParse(searchString, out parsedDateTime))
								{

									System.DateTime dt = System.DateTime.MinValue;
									System.DateTime? extractedSystemDateTime = (System.DateTime)dr[dtc.DataColumn.ColumnName];
									if (extractedSystemDateTime != null)
										dt = (System.DateTime)extractedSystemDateTime;
									if (extractedSystemDateTime != null &&
										DateTime.Compare(dt, parsedDateTime) == 0)
									{
										locatedReportCell = reportCell;
										matchFound = true;
										break;
									}
								}
								else
								{
									//Partial search inside a date.

									// Get the Date Time from the database.
									System.DateTime dt = System.DateTime.MinValue;
									System.DateTime? extractedSystemDateTime = (System.DateTime)dr[dtc.DataColumn.ColumnName];
									if (extractedSystemDateTime != null)
										dt = (System.DateTime)extractedSystemDateTime;
									if (System.Text.RegularExpressions.Regex.IsMatch(dt.ToString(), searchString, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
									{
										locatedReportCell = reportCell;
										matchFound = true;
										break;
									}

								}
							}
							#endregion Date Time type search

						} // end if (reportCell.Content.Key is DataTableCoordinate)
					}
					// We have found a match break out of the inner loop.
					if (matchFound ||
						FluidTrade.Core.Utilities.ApplicationHelper.IsAppExiting == true)
						break;

					// Move to the cell to the right.
					previousSearchCell = currentSearchCell;
					//currentSearchCell = FindClosestHorizontalCell(currentSearchCell, 0.0, 1);
					this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
					{
						currentSearchCell = FindClosestHorizontalCell(currentSearchCell, 0.0, 1);
					}));

					if (currentSearchCell == null)
					{
						// We have reached the right most cell in the row.
						// roll back to the first cell in the the row.
						currentSearchCell = previousSearchCell;
						bool exitLoop = false;
						do
						{
							if (FluidTrade.Core.Utilities.ApplicationHelper.IsAppExiting == true)
								break;

							//currentSearchCell = FindClosestHorizontalCell(currentSearchCell, 0.0, -1);
							this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
							{
								currentSearchCell = FindClosestHorizontalCell(currentSearchCell, 0.0, -1);
							}));
							if (currentSearchCell == null)
							{
								exitLoop = true;
							}
							else
							{
								previousSearchCell = currentSearchCell;
							}
						} while (!exitLoop);
						currentSearchCell = previousSearchCell;
						innerBreakLoop = true;

						//TODO: add the ability to go to top of the grid and start the search again.
						//currentSearchCell = this.Rows.FindRowAt(0)[this.Columns.FindColumnAt(0)];
					}

				} // end inner loop.

				// We have found a match break out of the outer loop.
				if (matchFound ||
						FluidTrade.Core.Utilities.ApplicationHelper.IsAppExiting == true)
					break;

				// Go to the next cell down.
				previousSearchCell = currentSearchCell;
				//currentSearchCell = FindClosestVerticalCell(currentSearchCell, 0.0, 1);
				this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
				{
					currentSearchCell = FindClosestVerticalCell(currentSearchCell, 0.0, 1);
				}));
				if (currentSearchCell == null)
				{
					// We have gone down  a cell and reached the bottom of the grid.
					outerBreakLoop = true;

					//TODO: add the ability to go to top of the grid and start the search again.
					//currentSearchCell = this.Rows.FindRowAt(0)[this.Columns.FindColumnAt(0)];
				}
				else
					innerBreakLoop = false;

			} // end outer loop.

			if (locatedReportCell == null)
			{
				string message = "Search found no matching cells";
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, message, Application.Current.MainWindow.Title)));
				//return false;
			}
			else
			{
				// Bring this ReportCell into view.
				ReportCell rc = locatedReportCell;
				//SetFocusedCell(rc);
				//this.bodyCanvas.SetFocusedCell(rc);
				//BringCellInToView(rc);

				this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
				{
					SetFocusedCell(rc);
					this.bodyCanvas.SetFocusedCell(rc);
					BringCellInToView(rc);
				}));

				previousSearchCell = locatedReportCell;
				//return true;
			}

			if (FluidTrade.Core.Utilities.ApplicationHelper.IsAppExiting == true)
				return;
			RequestStop();

			#endregion Search code
			//Console.WriteLine("worker thread: terminating gracefully.");
		}

		/// <summary>
		/// All requests to stop the search from this method.
		/// </summary>
		public void CancelSearch()
		{
			// Request that the worker thread stop itself:
			RequestStop();
		}

		/// <summary>
		/// Boolean flag to allow the Search thread to be stopped.
		/// </summary>
		public void RequestStop()
		{
			_shouldStop = true;
			this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
			{
				if (this.searchCompletedAction != null)
					this.searchCompletedAction();
			}));
		}


		/// <summary>
		/// Searches for a string in the report grid.
		/// </summary>
		/// <param name="searchString"></param>
		/// <returns></returns>
		public bool FindSearchedCell(string searchStringParam, Action completeAction)
		{
			searchString = searchStringParam;
			searchCompletedAction = completeAction;

			// Place the find/search into a Thread.
			#region Threading code.
			Thread workerThread = new Thread(DoSearchWork);
			workerThread.IsBackground = true;
			workerThread.Name = string.Concat("DoSearch_", DateTime.Now.ToString("hh_mm"));
			// Start the worker thread.
			workerThread.Start();

			// Loop until worker thread activates.
			while (!workerThread.IsAlive) ;


			return true;
			#endregion Threading code.
		}


		/// <summary>
		/// Bring the Report Cell into view.
		/// </summary>
		/// <param name="targetCell"></param>
		private void BringCellInToView(ReportCell targetCell)
		{
			// If the target cell is above of the viewport, then the contents of the report are scrolled to make the target
			// visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
			// framework element is required to set the keyboard focus.
			if (targetCell.Rect.Right <= this.bodyCanvas.Viewport.Left)
			{
				SetHorizontalOffset(targetCell.Rect.Left - this.Split.Width);
				this.UpdateLayout();
			}

			// If the target cell is below the viewport, then the contents of the report are scrolled to make the target
			// visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
			// framework element is required to set the keyboard focus.
			if (targetCell.Rect.Left >= this.bodyCanvas.Viewport.Right)
			{
				SetHorizontalOffset(targetCell.Rect.Right - this.bodyCanvas.Viewport.Width - this.Split.Width);
				this.UpdateLayout();
			}

			// If the target cell is above of the viewport, then the contents of the report are scrolled to make the target
			// visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
			// framework element is required to set the keyboard focus.
			if (targetCell.Rect.Bottom <= this.bodyCanvas.Viewport.Top)
			{
				SetVerticalOffset(targetCell.Rect.Top - this.Split.Height);
				this.UpdateLayout();
			}

			// If the target cell is below the viewport, then the contents of the report are scrolled to make the target
			// visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
			// framework element is required to set the keyboard focus.
			if (targetCell.Rect.Top >= this.bodyCanvas.Viewport.Bottom)
			{
				SetVerticalOffset(targetCell.Rect.Bottom - this.bodyCanvas.Viewport.Height - this.Split.Height);
				this.UpdateLayout();
			}

		}

        /// <summary>
        /// Finds the first focusable element in the report.
        /// </summary>
        /// <returns>The first focusable element in the report.</returns>
        private FrameworkElement FindFirstElement()
        {

            // The general idea is to test the distance of all the instantiated elements to see which one is focusable and is
            // closest to the origin of the report.
            Point startingPoint = new Point();
            FrameworkElement closestElement = null;
            Double closestDistance = Double.MaxValue;

            // Crawl through each of the four canvases and each of the children in those canvases and examine the visual elements
            // to see if they can receive the input focus.  Note that the virtual elements can't be tested as the ability to focus
            // on them is not part of the information that's kept around in the virtual data structures.  Only the instantiated
            // elements have that information.
            foreach (ReportCanvas reportCanvas in this.reportCanvases)
                foreach (UIElement uiElement in reportCanvas.Children)
                    if (uiElement is FrameworkElement)
                    {
                        FrameworkElement frameworkElement = uiElement as FrameworkElement;
                        Point childPoint = uiElement.TranslatePoint(new Point(), this);
                        if (frameworkElement.Focusable)
                        {
                            Double distance = Point.Subtract(startingPoint, childPoint).Length;
                            if (distance < closestDistance)
                            {
                                closestElement = frameworkElement;
                                closestDistance = distance;
                            }
                        }
                    }

            // This will hold the element that is the closest to the origin, or null if no elements qualify.
            return closestElement;

        }

        private FrameworkElement FindElement(IContent iContent)
        {

            foreach (ReportCanvas reportCanvas in this.reportCanvases)
                foreach (UIElement uiElement in reportCanvas.Children)
                    if (uiElement is FrameworkElement)
                    {
                        FrameworkElement frameworkElement = uiElement as FrameworkElement;
                        if (frameworkElement.Focusable && iContent.Equals(frameworkElement.DataContext))
                            return frameworkElement;
                    }

            return null;

        }

        void OnElementLostFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (!frameworkElement.IsKeyboardFocusWithin)
            {
                frameworkElement.LostFocus -= new RoutedEventHandler(OnElementLostFocus);
                IsChildInFocus = false;
            }
        }

        private void ExitElement()
        {

            DependencyObject dependencyObject = FocusManager.GetFocusedElement(this) as DependencyObject;

            while (dependencyObject != null)
            {

                DependencyObject parentObject = VisualTreeHelper.GetParent(dependencyObject);

                if (parentObject is ReportCanvas && dependencyObject is IInputElement)
                {
                    Keyboard.Focus(dependencyObject as IInputElement);
                    return;
                }

                dependencyObject = parentObject;

            }

        }

        private Boolean EnterElement(FrameworkElement frameworkElement)
        {

            if (frameworkElement.Focusable)
            {
                Keyboard.Focus(frameworkElement);
                return true;

            }

            int childElementCount = VisualTreeHelper.GetChildrenCount(frameworkElement);

            for (int childIndex = 0; childIndex < childElementCount; childIndex++)
            {
                DependencyObject childObject = VisualTreeHelper.GetChild(frameworkElement, childIndex);
                if (childObject is IInputElement)
                {
                    IInputElement iInputElement = childObject as IInputElement;
                    if (iInputElement.Focusable)
                    {
                        Keyboard.Focus(iInputElement);
                        return true;
                    }
                }
            }

            for (int childIndex = 0; childIndex < childElementCount; childIndex++)
            {
                DependencyObject childObject = VisualTreeHelper.GetChild(frameworkElement, childIndex);
                if (childObject is FrameworkElement)
                    if (EnterElement(childObject as FrameworkElement))
                        return true;
            }

            return false;

        }

        /// <summary>
        /// Move the input focus up one line.
        /// </summary>
        /// <param name="frameworkElement">The current focus element.</param>
        /// <param name="ignoreFocusScope">false to restrict focus movement to the focus scope.</param>
        private void MoveFocusByColumn(Int32 direction)
        {

            // In a virtualized display, there's no guarantee that the element with the focus is actually part of the display.  It
            // is possible to select an element and then scroll it out of view.  To complicate matters, it's also possible that the
            // element with the focus has been sorted or filtered since it was last selected.  Moving the focus involves getting 
            // the focus element from the FocusScope.  Note that it may not be connected to a parent at this point if the element
            // isn't visible.
            if (this.focusedCell != null)
            {

                // This wll attempt to find the next virtual cell above the current focus cell.
                ReportCell targetCell = this.FindClosestHorizontalCell(this.focusedCell, 0.0, direction);
                if (targetCell != null)
                {

                    // If the target cell is above of the viewport, then the contents of the report are scrolled to make the target
                    // visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
                    // framework element is required to set the keyboard focus.
                    if (targetCell.Rect.Right <= this.bodyCanvas.Viewport.Left)
                    {
                        SetHorizontalOffset(targetCell.Rect.Left - this.Split.Width);
                        this.UpdateLayout();
                    }

                    // If the target cell is below the viewport, then the contents of the report are scrolled to make the target
                    // visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
                    // framework element is required to set the keyboard focus.
                    if (targetCell.Rect.Left >= this.bodyCanvas.Viewport.Right)
                    {
                        SetHorizontalOffset(targetCell.Rect.Right - this.bodyCanvas.Viewport.Width - this.Split.Width);
                        this.UpdateLayout();
                    }

                    // Now that the target cell is visible it can receive the focus.
                    FrameworkElement targetElement = this.FindElement(targetCell.Content);
                    if (targetElement != null)
                        Keyboard.Focus(targetElement);

                }

            }

        }

        /// <summary>
        /// Move the input focus up one line.
        /// </summary>
        /// <param name="frameworkElement">The current focus element.</param>
        /// <param name="ignoreFocusScope">false to restrict focus movement to the focus scope.</param>
        private void MoveFocusByRow(Int32 direction)
        {

            // In a virtualized display, there's no guarantee that the element with the focus is actually part of the display.  It
            // is possible to select an element and then scroll it out of view.  To complicate matters, it's also possible that the
            // element with the focus has been sorted or filtered since it was last selected.  Moving the focus involves getting 
            // the focus element from the FocusScope.  Note that it may not be connected to a parent at this point if the element
            // isn't visible.
            if (this.focusedCell != null)
            {

                // This wll attempt to find the next virtual cell above the current focus cell.
                ReportCell targetCell = FindClosestVerticalCell(this.focusedCell, 0.0, direction);
                if (targetCell != null)
                {

                    // If the target cell is above of the viewport, then the contents of the report are scrolled to make the target
                    // visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
                    // framework element is required to set the keyboard focus.
                    if (targetCell.Rect.Bottom <= this.bodyCanvas.Viewport.Top)
                    {
                        SetVerticalOffset(targetCell.Rect.Top - this.Split.Height);
                        this.UpdateLayout();
                    }

                    // If the target cell is below the viewport, then the contents of the report are scrolled to make the target
                    // visible.  Note that the virtual elements are instantiated with a forced update of the layout.  An actualized
                    // framework element is required to set the keyboard focus.
                    if (targetCell.Rect.Top >= this.bodyCanvas.Viewport.Bottom)
                    {
                        SetVerticalOffset(targetCell.Rect.Bottom - this.bodyCanvas.Viewport.Height - this.Split.Height);
                        this.UpdateLayout();
                    }

                    // Now that the target cell is visible it can receive the focus.
                    FrameworkElement targetElement = this.FindElement(targetCell.Content);
                    if (targetElement != null)
                        Keyboard.Focus(targetElement);

                }

            }

        }

        /// <summary>
        /// Move the input focus up one line.
        /// </summary>
        /// <param name="frameworkElement">The current focus element.</param>
        /// <param name="ignoreFocusScope">false to restrict focus movement to the focus scope.</param>
        private void MoveFocusByPage(Int32 direction)
        {

            // In a virtualized display, there's no guarantee that the element with the focus is actually part of the display.  It
            // is possible to select an element and then scroll it out of view.  To complicate matters, it's also possible that the
            // element with the focus has been sorted or filtered since it was last selected.  Moving the focus involves getting 
            // the focus element from the FocusScope.  Note that it may not be connected to a parent at this point if the element
            // isn't visible.
            if (this.focusedCell != null)
            {

                Double cellOffset = this.focusedCell.Rect.Top - this.bodyCanvas.Viewport.Top;

                // This wll attempt to find the next virtual cell above the current focus cell.
                Double pageOffset = this.bodyCanvas.Viewport.Height * Convert.ToDouble(direction);
                ReportCell targetCell = FindClosestVerticalCell(this.focusedCell, pageOffset, direction);
                if (targetCell != null)
                {

                    SetVerticalOffset(targetCell.Rect.Top - this.Split.Height - cellOffset);
                    this.UpdateLayout();

                    // Now that the target cell is visible it can receive the focus.
                    FrameworkElement targetElement = this.FindElement(targetCell.Content);
                    if (targetElement != null)
                        Keyboard.Focus(targetElement);

                }

            }

        }

        /// <summary>
        /// Finds the cell that is closest to the source cell in the same column.
        /// </summary>
        /// <param name="sourceCell">The reference point for the search operation.</param>
        /// <param name="offset">The offset to the target location.</param>
        /// <param name="direction">-1 to search up, 1 to search down.</param>
        /// <returns>The ReportCell that is closest to the referenced cell plus the offset.</returns>
        public ReportCell FindClosestHorizontalCell(ReportCell sourceCell, Double offset, int direction)
        {
			if (sourceCell == null)
				return null;

            // This will crawl through all the cells in the same column as the source cell looking for the on that is closest to
            // the given cell.  The offset is added to the starting location to handle movements outside a single row.
            ReportCell closestCell = null;
            Double closestDistance = Double.MaxValue;
            foreach (ReportColumn reportColumn in this.reportColumnCollection)
            {
                ReportCell reportCell = sourceCell.ReportRow[reportColumn];
                if (reportCell != sourceCell)
                {
					Double distance = reportCell.Rect.Left - sourceCell.Rect.Left - offset;
					if (Math.Sign(distance) == direction && Math.Abs(distance) < Math.Abs(closestDistance))
					{
						closestCell = reportCell;
						closestDistance = distance;
					}
                }

            }

            // This is the cell that is closest to the source cell (with the given offset), or null if no cells could be
            // found that matched the criteria.
            return closestCell;

        }

        /// <summary>
        /// Finds the cell that is closest to the source cell in the same column.
        /// </summary>
        /// <param name="sourceCell">The reference point for the search operation.</param>
        /// <param name="offset">The offset to the target location.</param>
        /// <param name="direction">-1 to search up, 1 to search down.</param>
        /// <returns>The ReportCell that is closest to the referenced cell plus the offset.</returns>
        public ReportCell FindClosestVerticalCell(ReportCell sourceCell, Double offset, int direction)
        {

			if (sourceCell == null)
				return null;

            // This will crawl through all the cells in the same column as the source cell looking for the on that is closest to
            // the given cell.  The offset is added to the starting location to handle movements outside a single row.
            ReportCell closestCell = null;
            Double closestDistance = Double.MaxValue;
            foreach (ReportRow reportRow in this.reportRowCollection)
            {
                ReportCell reportCell = reportRow[sourceCell.ReportColumn];
                if (reportCell != sourceCell)
                {
                    Double distance = reportCell.Rect.Top - sourceCell.Rect.Top - offset;
                    if (Math.Sign(distance) == direction && Math.Abs(distance) < Math.Abs(closestDistance))
                    {
                        closestCell = reportCell;
                        closestDistance = distance;
                    }
                }
            }

            // This is the cell that is closest to the source cell (with the given offset), or null if no cells could be
            // found that matched the criteria.
            return closestCell;

        }

        /// <summary>
        /// Gets or sets the time it take to perform animated operations.
        /// </summary>
        public Duration Duration
        {
            get { return this.duration; }
            set { this.duration = value; }
        }

		internal DynamicReport DynamicReport
        {
			get { return this.Parent as DynamicReport; }
        }

        /// <summary>
        /// Gets the collection of objects that define the available fields in the report.
        /// </summary>
        public ReportFieldCollection Fields
        {
            get { return this.reportFieldCollection; }
        }

        /// <summary>
        /// Gets a collection of objects that define the columns in the report.
        /// </summary>
        public ReportColumnCollection Columns
        {
            get { return this.reportColumnCollection; }
        }


        /// <summary>
        /// Gets a collection of objects that define the rows in the report.
        /// </summary>
        public ReportRowCollection Rows
        {
            get { return this.reportRowCollection; }
        }

        /// <summary>
        /// Gets a collection of templates that define the hierarchical organization of the report.
        /// </summary>
        public RowTemplateCollection RowTemplates
        {
            get { return this.rowTemplateCollection; }
        }

        /// <summary>
        /// Gets or sets the magnification factor of the report.
        /// </summary>
        public Double Scale
        {
            get { return (Double)this.GetValue(ReportGrid.ScaleProperty); }
            set { this.SetValue(ReportGrid.ScaleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the location of the headers.
        /// </summary>
        public Size Split
        {
            get { return (Size)this.GetValue(ReportGrid.SplitProperty); }
            set { this.SetValue(ReportGrid.SplitProperty, value); }
        }

        public event RoutedEventHandler ChildFocus
        {
            add { AddHandler(ChildFocusEvent, value); }
            remove { RemoveHandler(ChildFocusEvent, value); }
        }

		/// <summary>
		/// Event raised when grid is about to show the tooltip
		/// subscriber must set the Content of ReportGridtToolTipEventArgs.ToolTip
		/// to display a tooltip. 
		/// </summary>
		public event RoutedEventHandler ShowToolTip
		{
			add { AddHandler(ShowToolTipEvent, value); }
			remove { RemoveHandler(ShowToolTipEvent, value); }
		}

        /// <summary>
        /// Handles the deletion of one or more columns from the report.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="columnsChangedEventArgs">The event arguments.</param>
        internal void RemoveColumns(List<ReportColumn> reportColumnList)
        {

            // This will collect all the cells that are obsolete and remove them from each row in the report.
            foreach (ReportRow reportRow in this.reportRowCollection)
                foreach (ReportColumn reportColumn in reportColumnList)
                    reportRow.Remove(reportColumn);

            // This will remove the user interface elements associated with the obsolete columns.
            foreach (ReportCanvas reportCanvas in this.reportCanvases)
                foreach (ReportColumn reportColumn in reportColumnList)
                    reportCanvas.Remove(reportColumn);

            // Invalidating the report will cause it to redraw without the obsolete columns.
            this.InvalidateMeasure();

        }

        /// <summary>
        /// Handles the addition of one or more column definitions to the report.
        /// </summary>
        public void AddReportColumn()
        {

            // This similar to the process that created the entire report but works on the limited set of columns that are not already part of the data
            // structure.  However, this process is made more simple by the assumption that the rows already exist and simply need a column added to them.  The
            // template for the root of the report is the starting point for the recursion into the hierarchy of the document.  This method will dig into the
            // content and
			if (this.DynamicReport != null && this.DynamicReport.Content != null && this.rowTemplateCollection.Count > 0)
            {
                RowTemplate rowTemplate = this.rowTemplateCollection[0] as RowTemplate;
				this.reportRowCollection.RecursivelyAddColumns(this.DynamicReport.Content, rowTemplate);
            }

            // Each of the ReportCanvases will need to evaluate the arrangement of child windows when the columns have been added.
            if (this.duration.TimeSpan != TimeSpan.Zero)
                this.InvalidateMeasure();

        }

        void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {

            FrameworkElement frameworkElement = e.TargetObject as FrameworkElement;
            if (frameworkElement != null)
            {
                ReportCanvas parentCanvas = frameworkElement.Parent as ReportCanvas;
                if (parentCanvas != null)
                {

                    Rect controlRect = new Rect(Canvas.GetLeft(frameworkElement), Canvas.GetTop(frameworkElement), frameworkElement.ActualWidth, frameworkElement.ActualHeight);
                    Rect canvasRect = parentCanvas.Viewport;
                    if (!canvasRect.Contains(controlRect))
                    {
                        Rect unionRect = Rect.Union(canvasRect, controlRect);
                        if (unionRect.Left < canvasRect.Left)
                            SetHorizontalOffset(this.offset.X + (unionRect.Left - canvasRect.Left));
                        if (unionRect.Right > canvasRect.Right)
                            SetHorizontalOffset(this.offset.X + (unionRect.Right - canvasRect.Right));
                        if (unionRect.Top < canvasRect.Top)
                            SetVerticalOffset(this.offset.Y + (unionRect.Top - canvasRect.Top));
                        if (unionRect.Bottom > canvasRect.Bottom)
                            SetVerticalOffset(this.offset.Y + (unionRect.Bottom - canvasRect.Bottom));

                    }
                }

            }

        }

        /// <summary>
        /// Handle focus within the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChildFocusHandler(object sender, RoutedEventArgs e)
        {
            if (this.focusedCell != null && !IsChildInFocus)
            {
                FrameworkElement frameworkElement = FocusManager.GetFocusedElement(this) as FrameworkElement;
                if (frameworkElement != null && this.EnterElement(frameworkElement))
                {
                    frameworkElement.LostFocus += new RoutedEventHandler(OnElementLostFocus);
                    IsChildInFocus = true;
                }
            }
        }

        /// <summary>
        /// Brings all of the user interface elements associated with the columns to the front of the Z order.
        /// </summary>
        /// <param name="sender">The object that originated this event.</param>
        /// <param name="columnsChangedEventArgs">The event arguments.</param>
        private void ColumnCollectionBringToFront(object sender, ColumnDefinitionsEventArgs reportColumnsEventArgs)
        {

            // This tells the canvases to bring all cells in the specified columns to the front of the Z order.  It is used
            // when moving columns around so that the column appears to be a single unit.
            this.BringToFront(reportColumnsEventArgs.Columns);

        }

        /// <summary>
        /// Handles the comletion of the drag-and-drop operation to resize the horizontal split between the canvases.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The event arguments.</param>
        private void HorizontalSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {

            // If the split is bigger than the document then the split screen is disabled.  Otherwise the split is 'snapped' to 
            // the closest row if one is near.
            Double targetSplit = this.Split.Height + e.VerticalChange;
            ReportRow reportRow = this.Rows.FindRowAt(targetSplit);
            if (reportRow == null)
                targetSplit = 0.0;
            else
            {
                Double leftRange = targetSplit - this.splitterLength;
                Double rightRange = targetSplit + this.splitterLength;
                if (leftRange <= reportRow.Top && reportRow.Top <= rightRange)
                    targetSplit = reportRow.Top;
                if (leftRange <= reportRow.Bottom && reportRow.Bottom <= rightRange)
                    targetSplit = reportRow.Bottom;
            }

            // This will resize the canvases to the new position of the horizontal split.
            ReportGrid.SetSplit.Execute(new Size(this.Split.Width, targetSplit), this);

        }

        /// <summary>
        /// Handles the comletion of the drag-and-drop operation to resize the vertical split between the canvases.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The event arguments.</param>
        private void VerticalSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {

            // If the split is bigger than the document then the split screen is disabled.  Otherwise the split is 'snapped' to the
            // closest column if one is near.
            Double targetSplit = this.Split.Width + e.HorizontalChange;
            ReportColumn reportColumn = this.Columns.FindColumnAt(targetSplit);
            if (reportColumn == null)
                targetSplit = 0.0;
            else
            {
                Double topRange = targetSplit - this.splitterLength;
                Double bottomRange = targetSplit + this.splitterLength;
                if (topRange <= reportColumn.Left && reportColumn.Left <= bottomRange)
                    targetSplit = reportColumn.Left;
                if (topRange <= reportColumn.Right && reportColumn.Right <= bottomRange)
                    targetSplit = reportColumn.Right;
            }

            // This will resize the canvases to the new position of the vertical split.
            ReportGrid.SetSplit.Execute(new Size(targetSplit, this.Split.Height), this);

        }

        /// <summary>
        /// Handles a change to the size of the viewing area.
        /// </summary>
        /// <param name="sizeInfo">Information about the change made to the viewing area of the report.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {

            // Changing the size of the rendered window will change the amount of the report that scrolls with respect to the size
            // of the virtual document.
            SetViewport(sizeInfo.NewSize);

            // Allow the base class to handle the rest of the event.
            base.OnRenderSizeChanged(sizeInfo);

        }

        /// <summary>
        /// Adjusts the scrolling properties to account for a change in the viewport metrics.
        /// </summary>
        /// <param name="size">The new size of the report.</param>
        private void SetViewport(Size size)
        {

            // This will calculate the size of the portion of the report that is not fixed in the headers.  That is, the scrollable
            // part of the report.  If this 'viewport' into the report has changed since the last time it was measured, then all
            // the parameters associated with the scrolling are updated.
            Size viewport = new Size(size.Width < this.Split.Width ? 0.0 : size.Width - this.Split.Width,
                size.Height < this.Split.Height ? 0.0 : size.Height - this.Split.Height);
            if (viewport != this.viewport)
            {
                this.viewport = viewport;
                this.scrollOwner.InvalidateScrollInfo();
            }

        }

        /// <summary>
        /// Sets the size of the scrollable area of the report.
        /// </summary>
        /// <param name="totalExtent">The total virtual size of the report.</param>
        internal void UpdateExtent()
        {

            // This is the size of the entire virtual document.  The "Extent" of this control is that portion of the total size which is visible in the
            // scrolling pane.  Since the control has optional header areas for rows and columns, this area must be removed from the total size when
            // calculating the "Extent".
            Size size = new Size(this.reportColumnCollection.Width, this.reportRowCollection.Height);

            // The "Extent" in the ReportGrid is the total amount of space for the scrollable portion of the report.  Neither the row nor column header can
            // scroll, so these areas are removed from the value.
            this.extent = new Size(
                size.Width < this.Split.Width ? 0.0 : size.Width - this.Split.Width,
                size.Height < this.Split.Height ? 0.0 : size.Height - this.Split.Height);

            // This will update the scrollbars with the new virtual size of the scrolling area of the document.
            if (this.scrollOwner != null)
                this.scrollOwner.InvalidateScrollInfo();

        }

        private static void OnScaleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the target of the propery change event from the generic arguments.
            ReportGrid reportGrid = (ReportGrid)dependencyObject;
            Double scaleFactor = (Double)dependencyPropertyChangedEventArgs.NewValue;

            // This will inhibit the screen from attempting to redraw when the split hasn't changed.
            if (reportGrid.scaleFactor != scaleFactor)
            {

                // The old scale factor is broadcast as part of the event notification.
                Double oldScaleFactor = reportGrid.scaleFactor;

                // The background threads are not allowed to access this member directly.
                reportGrid.scaleFactor = scaleFactor;
                reportGrid.scaleTransform.ScaleX = reportGrid.scaleFactor;
                reportGrid.scaleTransform.ScaleY = reportGrid.scaleFactor;

                // Changing the scale will change the amount of the report that can be displayed in the viewport.
                reportGrid.SetViewport(new Size(reportGrid.ActualWidth, reportGrid.ActualHeight));

            }

        }

        /// <summary>
        /// Sets the location of the screen split.
        /// </summary>
        private static void OnSplitChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {

            // Extract the target and values for the property change operation.
            ReportGrid reportGrid = (ReportGrid)dependencyObject;
            Size splitSize = (Size)dependencyPropertyChangedEventArgs.NewValue;

            // The report is split into four different quadrants using the rows and columns of the grid control.  As the grid
            // measurements change, so will the size of the quadrants.
            ((Grid)reportGrid).ColumnDefinitions[0].Width = new GridLength(splitSize.Width);
            ((Grid)reportGrid).RowDefinitions[0].Height = new GridLength(splitSize.Height);

            // The offsets for each of the scrolling canvases must be recalculated when the split has changed because the split
            // is one of the factors used.
            reportGrid.SetOffset(reportGrid.offset);

            // The scrollable area of the report will also change if the split size is changed.
            reportGrid.UpdateExtent();

            // Changing the split size will change the amount of the report that can be displayed in the viewport.
            reportGrid.SetViewport(new Size(reportGrid.ActualWidth, reportGrid.ActualHeight));

        }

        private static void OnChildFocusChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ReportGrid reportGrid = (ReportGrid)dependencyObject;
            ReportCell cell = reportGrid.focusedCell;
        }

        /// <summary>
        /// Sets extent for the scroll bars.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The event arguments.</param>
        internal void ExtentChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {

            // Call an internal method to set the extent of the scrollable screen given the extent of the entire virtual document.
            UpdateExtent();

        }

        /// <summary>
        /// Calculates the offsets for the report and the canvases that display the split screens.
        /// </summary>
        /// <param name="offset"></param>
        private void SetOffset(Point offset)
        {

            // This will scroll the entire virtual document so that the desired portion will appear in the viewports.
            if (this.offset != offset)
                this.offset = offset;

            // The document is scrolled vertically and horizontally by adjusting the transform offsets for each of the panes in the
            // view.  The report is broken up into four different 'views' representing the fixed column header, the fixed row
            // header and the scrolling content of the report.  The column headers will appear to scroll horizontally with the
            // content and the row headers will appear to scroll vertically with the content.
            Point splitOffset = new Point(-offset.X - this.Split.Width, -offset.Y - this.Split.Height);
            this.reportCanvases[0].Offset = new Point(0.0, 0.0);
            this.reportCanvases[1].Offset = new Point(splitOffset.X, 0.0);
            this.reportCanvases[2].Offset = new Point(0.0, splitOffset.Y);
            this.reportCanvases[3].Offset = splitOffset;

        }

        /// <summary>
        /// Handles a change to the properties that can change the position of the document inside the viewport.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="ScrollChangedEventArgs">The event arguments.</param>
        internal void ScrollChanged(object sender, ScrollChangedEventArgs ScrollChangedEventArgs)
        {

            // The offsets are adjusted here to insure that the document fills up the available viewport.  Shrinking the document
            // or resizing the window can leave an open space which should be filled with the report.  This will adjust the offsets
            // to insure the document allways fills up the available viewport.
            SetOffset(new Point(this.offset.X + this.viewport.Width <= this.extent.Width ? this.offset.X : this.extent.Width -
                this.viewport.Width < 0.0 ? 0.0 : this.extent.Width - this.viewport.Width, this.offset.Y +
                this.viewport.Height <= this.extent.Height ? this.offset.Y : this.extent.Height - this.viewport.Height < 0.0 ?
                0.0 : this.extent.Height - this.viewport.Height));

        }

        /// <summary>
        /// Handles an instruction from a child canvas to display the visual queues for resizing a column.
        /// </summary>
        /// <param name="sender">The child canvas that originated the message.</param>
        /// <param name="resizeMouseMoveEventArgs">The arguments that describe how to visualize the resizing operation.</param>
        private void columnHeaderCanvas_ResizeMouseMove(object sender, ResizeColumnEventArgs resizeMouseMoveEventArgs)
        {

            // This window provides the lines that indicate the boundaries of the column when the user is resizing.  While the 
            // child canvas that hosts the column headers can draw outside of its boundaries, it doesn't have dimensions of parent
            // window.  In a sense, the parent window provides a service here for the child by drawing the boundaries across the
            // two windows (the header and the main body of the report).  The final act of drawing the resizing indicators is to
            // remove them from the screen.
            if (resizeMouseMoveEventArgs.IsFinal)
            {
                this.Children.Remove(this.resizeColumnStart);
                this.Children.Remove(this.resizeColumnEnd);
            }
            else
            {

                // The first act of drawing the resizing indicators is to add the indicators to the screen and set the fixed
                // position of the left indicator.  Note that, to be consistent with Excel, the column width indicator isn't drawn
                // in the header.
                if (!this.Children.Contains(this.resizeColumnStart))
                {
                    this.resizeColumnStart.X1 = resizeMouseMoveEventArgs.ColumnDefinition.Left - this.offset.X -
                        this.resizeColumnStart.StrokeThickness / 2.0;
                    this.resizeColumnStart.Y1 = this.Split.Height;
                    this.resizeColumnStart.X2 = resizeMouseMoveEventArgs.ColumnDefinition.Left - this.offset.X -
                        this.resizeColumnStart.StrokeThickness / 2.0;
                    this.resizeColumnStart.Y2 = this.ActualHeight;
                    this.Children.Add(this.resizeColumnStart);
                }

                // As the user drags the column edge, this indicator moves back and fourth to provide feedback about the final
                // dimension of the column.
                this.resizeColumnEnd.X1 = resizeMouseMoveEventArgs.ColumnDefinition.Left - this.offset.X +
                    resizeMouseMoveEventArgs.Width - this.resizeColumnEnd.StrokeThickness / 2.0;
                this.resizeColumnEnd.Y1 = this.Split.Height;
                this.resizeColumnEnd.X2 = resizeMouseMoveEventArgs.ColumnDefinition.Left - this.offset.X +
                    resizeMouseMoveEventArgs.Width - this.resizeColumnEnd.StrokeThickness / 2.0;
                this.resizeColumnEnd.Y2 = this.ActualHeight;

                // The end indicator is added when the operations starts and removed when it ends.
                if (!this.Children.Contains(this.resizeColumnEnd))
                    this.Children.Add(this.resizeColumnEnd);

            }

        }

        /// <summary>
        /// Invalidates the arrange state (layout) for the element.
        /// </summary>
        internal new void InvalidateMeasure()
        {

            // This method is re-interpreted to arranging the elements within each of the four quadrants.  Rearranging the actual
            // layout of the grid is accomplished by setting the Split size.
            foreach (ReportCanvas reportCanvas in this.reportCanvases)
                reportCanvas.InvalidateMeasure();

        }

        internal bool IsColumnHeaderFrozen
        {
            get { return this.columnHeaderCanvas.IsHeaderFrozen; }
            set { this.columnHeaderCanvas.IsHeaderFrozen = value; }
        }

        internal bool IsRowHeaderFrozen
        {
            get { return this.rowHeaderCanvas.IsHeaderFrozen; }
            set { this.rowHeaderCanvas.IsHeaderFrozen = value; }
        }

        public List<List<ReportRow>> SelectedRowBlocks
        {
            //get { return this.rowHeaderCanvas.SelectedRowBlocks; }
            get { return this.selectedRanges; }
            
        }

        /// <summary>
        /// Get the Select rows using the row header canvas.
        /// </summary>
        public List<List<ReportRow>> SelectedRowHeaderBlocks
        {
            get { return this.rowHeaderCanvas.SelectedRowBlocks; }
        }

        /// <summary>
        /// Freezes or unfreezes the split screen panels.
        /// </summary>
        /// <param name="isFrozen">true to freeze the panels in place, false to allow the user to change the position.</param>
        internal void FreezePanes(bool isLayoutFrozen, UndoAction undoAction)
        {

            // This prevents redundant changes to the property.
            if (this.isLayoutFrozen != isLayoutFrozen)
            {

                // The grid splitting panes are placed on the grid between the panels.  For example: Fixed Document Header, Vertical 
                // Grid Splitter, Column Header.  Hiding the GridSplitter column and row effectively removes them from the document.
                if (this.isLayoutFrozen = isLayoutFrozen)
                {
                    base.RowDefinitions[1].Height = new GridLength(0.0);
                    base.ColumnDefinitions[1].Width = new GridLength(0.0);
                    this.Children.Remove(this.horizontalGridSplitter);
                    this.Children.Remove(this.verticalGridSplitter);
                }
                else
                {
                    base.RowDefinitions[1].Height = GridLength.Auto;
                    base.ColumnDefinitions[1].Width = GridLength.Auto;
                    this.Children.Add(this.horizontalGridSplitter);
                    this.Children.Add(this.verticalGridSplitter);
                }

            }

        }

        /// <summary>
        /// Resets the canvases of the report.
        /// </summary>
        internal void Clear()
        {

            // This will restore the Report control to the initial values.
            this.focusedCell = null;
            this.undoManager.Clear();
            this.Resources.Clear();
            this.reportRowCollection.Clear();
            this.reportColumnCollection.Clear();
            this.reportFieldCollection.Clear();
            this.rowTemplateCollection.Clear();

            // The report is made up of four screens that show different views of the same underlying report.  Each must be 
            // cleared.
            foreach (ReportCanvas reportCanvas in this.reportCanvases)
                reportCanvas.Clear();

        }

        /// <summary>
        /// Brings a column to the front of the Z order.
        /// </summary>
        /// <param name="reportColumn">The column to be brought to the front of the Z order.</param>
        internal void BringToFront(List<ReportColumn> reportColumns)
        {

            // This is mainly used for column animation functions.  Moving a column to the front of the order makes it very obvious
            // which column is being moved.
            foreach (ReportCanvas reportCanvas in this.reportCanvases)
                reportCanvas.BringToFront(reportColumns);

        }

        /// <summary>
        /// Handles the opening of the context menu.
        /// </summary>
        /// <param name="e">Menu opening parameters</param>
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {

            // This event handler will remember the quandrant over which the menu was opened.  This is done to be able to direct
            // the routed commands from the menu commands to the propert quadrant.
            //this.contextMenuCanvas = null;
            //foreach (ReportCanvas reportCanvas in this.reportCanvases)
            //    if (reportCanvas.IsMouseOver)
            //    {
            //        this.contextMenuCanvas = reportCanvas;
            //        break;
            //    }

            // Allow the base class to handle the rest of the event.
            base.OnContextMenuOpening(e);

        }


        /// <summary>
        /// Handles the mouse button being pressed.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

			try
			{
				Point mouseDownLocation = e.GetPosition(this);

				// TODO: Figure out 'focusCell' work as there is a problem when selecting a folder node focus to a cell in the grid does not happen.
				// For now this is commented out as it prevents cell editing.
				// Set the focus to the grid.
				//FocusManager.SetFocusedElement(this, null);

				//Since the user is selecting a cell, it will invalidate any row selection that there may be.
				//this.SelectedRowBlocks.Clear();

				IInputElement iInputElement = this.InputHitTest(mouseDownLocation);
				DependencyObject dependencyObject = iInputElement as DependencyObject;
				while (dependencyObject != null)
				{
					ReportCell reportCell = DynamicReport.GetCell(dependencyObject);

					if (reportCell != null)
					{
						SetFocusedCell(reportCell);
						// Select All and highlight all rows and and headers.
						if ((reportCell.ActualRect.Top == 0.0) && (reportCell.ActualRect.Left == 0.0) && (reportCell.ReportColumn.ColumnId == "SelectRowColumn"))
						{
							SelectAllRows();
							//e.Handled = true;
							break;
						}
						// Select row and highlight column header row.
						if ((reportCell.ActualRect.Left == 0.0) && (reportCell.ReportColumn.ColumnId == "SelectRowColumn") && (e.RightButton != MouseButtonState.Pressed))
						{
							SelectColumnHeadersOfCurrentCellRow(reportCell);
						}
						// Select cell and highlight cell's row header and column header.
						else
						{
							SelectHeadersOfCurrentCell(reportCell);
						}
					}
					if (dependencyObject is Visual)
					{
						// Search up the tree until we have a valid dependency object.
						dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
					}
					else
					{
						//e.Handled = true;
						break;
					}
				}
			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}
        }


		/// <summary>
		/// Highlight the column headers for the row of the passed in report cell.
		/// </summary>
		/// <param name="reportCell"> Cell this is the focus of the highlight of the headers.</param>
		public void SelectColumnHeadersOfCurrentCellRow(ReportCell reportCell)
		{

			ReportColumn reportColumn = reportCell.ReportColumn;
			ReportRow reportRow = reportCell.ReportRow;



			// Get the column Header Row for the Grid.
			ReportRow reportHeaderRow = this.reportRowCollection.FindRowAt(0);

			foreach (ReportColumn rc in this.Columns)
			{

				ReportCell reportColumnHeaderCell = null;
				bool ok = reportHeaderRow.TryGetValue(rc, out reportColumnHeaderCell);
				reportColumnHeaderCell.IsSelected = true;

			}

			// Select the row header also as we are attempting to highlight the row then like Microsoft Excel highlight the row header too.
			reportHeaderRow.IsSelected = true;
			
			// This will redraw the newly selected cells with the highlighting.
			this.InvalidateMeasure();

		}

		
		/// <summary>
		/// Highlight the column and row headers for the passed in report cell.
		/// </summary>
		/// <param name="reportCell"> Cell this is the focus of the highlight of the headers.</param>
		public void SelectHeadersOfCurrentCell(ReportCell reportCell)
		{
			ReportColumn reportColumn = reportCell.ReportColumn;
			ReportRow reportRow = reportCell.ReportRow;
			// Get the column Header Row for the Grid.
			ReportRow reportHeaderRow = this.reportRowCollection.FindRowAt(0);

			foreach (ReportColumn rc in this.Columns)
			{

				// Locate the Column of the selected cell.
				if (reportColumn.ColumnId == rc.ColumnId)
				{

					ReportCell reportColumnHeaderCell = null;
					bool ok = reportHeaderRow.TryGetValue(rc, out reportColumnHeaderCell);

					if (ok)
						reportColumnHeaderCell.IsSelected = true;
				}

				// Locate the Select row "Dummy Column" of the Row Header of the selected cell.
				if (rc.ColumnId == "SelectRowColumn")
				{
					ReportCell reportRowHeaderCell = null;
					bool ok = reportRow.TryGetValue(rc, out reportRowHeaderCell);
					if (ok)
						reportRowHeaderCell.IsSelected = true;
				}

			}

			// This will redraw the newly selected cells with the highlighting.
			this.InvalidateMeasure();

		}


        /// <summary>
        /// Select and Highlight all the rows in the grid.
        /// </summary>
        public void SelectAllRows()
        {

			List<ReportRow> selection = new List<ReportRow>();

			this.selectedRanges.Clear();

            foreach (ReportRow reportRow in this.Rows)
            {
                foreach (ReportCell cell in reportRow.Cells)
                    cell.IsSelected = true;

                selection.Add(reportRow);

            }

			this.selectedRanges.Add(selection);

            // Set the Selected all rows on the row header canvas.
            this.rowHeaderCanvas.SetSelectedRowBlocks(this.selectedRanges);

            // This will redraw the newly selected cells with the highlighting.
            this.InvalidateVisual();
        }

        /// <summary>
        /// Recursively applies names of a DependencyObject and its children to a NameScope.
        /// </summary>
        /// <param name="dependencyObject">A part of a visual tree that will be examined.</param>
        /// <param name="nameScope">A NameScope to which any names will be applied.</param>
        internal static void ApplyNames(DependencyObject dependencyObject, NameScope nameScope)
        {

            // If the current object has a FrameworkElement name, then it gets applied to the NameScope so it can be found during
            // binding and other XAML extension operations.
            string name = dependencyObject.GetValue(FrameworkElement.NameProperty) as string;
            if (name != string.Empty)
                nameScope.RegisterName(name, dependencyObject);

            // This will recurse into the children of the object looking for more names.
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(dependencyObject); index++)
            {
                DependencyObject childObject = VisualTreeHelper.GetChild(dependencyObject, index);
                if (NameScope.GetNameScope(childObject) == null)
                    ApplyNames(childObject, nameScope);
            }

        }

        /// <summary>
        /// Sets the scale (magnification) factor for the report.
        /// </summary>
        /// <param name="sender">The object that originated the routed event.</param>
        /// <param name="routedEventArgs">The routed event arguments.</param>
        public void OnSetSplit(object sender, RoutedEventArgs routedEventArgs)
        {

            // The idea is to extract the parameters from the event data and set the split location based on the values provided.
            ExecutedRoutedEventArgs executedRoutedEventArgs = routedEventArgs as ExecutedRoutedEventArgs;

            // The simplest form for this command simply passes in a value.  The command parameter is burpped back up to the
            // property which then issues a command back here with the proper Undo information encoded in the command.  The idea is
            // to simplify the calling parameters for an externally invoked command while still maintaining a consistent format
            // internally for handling the undo/redo operations.
            if (executedRoutedEventArgs.Parameter is Size)
                SetValue(ReportGrid.SplitProperty, (Size)executedRoutedEventArgs.Parameter);

            // This parameter format handles the undo/redo logic.
            if (executedRoutedEventArgs.Parameter is UndoObject)
            {

                // Extract the parameter and do the actual work involved with changing the magnification factor.
                UndoObject undoObject = executedRoutedEventArgs.Parameter as UndoObject;
                //				this.OnSetSplit((Size)undoObject.NewValue, undoObject.UndoAction);

                // Once the command is finished, this will broadcast the event.  The primary consumer is the Undo logic which will
                // save the event data on the Undo or Redo stack and can replay it when needed.
				RaiseEvent(new UndoPropertyChangedEventArgs(DynamicReport.UndoPropertyChangedEvent, undoObject.UndoAction,
                    ReportGrid.SplitProperty, undoObject.OldValue, undoObject.NewValue));

                // The property must be set when this operation is called from the Undo or Redo methods.  However, setting the
                // value will lead the logic right back here.  This will inhibit the command processing while the Undone or Redone
                // value is restored.
                if (undoObject.UndoAction == UndoAction.Undo || undoObject.UndoAction == UndoAction.Redo)
                {
                    this.CommandBindings.Remove(this.commandBingingMap[ReportGrid.SetSplit]);
                    SetValue(ReportGrid.SplitProperty, undoObject.NewValue);
                    this.CommandBindings.Add(this.commandBingingMap[ReportGrid.SetSplit]);
                }

            }

        }

        /// <summary>
        /// Sets the scale (magnification) factor for the report.
        /// </summary>
        /// <param name="sender">The object that originated the routed event.</param>
        /// <param name="routedEventArgs">The routed event arguments.</param>
        public void OnSetScale(object sender, RoutedEventArgs routedEventArgs)
        {

            // The idea is to extract the parameters from the event data and set the magnification factor based on the values
            // provided.
            ExecutedRoutedEventArgs executedRoutedEventArgs = routedEventArgs as ExecutedRoutedEventArgs;

            // The simplest form for this command simply passes in a value.  The command parameter is burpped back up to the
            // property which then issues a command back here with the proper Undo information encoded in the command.  The idea is
            // to simplify the calling parameters for an externally invoked command while still maintaining a consistent format
            // internally for handling the undo/redo operations.
            if (executedRoutedEventArgs.Parameter is Double)
                SetValue(ReportGrid.ScaleProperty, (Double)executedRoutedEventArgs.Parameter);

            // This parameter format handles the undo/redo logic.
            if (executedRoutedEventArgs.Parameter is UndoObject)
            {

                // Extract the parameter and do the actual work involved with changing the magnification factor.
                UndoObject undoObject = executedRoutedEventArgs.Parameter as UndoObject;
                //				this.OnSetScale((Double)undoObject.NewValue, undoObject.UndoAction);

                // Once the command is finished, this will broadcast the event.  The primary consumer is the Undo logic which will
                // save the event data on the Undo or Redo stack and can replay it when needed.
				RaiseEvent(new UndoPropertyChangedEventArgs(DynamicReport.UndoPropertyChangedEvent, undoObject.UndoAction, ReportGrid.ScaleProperty, undoObject.OldValue, undoObject.NewValue));

                // The property must be set when this operation is called from the Undo or Redo methods.  However, setting the
                // value will lead the logic right back here.  This will inhibit the command processing while the Undone or Redone
                // value is restored.
                if (undoObject.UndoAction == UndoAction.Undo || undoObject.UndoAction == UndoAction.Redo)
                {
                    this.CommandBindings.Remove(this.commandBingingMap[ReportGrid.SetScale]);
                    SetValue(ReportGrid.ScaleProperty, undoObject.NewValue);
                    this.CommandBindings.Add(this.commandBingingMap[ReportGrid.SetScale]);
                }

            }

        }

        #region IScrollInfo Members

        public bool CanHorizontallyScroll
        {
            get { return this.canHorizontallyScroll; }
            set { this.canHorizontallyScroll = value; }
        }

        public bool CanVerticallyScroll
        {
            get { return this.canVerticallyScroll; }
            set { this.canVerticallyScroll = value; }
        }

        public Double ExtentHeight
        {
            get { return this.extent.Height; }
        }

        public Double ExtentWidth
        {
            get { return this.extent.Width; }
        }

        public Double HorizontalOffset
        {
            get { return this.offset.X; }
        }

        public void LineDown()
        {
            SetVerticalOffset(this.offset.Y + (DefaultDocument.RowHeight * this.scaleFactor));
        }

        public void LineLeft()
        {
            SetHorizontalOffset(this.offset.X - (DefaultDocument.ColumnWidth * this.scaleFactor));
        }

        public void LineRight()
        {
            SetHorizontalOffset(this.offset.X + (DefaultDocument.ColumnWidth * this.scaleFactor));
        }

        public void LineUp()
        {
            SetVerticalOffset(this.offset.Y - (DefaultDocument.RowHeight * this.scaleFactor));
        }

        /// <summary>
        /// Forces content to scroll until the coordinate space of a Visual object is visible.
        /// </summary>
        /// <param name="visual">A Visual that becomes visible.</param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        /// <returns>A Rect that is visible.</returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {

            // As of now, the visuals are left to the user to scroll into a usable area.  Sometime in the future this should be
            // changed so that the ReportCanvas is scrolled enough to make the visual completely visible in the viewport.
            return rectangle;

        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(this.offset.Y + (DefaultDocument.RowHeight * this.scaleFactor));
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(this.offset.X - (DefaultDocument.ColumnWidth * this.scaleFactor));
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(this.offset.X + (DefaultDocument.ColumnWidth * this.scaleFactor));
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(this.offset.Y - (DefaultDocument.RowHeight * this.scaleFactor));
        }

        public void PageDown()
        {
            SetVerticalOffset(this.offset.Y + this.viewport.Height);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(this.offset.X - this.viewport.Width);
        }

        public void PageRight()
        {
            SetHorizontalOffset(this.offset.X + this.viewport.Width);
        }

        public void PageUp()
        {
            SetVerticalOffset(this.offset.Y - this.viewport.Height);
        }

        public ScrollViewer ScrollOwner
        {
            get { return this.scrollOwner; }
            set { this.scrollOwner = value; }
        }

        public void SetHorizontalOffset(Double horizontalOffset)
        {

            if (horizontalOffset < 0.0 || this.viewport.Width >= this.extent.Width)
            {
                horizontalOffset = 0;
            }
            else
            {
                if (horizontalOffset + this.viewport.Width >= this.extent.Width)
                {
                    horizontalOffset = this.extent.Width - this.viewport.Width;
                }
            }

            SetOffset(new Point(horizontalOffset, this.offset.Y));

            if (this.scrollOwner != null)
                this.scrollOwner.InvalidateScrollInfo();


        }

        public void SetVerticalOffset(Double verticalOffset)
        {

            if (verticalOffset < 0.0 || this.viewport.Height >= this.extent.Height)
            {
                verticalOffset = 0.0;
            }
            else
            {
                if (verticalOffset + this.viewport.Height >= this.extent.Height)
                {
                    verticalOffset = this.extent.Height - this.viewport.Height;
                }
            }

            // Force us to realize the correct children
            SetOffset(new Point(this.offset.X, verticalOffset));

            if (this.scrollOwner != null)
                this.scrollOwner.InvalidateScrollInfo();

        }

        public Double VerticalOffset
        {
            get { return this.offset.Y; }
        }

        public Double ViewportHeight
        {
            get { return this.viewport.Height; }
        }

        public Double ViewportWidth
        {
            get { return this.viewport.Width; }
        }

        #endregion

    }

}

namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;
    using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Markup;
	using System.Windows.Threading;
	using System.Xml.Linq;

	/// <summary>
	/// A high performance report for displaying real-time data.
	/// </summary>
	[ContentProperty("Content")]
	public class DynamicReport : ScrollViewer
	{

		// Internal Constants
		internal const Int32 defaultDuration = 0;
		internal const Double headerDragTrigger = 4.0;
		internal const Double splitBorder = 8.0;
		internal const int minimumCollectionTickCount = 40000;

		// Internal Static Fields
		internal static readonly XName xNameReport;
		internal static readonly XNamespace namespaceXaml;
		internal static readonly XNamespace namespacePresentation;
		//public static Dictionary<Guid, DateTime> openReports;

		private static int lastCollectionTick = 0;

		// Private Instance Fields
		private Duration duration;
		
		// Private Delegates
		private delegate void SetContentDelegate(IContent iContent);
		private delegate void SetSourceDelegate(XDocument xDocument);

        //Protected Fields
        public ReportGrid reportGrid;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.Cell attached property.
		/// </summary>
		public static readonly DependencyProperty CellProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.ClearContents routed command.
		/// </summary>
		public static readonly RoutedUICommand ClearContents;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.Content dependency property.
		/// </summary>
		public new static readonly DependencyProperty ContentProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.IsActive attached property.
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.IsEven attached property.
		/// </summary>
		public static readonly DependencyProperty IsEvenProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.IsHeaderFrozen dependency property.
		/// </summary>
		public static readonly DependencyProperty IsHeaderFrozenProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.IsLayoutFrozen dependency property.
		/// </summary>
		public static readonly DependencyProperty IsLayoutFrozenProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.IsPopup dependency property.
		/// </summary>
		public static readonly DependencyProperty IsPopupProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.IsSelected dependency property.
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.Scale dependency property.
		/// </summary>
		public static readonly DependencyProperty ScaleProperty;

		/// <summary>
		/// Gets a value that represents the SelectColumn command.
		/// </summary>
		public static readonly RoutedUICommand SelectColumn;

		/// <summary>
		/// Gets a value that represents the SetIsLayoutFrozen command.
		/// </summary>
		public static readonly RoutedUICommand SetIsLayoutFrozen;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.Split dependency property.
		/// </summary>
		public static readonly System.Windows.DependencyProperty SplitProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.DynamicReport.Source dependency property.
		/// </summary>
		public static readonly DependencyProperty SourceProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.UndoReportChange event.
		/// </summary>
		public static readonly RoutedEvent UndoPropertyChangedEvent;

		/// <summary>
		/// Occurs when the selection has changed.
		/// </summary>
		public event EventHandler SelectionChanged;

		/// <summary>
		/// Initializes the static elements of the document viewer.
		/// </summary>
		static DynamicReport()
		{

			// These namespaces prevent name conflicts by qualifying the XAML values used in the source code for this report.
			DynamicReport.namespaceXaml = "http://schemas.microsoft.com/winfx/2006/xaml";
			DynamicReport.namespacePresentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
			DynamicReport.xNameReport = DynamicReport.namespacePresentation + "DynamicReport";

			// Cell
			DynamicReport.CellProperty = DependencyProperty.RegisterAttached("Cell", typeof(ReportCell), typeof(DynamicReport));

			// Content
			DynamicReport.ContentProperty = DependencyProperty.Register(
				"Content",
				typeof(IContent),
				typeof(DynamicReport),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnContentChanged)));

			// IsActive
			DynamicReport.IsActiveProperty = DependencyProperty.RegisterAttached(
				"IsActive",
				typeof(Boolean),
				typeof(DynamicReport),
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

			// IsEven
			DynamicReport.IsEvenProperty = DependencyProperty.RegisterAttached("IsEven", typeof(Boolean), typeof(DynamicReport), new PropertyMetadata(false));

			// IsSelected
			DynamicReport.IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(Boolean), typeof(DynamicReport), new PropertyMetadata(false));

			// IsPopup
			DynamicReport.IsPopupProperty = DependencyProperty.RegisterAttached("IsPopup", typeof(Boolean), typeof(DynamicReport), new PropertyMetadata(false));

			// IsLayoutFrozen
			DynamicReport.IsLayoutFrozenProperty = DependencyProperty.Register(
				"IsLayoutFrozen",
				typeof(Object),
				typeof(DynamicReport),
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange, new PropertyChangedCallback(OnIsLayoutFrozenChanged)));

			// IsHeaderFrozen
			DynamicReport.IsHeaderFrozenProperty = DependencyProperty.Register(
				"IsHeaderFrozen",
				typeof(Boolean),
				typeof(DynamicReport),
				new PropertyMetadata(true, new PropertyChangedCallback(OnIsHeaderFrozenChanged)));

			// Scale
			DynamicReport.ScaleProperty = DependencyProperty.Register("Scale",
				typeof(Double),
				typeof(DynamicReport),
				new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnScaleChanged)));

			// Source
			DynamicReport.SourceProperty = DependencyProperty.Register("Source", typeof(XDocument), typeof(DynamicReport), new FrameworkPropertyMetadata(OnSourceChanged));

			// The Split property determines where the row and column headers are positioned.
			DynamicReport.SplitProperty = DependencyProperty.Register("Split",
				typeof(Size),
				typeof(DynamicReport),
				new FrameworkPropertyMetadata(new Size(), FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnSplitChanged)));

			// UndoPropertyChanged
			DynamicReport.UndoPropertyChangedEvent = EventManager.RegisterRoutedEvent("UndoPropertyChanged", RoutingStrategy.Bubble, typeof(UndoPropertyChangedEventHandler), typeof(DynamicReport));

			// ClearContents
			DynamicReport.ClearContents = new RoutedUICommand("Clear Contents", "ClearContents", typeof(DynamicReport));

			// Select Column
			DynamicReport.SelectColumn = new RoutedUICommand("Select", "SelectColumn", typeof(DynamicReport));

			// SetIsLayoutFrozen
			DynamicReport.SetIsLayoutFrozen = new RoutedUICommand("Is Layout Frozen", "SetIsLayoutFrozen", typeof(DynamicReport));

			//openReports = new Dictionary<Guid, DateTime>();

		}

		/// <summary>
		/// Creates a high performance report for displaying real-time data.
		/// </summary>
		public DynamicReport()
		{

			// Most property changes will translate to an internal command so the Undo Manager can track the changes and undo them
			// when requested.
			this.CommandBindings.Add(new CommandBinding(DynamicReport.SetIsLayoutFrozen, OnSetIsLayoutFrozen));

			// Set the default duration of animations.
			this.duration = new Duration(TimeSpan.FromMilliseconds(DynamicReport.defaultDuration));

			// This sets up the ScrollViewer to allow the content to scroll its own data and allows the scroll bars to appear and
			// disappear when needed.  The content of the ScrollViewer is the grid where the data is displayed.
			this.CanContentScroll = true;
			this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;


		}

		public ReportCell FocusedCell
		{

			get
			{

				if (this.reportGrid != null)
					return this.reportGrid.CurrentReportCell;
				else
					return null;
			
			}

		}

        public ReportRow FocusedRow
        {
            get
            {
                if (this.reportGrid != null)
                    return this.reportGrid.CurrentReportRow;
               
                return null;
            }
        }

		public ReportColumnCollection Columns
		{

			get
			{

				if (this.reportGrid != null)
					return this.reportGrid.reportColumnCollection;
				else
					return null;

			}

		}

		public ReportRowCollection Rows
		{

			get
			{

				if (this.reportGrid != null)
					return this.reportGrid.reportRowCollection;
				else
					return null;
			
			}

		}
        
		/// <summary>
		/// The content of the report.
		/// </summary>
		public new IContent Content
		{
			get { return (IContent)this.GetValue(DynamicReport.ContentProperty); }
			set { 
				this.SetValue(DynamicReport.ContentProperty, value);
			}
		}
		private void TryCollect()
		{
			if (Environment.TickCount - lastCollectionTick > minimumCollectionTickCount)
			{
				System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced);
				lastCollectionTick = Environment.TickCount;
			}
		}

		/// <summary>
		/// Gets or sets the time it take to perform animated operations.
		/// </summary>
		public Duration Duration
		{
			get { return this.duration; }
			set
			{
				this.duration = value;
				if (this.reportGrid != null)
					this.reportGrid.Duration = value;
			}
		}

		/// <summary>
		/// Gets or sets an indicator of whether the panes are frozen or allowed to move.
		/// </summary>
		public Boolean IsHeaderFrozen
		{
			get { return (Boolean)this.GetValue(DynamicReport.IsHeaderFrozenProperty); }
			set { this.SetValue(DynamicReport.IsHeaderFrozenProperty, value); }
		}

		/// <summary>
		/// Gets or sets an indicator of whether the panes are frozen or allowed to move.
		/// </summary>
		public Boolean IsLayoutFrozen
		{
			get { return (Boolean)this.GetValue(DynamicReport.IsLayoutFrozenProperty); }
			set { this.SetValue(DynamicReport.IsLayoutFrozenProperty, value); }
		}

		/// <summary>
		/// Gets the resources available to this report.
		/// </summary>
		public new ResourceDictionary Resources
		{
			get
			{

				if (this.reportGrid != null)
					return this.reportGrid.Resources;
				else
					return null;
			
			}
		}

		/// <summary>
		/// Gets or sets the magnification factor of the report.
		/// </summary>
		public Double Scale
		{
			get { return (Double)this.GetValue(DynamicReport.ScaleProperty); }
			set { this.SetValue(DynamicReport.ScaleProperty, value); }
		}

		/// <summary>
		/// Gets or sets the XAML document that describes the resources and other properties of the report.
		/// </summary>
		public XDocument Source
		{
			get { return (XDocument)this.GetValue(DynamicReport.SourceProperty); }
			set { this.SetValue(DynamicReport.SourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the location of the headers.
		/// </summary>
		public Size Split
		{
			get { return (Size)this.GetValue(DynamicReport.SplitProperty); }
			set { this.SetValue(DynamicReport.SplitProperty, value); }
		}

		public void ClearUndo()
		{

			if (this.reportGrid != null)
				this.reportGrid.undoManager.Clear();

		}
	
		public new Object FindName(String name)
		{

			if (this.reportGrid != null)
				return this.reportGrid.FindName(name);
			else
				return null;

		}

		/// <summary>
		/// Handles a change to the content of the report.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnContentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Load the new content into the core report.
			DynamicReport report = dependencyObject as DynamicReport;
			IContent iContent = dependencyPropertyChangedEventArgs.NewValue as IContent;
			if (report.reportGrid != null)
				report.reportGrid.reportRowCollection.Load(iContent);

		}

		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			if (e.NewFocus is DynamicReport)
			{
				if (this.reportGrid != null)
					Keyboard.Focus(this.reportGrid);
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles a change to the IsHeaderFrozen dependency property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnIsHeaderFrozenChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// The headers can be frozen to prevent any changes to the position, sort order, visibility, etc.
			DynamicReport report = dependencyObject as DynamicReport;
			Boolean isHeaderFrozen = (Boolean)dependencyPropertyChangedEventArgs.NewValue;
			if (report.reportGrid != null)
			{
				report.reportGrid.IsColumnHeaderFrozen = isHeaderFrozen;
				report.reportGrid.IsRowHeaderFrozen = isHeaderFrozen;
			}

		}

		/// <summary>
		/// Handles a change to the IsLayoutFrozen dependency property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnIsLayoutFrozenChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// The layout controls the relative size of the row and column headers.
			UndoObject undoObject = new UndoObject(dependencyPropertyChangedEventArgs.OldValue, dependencyPropertyChangedEventArgs.NewValue, UndoAction.Create);
			DynamicReport.SetIsLayoutFrozen.Execute(undoObject, dependencyObject as DynamicReport);

		}

		/// <summary>
		/// Handles a change to the Scale dependency property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnScaleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// The 'Scale' property is just a convinient way to generate a command that is used to set the scale factor.  The
			// commands are the only way to register events with the Undo manager.
			DynamicReport report = dependencyObject as DynamicReport;
			if (report.reportGrid != null)
				report.reportGrid.Scale = (Double)dependencyPropertyChangedEventArgs.NewValue;

		}

		/// <summary>
		/// Freezes or unfreezes the layout of the column and row header panels.
		/// </summary>
		/// <param name="sender">The object that originated the routed event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		public void OnSetIsLayoutFrozen(Object sender, RoutedEventArgs routedEventArgs)
		{

			// The idea is to extract the parameters from the event data and set frozen state of the panel layout based on the 
			// values provided.
			ExecutedRoutedEventArgs executedRoutedEventArgs = routedEventArgs as ExecutedRoutedEventArgs;

			// The simplest form for this command simply passes in a value.  The command parameter is burpped back up to the
			// property which then issues a command back here with the proper Undo information encoded in the command.  The idea is
			// to simplify the calling parameters for an externally invoked command while still maintaining a consistent format
			// internally for handling the undo/redo operations.
			if (executedRoutedEventArgs.Parameter is Boolean)
				SetValue(DynamicReport.IsLayoutFrozenProperty, (Boolean)executedRoutedEventArgs.Parameter);

			// This parameter format handles the undo/redo logic.
			if (this.reportGrid != null && executedRoutedEventArgs.Parameter is UndoObject)
			{

				// Extract the parameter and do the actual work involved with freezing the panes.
				UndoObject undoObject = executedRoutedEventArgs.Parameter as UndoObject;
				this.reportGrid.FreezePanes((Boolean)undoObject.NewValue, undoObject.UndoAction);

				// Once the command is finished, this will broadcast the event.  The primary consumer is the Undo logic which will
				// save the event data on the Undo or Redo stack and can replay it when needed.
				RaiseEvent(new UndoPropertyChangedEventArgs(DynamicReport.UndoPropertyChangedEvent, undoObject.UndoAction,
					DynamicReport.IsLayoutFrozenProperty, undoObject.OldValue, undoObject.NewValue));

				// The property must be set when this operation is called from the Undo or Redo methods.  However, setting the
				// value will lead the logic right back here.  This will inhibit the command processing while the Undone or Redone
				// value is restored.
				if (undoObject.UndoAction == UndoAction.Undo || undoObject.UndoAction == UndoAction.Redo)
				{
					this.CommandBindings.Remove(this.reportGrid.commandBingingMap[DynamicReport.SetIsLayoutFrozen]);
					SetValue(DynamicReport.IsLayoutFrozenProperty, undoObject.NewValue);
					this.CommandBindings.Add(this.reportGrid.commandBingingMap[DynamicReport.SetIsLayoutFrozen]);
				}

			}

		}

		internal void ChangeSelection()
		{
			OnSelectionChanged(EventArgs.Empty);
		}
	
		/// <summary>
		/// Broadcasts an event to indicate that the selection has changed.
		/// </summary>
		protected virtual void OnSelectionChanged(EventArgs eventArgs)
		{

			// Broadcast to anyone listening that the selection has changed.
			if (this.SelectionChanged != null)
				this.SelectionChanged(this, EventArgs.Empty);

		}

		/// <summary>
		/// Handles a change to the Split dependency property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnSplitChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// The 'Scale' property is just a convinient way to generate a command that is used to set the scale factor.  The
			// commands are the only way to register events with the Undo manager.
			DynamicReport report = dependencyObject as DynamicReport;
			report.reportGrid.Split = (Size)dependencyPropertyChangedEventArgs.NewValue;

		}

		/// <summary>
		/// Handles a change to the XAML source code property for this report.
		/// </summary>
		/// <param name="dependencyObject">The target object of the property change.</param>
		/// <param name="dependencyPropertyChangedEventArgs">The property change event arguments.</param>
		private static void OnSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Changing the source code involves compiling a new core for the report.  This operation will require access to some
			// of the protected properties of the class and so an instance method must be called to handle the change.
			DynamicReport report = dependencyObject as DynamicReport;
			report.OnSourceChanged();

		}

		/// <summary>
		/// Handles a change to the source code property.
		/// </summary>
		private void OnSourceChanged()
		{

			// Changing the source code means completely disengaging the core of the report.  When the scroll bar moves on this
			// containing control, that event needs to be passed onto the core report which will move the offsets of the various
			// quadrants to track the changes.
			if (this.reportGrid != null)
				this.ScrollChanged -= new ScrollChangedEventHandler(this.reportGrid.ScrollChanged);

			try
			{

				// There is a lot of work going on here.  A new instance of the report core is created and the XAML parser populates
				// the public properties with the compiled resources found in the description of the report.  Finally, the compiled 
				// XAML object becomes the the real 'content' of this window.  Meanwhile, this class pretends to be a content control
				// and usurps the 'Content' property for the actual data that appears   the report.
				System.IO.Stream stream = this.Source.ToString().ToStream();
				this.reportGrid = XamlReader.Load(stream) as ReportGrid;
				this.reportGrid.Scale = this.Scale;
				this.reportGrid.Duration = this.duration;
				this.reportGrid.IsColumnHeaderFrozen = this.IsHeaderFrozen;
				this.reportGrid.IsRowHeaderFrozen = this.IsHeaderFrozen;
				this.reportGrid.FreezePanes(this.IsLayoutFrozen, UndoAction.None);
				base.Content = this.reportGrid;

				// This will allow the core report to follow changes made to the viewports by the scroll bars.
				this.ScrollChanged += new ScrollChangedEventHandler(this.reportGrid.ScrollChanged);

				// The content of the report must be applied against the new sorce code for the changes to be visible.
				this.reportGrid.reportRowCollection.Load(this.Content);

			}
			catch (Exception exception)
			{

				EventLog.Warning(
					"Loading {0} report failed.\n{1}: {2}\n{3}",
					this.GetType(),
					exception.GetType(),
					exception.Message,
					exception.StackTrace);

			}

		}

		/// Background thead used to redraw the document.
		/// </summary>
		/// <param name="parameter">Unused thread initialization parameter.</param>
		public virtual void Refresh() { }

		/// <summary>
		/// Sets the content of the report.
		/// </summary>
		/// <param name="iContent">The report's content.</param>
		/// <remarks>Can be called from a background thread.</remarks>
		/// <param name="requestGC">should try to collect after the content is set</param>
		public void SetContent(IContent iContent, bool requestGC)
		{

			// Call the foreground asynchronously to set the report's content.
			this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new SetContentDelegate((content) =>
					this.Content = content),
				iContent);

			if (requestGC == true)
			{
				this.Dispatcher.BeginInvoke(new Action(TryCollect), DispatcherPriority.SystemIdle);
			}
		}

		public static void SetIsActive(DependencyObject dependencyObject, Boolean value)
		{
			dependencyObject.SetValue(DynamicReport.IsActiveProperty, value);
		}


        /// <summary>
		/// Select all the cells in the report.
		/// </summary>
		public void SelectAll()
		{

			if (this.reportGrid != null)
			{
                this.reportGrid.SelectAllRows();
            }
        }

		/// <summary>
		/// Allows the user to select, hide or change the order of columns in the report.
		/// </summary>
		public void SelectColumns()
		{

			if (this.reportGrid != null)
			{

				// This dialog will prompt the user to select a confiruation of columns for the report.
				ColumnSelector columnSelector = new ColumnSelector();

				// This is the list of all available fields.
				List<ReportField> fieldDefinitions = new List<ReportField>();
				foreach (ReportField fieldDefinition in this.reportGrid.reportFieldCollection)
					fieldDefinitions.Add(fieldDefinition);
				columnSelector.FieldDefinitions = fieldDefinitions;

				// This is a list of currently select columns in the view.
				columnSelector.ColumnDefinitions = this.reportGrid.reportColumnCollection.Columns.ToArray();
				columnSelector.Owner = Application.Current.MainWindow;

				// If the user accepts the configuration of columns they will be applied to the report.
				if (columnSelector.ShowDialog() == true)
				{
					List<ReportColumn> displayedColumns = new List<ReportColumn>();
					foreach (ReportField fieldDefinition in columnSelector.DisplayedFields)
					{

						ReportColumn reportColumn;
						if (!this.reportGrid.reportColumnCollection.TryGetValue(fieldDefinition.ColumnId, out reportColumn))
						{
							reportColumn = new ReportColumn();
							reportColumn.ColumnId = fieldDefinition.ColumnId;
							reportColumn.Width = fieldDefinition.Width;
						}

						displayedColumns.Add(reportColumn);

					}

					this.reportGrid.reportColumnCollection.Replace(displayedColumns);

				}

			}

		}

		/// <summary>
		/// Sets the XAML source used to display the content of the report.
		/// </summary>
		/// <param name="xDocument"></param>
		public void SetSource(XDocument xDocument)
		{

			// Call the foreground asynchronously to set the XAML source for the report.
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SetSourceDelegate)((document) => { this.Source = document; }), xDocument);

		}

		public static void SetCell(DependencyObject dependencyObject, ReportCell value)
		{
			dependencyObject.SetValue(DynamicReport.CellProperty, value);
		}

		public static ReportCell GetCell(DependencyObject dependencyObject)
		{
			return (ReportCell)dependencyObject.GetValue(DynamicReport.CellProperty);
		}

		public static Boolean GetIsActive(DependencyObject dependencyObject)
		{
			return (Boolean)dependencyObject.GetValue(DynamicReport.IsActiveProperty);
		}

		public static void SetIsEven(DependencyObject dependencyObject, Boolean value)
		{
			dependencyObject.SetValue(DynamicReport.IsEvenProperty, value);
		}

		public static Boolean GetIsEven(DependencyObject dependencyObject)
		{
			return (Boolean)dependencyObject.GetValue(DynamicReport.IsEvenProperty);
		}

		public static void SetIsSelected(DependencyObject dependencyObject, Boolean value)
		{
			dependencyObject.SetValue(DynamicReport.IsSelectedProperty, value);
		}

		public static Boolean GetIsSelected(DependencyObject dependencyObject)
		{
			return (Boolean)dependencyObject.GetValue(DynamicReport.IsSelectedProperty);
		}

		public static void SetIsPopup(DependencyObject dependencyObject, Boolean value)
		{
			dependencyObject.SetValue(DynamicReport.IsPopupProperty, value);
		}

		public static Boolean GetIsPopup(DependencyObject dependencyObject)
		{
			return (Boolean)dependencyObject.GetValue(DynamicReport.IsPopupProperty);
		}

	}

}

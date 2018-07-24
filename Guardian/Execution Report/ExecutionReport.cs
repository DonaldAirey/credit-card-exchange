namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using System.Windows.Threading;
	using System.Xml.Linq;
	using FluidTrade.Core;
	using FluidTrade.Core.Windows;
	using FluidTrade.Core.Windows.Controls;

	/// <summary>Schema.Execution.Execution
	/// 
	/// </summary>
	public class ExecutionReport : DynamicReport
	{
		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		// Private Static Fields
		private static Duration[] animationDurations;
		private static Dictionary<String, IComparer<Schema.Execution.Execution>> sortMethods;

		// Private Instance Fields
		private Guid blotterId;
		private List<Guid> blotterList;
		private ComplexComparer<Schema.Execution.Execution> comparer;
		private ComplexFilter<Schema.Execution.Execution> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private Boolean isHierarchyChanged;
		private ComplexFilter<ExecutionRow> prefilter;
		private Guid reportId;
		private SetBlotterFilterHandler setBlotterFilterHandler;

		// Private Delegates
		private delegate void SourceDelegate(XDocument xDocument);

		// Private Delegates
		private delegate void SetBlotterFilterHandler(List<Guid> blotterList);

		/// <summary>
		/// Create the static resources required for this report.
		/// </summary>
		static ExecutionReport()
		{

			// AnimationSpeed
			ExecutionReport.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ExecutionReport),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// These constants control the animation speed.
			ExecutionReport.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

			ExecutionReport.sortMethods = new Dictionary<string, IComparer<Schema.Execution.Execution>>();
			ExecutionReport.sortMethods.Add("ExecutionQuantityColumn", new Schema.Execution.ExecutionQuantityComparer());
			ExecutionReport.sortMethods.Add("ExecutionPriceColumn", new Schema.Execution.ExecutionPriceComparer());
			ExecutionReport.sortMethods.Add("CreatedTimeColumn", new Schema.Execution.CreatedTimeComparer());
			
		}

		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ExecutionReport()
		{

			// All records in the presentation layer of the report require a unique identifier.  When the report is updated, this
			// identifier is used to map the data to an existing record or to create a new one.  The starting point for the report
			// is the header record which uses this identifier.  The rest of the records in the report will generally use the
			// source DataRow as the unique identifier.
			this.guid = Guid.NewGuid();

			this.reportId = Guid.Empty;

			// These objects are required for sorting, filtering and ordering the report.
			this.prefilter = new ComplexFilter<ExecutionRow>();
			this.prefilter.Add(this.FilterBlotters);
			this.filter = new ComplexFilter<Schema.Execution.Execution>();
			this.comparer = new ComplexComparer<Schema.Execution.Execution>();

			// This is the list of all the blotters on display in this report.  A single blotter can be displayed or several may be
			// aggregated.  The blotter list is used by the 'prefilter' to determine which Match rows from the data model
			// should be transformed into the presentation layer objects.
			this.setBlotterFilterHandler = new SetBlotterFilterHandler(OnSetBlotterFilter);
			this.blotterList = new List<Guid>();

			// This is needed to satisfy the compiler.  In practice, this value is loaded from the user settings and defaulted
			// through the same mechanism.
			this.AnimationSpeed = AnimationSpeed.Off;

			this.CommandBindings.Add(new CommandBinding(FluidTradeCommands.SortReport, OnSortReport));

			// These handlers will update the middle tier in response to changes in the report.
			this.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnToggleButtonChange));
			this.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnToggleButtonChange));
			this.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(OnSelectorSelectionChanged));

			// These handlers take care of installing and uninstalling this window in the data model update events.
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

		}

		/// <summary>
		/// Handles a request to sort the report.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		private void OnSortReport(object sender, RoutedEventArgs routedEventArgs)
		{

			// Extract the command event argments.
			ExecutedRoutedEventArgs executedRoutedEventArgs = routedEventArgs as ExecutedRoutedEventArgs;
			SortEventArgs sortEventArgs = executedRoutedEventArgs.Parameter as SortEventArgs;

			// Clear out the previous comparison used to sort the report and replace it with the latest one computed from the
			// column headings.
			this.comparer.Clear();
			foreach (SortItem sortItem in sortEventArgs.Items)
			{
				IComparer<Schema.Execution.Execution> comparer;
				if (ExecutionReport.sortMethods.TryGetValue(sortItem.Column.ColumnId, out comparer))
					this.comparer.Add(comparer, sortItem.SortOrder);
			}

			// Generate a new document when the new comparison operators for a working order are installed.
			this.Refresh();

		}

		/// <summary>
		/// Initializes the components found in the dynamic XAML source.
		/// </summary>
		public void LoadSource()
		{

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(LoadSource);

		}

		private void LoadSource(object state)
		{

			lock (DataModel.SyncRoot)
			{

				BlotterConfigurationRow blotterConfigurationRow = DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
					this.blotterId,
					DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.Execution).ReportTypeId);

				if (blotterConfigurationRow != null)
				{

					FluidTrade.Guardian.ReportRow reportRow = blotterConfigurationRow.ReportRow;
					if (reportRow != null && reportRow.ReportId != this.reportId)
					{
						this.reportId = reportRow.ReportId;
						StringReader stringReader = new StringReader(reportRow.Xaml);
						XDocument xDocument = XDocument.Load(stringReader);
						this.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							(SourceDelegate)((XDocument source) => { this.Source = source; }), xDocument);

					}

				}

			}

		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// These events will keep the report updated with live content once the report data model is compiled.
			DataModel.Blotter.BlotterRowChanged += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Blotter.BlotterRowDeleted += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Report.ReportRowChanged += new ReportRowChangeEventHandler(OnReportRowChanged);
			DataModel.Entity.EntityRowChanged += new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged += new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.Execution.ExecutionRowChanged += new ExecutionRowChangeEventHandler(OnexecutionRowChanged);
			DataModel.Execution.ExecutionRowDeleted += new ExecutionRowChangeEventHandler(OnexecutionRowChanged);
			DataModel.EndMerge += new EventHandler(OnEndMerge);

		}

		private void OnReportRowChanged(object sender, ReportRowChangeEventArgs e)
		{

			// This will force the current report to reload from the newly loaded source.
			if (e.Action == DataRowAction.Commit)
				if (e.Row.RowState != DataRowState.Detached && e.Row.ReportId == this.reportId)
				{
					this.reportId = Guid.Empty;
					LoadSource();
				}

		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{

			DataModel.Blotter.BlotterRowDeleted -= new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Entity.EntityRowChanged -= new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged -= new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.Execution.ExecutionRowChanged -= new ExecutionRowChangeEventHandler(OnexecutionRowChanged);
			DataModel.Execution.ExecutionRowDeleted -= new ExecutionRowChangeEventHandler(OnexecutionRowChanged);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);

		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ExecutionReport.AnimationSpeedProperty); }
			set { this.SetValue(ExecutionReport.AnimationSpeedProperty, value); }
		}

		/// <summary>
		/// BlotterId
		/// </summary>
		public Guid BlotterId
		{
			get { return this.blotterId; }
			set
			{
				this.blotterId = value;
				LoadSource();
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(this.ExpandBlotterId, this.blotterId);
			}

		}

		/// <summary>
		/// Called from the background to set the universe of blotters that are of interest to the foreground.
		/// </summary>
		/// <param name="blotterList">The list of blotters associated with this viewer.</param>
		private void OnSetBlotterFilter(List<Guid> blotterList)
		{

			try
			{

				// Clear the list used by the foreground for determining what blotters and their associated children and events are of interest to viewers of
				// this report.
				this.blotterList.Clear();

				// This will order the list so each blotter can be indexed using a binary search.
				foreach (Guid blotterId in blotterList)
				{
					int index = this.blotterList.BinarySearch(blotterId);
					if (index < 0)
					{
						this.blotterList.Insert(~index, blotterId);
					}
				}

			}
			catch (Exception exception)
			{

				// Theoretically this should never generate an exception.  However, we've seen it with large data sets.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

			}

			// Redraw the document with the new universe of blotters.
			Refresh();

		}

		private void ExpandBlotterId(Object state)
		{

			Guid blotterId = (Guid)state;
			List<Guid> blotterList = new List<Guid>();

			lock (DataModel.SyncRoot)
			{
				BlotterRow blotterRow = DataModel.Blotter.BlotterKey.Find(blotterId);
				if (blotterRow != null)
					ExpandBlotterRow(blotterList, blotterRow);
			}

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, this.setBlotterFilterHandler, blotterList);

		}

		private void ExpandBlotterRow(List<Guid> blotterList, BlotterRow blotterRow)
		{

			blotterList.Add(blotterRow.EntityRow.EntityId);
			foreach (EntityTreeRow entityTreeRow in blotterRow.EntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
				foreach (BlotterRow childRow in entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId.GetBlotterRows())
					ExpandBlotterRow(blotterList, childRow);

		}

		/// <summary>
		/// Global handler for commands that always execute.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments used to indicate whether the given command can execute.</param>
		private void OnAlwaysExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		/// <summary>
		/// Handles a change to the animation speed.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnAnimationSpeedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Set the speed for animation.
			ExecutionReport reportPrototype = dependencyObject as ExecutionReport;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ExecutionReport.animationDurations[(Int32)animationSpeed];

		}

		/// <summary>
		/// Handles a change to the executionRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnBlotterRowChanged(object sender, BlotterRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles the end of merging data into the client side data model.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="eventArgs">The unused event arguments.</param>
		private void OnEndMerge(object sender, EventArgs eventArgs)
		{

			// If the hierarchy was changed then the list of blotters visible in this report needs to be evaluated and passed into
			// the filter.  Note that changing the hierarchy will also force a refresh of the report in the foreground giving it a
			// higher precendence than a normal data update.  Said differently, if the hierarchy has changed, the report will be
			// refreshed when the new filter is installed and there is no reason to call the refresh thread from here.
			if (this.isHierarchyChanged)
			{
				isHierarchyChanged = false;

				// This will recreate the list of blotters allowed into this report.  Note that the filter must be set in the
				// foreground once the hierarchy is expanded.
				List<Guid> blotterList = new List<Guid>();
				BlotterRow blotterRow = DataModel.Blotter.BlotterKey.Find(this.blotterId);
				if (blotterRow != null)
					ExpandBlotterRow(blotterList, blotterRow);
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, this.setBlotterFilterHandler, blotterList);
			}
			else
			{

				// The content of the report is regenerated in a worker thread when the data related to this report has changed.  When
				// the content is regenerated, it will be sent to the foreground to be presented in the report.
				if (this.isDataChanged)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(RefreshThread);
					this.isDataChanged = false;
				}

			}

		}

		/// <summary>
		/// Handles a change to the Entity table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnEntityRowChanged(object sender, EntityRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles a change to the EntityTree table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnEntityTreeRowChanged(object sender, EntityTreeRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isHierarchyChanged = true;
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles a change to the executionRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnPriceRowChanged(object sender, PriceRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handlers for the ComboBox class of controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		private void OnSelectorSelectionChanged(object sender, RoutedEventArgs routedEventArgs)
		{

			// The main idea of this handler is to sort out the user generated actions from the machine generated actions.  Once
			// its determined that it was a user action, a background thread is called to change the associated field to the value
			// selected by the ComboBox.
			SelectionChangedEventArgs selectionChangedEventArgs = routedEventArgs as SelectionChangedEventArgs;

			// Handle changes to ComboBox elements.
			if (selectionChangedEventArgs.OriginalSource is ComboBox)
			{

				ComboBox comboBox = selectionChangedEventArgs.OriginalSource as ComboBox;
				IContent iContent = comboBox.DataContext as IContent;

				// This filters all the ComboBox events looking for user initiated actions that are bound to the data model.
				if (InputHelper.IsUserInitiated(comboBox, ComboBox.SelectedValueProperty) &&
					iContent != null && iContent.Key is DataTableCoordinate)
				{

					// At this point, a ComboBox was modified by the user and it is connected to a data model field.  This will extract
					// the coordinates of the field in the table.  That, in turn, drives the decision about how to update the shared
					// data model.
					DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
					ExecutionRow executionRow = dataTableCoordiante.DataRow as ExecutionRow;

				}

			}

		}

		/// <summary>
		/// Handlers for the Toggle button class of controls.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		private void OnToggleButtonChange(object sender, RoutedEventArgs routedEventArgs)
		{

			// The main idea of this handler is to sort out the user generated actions from the machine generated actions.  Once
			// its determined that it was a user action, a background thread is called to change the associated field to the value
			// selected by the ToggleButton.
			ToggleButton toggleButton = routedEventArgs.OriginalSource as ToggleButton;
			IContent iContent = toggleButton.DataContext as IContent;

			// This filters all the ToggleButton events looking for user initiated actions that are bound to the data model.
			if (InputHelper.IsUserInitiated(toggleButton, ToggleButton.IsCheckedProperty) &&
				iContent != null && iContent.Key is DataTableCoordinate)
			{

				// At this point, a ToggleButton was modified by the user and it is connected to a data model field.  This will 
				// extract the coordinates of the field in the table.  That, in turn, drives the decision about how to update the
				// shared data model.
				DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
				ExecutionRow executionRow = dataTableCoordiante.DataRow as ExecutionRow;

			}

		}

		/// <summary>
		/// Handles a change to the ExecutionRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnExecutionRowChanged(object sender, ExecutionRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles a change to the DestinationOrderRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnDestinationOrderRowChanged(object sender, DestinationOrderRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles a change to the SourceOrderRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnSourceOrderRowChanged(object sender, SourceOrderRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}


		/// <summary>
		/// Handles a change to the executionRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnexecutionRowChanged(object sender, ExecutionRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}


		/// <summary>
		/// Updates the data in the report.
		/// </summary>
		public override void Refresh()
		{

			ThreadPoolHelper.QueueUserWorkItem(RefreshThread);			

		}

		/// <summary>
		/// Refreshes the data in the report in a worker thread.
		/// </summary>
		/// <param name="state">The unused thread start parameters.</param>
		private void RefreshThread(object state)
		{
			bool requestGC = true.Equals(state);
			
			// Once the module is compiled and loaded into memory, an instance of the specified viewer is created.  The
			// IDocumentView is all that is needed to communicate with the loaded view.  Note that an array of delegates is passed
			// into the newly compiled and loaded View that allow the View to communicate back to the host Viewer.
			lock (DataModel.SyncRoot)
			{
				this.Dispatcher.BeginInvoke(new Action(delegate()
				{
					this.SetContent(this.ExecutionHeaderSelector(this.guid), requestGC);
				})
				, DispatcherPriority.Normal);
			}
		}

		private Schema.ExecutionHeader.ExecutionHeader ExecutionHeaderSelector(Guid guid)
		{
			Schema.ExecutionHeader.ExecutionHeader ExecutionHeader = new Schema.ExecutionHeader.ExecutionHeader();
			ExecutionHeader.Prefilter = this.prefilter;
			ExecutionHeader.Selector = MatchSelector;
			ExecutionHeader.Filter = this.filter;
			ExecutionHeader.Comparer = this.comparer;
			return ExecutionHeader.Select(guid);
		}

		private Schema.Execution.Execution MatchSelector(ExecutionRow executionRow)
		{
			Schema.Execution.Execution match = new Schema.Execution.Execution();
			return match.Select(executionRow);
		}


		private bool FilterBlotters(ExecutionRow executionRow)
		{
			return this.blotterList.BinarySearch(executionRow.BlotterId) >= 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.guid.ToString();
		}

	}

}





//namespace FluidTrade.Guardian
//{
//    using System;
//    using System.Threading;
//    using FluidTrade.Core;
//    using FluidTrade.Core.Windows.Controls;
//    using FluidTrade.Guardian.Windows;

//    /// <summary>
//    /// A viewer for the executions of orders placed with a destination.
//    /// </summary>
//    public class ExecutionReport : Report
//    {

//        // Public Events
//        public event System.EventHandler Execution;

//        // Private Instance Fields
//        private System.Object content;

//        /// <summary>
//        /// Constructor for the ExecutionReport
//        /// </summary>
//        public ExecutionReport()
//        {

//        }

//        /// <summary>
//        /// Initializes the background destinations for the viewer.
//        /// </summary>
//        public new Object Content
//        {

//            set
//            {

//                this.content = value;

//                try
//                {

//                    // Lock the data model while the tables are read.
//                    Monitor.Enter(DataModel.SyncRoot);

//                    // The Data Transform contains the instructions for building and displaying the customized view of the data.
//                    BlotterConfigurationRow blotterConfigurationRow = null;

//                    // This will open the viewer to show the destination orders for an entire blotter.
//                    if (this.Tag is Blotter)
//                    {

//                        // This helps to examine the values used to open this document.
//                        Blotter blotter = this.Tag as Blotter;

//                        // Find the Data Transform for the Destination Order viewer for this blotter.
//                        blotterConfigurationRow =
//                            DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeCode.Find(
//                            blotter.BlotterId, ReportType.Execution);

//                    }

//                    // This will open the viewer to show only selected working orders.
//                    if (this.Tag is BlotterWorkingOrderDetail)
//                    {

//                        // This helps to examine the values used to open this document.
//                        BlotterWorkingOrderDetail blotterWorkingOrderDetail = this.Tag as BlotterWorkingOrderDetail;

//                        // Find the Data Transform for the Destination Order Detail viewer for this blotter.
//                        blotterConfigurationRow =
//                            DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeCode.Find(
//                            blotterWorkingOrderDetail.Blotter.BlotterId, ReportType.ExecutionDetail);

//                    }

//                    // Throw an exception if there is no data transform for displaying the data associated with the tag.
//                    if (blotterConfigurationRow == null)
//                        throw new Exception(String.Format("The {0} can not display an object of {1}", this.GetType(), this.Tag));

//                }
//                catch (Exception exception)
//                {

//                    // Write the error and stack trace out to the debug listener
//                    EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

//                }
//                finally
//                {

//                    // Allow other threads to access the data model.
//                    Monitor.Exit(DataModel.SyncRoot);

//                }

//            }

//        }

//        /// <summary>
//        /// Called when an execution quantity is added to the block order.
//        /// </summary>
//        /// <param name="executionEventArgs">Event parameters.</param>
//        protected virtual void OnExecution()
//        {

//            // Broadcast the event to anyone listening.
//            if (this.Execution != null)
//                this.Execution(this, EventArgs.Empty);

//        }

//    }

//}


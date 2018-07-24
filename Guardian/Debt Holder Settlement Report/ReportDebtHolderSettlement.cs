namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.ServiceModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using System.Windows.Threading;
	using System.Xml.Linq;
	using FluidTrade.Core;
	using FluidTrade.Core.Windows;
	using FluidTrade.Core.Windows.Controls;
	using FluidTrade.Core.Windows.Utilities;
	using FluidTrade.Guardian.TradingSupportReference;
	using FluidTrade.Guardian.Windows;
	 
	/// <summary>
    /// Schema.Settlement.Settlement
	/// </summary>
	public class ReportDebtHolderSettlement : DynamicReport
	{
			/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		// Private Static Fields
		private static Duration[] animationDurations;
        private static Dictionary<String, IComparer<Schema.DebtHolderSettlement.Settlement>> sortMethods;

		// Private Instance Fields
		private Guid blotterId;
		private List<Guid> blotterList;
        private ComplexComparer<Schema.DebtHolderSettlement.Settlement> comparer;
        private ComplexFilter<Schema.DebtHolderSettlement.Settlement> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private Boolean isHierarchyChanged;
		private ComplexFilter<ConsumerDebtSettlementRow> prefilter;		
		private Guid reportId;
		private SetBlotterFilterHandler setBlotterFilterHandler;
		
		// Private Delegates
		private delegate void SourceDelegate(XDocument xDocument);

		// Private Delegates
		private delegate void SetBlotterFilterHandler(List<Guid> blotterList);

		/// <summary>
		/// Create the static resources required for this report.
		/// </summary>
		static ReportDebtHolderSettlement()
		{

			// AnimationSpeed
			ReportDebtHolderSettlement.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ReportDebtHolderSettlement),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// These constants control the animation speed.
			ReportDebtHolderSettlement.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

            ReportDebtHolderSettlement.sortMethods = new Dictionary<string, IComparer<Schema.DebtHolderSettlement.Settlement>>();
			ReportDebtHolderSettlement.sortMethods.Add("ModifiedDateTimeColumn", new Schema.DebtHolderSettlement.ModifiedDateTimeComparer());

                        
		}

		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ReportDebtHolderSettlement()
		{

			// All records in the presentation layer of the report require a unique identifier.  When the report is updated, this
			// identifier is used to map the data to an existing record or to create a new one.  The starting point for the report
			// is the header record which uses this identifier.  The rest of the records in the report will generally use the
			// source DataRow as the unique identifier.
			this.guid = Guid.NewGuid();

			this.reportId = Guid.Empty;

			// These objects are required for sorting, filtering and ordering the report.
			this.prefilter = new ComplexFilter<ConsumerDebtSettlementRow>();
			this.prefilter.Add(this.FilterBlotters);
            this.filter = new ComplexFilter<Schema.DebtHolderSettlement.Settlement>();
			this.comparer = new ComplexComparer<Schema.DebtHolderSettlement.Settlement>();
			this.comparer.Add(new Schema.DebtHolderSettlement.ModifiedDateTimeComparer(), SortOrder.Descending);
			
			// This is the list of all the blotters on display in this report.  A single blotter can be displayed or several may be
			// aggregated.  The blotter list is used by the 'prefilter' to determine which Settlement rows from the data model
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
                IComparer<Schema.DebtHolderSettlement.Settlement> comparer;
				if (ReportDebtHolderSettlement.sortMethods.TryGetValue(sortItem.Column.ColumnId, out comparer))
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

			try
			{

				lock (DataModel.SyncRoot)
				{

					BlotterConfigurationRow blotterConfigurationRow = DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
						this.blotterId,
						DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.Settlement).ReportTypeId);

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
			catch(Exception exception)
			{

				EventLog.Warning(String.Format("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace));

			}

		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{

			// These events will keep the report updated with live content once the report data model is compiled.
			DataModel.Blotter.BlotterRowChanged += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Blotter.BlotterRowDeleted += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.BlotterConfiguration.BlotterConfigurationRowChanged += new BlotterConfigurationRowChangeEventHandler(OnBlotterConfigurationRowChanged);
			DataModel.Report.ReportRowChanged += new ReportRowChangeEventHandler(OnReportRowChanged);
			DataModel.Entity.EntityRowChanged += new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged += new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
            DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementRowChanged += new ConsumerDebtSettlementRowChangeEventHandler(OnConsumerDebtSettlementRowChanged);
            DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementRowDeleted += new ConsumerDebtSettlementRowChangeEventHandler(OnConsumerDebtSettlementRowChanged);
			DataModel.EndMerge += new EventHandler(OnEndMerge);

		}

		/// <summary>
		/// Handles the changing of a blotter configuration record.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnBlotterConfigurationRowChanged(object sender, BlotterConfigurationRowChangeEventArgs e)
		{

			// The main idea here is to determin if this update is relevant for the currently displayed document.  If it is, then the document needs to be
			// redrawn.
			if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
			{

				// We want to find out what report is currently assigned to this blotter for displaying working orders.
				ReportTypeRow reportTypeRow = DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.WorkingOrder);
				if (reportTypeRow != null)
				{

					// This determines what report is used as a template based on the blotter being displayed and the need for a Working Order template.
					if (e.Row.ReportTypeId == reportTypeRow.ReportTypeId && e.Row.BlotterId == this.blotterId)
					{
						this.reportId = Guid.Empty;
						LoadSource();
					}
				}
			}
		}

		/// <summary>
		/// Handles a change to a Report record.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The record change event arguments.</param>
		private void OnReportRowChanged(object sender, ReportRowChangeEventArgs e)
		{

			// The main idea here is to determin if this update is relevant for the currently displayed document.  If it is, then the document needs to be
			// redrawn.
			if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
			{

				// We want to find out what report is currently assigned to this blotter for displaying working orders.
				ReportTypeRow reportTypeRow = DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.WorkingOrder);
				if (reportTypeRow != null)
				{

					// This determines what report is used as a template based on the blotter being displayed and the need for a Working Order template.
					BlotterConfigurationRow blotterConfigurationRow = DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
						this.blotterId,
						reportTypeRow.ReportTypeId);

					// Now taht we have the blotter configuration row we can check to see if the report that was changed was the one that is supposed to be
					// used by this report.  Note that you can't use the 'reportId' member because that may not be set at this time.
					if (blotterConfigurationRow != null)
					{
						FluidTrade.Guardian.ReportRow reportRow = blotterConfigurationRow.ReportRow;
						if (reportRow != null && e.Row.ReportId == reportRow.ReportId)
						{
							this.reportId = Guid.Empty;
							LoadSource();
						}
					}
				}
			}

		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
            DataModel.Blotter.BlotterRowChanged -= new BlotterRowChangeEventHandler(OnBlotterRowChanged);
            DataModel.Blotter.BlotterRowDeleted -= new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.BlotterConfiguration.BlotterConfigurationRowChanged -= new BlotterConfigurationRowChangeEventHandler(OnBlotterConfigurationRowChanged);
			DataModel.Report.ReportRowChanged -= new ReportRowChangeEventHandler(OnReportRowChanged);
            DataModel.Entity.EntityRowChanged -= new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged -= new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
            DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementRowChanged -= new ConsumerDebtSettlementRowChangeEventHandler(OnConsumerDebtSettlementRowChanged);
            DataModel.ConsumerDebtSettlement.ConsumerDebtSettlementRowDeleted -= new ConsumerDebtSettlementRowChangeEventHandler(OnConsumerDebtSettlementRowChanged);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);
		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ReportDebtHolderSettlement.AnimationSpeedProperty);}
			set { this.SetValue(ReportDebtHolderSettlement.AnimationSpeedProperty, value); }
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
			ReportDebtHolderSettlement reportPrototype = dependencyObject as ReportDebtHolderSettlement;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ReportDebtHolderSettlement.animationDurations[(Int32)animationSpeed];

		}

		/// <summary>
		/// Handles a change to the BlotterRow table.
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
		/// Handles a change to the PriceRow table.
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
                    // TODO: Review this and figure out what should be here for the Settlement if anything.
                    //  Not sure if this even get called.

					// At this point, a ComboBox was modified by the user and it is connected to a data model field.  This will extract
					// the coordinates of the field in the table.  That, in turn, drives the decision about how to update the shared
					// data model.
					DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
					//MatchRow matchRow = dataTableCoordiante.DataRow as MatchRow;

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
                // TODO: Review this and figure out what should be here for the Settlement if anything.
                //  Not sure if this even get called.

				// At this point, a ToggleButton was modified by the user and it is connected to a data model field.  This will 
				// extract the coordinates of the field in the table.  That, in turn, drives the decision about how to update the
				// shared data model.
				DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
				//MatchRow matchRow = dataTableCoordiante.DataRow as MatchRow;

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
		/// Handles a change to the SettlementRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnConsumerDebtSettlementRowChanged(object sender, ConsumerDebtSettlementRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Updates the data in the report.
		/// </summary>
		public override void Refresh()
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(true);

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

			// Note that SetContent method will dispatch the thread hence it was not done here.
			lock (DataModel.SyncRoot)
				this.SetContent(this.SettlementHeaderSelector(this.guid), requestGC);

			ThreadPoolHelper.QueueUserWorkItem(RefreshThreadCompleted);

		}

		private void RefreshThreadCompleted(object state)
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(false);

		}

        private Schema.DebtHolderSettlementHeader.SettlementHeader SettlementHeaderSelector(Guid guid)
		{
            Schema.DebtHolderSettlementHeader.SettlementHeader SettlementHeader = new Schema.DebtHolderSettlementHeader.SettlementHeader();
			SettlementHeader.Prefilter = this.prefilter;
			SettlementHeader.Selector = SettlementSelector;
			SettlementHeader.Filter = this.filter;
			SettlementHeader.Comparer = this.comparer;
			return SettlementHeader.Select(guid);
		}

        private Schema.DebtHolderSettlement.Settlement SettlementSelector(ConsumerDebtSettlementRow consumerDebtSettlementRow)
		{
            Schema.DebtHolderSettlement.Settlement settlement = new Schema.DebtHolderSettlement.Settlement();
            return settlement.Select(consumerDebtSettlementRow);
		}


		private bool FilterBlotters(ConsumerDebtSettlementRow settlementRow)
		{
			return this.blotterList.BinarySearch(settlementRow.BlotterId) >= 0;			
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.guid.ToString();
		}

		/// <summary>
		/// Safely retrieve ConsumerDebtSettlementRow from a reportRow
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		private ConsumerDebtSettlementRow GetandValidateWorkingRow(FluidTrade.Core.Windows.Controls.ReportRow row)
		{
			ConsumerDebtSettlementRow workingOrderRow = row.NullSafe(datarow => datarow.IContent).NullSafe(Content => Content.Key) as ConsumerDebtSettlementRow;

			try
			{
				if (workingOrderRow != null)
				{
					if (workingOrderRow.ConsumerDebtSettlementId != null)
						return workingOrderRow;
				}
			}
			catch (RowNotInTableException)
			{
				//Deleted row. 
				workingOrderRow = null;

			}

			return workingOrderRow;
		}


		/// <summary>
		/// Regenerate the Settlement Letter.
		/// </summary>
		public void RegenerateSettlement()
		{
			
			// This operation is only defined if one or more rows have been selected.
			if (reportGrid.SelectedRowHeaderBlocks.Count >= 1)
			{

				// Make it work for multiple selected row section using control key. TODO: Fix bug in selectedRanges that does not get the correct selected
				// rows and cause the server side to throw an exception. Problem is that reportGrid.SelectedRowHeaderBlocks contains one extra item in the
				// inner list collection due to the one that is right clicked get added to the selected Ranges? This what need to be fixed.
				List<List<FluidTrade.Core.Windows.Controls.ReportRow>> selectedRowBlocks = reportGrid.SelectedRowHeaderBlocks;
				List<BaseRecord> settlementRows = new List<BaseRecord>();

				foreach (List<FluidTrade.Core.Windows.Controls.ReportRow> selectedRows in selectedRowBlocks)
				{
					foreach (FluidTrade.Core.Windows.Controls.ReportRow reportRow in selectedRows)
					{
						ConsumerDebtSettlementRow consumerDebtSettlementRow = reportRow.IContent.Key as ConsumerDebtSettlementRow;
						if (consumerDebtSettlementRow != null)
						{
							// If we are multi-selecting the settlement rows then we cannot display a message for every settlemment that has been already approved.
							// Hence we just do not add it to the list of settlements that need to be approved.
							if ((consumerDebtSettlementRow.StatusRow.StatusCode.Equals(FluidTrade.Guardian.Status.Pending)) ||
								(consumerDebtSettlementRow.StatusRow.StatusCode.Equals(FluidTrade.Guardian.Status.New)))
								settlementRows.Add(new BaseRecord()
								{
									RowId = consumerDebtSettlementRow.ConsumerDebtSettlementId,
									RowVersion = consumerDebtSettlementRow.RowVersion
								});
						}
					}
				}

				if (settlementRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(RegenerateSettlementThread, settlementRows);
				}
			}
			else
			{
				// We have only a single cell selected, get the row and operate on it.

				ConsumerDebtSettlementRow consumerDebtSettlementRow = null;
				List<ConsumerDebtSettlementRow> settlementRows = new List<ConsumerDebtSettlementRow>();
								
				if (consumerDebtSettlementRow == null)
				{
					FluidTrade.Core.Windows.Controls.ReportRow row = reportGrid.CurrentReportCell.ReportRow;
					consumerDebtSettlementRow = GetandValidateWorkingRow(row);
				}

				if ((!settlementRows.Contains(consumerDebtSettlementRow)) && (consumerDebtSettlementRow.StatusRow.StatusCode.Equals(FluidTrade.Guardian.Status.Accepted)))
					settlementRows.Add(consumerDebtSettlementRow);

				if (settlementRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(RegenerateSettlementThread, settlementRows);
				}
			}

		}

		/// <summary>
		/// Marks a set of record(s) as read.
		/// </summary>
		/// <param name="state">The list to delete.</param>
		private void RegenerateSettlementThread(object state)
		{

			List<BaseRecord> consumerDebtSettlementRows = state as List<BaseRecord>;
			MethodResponseErrorCode response;

			// Send the regenerate records.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{
				response = tradingSupportClient.ResetConsumerDebtSettlement(consumerDebtSettlementRows.ToArray());

				if (!response.IsSuccessful)
				{

					List<BaseRecord> retryRecords = new List<BaseRecord>();

					foreach (ErrorInfo errorInfo in response.Errors)
					{
						EventLog.Error("ResetConsumerDebtSettlement {0} failed with following message: {1}", errorInfo.BulkIndex, errorInfo.Message);						
					
					}

					throw new FaultException("Not all the records were reset.  Please see Event Viewer for detailed information about the errors.");
					
				}

			}
			catch (FaultException faultException)
			{
				EventLog.Error("{0}, {1}", faultException.Message, faultException.StackTrace);

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, faultException.Message, Application.Current.MainWindow.Title, MessageBoxButton.OK,
					MessageBoxImage.Error)));
			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged or might not have a valid match row cell to bold or unbold.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Failed to perform Regenerate Settlement operation.", Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

		}

		/// <summary>
		/// Approve the settlement.
		/// </summary>
		public void ApproveSettlement()
		{

			// This operation is only defined if one or more rows have been selected.
			if (reportGrid.SelectedRowHeaderBlocks.Count >= 1)
			{

				// Make it work for multiple selected row section using control key. TODO: Fix bug in selectedRanges that does not get the correct selected
				// rows and cause the server side to throw an exception. Problem is that reportGrid.SelectedRowHeaderBlocks contains one extra item in the
				// inner list collection due to the one that is right clicked get added to the selected Ranges? This what need to be fixed.
				List<List<FluidTrade.Core.Windows.Controls.ReportRow>> selectedRowBlocks = reportGrid.SelectedRowHeaderBlocks;
				List<ConsumerDebtSettlementRow> settlementRows = new List<ConsumerDebtSettlementRow>();

				foreach (List<FluidTrade.Core.Windows.Controls.ReportRow> selectedRows in selectedRowBlocks)
				{
					foreach (FluidTrade.Core.Windows.Controls.ReportRow reportRow in selectedRows)
					{
						ConsumerDebtSettlementRow consumerDebtSettlementRow = reportRow.IContent.Key as ConsumerDebtSettlementRow;
						if (consumerDebtSettlementRow != null)
						{
							// If we are multi-selecting the settlement rows then we cannot display a message for every settlemment that has been already approved.
							// Hence we just do not add it to the list of settlements that need to be approved.
							if ((!settlementRows.Contains(consumerDebtSettlementRow)) && (!consumerDebtSettlementRow.StatusRow.StatusCode.Equals(FluidTrade.Guardian.Status.Accepted)))
								settlementRows.Add(consumerDebtSettlementRow);
						}
					}
				}

				if (settlementRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(ApproveSettlementThread, settlementRows);
				}
			}
			else
			{
				// We have only a single cell selected, get the row and operate on it.

				ConsumerDebtSettlementRow consumerDebtSettlementRow = null;
				List<ConsumerDebtSettlementRow> settlementRows = new List<ConsumerDebtSettlementRow>();
								
				if (consumerDebtSettlementRow == null)
				{
					FluidTrade.Core.Windows.Controls.ReportRow row = reportGrid.CurrentReportCell.ReportRow;
					consumerDebtSettlementRow = GetandValidateWorkingRow(row);
				}

				// For a single item we can display a message where a settlement has already been approved.
				if (consumerDebtSettlementRow.StatusRow.StatusCode.Equals(FluidTrade.Guardian.Status.Accepted))
				{
					MessageBox.Show(Application.Current.MainWindow, "Approve Settlement operation failed as this settlement is already approved.", Application.Current.MainWindow.Title);
					return;
				}
				
				if ((!settlementRows.Contains(consumerDebtSettlementRow)) && (!consumerDebtSettlementRow.StatusRow.StatusCode.Equals(FluidTrade.Guardian.Status.Accepted)))
					settlementRows.Add(consumerDebtSettlementRow);

				if (settlementRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(ApproveSettlementThread, settlementRows);
				}
			}

		}

		/// <summary>
		/// Marks a set of record(s) as read.
		/// </summary>
		/// <param name="state">The list to delete.</param>
		private void ApproveSettlementThread(object state)
		{

			List<ConsumerDebtSettlementRow> consumerDebtSettlementRows = state as List<ConsumerDebtSettlementRow>;
			List<ConsumerDebtSettlementAcceptInfo> consumerDebtSettlementAcceptInfos = new List<ConsumerDebtSettlementAcceptInfo>();

			// Set up the list of accept records to send.
			try
			{

				lock (DataModel.SyncRoot)
				{
					foreach (ConsumerDebtSettlementRow consumerDebtSettlementRow in consumerDebtSettlementRows)
					{

						// Construct a version of the negotiation record from the known information and update the 'IsRead' flag.
						ConsumerDebtSettlementAcceptInfo consumerDebtSettlementAcceptInfo = new ConsumerDebtSettlementAcceptInfo();
						consumerDebtSettlementAcceptInfo.ConsumerDebtSettlementId = consumerDebtSettlementRow.ConsumerDebtSettlementId;
						consumerDebtSettlementAcceptInfo.RowVersion = consumerDebtSettlementRow.RowVersion;
						consumerDebtSettlementAcceptInfos.Add(consumerDebtSettlementAcceptInfo);
					}
				}

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged or might not have a valid match row cell to bold or unbold.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Failed to perform Approve Settlement operation.", Application.Current.MainWindow.Title)));
				return;

			}

			// Send the accept records.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			try
			{								
				tradingSupportClient.AcceptConsumerDebtSettlement(consumerDebtSettlementAcceptInfos.ToArray());								
			}
			catch(FaultException faultException)
			{
				EventLog.Error("{0}, {1}", faultException.Message, faultException.StackTrace);

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, faultException.Message, Application.Current.MainWindow.Title)));
			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged or might not have a valid match row cell to bold or unbold.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Failed to perform Approve Settlement operation.", Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

		}

	}

}

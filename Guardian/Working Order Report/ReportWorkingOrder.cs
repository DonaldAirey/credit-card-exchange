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
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
	/// </summary>
	public class ReportWorkingOrder : DynamicReport
	{

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.IsFilledFilter dependency property.
		/// </summary>
		public static readonly DependencyProperty IsFilledFilterProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.IsRunningFilter dependency property.
		/// </summary>
		public static readonly DependencyProperty IsRunningFilterProperty;

		// Private Static Fields
		private static Duration[] animationDurations;
		private static Dictionary<String, IComparer<Schema.ConsumerTrustWorkingOrder.WorkingOrder>> sortMethods;

		// Private Instance Fields
		private Guid blotterId;
		private List<Guid> blotterList;
		private ComplexComparer<Schema.ConsumerTrustWorkingOrder.WorkingOrder> comparer;
		private ComplexFilter<Schema.ConsumerTrustWorkingOrder.WorkingOrder> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private Boolean isHierarchyChanged;
		private ComplexFilter<WorkingOrderRow> prefilter;
		private Guid reportId;
		private SetBlotterFilterHandler setBlotterFilterHandler;
		private String symbolFilter;

        public bool CurrentSelectionChanged { get; set; }

		// Private Delegates
		private delegate void SourceDelegate(XDocument xDocument);

		// Private Delegates
		private delegate void SetBlotterFilterHandler(List<Guid> blotterList);

		/// <summary>
		/// Create the static resources required for this report.
		/// </summary>
		static ReportWorkingOrder()
		{

			// AnimationSpeed
			ReportWorkingOrder.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ReportWorkingOrder),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// IsFilledFilter
			ReportWorkingOrder.IsFilledFilterProperty = DependencyProperty.Register(
				"IsFilledFilter",
				typeof(Boolean),
				typeof(ReportWorkingOrder),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsFilledFilterChanged)));

			// IsRunningFilter
			ReportWorkingOrder.IsRunningFilterProperty = DependencyProperty.Register(
				"IsRunningFilter",
				typeof(Boolean),
				typeof(ReportWorkingOrder),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsRunningFilterChanged)));

			// These constants control the animation speed.
			ReportWorkingOrder.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

			ReportWorkingOrder.sortMethods = new Dictionary<string, IComparer<Schema.ConsumerTrustWorkingOrder.WorkingOrder>>();
            ReportWorkingOrder.sortMethods.Add("Address1Column", new Schema.ConsumerTrustWorkingOrder.Address1Comparer());
			ReportWorkingOrder.sortMethods.Add("AvailableQuantityColumn", new Schema.ConsumerTrustWorkingOrder.AvailableQuantityComparer());
			ReportWorkingOrder.sortMethods.Add("DestinationOrderQuantityColumn", new Schema.ConsumerTrustWorkingOrder.DestinationOrderQuantityComparer());
            ReportWorkingOrder.sortMethods.Add("CityColumn", new Schema.ConsumerTrustWorkingOrder.CityComparer());
            ReportWorkingOrder.sortMethods.Add("CollectionDateColumn", new Schema.ConsumerTrustWorkingOrder.CollectionDateComparer());
			ReportWorkingOrder.sortMethods.Add("CreditCardIssuerColumn", new Schema.ConsumerTrustWorkingOrder.CreditCardIssuerComparer());
			ReportWorkingOrder.sortMethods.Add("DateOfDelinquencyColumn", new Schema.ConsumerTrustWorkingOrder.DateOfDelinquencyComparer());
			ReportWorkingOrder.sortMethods.Add("ExecutionQuantityColumn", new Schema.ConsumerTrustWorkingOrder.ExecutionQuantityComparer());
            ReportWorkingOrder.sortMethods.Add("FirstNameColumn", new Schema.ConsumerTrustWorkingOrder.FirstNameComparer());
			ReportWorkingOrder.sortMethods.Add("LastNameColumn", new Schema.ConsumerTrustWorkingOrder.LastNameComparer());            
			ReportWorkingOrder.sortMethods.Add("LeavesQuantityColumn", new Schema.ConsumerTrustWorkingOrder.LeavesQuantityComparer());
			ReportWorkingOrder.sortMethods.Add("MarketValueColumn", new Schema.ConsumerTrustWorkingOrder.MarketValueComparer());
			ReportWorkingOrder.sortMethods.Add("AccountBalanceColumn", new Schema.ConsumerTrustWorkingOrder.AccountBalanceComparer());
			ReportWorkingOrder.sortMethods.Add("ProvinceColumn", new Schema.ConsumerTrustWorkingOrder.ProvinceComparer());
			ReportWorkingOrder.sortMethods.Add("SecurityColumn", new Schema.ConsumerTrustWorkingOrder.SecurityComparer());
			ReportWorkingOrder.sortMethods.Add("SocialSecurityNumberColumn", new Schema.ConsumerTrustWorkingOrder.SocialSecurityNumberComparer());
			ReportWorkingOrder.sortMethods.Add("SourceOrderQuantityColumn", new Schema.ConsumerTrustWorkingOrder.SourceOrderQuantityComparer());            

		}

		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ReportWorkingOrder()
		{

			// All records in the presentation layer of the report require a unique identifier.  When the report is updated, this
			// identifier is used to map the data to an existing record or to create a new one.  The starting point for the report
			// is the header record which uses this identifier.  The rest of the records in the report will generally use the
			// source DataRow as the unique identifier.
			this.guid = Guid.NewGuid();

			this.reportId = Guid.Empty;

			// These objects are required for sorting, filtering and ordering the report.
			this.prefilter = new ComplexFilter<WorkingOrderRow>();
			this.prefilter.Add(this.FilterBlotters);
			this.filter = new ComplexFilter<Schema.ConsumerTrustWorkingOrder.WorkingOrder>();
			this.comparer = new ComplexComparer<Schema.ConsumerTrustWorkingOrder.WorkingOrder>();
			this.comparer.Add(new Schema.ConsumerTrustWorkingOrder.MarketValueComparer(), SortOrder.Descending);

			// This is the list of all the blotters on display in this report.  A single blotter can be displayed or several may be
			// aggregated.  The blotter list is used by the 'prefilter' to determine which WorkingOrder rows from the data model
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
            this.AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(OnTextChanged));
            this.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler(OnTextBoxLostFocus));

			// These handlers take care of installing and uninstalling this window in the data model update events.
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);            

            CommandBinding cb = new CommandBinding(ApplicationCommands.Delete);
            cb.Executed += new ExecutedRoutedEventHandler(cb_Executed);
            this.CommandBindings.Add(cb);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException("Work in Progress");
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
				IComparer<Schema.ConsumerTrustWorkingOrder.WorkingOrder> comparer;
				if (ReportWorkingOrder.sortMethods.TryGetValue(sortItem.Column.ColumnId, out comparer))
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
					DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.WorkingOrder).ReportTypeId);

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
			DataModel.Price.PriceRowChanged += new PriceRowChangeEventHandler(OnPriceRowChanged);
			DataModel.Price.PriceRowDeleted += new PriceRowChangeEventHandler(OnPriceRowChanged);
			DataModel.Entity.EntityRowChanged += new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged += new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.DestinationOrder.DestinationOrderRowChanged += new DestinationOrderRowChangeEventHandler(OnDestinationOrderRowChanged);
			DataModel.DestinationOrder.DestinationOrderRowDeleted += new DestinationOrderRowChangeEventHandler(OnDestinationOrderRowChanged);
			DataModel.Execution.ExecutionRowChanged += new ExecutionRowChangeEventHandler(OnExecutionRowChanged);
			DataModel.Execution.ExecutionRowDeleted += new ExecutionRowChangeEventHandler(OnExecutionRowChanged);
			DataModel.SourceOrder.SourceOrderRowChanged += new SourceOrderRowChangeEventHandler(OnSourceOrderRowChanged);
			DataModel.SourceOrder.SourceOrderRowDeleted += new SourceOrderRowChangeEventHandler(OnSourceOrderRowChanged);
			DataModel.WorkingOrder.WorkingOrderRowChanged += new WorkingOrderRowChangeEventHandler(OnWorkingOrderRowChanged);
			DataModel.WorkingOrder.WorkingOrderRowDeleted += new WorkingOrderRowChangeEventHandler(OnWorkingOrderRowChanged);
            DataModel.Consumer.ConsumerRowChanged += new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
            DataModel.Consumer.ConsumerRowDeleted += new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.EndMerge += new EventHandler(OnEndMerge);

            Refresh();

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
			DataModel.Price.PriceRowChanged -= new PriceRowChangeEventHandler(OnPriceRowChanged);
			DataModel.Price.PriceRowDeleted -= new PriceRowChangeEventHandler(OnPriceRowChanged);
			DataModel.Entity.EntityRowChanged -= new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged -= new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.DestinationOrder.DestinationOrderRowChanged -= new DestinationOrderRowChangeEventHandler(OnDestinationOrderRowChanged);
			DataModel.DestinationOrder.DestinationOrderRowDeleted -= new DestinationOrderRowChangeEventHandler(OnDestinationOrderRowChanged);
			DataModel.Execution.ExecutionRowChanged -= new ExecutionRowChangeEventHandler(OnExecutionRowChanged);
			DataModel.Execution.ExecutionRowDeleted -= new ExecutionRowChangeEventHandler(OnExecutionRowChanged);
			DataModel.SourceOrder.SourceOrderRowChanged -= new SourceOrderRowChangeEventHandler(OnSourceOrderRowChanged);
			DataModel.SourceOrder.SourceOrderRowDeleted -= new SourceOrderRowChangeEventHandler(OnSourceOrderRowChanged);
			DataModel.WorkingOrder.WorkingOrderRowChanged -= new WorkingOrderRowChangeEventHandler(OnWorkingOrderRowChanged);
			DataModel.WorkingOrder.WorkingOrderRowDeleted -= new WorkingOrderRowChangeEventHandler(OnWorkingOrderRowChanged);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);

		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ReportWorkingOrder.AnimationSpeedProperty);}
			set { this.SetValue(ReportWorkingOrder.AnimationSpeedProperty, value); }
		}

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

		public String SymbolFilter
		{
			get { return this.symbolFilter;}
			set
			{

				this.symbolFilter = value;

				if (this.symbolFilter.Length == 0)
				{
					if (this.filter.Contains(this.FilterSymbols))
						this.filter.Remove(this.FilterSymbols);
				}
				else
				{
					if (!this.filter.Contains(this.FilterSymbols))
						this.filter.Add(this.FilterSymbols);
				}

				Refresh();

			}
		}

		public Boolean IsFilledFilter
		{
			get { return (Boolean)this.GetValue(ReportWorkingOrder.IsFilledFilterProperty); }
			set { this.SetValue(ReportWorkingOrder.IsFilledFilterProperty, value); }
		}

		public Boolean IsRunningFilter
		{
			get { return (Boolean)this.GetValue(ReportWorkingOrder.IsRunningFilterProperty); }
			set { this.SetValue(ReportWorkingOrder.IsRunningFilterProperty, value); }
		}

		/// <summary>
		/// Handles a change to the animation speed.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnAnimationSpeedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Set the speed for animation.
			ReportWorkingOrder reportPrototype = dependencyObject as ReportWorkingOrder;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ReportWorkingOrder.animationDurations[(Int32)animationSpeed];

		}

		/// <summary>
		/// Handles a change to the WorkingOrderRow table.
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
				{

					ExpandBlotterRow(blotterList, blotterRow);
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, this.setBlotterFilterHandler, blotterList);

				}

			}
			else
			{

				// The content of the report is regenerated in a worker thread when the data related to this report has changed.  When
				// the content is regenerated, it will be sent to the foreground to be presented in the report.
				if (this.isDataChanged)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(RefreshThread, false);
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
		/// Handles a change to the status of the 'Filled' filter.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnIsFilledFilterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Insall or remove the filter for the filled orders.
			ReportWorkingOrder reportPrototype = dependencyObject as ReportWorkingOrder;
			Boolean isFilledFilter = (Boolean)dependencyPropertyChangedEventArgs.NewValue;
			if (isFilledFilter)
			{
				if (!reportPrototype.filter.Contains(reportPrototype.FilterFilledOrders))
					reportPrototype.filter.Add(reportPrototype.FilterFilledOrders);
			}
			else
			{
				if (reportPrototype.filter.Contains(reportPrototype.FilterFilledOrders))
					reportPrototype.filter.Remove(reportPrototype.FilterFilledOrders);
			}

			// Once the filter is changed, the report will need to be regenerated.
			reportPrototype.Refresh();

		}

		/// <summary>
		/// Handles a change to the status of the 'Running' filter.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnIsRunningFilterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Insall or remove the filter for the filled orders.
			ReportWorkingOrder reportPrototype = dependencyObject as ReportWorkingOrder;
			Boolean isRunningFilter = (Boolean)dependencyPropertyChangedEventArgs.NewValue;
			if (isRunningFilter)
			{
				if (!reportPrototype.filter.Contains(reportPrototype.FilterRunningOrders))
					reportPrototype.filter.Add(reportPrototype.FilterRunningOrders);
			}
			else
			{
				if (reportPrototype.filter.Contains(reportPrototype.FilterRunningOrders))
					reportPrototype.filter.Remove(reportPrototype.FilterRunningOrders);
			}

			// Once the filter is changed, the report will need to be regenerated.
			reportPrototype.Refresh();

		}

		/// <summary>
		/// Handles a change to the WorkingOrderRow table.
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

					Guid selectedValue = (Guid)comboBox.SelectedValue;

					// At this point, a ComboBox was modified by the user and it is connected to a data model field.  This will extract
					// the coordinates of the field in the table.  That, in turn, drives the decision about how to update the shared
					// data model.
					DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
					WorkingOrderRow workingOrderRow = dataTableCoordiante.DataRow as WorkingOrderRow;                    

					// This will update changes make to the Time In Force field in a background thread.
					if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.TimeInForceIdColumn)
						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
							TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { TimeInForceId = selectedValue })));
					
					// This will update changes make to the Side field in a background thread.
                    if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.SideIdColumn)
						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
							TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { SideId = selectedValue })));

				}

			}

		}

        private void OnTextChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            TextBox textBox = routedEventArgs.OriginalSource as TextBox;
            ReportWorkingOrder report = sender as ReportWorkingOrder;

            if (textBox == null || report == null)
                return;

            if (InputHelper.IsUserInitiated(textBox, TextBox.TextProperty))
            {
                report.CurrentSelectionChanged = true;
            }
                                
        }

        /// <summary>
        /// Handler for the textbox class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void OnTextBoxLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            TextBox textBox = routedEventArgs.OriginalSource as TextBox;
            ReportWorkingOrder report = sender as ReportWorkingOrder;
            
            if (textBox == null || report == null)
                return;

            IContent iContent = textBox.DataContext as IContent;

            // This filters all the ComboBox events looking for user initiated actions that are bound to the data model.
            if (report.CurrentSelectionChanged == true &&
                iContent != null && iContent.Key is DataTableCoordinate)
            {
                DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
                ConsumerRow workingOrderRow = dataTableCoordiante.DataRow as ConsumerRow;

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.SocialSecurityNumberColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer( new Consumer(workingOrderRow) { SocialSecurityNumber = textBox.Text })));
				
                if (dataTableCoordiante.DataColumn == DataModel.Consumer.FirstNameColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { FirstName = textBox.Text })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.LastNameColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { LastName = textBox.Text })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.Address1Column)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { Address1 = textBox.Text })));


                if (dataTableCoordiante.DataColumn == DataModel.Consumer.Address2Column)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { Address2 = textBox.Text })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.CityColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { City = textBox.Text })));

                
                report.CurrentSelectionChanged = false;
            }

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Delete)
            {
                if (TryToDeleteRows() == true)
                {
                    e.Handled = true;
                }
                             
            }
        }

        private bool TryToDeleteRows()
        {
            List<List<FluidTrade.Core.Windows.Controls.ReportRow>> selectedRowBlocks = reportGrid.SelectedRowBlocks;
            List<WorkingOrderRow> toDeleteRows = new List<WorkingOrderRow>();

            //Iterate over collections of selected items
            foreach (var selectedRows in selectedRowBlocks)
            {
				foreach (FluidTrade.Core.Windows.Controls.ReportRow row in selectedRows)
                {
                    WorkingOrderRow workingOrderRow = GetandValidateWorkingRow(row);
                    if (workingOrderRow != null)
                    {
                        toDeleteRows.Add(workingOrderRow);
                    }
                }
            }

            if (toDeleteRows.Count > 0)
            {
                FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(DestroyRecords, toDeleteRows);
                return true;
            }

            return false;
        }

		private WorkingOrderRow GetandValidateWorkingRow(FluidTrade.Core.Windows.Controls.ReportRow row)
        {
            WorkingOrderRow workingOrderRow = row.IContent.Key as WorkingOrderRow;

            try
            {
                if (workingOrderRow != null)
                {
                    if(workingOrderRow.WorkingOrderId != null)
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
				WorkingOrderRow workingOrderRow = dataTableCoordiante.DataRow as WorkingOrderRow;

				// Update the Crossing column.
                if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.CrossingIdColumn)
                    UpdateCrossing(workingOrderRow, toggleButton.IsChecked);
                    
				// Update the IsInstitutionMatch column.
				if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.IsInstitutionMatchColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { IsInstitutionMatch = toggleButton.IsChecked })));

				// Update the IsBrokerMatch column.
				if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.IsBrokerMatchColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { IsBrokerMatch = toggleButton.IsChecked })));


				// Update the IsHedgeMatch column.
				if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.IsHedgeMatchColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { IsHedgeFundMatch = toggleButton.IsChecked })));



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
		/// Handles a change to the WorkingOrderRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnWorkingOrderRowChanged(object sender, WorkingOrderRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

        /// <summary>
        /// Handles a change to the ConsumerRowC table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConsumerRowChanged(object sender, ConsumerRowChangeEventArgs e)
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

			ThreadPoolHelper.QueueUserWorkItem(RefreshThread, true);

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
				this.SetContent(this.WorkingOrderHeaderSelector(this.guid), requestGC);

			ThreadPoolHelper.QueueUserWorkItem(RefreshThreadCompleted);

		}

		private void RefreshThreadCompleted(object state)
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(false);

		}


		private Schema.ConsumerTrustWorkingOrderHeader.WorkingOrderHeader WorkingOrderHeaderSelector(Guid guid)
		{
			Schema.ConsumerTrustWorkingOrderHeader.WorkingOrderHeader workingOrderHeader = new Schema.ConsumerTrustWorkingOrderHeader.WorkingOrderHeader();
			workingOrderHeader.Prefilter = this.prefilter;
			workingOrderHeader.Selector = WorkingOrderSelector;
			workingOrderHeader.Filter = this.filter;
			workingOrderHeader.Comparer = this.comparer;
			return workingOrderHeader.Select(guid);
		}
		
		private Schema.ConsumerTrustWorkingOrder.WorkingOrder WorkingOrderSelector(WorkingOrderRow workingOrderRow)
		{
			Schema.ConsumerTrustWorkingOrder.WorkingOrder workingOrder = new Schema.ConsumerTrustWorkingOrder.WorkingOrder();
			return workingOrder.Select(workingOrderRow);
		}
        		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		private void UpdateField(object state) 
        {
			Func<MethodResponseErrorCode> orderClient = state as Func<MethodResponseErrorCode>;

			try
			{
				// Call the handler to update the working order record.
				MethodResponseErrorCode returnCode = orderClient();

			}
			catch (FaultException<OptimisticConcurrencyFault> optimisticConcurrencyException)
			{

				// The record is busy.
				this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
					delegate()
					{
						AlertMessageBox.Instance.Show(optimisticConcurrencyException);
					}
				));

			}
			catch (FaultException<RecordNotFoundFault> recordNotFoundException)
			{

				// The record is busy.
				this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
					delegate()
					{
						AlertMessageBox.Instance.Show(recordNotFoundException);
					}
				));

			}
			catch (CommunicationException)
			{

				// Log communication problems.
				this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
					delegate()
					{
						AlertMessageBox.Instance.Show(AlertMessageBoxType.LostConnectionToServer);
					}
				));

			}
			catch (Exception exception)
			{

				// Log communication problems.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

			}


            
        }
		
		/// <summary>
		/// Update the Crossing field.
		/// </summary>
		/// <param name="dataModelClient">The client channel to the shared data model.</param>
		/// <param name="workingOrderRow">The record that is to be updated.</param>
		/// <param name="value">The value of the IsInstutitionMatch field.</param>
        private void UpdateCrossing(WorkingOrderRow workingOrderRow, object newValue)
		{
			
			// Update the Crossing field.
            Crossing crossing = (Boolean)newValue ? Crossing.AlwaysMatch : Crossing.NeverMatch;
			Object submittedTime = crossing == Crossing.AlwaysMatch ? (Object)DateTime.UtcNow : (Object)null;
            CrossingRow crossingRow = DataModel.Crossing.CrossingKeyCrossingCode.Find(crossing);

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
				TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { CrossingCode = crossingRow.CrossingId, SubmittedUTCTime = submittedTime })));

		}

		
		
		private bool FilterBlotters(WorkingOrderRow workingOrderRow)
		{
			return this.blotterList.BinarySearch(workingOrderRow.BlotterId) >= 0;

		}
	
		private bool FilterRunningOrders(Schema.ConsumerTrustWorkingOrder.WorkingOrder workingOrder)
		{

			return workingOrder.Status.StatusCode != Status.Filled &&
                workingOrder.Status.StatusCode != Status.Canceled &&
				workingOrder.Status.StatusCode != Status.Closed &&
                workingOrder.Status.StatusCode != Status.Suspended;

		}

		private bool FilterFilledOrders(Schema.ConsumerTrustWorkingOrder.WorkingOrder workingOrder)
		{

			return workingOrder.Status.StatusCode == Status.Filled;

		}

		private bool FilterSymbols(Schema.ConsumerTrustWorkingOrder.WorkingOrder workingOrder)
		{

			return workingOrder.SecuritySymbol.Symbol.StartsWith(this.symbolFilter);

		}

		public override string ToString()
		{
			return this.guid.ToString();
		}

        public void DestroyRecords(object state)
        {
            List<WorkingOrderRow> toDeleteRows = state as List<WorkingOrderRow>;

            DataModelClient dataModelClient = new DataModelClient(Guardian.Properties.Settings.Default.DataModelEndpoint);
            try
            {
                foreach (WorkingOrderRow row in toDeleteRows)
                {
                    dataModelClient.DestroyWorkingOrder(row.RowVersion, new object[] { row.WorkingOrderId });
                }
            }
            finally
            {
                if (dataModelClient != null)
                    dataModelClient.Close();
            }
        }

  	}

}

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
	using FluidTrade.Guardian.Windows;

	/// <summary>
	/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
	/// </summary>
	public class ReportDebtHolderWorkingOrder : DynamicReport
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
        private static Dictionary<String, IComparer<Schema.DebtHolderWorkingOrder.WorkingOrder>> sortMethods;

		// Private Instance Fields
		private Guid blotterId;
		private List<Guid> blotterList;
        private ComplexComparer<Schema.DebtHolderWorkingOrder.WorkingOrder> comparer;
        private ComplexFilter<Schema.DebtHolderWorkingOrder.WorkingOrder> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private Boolean isHierarchyChanged;
		private ComplexFilter<WorkingOrderRow> prefilter;
		private Guid reportId;
		private String postalCodeFilter;
        private Decimal balanceFilter;

        /// <summary>
        /// Current selection changed property.
        /// </summary>
        public bool CurrentSelectionChanged { get; set; }

		// Private Delegates
		private delegate void SourceDelegate(XDocument xDocument);

		// Private Delegates
		private delegate void SetBlotterFilterHandler(List<Guid> blotterList);

		/// <summary>
		/// Create the static resources required for this report.
		/// </summary>
		static ReportDebtHolderWorkingOrder()
		{

			// AnimationSpeed
			ReportDebtHolderWorkingOrder.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ReportDebtHolderWorkingOrder),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// These constants control the animation speed.
			ReportDebtHolderWorkingOrder.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

            ReportDebtHolderWorkingOrder.sortMethods = new Dictionary<string, IComparer<Schema.DebtHolderWorkingOrder.WorkingOrder>>();
            ReportDebtHolderWorkingOrder.sortMethods.Add("AccountBalanceColumn", new Schema.DebtHolderWorkingOrder.AccountBalanceComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("AccountNumberColumn", new Schema.DebtHolderWorkingOrder.AccountNumberComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("Address1Column", new Schema.DebtHolderWorkingOrder.Address1Comparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("Address2Column", new Schema.DebtHolderWorkingOrder.Address2Comparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("CityColumn", new Schema.DebtHolderWorkingOrder.CityComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("CreatedDateTimeColumn", new Schema.DebtHolderWorkingOrder.CreatedDateTimeComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("DateOfBirthColumn", new Schema.DebtHolderWorkingOrder.DateOfBirthComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("DateOfDelinquencyColumn", new Schema.DebtHolderWorkingOrder.DateOfDelinquencyComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("DebtClassColumn", new Schema.DebtHolderWorkingOrder.DebtClassComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("FirstNameColumn", new Schema.DebtHolderWorkingOrder.FirstNameComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("HeatIndexColumn", new Schema.DebtHolderWorkingOrder.HeatIndexComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("IsEmployedColumn", new Schema.DebtHolderWorkingOrder.IsEmployedComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("DebtHolderNameColumn", new Schema.DebtHolderWorkingOrder.DebtHolderNameComparer()); 
			ReportDebtHolderWorkingOrder.sortMethods.Add("LastNameColumn", new Schema.DebtHolderWorkingOrder.LastNameComparer());   
			ReportDebtHolderWorkingOrder.sortMethods.Add("PostalCodeColumn", new Schema.DebtHolderWorkingOrder.PostalCodeComparer());  
			ReportDebtHolderWorkingOrder.sortMethods.Add("MiddleNameColumn", new Schema.DebtHolderWorkingOrder.MiddleNameComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("ModifiedDateTimeColumn", new Schema.DebtHolderWorkingOrder.ModifiedDateTimeComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("ProvinceColumn", new Schema.DebtHolderWorkingOrder.ProvinceComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("RuleIdColumn", new Schema.DebtHolderWorkingOrder.RuleIdComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("SalutationColumn", new Schema.DebtHolderWorkingOrder.SalutationComparer());
            ReportDebtHolderWorkingOrder.sortMethods.Add("SecurityNameColumn", new Schema.DebtHolderWorkingOrder.SecurityNameComparer()); // aka Original Account Number	
			ReportDebtHolderWorkingOrder.sortMethods.Add("SocialSecurityNumberColumn", new Schema.DebtHolderWorkingOrder.SocialSecurityNumberComparer());	
			ReportDebtHolderWorkingOrder.sortMethods.Add("StatusColumn", new Schema.DebtHolderWorkingOrder.StatusComparer());
			ReportDebtHolderWorkingOrder.sortMethods.Add("SuffixColumn", new Schema.DebtHolderWorkingOrder.SuffixComparer());

		}


		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ReportDebtHolderWorkingOrder()
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
            this.filter = new ComplexFilter<Schema.DebtHolderWorkingOrder.WorkingOrder>();
			this.comparer = new ComplexComparer<Schema.DebtHolderWorkingOrder.WorkingOrder>();
			this.comparer.Add(new Schema.DebtHolderWorkingOrder.HeatIndexComparer(), SortOrder.Ascending);

			// This is the list of all the blotters on display in this report.  A single blotter can be displayed or several may be
			// aggregated.  The blotter list is used by the 'prefilter' to determine which WorkingOrder rows from the data model
			// should be transformed into the presentation layer objects.
			this.blotterList = new List<Guid>();

			// This is needed to satisfy the compiler.  In practice, this value is loaded from the user settings and defaulted
			// through the same mechanism.
			this.AnimationSpeed = AnimationSpeed.Off;

			this.CommandBindings.Add(new CommandBinding(FluidTradeCommands.SortReport, OnSortReport));

			// These handlers will update the middle tier in response to changes in the report.
			this.AddHandler(FluidTrade.Actipro.DateTimePicker.DateTimeChangedEvent, new RoutedEventHandler(OnDateTimePicker)); 
			this.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnToggleButtonChange));
			this.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnToggleButtonChange));
			this.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(OnSelectorSelectionChanged));
            this.AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(OnTextChanged));
            this.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler(OnTextBoxLostFocus));
			this.AddHandler(ReportGrid.ShowToolTipEvent, new ReportGridtToolTipEventHandler(OnShowToolTipHandler));

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
                IComparer<Schema.DebtHolderWorkingOrder.WorkingOrder> comparer;
				if (ReportDebtHolderWorkingOrder.sortMethods.TryGetValue(sortEventArgs.Items[0].Column.ColumnId, out comparer))
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
						else if (reportRow == null)
						{

							EventLog.Error("Unable to find report {0} row for debt holder {1}", blotterConfigurationRow.ReportId, this.blotterId);

						}

					}
					else if (blotterConfigurationRow == null)
					{

						EventLog.Error("Unable to find configuration {0} row for debt holder {1}", ReportType.WorkingOrder, this.blotterId);

					}

				}
										
			}
			catch(Exception exception)
			{

				EventLog.Warning(String.Format("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace));

			}

		}

		/// <summary>
		/// Handles the loading of this control.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// These events will keep the report updated with live content once the report data model is compiled.
			DataModel.Blotter.BlotterRowChanged += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Blotter.BlotterRowDeleted += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.BlotterConfiguration.BlotterConfigurationRowChanged += new BlotterConfigurationRowChangeEventHandler(OnBlotterConfigurationRowChanged);
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
            DataModel.CreditCard.CreditCardRowChanged += new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
            DataModel.CreditCard.CreditCardRowDeleted += new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
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
            DataModel.Consumer.ConsumerRowChanged -= new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
            DataModel.Consumer.ConsumerRowDeleted -= new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
            DataModel.CreditCard.CreditCardRowChanged -= new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
            DataModel.CreditCard.CreditCardRowDeleted -= new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);
		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ReportDebtHolderWorkingOrder.AnimationSpeedProperty);}
			set { this.SetValue(ReportDebtHolderWorkingOrder.AnimationSpeedProperty, value); }
		}

        /// <summary>
        /// Blotter Id
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
		/// Expands the given blotter to include all of the child entities and recalculates the items shown in this report.
		/// </summary>
		/// <param name="state">The generic thread start parameter.</param>
		private void ExpandBlotterId(Object state)
		{

			// Extract the blotter identifier from the generic arguments.  This is the id of the record that is to be displayed in this report.
			Guid blotterId = (Guid)state;

			// Lock the data model while the identifiers of all of the children are extracted from the given blotter id.
			lock (DataModel.SyncRoot)
			{
				BlotterRow blotterRow = DataModel.Blotter.BlotterKey.Find(blotterId);
				if (blotterRow != null)
				{
					this.blotterList.Clear();
					ExpandBlotterRow(this.blotterList, blotterRow);
				}
			}

			// Redraw the document with the new universe of blotters.
			RefreshThread(true);

		}

		/// <summary>
		/// Recursively extract an ordered list of all the blotter ids that belong to a given BlotterRow record.
		/// </summary>
		/// <param name="blotterList">A list that is to be populated with the child blotter identifiers.</param>
		/// <param name="blotterRow">The current BlotterRow record in the recursion.</param>
		private void ExpandBlotterRow(List<Guid> blotterList, BlotterRow blotterRow)
		{

			// Add the parent record into the list in the proper order.
			int index = this.blotterList.BinarySearch(blotterRow.EntityRow.EntityId);
			if (index < 0)
			{
				this.blotterList.Insert(~index, blotterRow.BlotterId);
			}

			// Recurse into each of the child relations of this record looking for the identifiers.
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
        /// PostalCode Filter property.
        /// </summary>
        public String PostalCodeFilter
		{
			get { return this.postalCodeFilter;}
			set
			{

				this.postalCodeFilter = value;

				if (this.postalCodeFilter.Length == 0)
				{
					if (this.filter.Contains(this.FilterPostalCode))
						this.filter.Remove(this.FilterPostalCode);
				}
				else
				{
					if (!this.filter.Contains(this.FilterPostalCode))
						this.filter.Add(this.FilterPostalCode);
				}

				Refresh();

			}
		}

        public Decimal BalanceFilter
        {
            get { return this.balanceFilter; }
            set
            {

                this.balanceFilter = value;

                if (this.balanceFilter == 0)
                {
                    if (this.filter.Contains(this.FilterBalance))
                        this.filter.Remove(this.FilterBalance);
                }
                else
                {
                    if (!this.filter.Contains(this.FilterBalance))
                        this.filter.Add(this.FilterBalance);
                }

                Refresh();

            }
        }

        /// <summary>
        /// Is filled filter property.
        /// </summary>
		public Boolean IsFilledFilter
		{
			get { return (Boolean)this.GetValue(ReportDebtHolderWorkingOrder.IsFilledFilterProperty); }
			set { this.SetValue(ReportDebtHolderWorkingOrder.IsFilledFilterProperty, value); }
		}

        /// <summary>
        /// Is Filter running filter.
        /// </summary>
		public Boolean IsRunningFilter
		{
			get { return (Boolean)this.GetValue(ReportDebtHolderWorkingOrder.IsRunningFilterProperty); }
			set { this.SetValue(ReportDebtHolderWorkingOrder.IsRunningFilterProperty, value); }
		}

		/// <summary>
		/// Handles a change to the animation speed.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnAnimationSpeedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Set the speed for animation.
			ReportDebtHolderWorkingOrder reportPrototype = dependencyObject as ReportDebtHolderWorkingOrder;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ReportDebtHolderWorkingOrder.animationDurations[(Int32)animationSpeed];

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

			// If the hierarchy was changed then the list of blotters visible in this report needs to be evaluated and passed into the filter.  Note that
			// changing the hierarchy will also force a refresh of the report in the foreground giving it a higher precendence than a normal data update.  Said
			// differently, if the hierarchy has changed, the report will be refreshed when the new filter is installed and there is no reason to call the
			// refresh thread from here.
			if (this.isHierarchyChanged)
			{

				// This is a toggle.  Don't expand the hierarchy again again until the data model changes this field.
				this.isHierarchyChanged = false;

				// This will recreate the list of blotters allowed into this report.  Note that the filter must be set in the foreground once the hierarchy is
				// expanded.
				List<Guid> blotterList = new List<Guid>();
				BlotterRow blotterRow = DataModel.Blotter.BlotterKey.Find(this.blotterId);
				if (blotterRow != null)
				{
					ExpandBlotterRow(this.blotterList, blotterRow);
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(RefreshThread, false);
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

					// At this point, a ComboBox was modified by the user and it is connected to a data model field.  This will extract
					// the coordinates of the field in the table.  That, in turn, drives the decision about how to update the shared
					// data model.
					DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
					WorkingOrderRow workingOrderRow = dataTableCoordiante.DataRow as WorkingOrderRow;

					// Bailing out if the Selected Value is null.
					if (comboBox.SelectedValue == null)
						return;
                    
                    if (dataTableCoordiante.DataColumn == DataModel.ConsumerDebt.DebtRuleIdColumn)
                    {
                        object newDebtRuleId = comboBox.SelectedValue;
						
                        if ((Guid)newDebtRuleId == Guid.Empty)
                            newDebtRuleId = DBNull.Value;
						
						ConsumerDebtRow debtRow = dataTableCoordiante.DataRow as  ConsumerDebtRow;

						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
							TradingSupportWebService.UpdateConsumerDebt(new ConsumerDebt(debtRow) { DebtRuleId = newDebtRuleId })));

						return;
                    }

                    if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.BlotterIdColumn)
					{
						Guid localBlotterId = (Guid)comboBox.SelectedValue;
						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
                            TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { BlotterId = localBlotterId })));

						return;
					}

					if (dataTableCoordiante.DataColumn == DataModel.Consumer.ProvinceIdColumn)
					{
						object newprovinceId = comboBox.SelectedValue;
						if ((Guid)newprovinceId == Guid.Empty)
							newprovinceId = DBNull.Value;


						ConsumerRow consumerRow = dataTableCoordiante.DataRow as ConsumerRow;

						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
							TradingSupportWebService.UpdateConsumer(new TradingSupportReference.Consumer(consumerRow) { ProvinceId = (Guid)newprovinceId })));

						return;
					}

				}

			}

		}

        private void OnTextChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            TextBox textBox = routedEventArgs.OriginalSource as TextBox;
            ReportDebtHolderWorkingOrder report = sender as ReportDebtHolderWorkingOrder;

            if (textBox == null || report == null)
                return;

            if (InputHelper.IsUserInitiated(textBox, TextBox.TextProperty))
            {
                report.CurrentSelectionChanged = true;
            }
                                
        }

		private FluidTrade.Guardian.Windows.Controls.MatchPartsUserControl matchToolTipContent;
		/// <summary>
		/// Handle the grid requesting tooltip content
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnShowToolTipHandler(object sender, ReportGridtToolTipEventArgs e)
		{
			if(e.ReportCell.ReportColumn.ColumnId == "HeatIndexColumn")
			{
				if(this.matchToolTipContent == null)
					this.matchToolTipContent = new FluidTrade.Guardian.Windows.Controls.MatchPartsUserControl();

				IContent iContent = e.ReportCell.ReportRow.IContent;
				WorkingOrderRow workingOrderRow = iContent.Key as WorkingOrderRow;
				if(workingOrderRow != null)
				{
					MatchRow[] matchRowAr = workingOrderRow.GetMatchRows();
					if(matchRowAr.Length != 0 &&
						matchRowAr[0].IsHeatIndexDetailsNull() == false)
					{
						this.matchToolTipContent.SetDetails(matchRowAr[0].HeatIndexDetails);
						e.ToolTip.Content = this.matchToolTipContent;
					}
				}
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
            ReportDebtHolderWorkingOrder report = sender as ReportDebtHolderWorkingOrder;
            
            if (textBox == null || report == null)
                return;

            IContent iContent = textBox.DataContext as IContent;

            // This filters all the ComboBox events looking for user initiated actions that are bound to the data model.
            if (report.CurrentSelectionChanged == true &&
                iContent != null && iContent.Key is DataTableCoordinate)
            {
                DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
                ConsumerRow workingOrderRow = dataTableCoordiante.DataRow as ConsumerRow;
                string textBoxString = textBox.Text;
				Guid workingOrderId = dataTableCoordiante.Association;

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.SocialSecurityNumberColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { SocialSecurityNumber = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.FirstNameColumn)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { FirstName = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.LastNameColumn)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { LastName = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.Address1Column)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { Address1 = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.Address2Column)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { Address2 = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.CityColumn)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { City = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.PostalCodeColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { PostalCode = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.MiddleNameColumn)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { MiddleName = textBoxString, WorkingOrderId = workingOrderId })));
						
                if (dataTableCoordiante.DataColumn == DataModel.CreditCard.AccountBalanceColumn)
				{
                    CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow){ AccountBalance = textBoxString})));
				}

				if(dataTableCoordiante.DataColumn == DataModel.CreditCard.OriginalAccountNumberColumn)
				{
					CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { OriginalAccountNumber = textBoxString })));
				}

				if(dataTableCoordiante.DataColumn == DataModel.ConsumerTrust.SavingsBalanceColumn)
				{
					ConsumerTrustRow consumerTrustRow = dataTableCoordiante.DataRow as ConsumerTrustRow;
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						 TradingSupportWebService.UpdateConsumerTrust(new ConsumerTrust(consumerTrustRow) { SavingsBalance = textBoxString })));
				}



        if (dataTableCoordiante.DataColumn == DataModel.Security.SymbolColumn)
				{
                    SecurityRow securityRow = dataTableCoordiante.DataRow as SecurityRow;
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateSecurity(new Security(securityRow) {Symbol = textBoxString})));
				}

                if (dataTableCoordiante.DataColumn == DataModel.Entity.NameColumn)
				{
                    EntityRow entityRow = dataTableCoordiante.DataRow as EntityRow;
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateEntity(new TradingSupportReference.Entity(entityRow) { BlotterId = this.BlotterId,  Name = textBoxString})));
				}

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.SuffixColumn)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { Suffix = textBoxString, WorkingOrderId = workingOrderId })));

                if (dataTableCoordiante.DataColumn == DataModel.Consumer.SalutationColumn)
                    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { Salutation = textBoxString, WorkingOrderId = workingOrderId })));

                report.CurrentSelectionChanged = false;
            }

        }

		/// <summary>
		/// Using OnKeyDown to allow deeper level cell to handle the event first (using OnPreview* method).
		/// Do not use OnPreviewKeyDown to handle the delete event because then the FocusableTextBox will not get the event first.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				if ((this.FocusedCell != null) && (this.FocusedCell.Content is FluidTrade.Guardian.Schema.DebtHolderWorkingOrder.SelectRow))
				{
					// This will work but we can use Application Commands as below.
					//DeleteRows();
					// Discussion: Change over to Application Commands instead of direct calling method?
					FrameworkElement frameworkElement = e.Source as FrameworkElement;
					ApplicationCommands.Delete.Execute(null, frameworkElement);
				}
			}

			// The base class handles any keys not handled above.
			base.OnPreviewKeyDown(e);
		}

		/// <summary>
		/// Queue up selected working order rows to be deleted.
		/// </summary>
        public void DeleteRows()
        {
            List<WorkingOrderRow> toDeleteRows = this.GetSelectedRows();

			if (toDeleteRows.Count > 0)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(DestroyRecords, toDeleteRows);

        }

		/// <summary>
		/// Retrieeve a list of the currently selected working orders.
		/// </summary>
		/// <returns>The list of working order rows.</returns>
		private List<WorkingOrderRow> GetSelectedRows()
		{

			List<List<Core.Windows.Controls.ReportRow>> selectedRowBlocks = this.reportGrid.SelectedRowHeaderBlocks;
            List<WorkingOrderRow> rows = new List<WorkingOrderRow>();

			if (selectedRowBlocks.Count < 1)
			{

				if (this.reportGrid.CurrentReportCell != null)
				{

					Core.Windows.Controls.ReportRow row = reportGrid.CurrentReportCell.ReportRow;
					WorkingOrderRow workingOrderRow = this.GetandValidateWorkingRow(row);

					if (workingOrderRow != null)
						rows.Add(workingOrderRow);

				}

			}
			else
			{

				//Iterate over collections of selected items
				foreach (List<Core.Windows.Controls.ReportRow> selectedRows in selectedRowBlocks)
					foreach (Core.Windows.Controls.ReportRow row in selectedRows)
					{

						WorkingOrderRow workingOrderRow = this.GetandValidateWorkingRow(row);
						if (workingOrderRow != null)
							rows.Add(workingOrderRow);

					}

			}

			return rows;

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
		/// Move a list of working orders.
		/// </summary>
		/// <param name="state">The list to move.</param>
		public void MoveRecords(List<WorkingOrderRow> toMoveRows, GuardianObject folder)
        {
			List<IMovableObject> rows = new List<IMovableObject>();

			Dispatcher.BeginInvoke(new Action(() => this.Cursor = Cursors.AppStarting), DispatcherPriority.Normal);

			//Construct the IMovable list to pass to the WindowMoveProgress
			lock (DataModel.SyncRoot)
				foreach (WorkingOrderRow row in toMoveRows)
					if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached)
						rows.Add(WorkingOrder.New(row));

			//Need to initialize  it on the UI thread.
			this.Dispatcher.BeginInvoke(new Action(() => {
				WindowMoveProgress dialog = new WindowMoveProgress();
				dialog.MoveList = rows;
				dialog.Target = folder;
				dialog.Show();
			}), DispatcherPriority.Normal);

			Dispatcher.BeginInvoke(new Action(() => this.Cursor = Cursors.Arrow), DispatcherPriority.Normal);


		}

		
		/// <summary>
		/// Queue up selected working order rows to be moved.
		/// </summary>
        public void MoveRows()
        {

			List<WorkingOrderRow> toMoveRows = this.GetSelectedRows();
			if (toMoveRows.Count > 0)
			{
				try
				{
					WindowMove moveWindow = new WindowMove();
					moveWindow.Owner = Application.Current.MainWindow;
					moveWindow.ShowDialog();

					if (moveWindow.DialogResult.GetValueOrDefault() == true)
					{
						if (moveWindow.Folder != null)
						{
							//Create a local copy of the DependencyObject.
							GuardianObject folder = moveWindow.Folder;
							ThreadPoolHelper.QueueUserWorkItem(data => MoveRecords(toMoveRows, folder));

						}
					}
				}
				catch(Exception ex)
				{
					EventLog.Error(ex);
				}
			}
        }
		/// <summary>
		/// Handle sending data down to the server for a date time picker changed event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="routedEventArgs"></param>
		private void OnDateTimePicker(object sender, RoutedEventArgs routedEventArgs)
		{
			FluidTrade.Actipro.DateTimePicker originalSourceDateTimePicker = routedEventArgs.OriginalSource as FluidTrade.Actipro.DateTimePicker;
			if (originalSourceDateTimePicker != null)
			{
				IContent iContent = originalSourceDateTimePicker.DataContext as IContent;
				FluidTrade.Actipro.DateTimePicker sourceDateTimePicker = routedEventArgs.Source as FluidTrade.Actipro.DateTimePicker;

				if (InputHelper.IsUserInitiated(originalSourceDateTimePicker, FluidTrade.Actipro.DateTimePicker.DateTimeProperty) &&
					(sourceDateTimePicker != null) &&
					(iContent != null) &&
					(iContent.Key is DataTableCoordinate))
				{
					// At this point, a ComboBox was modified by the user and it is connected to a data model field.  This will 
					// extract the coordinates of the field in the table.  That, in turn, drives the decision about how to update the
					// shared data model.
					DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;


					// Update the Date of Birth column.
					if (dataTableCoordiante.DataColumn == DataModel.Consumer.DateOfBirthColumn)
					{
						ConsumerRow consumerRow = dataTableCoordiante.DataRow as ConsumerRow;
						object selectedItem = sourceDateTimePicker.DateTime == null ? DBNull.Value : (object)sourceDateTimePicker.DateTime.Value;

						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
							TradingSupportWebService.UpdateConsumer(new Consumer(consumerRow) { DateOfBirth = selectedItem })));
					}

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
				WorkingOrderRow workingOrderRow = dataTableCoordiante.DataRow as WorkingOrderRow;
				ConsumerRow reportRow = dataTableCoordiante.DataRow as ConsumerRow;
                bool toggleButtonState = toggleButton.IsChecked.GetValueOrDefault();

				// Update the Crossing column.
                if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.CrossingIdColumn)
                    UpdateCrossing(workingOrderRow, toggleButtonState);
                    
				// Update the IsInstitutionMatch column.
				if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.IsInstitutionMatchColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { IsInstitutionMatch = toggleButtonState } )));

				// Update the IsBrokerMatch column.
				if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.IsBrokerMatchColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { IsBrokerMatch = toggleButtonState } )));


				// Update the IsHedgeMatch column.
				if (dataTableCoordiante.DataColumn == DataModel.WorkingOrder.IsHedgeMatchColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateWorkingOrder(new WorkingOrderRecord(workingOrderRow) { IsHedgeFundMatch = toggleButtonState } )));
                
				// Update the IsEmployed column.
				if (dataTableCoordiante.DataColumn == DataModel.Consumer.IsEmployedColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { IsEmployed = toggleButtonState })));


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
        /// Handles a change to the CreditCard table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreditCardRowChanged(object sender, CreditCardRowChangeEventArgs e)
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

        private Schema.DebtHolderWorkingOrderHeader.WorkingOrderHeader WorkingOrderHeaderSelector(Guid guid)
		{
            Schema.DebtHolderWorkingOrderHeader.WorkingOrderHeader workingOrderHeader = new Schema.DebtHolderWorkingOrderHeader.WorkingOrderHeader();
			workingOrderHeader.Prefilter = this.prefilter;
			workingOrderHeader.Selector = WorkingOrderSelector;
			workingOrderHeader.Filter = this.filter;
			workingOrderHeader.Comparer = this.comparer;
			return workingOrderHeader.Select(guid);
		}

        private Schema.DebtHolderWorkingOrder.WorkingOrder WorkingOrderSelector(WorkingOrderRow workingOrderRow)
		{

			try
			{

				Schema.DebtHolderWorkingOrder.WorkingOrder workingOrder = new Schema.DebtHolderWorkingOrder.WorkingOrder();
				return workingOrder.Select(workingOrderRow);

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				return new Schema.DebtHolderWorkingOrder.WorkingOrder();

			}

		}

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

        private bool FilterPostalCode(Schema.DebtHolderWorkingOrder.WorkingOrder workingOrder)
		{
            String locationCode = workingOrder.PostalCode.Code.ToString();
            if(String.IsNullOrEmpty(locationCode))
                return false;
            else
                return locationCode.StartsWith(this.postalCodeFilter);

		}

        private bool FilterBalance(Schema.DebtHolderWorkingOrder.WorkingOrder workingOrder)
        {
            Decimal accountBalance = 0;           
            if (workingOrder.AccountBalance.Balance != null)
				accountBalance = (Decimal)workingOrder.AccountBalance.Balance;

            return accountBalance > this.balanceFilter;
        }

		/// <summary>
		/// Delete a list of working orders.
		/// </summary>
		/// <param name="state">The list to delete.</param>
        public void DestroyRecords(object state)
        {
			List<WorkingOrderRow> toDeleteRows = state as List<WorkingOrderRow>;
			List<GuardianObject> rows = new List<GuardianObject>();

			lock (DataModel.SyncRoot)
				foreach (WorkingOrderRow row in toDeleteRows)
					if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached)
						rows.Add(WorkingOrder.New(row));

			this.Dispatcher.BeginInvoke(new Action(() => this.DestroyRecords(rows)), DispatcherPriority.Normal);

		}

		/// <summary>
		/// Hand off records to be destroy to the delete window.
		/// </summary>
		/// <param name="rows">The records to be destroyed.</param>
		private void DestroyRecords(List<GuardianObject> rows)
		{

			Window deleteWindow;

			if (rows.Count == 0)
				return;
			else if (rows.Count == 1)
				deleteWindow = new WindowDeleteSingle() { Entity = rows[0] };
			else
				deleteWindow = new WindowDeleteMultiple() { DeleteList = rows };

			deleteWindow.Show();

		}
  	}

}



//////FOR FUTURE USE... PLEASE DO NOT DELETE
//ReportDebtHolderWorkingOrder.sortMethods = new Dictionary<string, ValueComparerParameters>();
//ReportDebtHolderWorkingOrder.sortMethods.Add("Address1Column", new ValueComparerParameters("Address1"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("Address2Column", new ValueComparerParameters("Address2"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("AvailableQuantityColumn", new ValueComparerParameters("AvailableQuantity", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("DestinationOrderQuantityColumn", new ValueComparerParameters("DestinationOrderQuantity", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("DataArchiveIdColumn", new ValueComparerParameters("DataArchiveId", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("CityColumn", new ValueComparerParameters("City"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("CollectionDateColumn", new ValueComparerParameters("CollectionDate", ValueComparerParameters.SortType.DateTime));
//ReportDebtHolderWorkingOrder.sortMethods.Add("CreditCardIssuerColumn", new ValueComparerParameters("CreditCardIssuer"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("DateOfDelinquencyColumn", new ValueComparerParameters("DateOfDelinquency", ValueComparerParameters.SortType.DateTime));
//ReportDebtHolderWorkingOrder.sortMethods.Add("ExecutionQuantityColumn", new ValueComparerParameters("ExecutionQuantity", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("FirstNameColumn", new ValueComparerParameters("FirstName"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("LastPaidDateColumn", new ValueComparerParameters("LastPaidDate", ValueComparerParameters.SortType.DateTime));
//ReportDebtHolderWorkingOrder.sortMethods.Add("LastNameColumn", new ValueComparerParameters("LastName"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("LeavesQuantityColumn", new ValueComparerParameters("LeavesQuantity", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("PostalCodeColumn", new ValueComparerParameters("PostalCode", ValueComparerParameters.SortType.Object));
//ReportDebtHolderWorkingOrder.sortMethods.Add("MiddleNameColumn", new ValueComparerParameters("MiddleName"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("MarketValueColumn", new ValueComparerParameters("MarketValue", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("OpenedDateColumn", new ValueComparerParameters("OpenedDate"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("PrincipalBalanceColumn", new ValueComparerParameters("PrincipalBalance", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("ProvinceColumn", new ValueComparerParameters("Province"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("SecurityColumn", new ValueComparerParameters("Security"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("SellersAccountNumberColumn", new ValueComparerParameters("SellersAccountNumber", ValueComparerParameters.SortType.Decimal));
//ReportDebtHolderWorkingOrder.sortMethods.Add("SocialSecurityNumberColumn", new ValueComparerParameters("SocialSecurityNumber"));
//ReportDebtHolderWorkingOrder.sortMethods.Add("SourceOrderQuantityColumn", new ValueComparerParameters("SourceOrderQuantity", ValueComparerParameters.SortType.Decimal));            
// On SorReport
//ValueComparerParameters valueCompParameters;
//if(ReportDebtHolderWorkingOrder.sortMethods.TryGetValue(sortItem.Column.ColumnId, out valueCompParameters))
//{
//    Type workingOrderType = typeof(Schema.DebtHolderWorkingOrder.WorkingOrder);

//    ValueComparer<Schema.DebtHolderWorkingOrder.WorkingOrder> valueComparer = 
//        new ValueComparer<Schema.DebtHolderWorkingOrder.WorkingOrder>(valueCompParameters);

//    this.comparer.Add(valueComparer, sortItem.SortOrder);
//}

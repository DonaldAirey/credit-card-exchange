namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Linq;
	using System.ServiceModel;
	using System.Threading;
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


	/// <summary>
	/// This is an example of how to override the FluidTrade.Windows.Controls.Report class.
	/// </summary>
	public class ReportDebtNegotiatorMatch : DynamicReport
	{

		/// <summary>
		/// Identifies the FluidTrade.Windows.Controls.PrototypeReport.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		// Private Static Fields
		private static Duration[] animationDurations;
		private static Dictionary<String, IComparer<Schema.DebtNegotiatorMatch.Match>> sortMethods;

		// Private Instance Fields
		private Guid blotterId;
		private List<Guid> blotterList;
		private ComplexComparer<Schema.DebtNegotiatorMatch.Match> comparer;
		private ComplexFilter<Schema.DebtNegotiatorMatch.Match> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private Boolean isHierarchyChanged;
		private ComplexFilter<MatchRow> prefilter;
		private Decimal balanceFilter;
		private bool isEmployedFilter;
		private Guid reportId;
		private SetBlotterFilterHandler setBlotterFilterHandler;

		/// <summary>
		/// boolean to indicate if multiple rows are selected.
		/// </summary>
		public bool multipleSelectedRows = false;

		/// <summary>
		/// 
		/// </summary>
		public bool CurrentSelectionChanged { get; set; }

		// Private Delegates
		private delegate void SourceDelegate(XDocument xDocument);

		// Private Delegates
		private delegate void SetBlotterFilterHandler(List<Guid> blotterList);

		/// <summary>
		/// Create the static resources required for this report.
		/// </summary>
		static ReportDebtNegotiatorMatch()
		{

			// AnimationSpeed
			ReportDebtNegotiatorMatch.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ReportDebtNegotiatorMatch),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// These constants control the animation speed.
			ReportDebtNegotiatorMatch.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

			ReportDebtNegotiatorMatch.sortMethods = new Dictionary<string, IComparer<Schema.DebtNegotiatorMatch.Match>>();
			ReportDebtNegotiatorMatch.sortMethods.Add("AccountNumberColumn", new Schema.DebtNegotiatorMatch.AccountNumberComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("AccountBalanceColumn", new Schema.DebtNegotiatorMatch.AccountBalanceComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("Address1Column", new Schema.DebtNegotiatorMatch.Address1Comparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("Address2Column", new Schema.DebtNegotiatorMatch.Address2Comparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("BlotterColumn", new Schema.DebtNegotiatorMatch.DebtClassComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("CityColumn", new Schema.DebtNegotiatorMatch.CityComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("CreatedByColumn", new Schema.DebtNegotiatorMatch.CreatedByComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("CreatedDateTimeColumn", new Schema.DebtNegotiatorMatch.CreatedDateTimeComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("DateOfBirthColumn", new Schema.DebtNegotiatorMatch.DateOfBirthComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("FirstNameColumn", new Schema.DebtNegotiatorMatch.FirstNameComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("HeatIndexColumn", new Schema.DebtNegotiatorMatch.HeatIndexComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("IsEmployedColumn", new Schema.DebtNegotiatorMatch.IsEmployedComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("IsReadColumn", new Schema.DebtNegotiatorMatch.IsReadComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("DebtHolderNameColumn", new Schema.DebtNegotiatorMatch.DebtHolderNameComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("LastNameColumn", new Schema.DebtNegotiatorMatch.LastNameComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("PostalCodeColumn", new Schema.DebtNegotiatorMatch.PostalCodeComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("MiddleNameColumn", new Schema.DebtNegotiatorMatch.MiddleNameComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("ModifiedByColumn", new Schema.DebtNegotiatorMatch.ModifiedByComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("ModifiedDateTimeColumn", new Schema.DebtNegotiatorMatch.ModifiedDateTimeComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("ProvinceColumn", new Schema.DebtNegotiatorMatch.ProvinceComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("SalutationColumn", new Schema.DebtNegotiatorMatch.SalutationComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("SavingsBalanceColumn", new Schema.DebtNegotiatorMatch.SavingsBalanceComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("SecurityNameColumn", new Schema.DebtNegotiatorMatch.SecurityNameComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("SocialSecurityNumberColumn", new Schema.DebtNegotiatorMatch.SocialSecurityNumberComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("StatusColumn", new Schema.DebtNegotiatorMatch.StatusColumnComparer());
			ReportDebtNegotiatorMatch.sortMethods.Add("SuffixColumn", new Schema.DebtNegotiatorMatch.SuffixComparer());
			
		}

		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ReportDebtNegotiatorMatch()
		{

			// All records in the presentation layer of the report require a unique identifier.  When the report is updated, this
			// identifier is used to map the data to an existing record or to create a new one.  The starting point for the report
			// is the header record which uses this identifier.  The rest of the records in the report will generally use the
			// source DataRow as the unique identifier.
			this.guid = Guid.NewGuid();

			this.reportId = Guid.Empty;

			// Added to resolve problem on XP that last column is cut off.
			this.Padding = new Thickness(0,0,5,0);

			// These objects are required for sorting, filtering and ordering the report.
			this.prefilter = new ComplexFilter<MatchRow>();
			this.prefilter.Add(this.FilterBlotters);
			this.filter = new ComplexFilter<Schema.DebtNegotiatorMatch.Match>();
			this.comparer = new ComplexComparer<Schema.DebtNegotiatorMatch.Match>();
			this.comparer.Add(new Schema.DebtNegotiatorMatch.ModifiedDateTimeComparer(), SortOrder.Descending);

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
				IComparer<Schema.DebtNegotiatorMatch.Match> comparer;
				if (ReportDebtNegotiatorMatch.sortMethods.TryGetValue(sortItem.Column.ColumnId, out comparer))
					this.comparer.Add(comparer, sortItem.SortOrder);
			}

			// Generate a new document when the new comparison operators for a working order are installed.
			this.Refresh();

		}

		/// <summary>
		/// BalanceFilter
		/// </summary>
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

		private bool FilterBalance(Schema.DebtNegotiatorMatch.Match workingOrder)
		{
			Decimal accountBalance = 0;
			if (workingOrder.SavingsBalance.Balance != null)
				accountBalance = (Decimal)workingOrder.SavingsBalance.Balance;

			return accountBalance > this.balanceFilter;
		}

		/// <summary>
		/// IsEmployedFilter
		/// </summary>
		public Boolean IsEmployedFilter
		{
			get { return this.isEmployedFilter; }
			set
			{

				this.isEmployedFilter = value;

				if (this.isEmployedFilter == false)
				{
					if (this.filter.Contains(this.FilterIsEmployed))
						this.filter.Remove(this.FilterIsEmployed);
				}
				else
				{
					if (!this.filter.Contains(this.FilterIsEmployed))
						this.filter.Add(this.FilterIsEmployed);
				}

				Refresh();

			}
		}

		private bool FilterIsEmployed(Schema.DebtNegotiatorMatch.Match workingOrder)
		{

			if (workingOrder.IsEmployed.Employed is Boolean)
			{
				return (Boolean)workingOrder.IsEmployed.Employed;
			}

			return false;


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
						DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.Match).ReportTypeId);

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
			DataModel.Report.ReportRowChanged += new ReportRowChangeEventHandler(OnReportRowChanged);
			DataModel.BlotterConfiguration.BlotterConfigurationRowChanged += new BlotterConfigurationRowChangeEventHandler(OnBlotterConfigurationRowChanged);
			DataModel.Entity.EntityRowChanged += new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged += new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.Match.MatchRowChanged += new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.Match.MatchRowDeleted += new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.MatchTimer.MatchTimerRowChanged += new MatchTimerRowChangeEventHandler(OnMatchTimerRowChanged);
			DataModel.MatchTimer.MatchTimerRowDeleted += new MatchTimerRowChangeEventHandler(OnMatchTimerRowChanged);
			DataModel.Consumer.ConsumerRowChanged += new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.Consumer.ConsumerRowDeleted += new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationRowChanged += new ConsumerTrustNegotiationRowChangeEventHandler(OnConsumerTrustNegotiationRowChanged);
			DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationRowDeleted += new ConsumerTrustNegotiationRowChangeEventHandler(OnConsumerTrustNegotiationRowChanged);
			DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationRowChanged += new ConsumerDebtNegotiationRowChangeEventHandler(OnConsumerDebtNegotiationRowChanged);
			DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationRowDeleted += new ConsumerDebtNegotiationRowChangeEventHandler(OnConsumerDebtNegotiationRowChanged);
			DataModel.CreditCard.CreditCardRowChanged += new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.CreditCard.CreditCardRowDeleted += new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.ConsumerTrust.ConsumerTrustRowChanged += new ConsumerTrustRowChangeEventHandler(OnConsumerTrustRowChanged);
			DataModel.ConsumerTrust.ConsumerTrustRowDeleted += new ConsumerTrustRowChangeEventHandler(OnConsumerTrustRowChanged);
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
			DataModel.Match.MatchRowChanged -= new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.Match.MatchRowDeleted -= new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.MatchTimer.MatchTimerRowChanged -= new MatchTimerRowChangeEventHandler(OnMatchTimerRowChanged);
			DataModel.MatchTimer.MatchTimerRowDeleted -= new MatchTimerRowChangeEventHandler(OnMatchTimerRowChanged);
			DataModel.Consumer.ConsumerRowChanged -= new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.Consumer.ConsumerRowDeleted -= new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationRowChanged -= new ConsumerTrustNegotiationRowChangeEventHandler(OnConsumerTrustNegotiationRowChanged);
			DataModel.ConsumerTrustNegotiation.ConsumerTrustNegotiationRowDeleted -= new ConsumerTrustNegotiationRowChangeEventHandler(OnConsumerTrustNegotiationRowChanged);
			DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationRowChanged -= new ConsumerDebtNegotiationRowChangeEventHandler(OnConsumerDebtNegotiationRowChanged);
			DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationRowDeleted -= new ConsumerDebtNegotiationRowChangeEventHandler(OnConsumerDebtNegotiationRowChanged);
			DataModel.CreditCard.CreditCardRowChanged -= new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.CreditCard.CreditCardRowDeleted -= new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.ConsumerTrust.ConsumerTrustRowChanged -= new ConsumerTrustRowChangeEventHandler(OnConsumerTrustRowChanged);
			DataModel.ConsumerTrust.ConsumerTrustRowDeleted -= new ConsumerTrustRowChangeEventHandler(OnConsumerTrustRowChanged);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);
		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ReportDebtNegotiatorMatch.AnimationSpeedProperty); }
			set { this.SetValue(ReportDebtNegotiatorMatch.AnimationSpeedProperty, value); }
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
			ReportDebtNegotiatorMatch reportPrototype = dependencyObject as ReportDebtNegotiatorMatch;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ReportDebtNegotiatorMatch.animationDurations[(Int32)animationSpeed];

		}

		/// <summary>
		/// Handles a change to the MatchRow table.
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
		/// Handles a change to the MatchRow table.
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
					MatchRow matchRow = dataTableCoordiante.DataRow as MatchRow;

					// Bailing out if the Selected Value is null.
					if (comboBox.SelectedValue == null)
						return;

					if (dataTableCoordiante.DataColumn == DataModel.Consumer.ProvinceIdColumn)
					{
						object newprovinceId = comboBox.SelectedValue;
						if (newprovinceId == null || (Guid)newprovinceId == Guid.Empty)
							newprovinceId = DBNull.Value;

						ConsumerRow consumerRow = dataTableCoordiante.DataRow as ConsumerRow;
						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
							TradingSupportWebService.UpdateConsumer(new TradingSupportReference.Consumer(consumerRow) { ProvinceId = (Guid)newprovinceId })));
					}

				}

			}

		}

		private void OnTextChanged(object sender, RoutedEventArgs routedEventArgs)
		{
			TextBox textBox = routedEventArgs.OriginalSource as TextBox;
			ReportDebtNegotiatorMatch report = sender as ReportDebtNegotiatorMatch;

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
				MatchRow matchRow = iContent.Key as MatchRow;
				if(matchRow != null &&
					matchRow.IsHeatIndexDetailsNull() == false)
				{
					this.matchToolTipContent.SetDetails(matchRow.HeatIndexDetails);
					e.ToolTip.Content = this.matchToolTipContent;
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
			ReportDebtNegotiatorMatch report = sender as ReportDebtNegotiatorMatch;

			if (textBox == null || report == null)
				return;

			IContent iContent = textBox.DataContext as IContent;

			// This filters all the ComboBox events looking for user initiated actions that are bound to the data model.
			if (report.CurrentSelectionChanged == true &&
				iContent != null && iContent.Key is DataTableCoordinate)
			{
				TextBoxValueChanged(textBox.Text, iContent);
				report.CurrentSelectionChanged = false;
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textBoxTxt"></param>
		/// <param name="iContent"></param>
		private void TextBoxValueChanged(string textBoxTxt, IContent iContent)
		{
			DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
			ConsumerRow reportRow = dataTableCoordiante.DataRow as ConsumerRow;
			CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;

			if (dataTableCoordiante.DataColumn == DataModel.CreditCard.AccountNumberColumn)
			{
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { AccountNumber = textBoxTxt })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.CreditCard.OriginalAccountNumberColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { OriginalAccountNumber = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.CreditCard.AccountBalanceColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { AccountBalance = textBoxTxt })));

			if(dataTableCoordiante.DataColumn == DataModel.ConsumerTrust.SavingsBalanceColumn)
			{
				ConsumerTrustRow consumerTrustRow = dataTableCoordiante.DataRow as ConsumerTrustRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					 TradingSupportWebService.UpdateConsumerTrust(new ConsumerTrust(consumerTrustRow) { SavingsBalance = textBoxTxt })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.Address1Column)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { Address1 = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.Address2Column)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { Address2 = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.CityColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { City = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.FirstNameColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { FirstName = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.LastNameColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { LastName = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.PostalCodeColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { PostalCode = textBoxTxt })));

			//TODO: Uncomment once Saving Balance cell is editable.
			//if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrust.SavingsBalanceColumn)
			//    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>, new TradingSupportWebServiceClient(dataTableCoordiante.DataRow as ConsumerTrustRow) { Record = new ConsumerTrust { SavingsBalance = textBoxTxt } });

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.MiddleNameColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { MiddleName = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.SocialSecurityNumberColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { SocialSecurityNumber = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Entity.NameColumn)
			{
				EntityRow entityRow = dataTableCoordiante.DataRow as EntityRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateEntity(new Entity(entityRow) { BlotterId = this.BlotterId, Name = textBoxTxt })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.SuffixColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { Suffix = textBoxTxt })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.SalutationColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { Salutation = textBoxTxt })));


		}

		/// <summary>
		/// TODO: Fill in.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// Commented out the Delete functionality as it is not allowed right now in match reports.
			// If in the future we needed we can uncomment below.
			// Further the Delete Operation for the context menu will need to be implemented.
			/*

			if (e.Key == Key.Delete)
			{
				List<List<FluidTrade.Core.Windows.Controls.ReportRow>> selectedRowBlocks = reportGrid.SelectedRowHeaderBlocks;
				List<MatchRow> toDeleteRows = new List<MatchRow>();

				//Iterate over collections of selected items
				foreach (var selectedRows in selectedRowBlocks)
				{
					foreach (FluidTrade.Core.Windows.Controls.ReportRow row in selectedRows)
					{
						MatchRow reportRow = GetandValidateWorkingRow(row);
						if (reportRow != null)
						{
							toDeleteRows.Add(reportRow);
						}
					}
				}

				if (toDeleteRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(DestroyRecords, toDeleteRows);
					e.Handled = true;
				}

			}
			*/

			// The base class handles any keys not handled above.
			base.OnKeyDown(e);
		}

		/// <summary>
		/// Handles bolding of a match row.
		/// Triggered by mouse click in the match row, from the menu, or context menu. 
		///
		/// To Bold the row send in a bool of markAsRead=false thus unread operation is performed.
		/// To Unbold the row send in a bool of markAsRead=true thus read operation is performed.
		/// </summary>
		/// <param name="reportRow">Match row to apply the bolding to</param>
		/// <param name="markAsRead">boolean to indicat if to mark the row is read or unread</param>
		public void BoldMatchRow(object o)
		{
			BoldingData boldingData = o as BoldingData;

			MatchRow reportRow = boldingData.ReportRow;
			bool markAsRead = boldingData.MarkAsRead;

			// Create a channel to the middle tier.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				// If the user was using the menu to then we will get a null match row, 
				//  null in which case we will do our best to get the match report provided the current focused cell is a valid match row.
				// These types should work FocusableTextbox, ValueBlock, or MaskTextBox not sure about other this might be an issue and throw an exception.
				if (reportRow == null)
				{
					FluidTrade.Core.Windows.Controls.ReportRow row = reportGrid.CurrentReportCell.ReportRow;
					reportRow = GetandValidateWorkingRow(row);
					if (reportRow == null)
					{
						// if this is still null then we are going to return to be safe alternatively we could throw an exception and log it.
						return;
					}
				}

				// The Web Service call will require the ConsumerTrustNegotiationRow which must be extracted from the data model.
				ConsumerTrustNegotiationRow consumerTrustNegotiationRow = null;

				// The data model must be locked while ConsumerTrustNegotiationRow is extracted using the matchId.  Then we bold the row.
				lock (DataModel.SyncRoot)
				{
					MatchRow matchRow = DataModel.Match.MatchKey.Find(reportRow.MatchId);
					if (matchRow == null)
					{
						// if this is null then we are going to return to be safe alternatively we could throw an exception and log it.
						return;
					}

					long maxVersion = matchRow.GetConsumerTrustNegotiationRows().Max(negotitiaon => negotitiaon.Version);
					consumerTrustNegotiationRow = matchRow.GetConsumerTrustNegotiationRows().Where(p => p.Version == maxVersion).Single();
				}

				// Only send the bold/unbold request if it is different and need to updated.
				if (consumerTrustNegotiationRow != null && consumerTrustNegotiationRow.IsRead != markAsRead)
				{
					// Construct a version of the negotiation record from the known information and update the 'IsRead' flag.
					ConsumerTrustNegotiationIsReadInfo consumerTrustNegotiationIsReadInfo = new ConsumerTrustNegotiationIsReadInfo();
					consumerTrustNegotiationIsReadInfo.ConsumerTrustNegotiationId = consumerTrustNegotiationRow.ConsumerTrustNegotiationId;
					consumerTrustNegotiationIsReadInfo.IsRead = markAsRead;
					consumerTrustNegotiationIsReadInfo.RowVersion = consumerTrustNegotiationRow.RowVersion;
					
					// Update the database with the modified offer.
					tradingSupportClient.UpdateConsumerTrustNegotiationIsRead(new ConsumerTrustNegotiationIsReadInfo[] { consumerTrustNegotiationIsReadInfo });
				}

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged or might not have a valid match row cell to bold or unbold.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				//this.Dispatcher.BeginInvoke(new Action(() =>
				//    MessageBox.Show(Application.Current.MainWindow, "Server Error: Failed to mark as read.", Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
		}

		/// <summary>
		/// Handles Mouse down event in this Match Report
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			// Added the ability to Ignore clicking on the HeaderRow. 
			// The only way a an unbold can be done is clicking one of the following types of Textboxes.
			// TODO: Expand this to make it better.
			if (((e.Source is FocusableTextbox) || (e.Source is ValueBlock) || (e.Source is MaskTextBox)) && (e.RightButton != MouseButtonState.Pressed))
			{
				FluidTrade.Core.Windows.Controls.ReportRow row = reportGrid.CurrentReportCell.ReportRow;
				MatchRow reportRow = GetandValidateWorkingRow(row);

				BoldingData boldingData = new BoldingData(reportRow, true);
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(BoldMatchRow), boldingData);
				
			}
		}

		/// <summary>
		/// Fill in comment here.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (this.reportGrid != null && (this.reportGrid.SelectedRowHeaderBlocks != null) && (this.reportGrid.SelectedRowHeaderBlocks.Count > 0))
			{
				if ((reportGrid.SelectedRowHeaderBlocks.Count > 1) || (reportGrid.SelectedRowHeaderBlocks[0].Count > 1))
				{
					this.multipleSelectedRows = true;
				}
				else
				{
					this.multipleSelectedRows = false;
				}
			}
		}

		/// <summary>
		/// Safely retrieve workingOrderRow from a reportRow
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		private MatchRow GetandValidateWorkingRow(FluidTrade.Core.Windows.Controls.ReportRow row)
		{
			MatchRow workingOrderRow = row.NullSafe(datarow => datarow.IContent).NullSafe(Content => Content.Key) as MatchRow;

			try
			{
				if (workingOrderRow != null)
				{
					if (workingOrderRow.WorkingOrderId != null)
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
						if (sourceDateTimePicker.DateTime != null)
						{
							ConsumerRow consumerRow = dataTableCoordiante.DataRow as ConsumerRow;
							DateTime selectedItem = (DateTime)sourceDateTimePicker.DateTime;
							FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
								TradingSupportWebService.UpdateConsumer(new Consumer(consumerRow) { DateOfBirth = selectedItem })));
						}
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
				MatchRow matchRow = dataTableCoordiante.DataRow as MatchRow;
				ConsumerRow reportRow = dataTableCoordiante.DataRow as ConsumerRow;
				bool toggleButtonState = toggleButton.IsChecked.GetValueOrDefault();

				// Update the IsEmployed column.
				if (dataTableCoordiante.DataColumn == DataModel.Consumer.IsEmployedColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumer(new Consumer(reportRow) { IsEmployed = toggleButtonState })));

				// Update the IsRead column.
				if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustNegotiation.IsReadColumn)
				{
					// Create a channel to the middle tier.
					TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
					try
					{

						// Construct a version of the negotiation record from the known information and update the 'IsRead' flag.
						ConsumerTrustNegotiationRow consumerTrustNegotiationRow = dataTableCoordiante.DataRow as ConsumerTrustNegotiationRow;
						ConsumerTrustNegotiationIsReadInfo consumerTrustNegotiationIsReadInfo = new ConsumerTrustNegotiationIsReadInfo();
						consumerTrustNegotiationIsReadInfo.ConsumerTrustNegotiationId = consumerTrustNegotiationRow.ConsumerTrustNegotiationId;
						consumerTrustNegotiationIsReadInfo.IsRead = toggleButtonState;
						consumerTrustNegotiationIsReadInfo.RowVersion = consumerTrustNegotiationRow.RowVersion;

						// Update the database with the modified offer.
						tradingSupportClient.UpdateConsumerTrustNegotiationIsRead(new ConsumerTrustNegotiationIsReadInfo[] { consumerTrustNegotiationIsReadInfo });

					}
					catch (Exception exception)
					{

						// Any issues trying to communicate to the server are logged or might not have a valid match row cell to bold or unbold.
						EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

						//this.Dispatcher.BeginInvoke(new Action(() =>
						//    MessageBox.Show(Application.Current.MainWindow, "Error: Failed to mark as read.", Application.Current.MainWindow.Title)));

					}
					finally
					{
						if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
							tradingSupportClient.Close();
					}
				}


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
		/// Handles a change to the MatchRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnMatchRowChanged(object sender, MatchRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles a change to the MatchRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnMatchTimerRowChanged(object sender, MatchTimerRowChangeEventArgs e)
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
		/// Handles a change to the ConsumerTrustNegotiation table.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConsumerTrustNegotiationRowChanged(object sender, ConsumerTrustNegotiationRowChangeEventArgs e)
		{
			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;
		}


		/// <summary>
		/// Handles a change to the ConsumerDebtNegotiation table.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConsumerDebtNegotiationRowChanged(object sender, ConsumerDebtNegotiationRowChangeEventArgs e)
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
		/// Handles a change to the ConsumerTrust table.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConsumerTrustRowChanged(object sender, ConsumerTrustRowChangeEventArgs e)
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
			
			// Ask the virtual method to update the data in the report.
			// Once the module is compiled and loaded into memory, an instance of the specified viewer is created.  The
			// IDocumentView is all that is needed to communicate with the loaded view.  Note that an array of delegates is passed
			// into the newly compiled and loaded View that allow the View to communicate back to the host Viewer.

			// Note that SetContent method will dispatch the thread hence it was not done here.
			lock (DataModel.SyncRoot)
					this.SetContent(this.MatchHeaderSelector(this.guid), requestGC);

			ThreadPoolHelper.QueueUserWorkItem(RefreshThreadCompleted);

		}

		private void RefreshThreadCompleted(object state)
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(false);	

		}

		private Schema.DebtNegotiatorMatchHeader.MatchHeader MatchHeaderSelector(Guid guid)
		{
			Schema.DebtNegotiatorMatchHeader.MatchHeader matchHeader = new Schema.DebtNegotiatorMatchHeader.MatchHeader();
			matchHeader.Prefilter = this.prefilter;
			matchHeader.Selector = MatchSelector;
			matchHeader.Filter = this.filter;
			matchHeader.Comparer = this.comparer;
			return matchHeader.Select(guid);
		}

		private Schema.DebtNegotiatorMatch.Match MatchSelector(MatchRow matchRow)
		{
			Schema.DebtNegotiatorMatch.Match match = new Schema.DebtNegotiatorMatch.Match();
			return match.Select(matchRow);
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

		private bool FilterBlotters(MatchRow matchRow)
		{
			return this.blotterList.BinarySearch(matchRow.BlotterId) >= 0;

		}

		// Let this here because not sure of its value yet.
		//public override string ToString()
		//{
		//    return this.guid.ToString();
		//}

		/// <summary>
		/// Delete a list of working orders.
		/// </summary>
		/// <param name="state">The list to delete.</param>
		public void DestroyRecords(object state)
		{

			List<MatchRow> toDeleteRows = state as List<MatchRow>;
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				int recordsPerCall = 100;
				TradingSupportReference.WorkingOrderRecord[] orders = null;
				int recordTotal = 0;
				int recordIndex = 0;

				foreach (MatchRow matchRow in toDeleteRows)
				{

					if (recordIndex == 0)
						orders = new TradingSupportReference.WorkingOrderRecord[
							toDeleteRows.Count - recordTotal < recordsPerCall ?
							toDeleteRows.Count - recordTotal :
							recordsPerCall];

					orders[recordIndex++] = new TradingSupportReference.WorkingOrderRecord()
					{
						WorkingOrderId = matchRow.WorkingOrderId,
						RowVersion = matchRow.WorkingOrderRow.RowVersion
					};

					if (recordIndex == orders.Length)
					{

						MethodResponseErrorCode response = tradingSupportClient.DeleteWorkingOrder(orders);
						if (!response.IsSuccessful)
							throw new Exception(String.Format("Server error {0}", response.Result));
						recordTotal += recordIndex;
						recordIndex = 0;

					}

				}

			}
			catch
			{

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Cannot delete working orders", Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

		}
		/// <summary>
		/// Marks row(s) as read.
		/// </summary>
		public void MarkMatchRowsAsRead()
		{
			if (reportGrid.SelectedRowHeaderBlocks.Count > 0)
			{
				//Make it work for multiple selected row section using control key.
				// TODO: Fix bug in selectedRanges that does not get the correct selected rows and cause the server side to throw an exception.
				// Problem is that reportGrid.SelectedRowHeaderBlocks contains one extra item in the inner list collection
				// due to the one that is right clicked get added to the selected Ranges? 
				// This what need to be fixed.
				List<List<FluidTrade.Core.Windows.Controls.ReportRow>> selectedRowBlocks = reportGrid.SelectedRowHeaderBlocks;
				List<MatchRow> matchRows = new List<MatchRow>();

				int selectedRowBlocksCount = selectedRowBlocks.Count;

				foreach (var selectedRows in selectedRowBlocks)
				{
					foreach (FluidTrade.Core.Windows.Controls.ReportRow row in selectedRows)
					{
						MatchRow reportRow = GetandValidateWorkingRow(row);
						if (reportRow != null)
						{
							// HACK: Make a routine to get rid of the duplicates here before calling server or should the server handle this?
							// Or figure out the root cause of in the selecting row blocks.
							if (!matchRows.Contains(reportRow))
								matchRows.Add(reportRow);
						}
					}
				}
				if (matchRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(MarkRecordsAsRead, matchRows);
				}
			}
			else
			{
				BoldingData boldingData = new BoldingData(null, true);
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(BoldMatchRow), boldingData);
			}
		}

		/// <summary>
		/// Marks a set of record(s) as read.
		/// </summary>
		/// <param name="state">The list to delete.</param>
		private void MarkRecordsAsRead(object state)
		{

			List<MatchRow> matchRows = state as List<MatchRow>;
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				ConsumerTrustNegotiationIsReadInfo[] consumerTrustNegotiationIsReadInfos;
				consumerTrustNegotiationIsReadInfos = new ConsumerTrustNegotiationIsReadInfo[matchRows.Count];

				int index = 0;
				foreach (MatchRow matchRow in matchRows)
				{
					// The Web Service call will require the ConsumerTrustNegotiationRow which must be extracted from the data model.
					ConsumerTrustNegotiationRow consumerTrustNegotiationRow = null;

					// The data model must be locked while ConsumerTrustNegotiationRow is extracted using the matchId.  Then we bold the row.
					lock (DataModel.SyncRoot)
					{
						consumerTrustNegotiationRow = matchRow.GetConsumerTrustNegotiationRows()[0];
					}

					// Construct a version of the negotiation record from the known information and update the 'IsRead' flag.
					ConsumerTrustNegotiationIsReadInfo consumerTrustNegotiationIsReadInfo = new ConsumerTrustNegotiationIsReadInfo();
					consumerTrustNegotiationIsReadInfo.ConsumerTrustNegotiationId = consumerTrustNegotiationRow.ConsumerTrustNegotiationId;

					if (consumerTrustNegotiationRow.IsRead != true)
					{

						consumerTrustNegotiationIsReadInfo.IsRead = true;
						consumerTrustNegotiationIsReadInfo.RowVersion = consumerTrustNegotiationRow.RowVersion;

						consumerTrustNegotiationIsReadInfos[index] = consumerTrustNegotiationIsReadInfo;
						index++;

					}

				}
				
				// Update the database with the modified offer.
				tradingSupportClient.UpdateConsumerTrustNegotiationIsRead(consumerTrustNegotiationIsReadInfos);

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged or might not have a valid match row cell to bold or unbold.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Failed to perform Mark as Read operation.", Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

		}

		/// <summary>
		/// Marks row(s) as unread.
		/// </summary>
		public void MarkMatchRowsAsUnread()
		{
			if (reportGrid.SelectedRowHeaderBlocks.Count > 0)
			{
				//Make it work for multiple selected row section using control key.
				// TODO: Fix bug in selectedRanges that does not get the correct selected rows and cause the server side to throw an exception.
				// Problem is that reportGrid.SelectedRowHeaderBlocks contains one extra item in the inner list collection
				// due to the one that is right clicked get added to the selected Ranges? 
				// This what need to be fixed.
				List<List<FluidTrade.Core.Windows.Controls.ReportRow>> selectedRowBlocks = reportGrid.SelectedRowHeaderBlocks;
				List<MatchRow> matchRows = new List<MatchRow>();

				int selectedRowBlocksCount = selectedRowBlocks.Count;

				foreach (var selectedRows in selectedRowBlocks)
				{
					foreach (FluidTrade.Core.Windows.Controls.ReportRow row in selectedRows)
					{
						MatchRow reportRow = GetandValidateWorkingRow(row);
						if (reportRow != null)
						{
							// HACK: Make a routine to get rid of the duplicates here before calling server or should the server handle this?
							// Or figure out the root cause of in the selecting row blocks.
							if (!matchRows.Contains(reportRow))
								matchRows.Add(reportRow);
						}
					}
				}
				if (matchRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(MarkRecordsAsUnread, matchRows);
				}
			}
			else
			{
				BoldingData boldingData = new BoldingData(null, false);
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(BoldMatchRow), boldingData);
			}
		}

		/// <summary>
		/// Marks a set of record(s) as unread.
		/// </summary>
		/// <param name="state">The list to delete.</param>
		private void MarkRecordsAsUnread(object state)
		{

			List<MatchRow> matchRows = state as List<MatchRow>;
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				ConsumerTrustNegotiationIsReadInfo[] consumerTrustNegotiationIsReadInfos;
				consumerTrustNegotiationIsReadInfos = new ConsumerTrustNegotiationIsReadInfo[matchRows.Count];

				int index = 0;
				foreach (MatchRow matchRow in matchRows)
				{
					// The Web Service call will require the ConsumerTrustNegotiationRow which must be extracted from the data model.
					ConsumerTrustNegotiationRow consumerTrustNegotiationRow = null;

					// The data model must be locked while ConsumerTrustNegotiationRow is extracted using the matchId.  Then we bold the row.
					lock (DataModel.SyncRoot)
					{
						consumerTrustNegotiationRow = matchRow.GetConsumerTrustNegotiationRows()[0];
					}

					// Construct a version of the negotiation record from the known information and update the 'IsRead' flag.
					ConsumerTrustNegotiationIsReadInfo consumerTrustNegotiationIsReadInfo = new ConsumerTrustNegotiationIsReadInfo();
					consumerTrustNegotiationIsReadInfo.ConsumerTrustNegotiationId = consumerTrustNegotiationRow.ConsumerTrustNegotiationId;

					if (consumerTrustNegotiationRow.IsRead != false)
					{

						consumerTrustNegotiationIsReadInfo.IsRead = false;
						consumerTrustNegotiationIsReadInfo.RowVersion = consumerTrustNegotiationRow.RowVersion;

						consumerTrustNegotiationIsReadInfos[index] = consumerTrustNegotiationIsReadInfo;
						index++;

					}
				}
				
				// Update the database with the modified offer.
				tradingSupportClient.UpdateConsumerTrustNegotiationIsRead(consumerTrustNegotiationIsReadInfos);

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged or might not have a valid match row cell to bold or unbold.
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Failed to perform Mark as Unread operation.", Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
		}
	}

	/// <summary>
	/// Class to hold Bolding Data need to pass infomation to the background thread from the foreground thread.
	/// </summary>
	public class BoldingData
	{
		/// <summary>
		/// 
		/// </summary>
		private MatchRow reportRow;
		private bool markAsRead;

		/// <summary>
		/// Match row to apply the bolding to
		/// </summary>
		public MatchRow ReportRow
		{
			get
			{
				return reportRow;
			}
			set
			{
				reportRow = value;
			}
		}

		/// <summary>
		/// Boolean to indicate if to mark the row is read or unread
		/// </summary>
		public bool MarkAsRead
		{
			get
			{
				return markAsRead;
			}
			set
			{
				markAsRead = value;
			}
		}

		/// <summary>
		/// Class to hold Bolding Data need to pass infomation to the background thread from the foreground thread.
		/// </summary>
		/// <param name="reportRowValue">Match row to apply the bolding to</param>
		/// <param name="markAsReadBool">boolean to indicate if to mark the row is read or unread</param>
		public BoldingData(MatchRow reportRowValue, bool markAsReadBool)
		{
			this.reportRow = reportRowValue;
			this.markAsRead = markAsReadBool;
		}
	}
}

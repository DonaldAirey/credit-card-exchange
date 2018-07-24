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

	/// <summary>
	/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
	/// </summary>
	public class ReportMatch : DynamicReport
	{

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		// Private Static Fields
		private static Duration[] animationDurations;
		private static Dictionary<String, IComparer<Schema.Match.Match>> sortMethods;

		// Private Instance Fields
		private Guid blotterId;
		private List<Guid> blotterList;
		private ComplexComparer<Schema.Match.Match> comparer;
		private ComplexFilter<Schema.Match.Match> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private Boolean isHierarchyChanged;
		private ComplexFilter<MatchRow> prefilter;
		private Decimal balanceFilter;
		private bool isEmpoyedFilter;
		private Guid reportId;
		private SetBlotterFilterHandler setBlotterFilterHandler;
		private String postalCodeFilter;

		// Private Delegates
		private delegate void SourceDelegate(XDocument xDocument);

		// Private Delegates
		private delegate void SetBlotterFilterHandler(List<Guid> blotterList);

		/// <summary>
		/// Create the static resources required for this report.
		/// </summary>
		static ReportMatch()
		{

			// AnimationSpeed
			ReportMatch.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ReportMatch),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// These constants control the animation speed.
			ReportMatch.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

			ReportMatch.sortMethods = new Dictionary<string, IComparer<Schema.Match.Match>>();			            
            ReportMatch.sortMethods.Add("SavingsBalanceColumn", new Schema.Match.SavingsBalanceComparer());
            ReportMatch.sortMethods.Add("Address1Column", new Schema.Match.Address1Comparer());    
            ReportMatch.sortMethods.Add("CityColumn", new Schema.Match.CityComparer());
			ReportMatch.sortMethods.Add("CreditCardBalanceColumn", new Schema.Match.CreditCardBalanceComparer());
			ReportMatch.sortMethods.Add("FirstNameColumn", new Schema.Match.FirstNameComparer());
			ReportMatch.sortMethods.Add("IsEmployedColumn", new Schema.Match.IsEmployedComparer());
			ReportMatch.sortMethods.Add("LastNameColumn", new Schema.Match.LastNameComparer());
            ReportMatch.sortMethods.Add("HeatIndexColumn", new Schema.Match.HeatIndexComparer());
            ReportMatch.sortMethods.Add("PostalCodeColumn", new Schema.Match.PostalCodeComparer());
            ReportMatch.sortMethods.Add("MatchedTimeColumn", new Schema.Match.MatchedTimeComparer());
            ReportMatch.sortMethods.Add("SecurityNameColumn", new Schema.Match.SecurityNameComparer());
            ReportMatch.sortMethods.Add("SocialSecurityNumberColumn", new Schema.Match.SocialSecurityNumberComparer());
			ReportMatch.sortMethods.Add("StatusColumn", new Schema.Match.StatusColumnComparer());
		}

		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ReportMatch()
		{

			// All records in the presentation layer of the report require a unique identifier.  When the report is updated, this
			// identifier is used to map the data to an existing record or to create a new one.  The starting point for the report
			// is the header record which uses this identifier.  The rest of the records in the report will generally use the
			// source DataRow as the unique identifier.
			this.guid = Guid.NewGuid();

			this.reportId = Guid.Empty;

			// These objects are required for sorting, filtering and ordering the report.
			this.prefilter = new ComplexFilter<MatchRow>();
			this.prefilter.Add(this.FilterBlotters);
			this.filter = new ComplexFilter<Schema.Match.Match>();
			this.comparer = new ComplexComparer<Schema.Match.Match>();
			this.comparer.Add(new Schema.Match.MatchedTimeComparer(), SortOrder.Descending);

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
				IComparer<Schema.Match.Match> comparer;
				if (ReportMatch.sortMethods.TryGetValue(sortItem.Column.ColumnId, out comparer))
					this.comparer.Add(comparer, sortItem.SortOrder);
			}

			// Generate a new document when the new comparison operators for a working order are installed.
			this.Refresh();

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

		private bool FilterBalance(Schema.Match.Match workingOrder)
		{
			Decimal accountBalance = 0;
			if (workingOrder.SavingsBalance.Balance != null)
				accountBalance = (Decimal)workingOrder.SavingsBalance.Balance;

			return accountBalance > this.balanceFilter;
		}

		public Boolean IsEmpoyedFilter
		{
			get { return this.isEmpoyedFilter; }
			set
			{

				this.isEmpoyedFilter = value;

				if (this.isEmpoyedFilter == false)
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

		private bool FilterIsEmployed(Schema.Match.Match workingOrder)
		{

			if (workingOrder.IsEmployed.Employed is Boolean)
			{
				return (Boolean)workingOrder.IsEmployed.Employed;
			}

			return false;


		}


		public Decimal CreditCardBalanceFilter
		{
			get { return this.balanceFilter; }
			set
			{

				this.balanceFilter = value;

				if (this.balanceFilter == 0)
				{
					if (this.filter.Contains(this.FilterCreditCardBalance))
						this.filter.Remove(this.FilterCreditCardBalance);
				}
				else
				{
					if (!this.filter.Contains(this.FilterCreditCardBalance))
						this.filter.Add(this.FilterCreditCardBalance);
				}

				Refresh();

			}
		}

		private bool FilterCreditCardBalance(Schema.Match.Match workingOrder)
		{
			Decimal accountBalance = 0;
			if (workingOrder.CreditCardBalance.Balance != null)
				accountBalance = (Decimal)workingOrder.CreditCardBalance.Balance;

			return accountBalance > this.balanceFilter;
		}

		public String PostalCodeFilter
		{
			get { return this.postalCodeFilter; }
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

		private bool FilterPostalCode(Schema.Match.Match workingOrder)
		{
			String postalCode = workingOrder.PostalCode.Code.ToString();
			if (String.IsNullOrEmpty(postalCode))
				return false;
			else
				return postalCode.StartsWith(this.postalCodeFilter);

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

		private void OnLoaded(object sender, RoutedEventArgs e)
		{

			// These events will keep the report updated with live content once the report data model is compiled.
			DataModel.Blotter.BlotterRowChanged += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Blotter.BlotterRowDeleted += new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Report.ReportRowChanged += new ReportRowChangeEventHandler(OnReportRowChanged);
			DataModel.Entity.EntityRowChanged += new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged += new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.Match.MatchRowChanged += new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.Match.MatchRowDeleted += new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.MatchTimer.MatchTimerRowChanged += new MatchTimerRowChangeEventHandler(OnMatchTimerRowChanged);
			DataModel.MatchTimer.MatchTimerRowDeleted += new MatchTimerRowChangeEventHandler(OnMatchTimerRowChanged);
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
			DataModel.Match.MatchRowChanged -= new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.Match.MatchRowDeleted -= new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);

		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ReportMatch.AnimationSpeedProperty);}
			set { this.SetValue(ReportMatch.AnimationSpeedProperty, value); }
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

		/// <summary>
		/// Handles a change to the animation speed.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnAnimationSpeedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Set the speed for animation.
			ReportMatch reportPrototype = dependencyObject as ReportMatch;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ReportMatch.animationDurations[(Int32)animationSpeed];

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
				this.SetContent(this.MatchHeaderSelector(this.guid), requestGC);

			ThreadPoolHelper.QueueUserWorkItem(RefreshThreadCompleted);

		}

		private void RefreshThreadCompleted(object state)
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(false);

		}


		private Schema.MatchHeader.MatchHeader MatchHeaderSelector(Guid guid)
		{
			Schema.MatchHeader.MatchHeader matchHeader = new Schema.MatchHeader.MatchHeader();
			matchHeader.Prefilter = this.prefilter;
			matchHeader.Selector = MatchSelector;
			matchHeader.Filter = this.filter;
			matchHeader.Comparer = this.comparer;
			return matchHeader.Select(guid);
		}
		
		private Schema.Match.Match MatchSelector(MatchRow matchRow)
		{
			Schema.Match.Match match = new Schema.Match.Match();
			return match.Select(matchRow);
		}

		/// <summary>
		/// Updates a Working Order record.
		/// </summary>
		/// <param name="state">The generic thread initialization parameter.</param>
		private void UpdateDataModel(object state)
		{

			// Extract the specific instructions for changing the working order from the generic argument.
			MatchChange matchChange = state as MatchChange;

			try
			{

				// Create a channel to the middle tier.
				DataModelClient dataModelClient = new DataModelClient(Guardian.Properties.Settings.Default.DataModelEndpoint);

				// Call the handler to update the working order record.
				matchChange.Handler(dataModelClient, matchChange.MatchRow, matchChange.Value);

				// At this point the client can be shut down gracefully.
				dataModelClient.Close();

			}
			catch (FaultException<OptimisticConcurrencyFault> optimisticConcurrencyException)
			{

				// The record is busy.
				this.Dispatcher.Invoke(DispatcherPriority.Normal,
					(MessageDelegate)((string message) => { MessageBox.Show(message, Application.Current.MainWindow.Title); }),
					String.Format(FluidTrade.Core.Properties.Resources.OptimisticConcurrencyError,
					optimisticConcurrencyException.Detail.TableName));

			}
			catch (FaultException<RecordNotFoundFault> recordNotFoundException)
			{

				// The record is busy.
				this.Dispatcher.Invoke(DispatcherPriority.Normal,
					(MessageDelegate)((string message) => { MessageBox.Show(message, Application.Current.MainWindow.Title); }),
					String.Format(FluidTrade.Core.Properties.Resources.RecordNotFoundError,
					CommonConversion.FromArray(recordNotFoundException.Detail.Key),
					recordNotFoundException.Detail.TableName));

			}
			catch (CommunicationException communicationException)
			{

				// Log communication problems.
				this.Dispatcher.Invoke(DispatcherPriority.Normal,
					(MessageDelegate)((string message) => { MessageBox.Show(message, Application.Current.MainWindow.Title); }),
					communicationException.Message);

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
	
		public override string ToString()
		{
			return this.guid.ToString();
		}

	}

}

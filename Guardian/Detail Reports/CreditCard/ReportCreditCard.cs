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

	/// <summary>Schema.CreditCard.CreditCard
	/// 
	/// </summary>
	public class ReportCreditCard : DynamicReport
	{
		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		// Private Static Fields
		private static Duration[] animationDurations;
		private static Dictionary<String, IComparer<Schema.CreditCard.CreditCard>> sortMethods;

		// Private Instance Fields
		private Guid consumerId;
		private Guid blotterId;
		private ComplexComparer<Schema.CreditCard.CreditCard> comparer;
		private ComplexFilter<Schema.CreditCard.CreditCard> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private ComplexFilter<CreditCardRow> prefilter;
		private Guid reportId;

		/// <summary>
		/// Boolean to indicate the text Selection has changed.
		/// </summary>
		public bool CurrentSelectionChanged { get; set; }

		// Private Delegates
		private delegate void SourceDelegate(XDocument xDocument);


		/// <summary>
		/// Create the static resources required for this report.
		/// </summary>
		static ReportCreditCard()
		{

			// AnimationSpeed
			ReportCreditCard.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ReportCreditCard),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// These constants control the animation speed.
			ReportCreditCard.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

			ReportCreditCard.sortMethods = new Dictionary<string, IComparer<Schema.CreditCard.CreditCard>>();
			ReportCreditCard.sortMethods.Add("AccountBalanceColumn", new Schema.CreditCard.AccountBalanceComparer());
			ReportCreditCard.sortMethods.Add("AccountNumberColumn", new Schema.CreditCard.AccountNumberComparer());
			ReportCreditCard.sortMethods.Add("ConsumerIdColumn", new Schema.CreditCard.ConsumerIdComparer());
			ReportCreditCard.sortMethods.Add("CreditCardIdColumn", new Schema.CreditCard.CreditCardIdComparer());
			ReportCreditCard.sortMethods.Add("DebtHolderNameColumn", new Schema.CreditCard.DebtHolderNameComparer());
			ReportCreditCard.sortMethods.Add("FirstNameColumn", new Schema.CreditCard.FirstNameComparer());
			ReportCreditCard.sortMethods.Add("HeatIndexColumn", new Schema.CreditCard.HeatIndexComparer());
			ReportCreditCard.sortMethods.Add("LastNameColumn", new Schema.CreditCard.LastNameComparer());
			ReportCreditCard.sortMethods.Add("RuleIdColumn", new Schema.CreditCard.RuleIdComparer());
			ReportCreditCard.sortMethods.Add("OriginalAccountNumberColumn", new Schema.CreditCard.OriginalAccountNumberComparer());	
			ReportCreditCard.sortMethods.Add("SocialSecurityColumn", new Schema.CreditCard.SocialSecurityNumberComparer());

		}

		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ReportCreditCard()
		{

			// All records in the presentation layer of the report require a unique identifier.  When the report is updated, this
			// identifier is used to map the data to an existing record or to create a new one.  The starting point for the report
			// is the header record which uses this identifier.  The rest of the records in the report will generally use the
			// source DataRow as the unique identifier.
			this.guid = Guid.NewGuid();

			this.reportId = Guid.Empty;

			// These objects are required for sorting, filtering and ordering the report.
			this.prefilter = new ComplexFilter<CreditCardRow>();
			this.prefilter.Add(this.FilterBlotters);
			this.filter = new ComplexFilter<Schema.CreditCard.CreditCard>();
			this.comparer = new ComplexComparer<Schema.CreditCard.CreditCard>();
			this.comparer.Add(new Schema.CreditCard.AccountBalanceComparer(), SortOrder.Ascending);

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
				IComparer<Schema.CreditCard.CreditCard> comparer;
				if (ReportCreditCard.sortMethods.TryGetValue(sortItem.Column.ColumnId, out comparer))
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
						DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.CreditCardDetail).ReportTypeId);

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
			DataModel.Entity.EntityRowChanged += new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged += new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.CreditCard.CreditCardRowChanged += new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.CreditCard.CreditCardRowDeleted += new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.Consumer.ConsumerRowChanged += new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.Consumer.ConsumerRowDeleted += new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.Match.MatchRowChanged += new MatchRowChangeEventHandler(OnMatchRowChanged);
			DataModel.Match.MatchRowDeleted += new MatchRowChangeEventHandler(OnMatchRowChanged);
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

			DataModel.Blotter.BlotterRowChanged -= new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Blotter.BlotterRowDeleted -= new BlotterRowChangeEventHandler(OnBlotterRowChanged);
			DataModel.Report.ReportRowChanged -= new ReportRowChangeEventHandler(OnReportRowChanged);
			DataModel.Entity.EntityRowChanged -= new EntityRowChangeEventHandler(OnEntityRowChanged);
			DataModel.EntityTree.EntityTreeRowChanged -= new EntityTreeRowChangeEventHandler(OnEntityTreeRowChanged);
			DataModel.CreditCard.CreditCardRowChanged -= new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.CreditCard.CreditCardRowDeleted -= new CreditCardRowChangeEventHandler(OnCreditCardRowChanged);
			DataModel.Consumer.ConsumerRowChanged -= new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.Consumer.ConsumerRowDeleted -= new ConsumerRowChangeEventHandler(OnConsumerRowChanged);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);

		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ReportCreditCard.AnimationSpeedProperty); }
			set { this.SetValue(ReportCreditCard.AnimationSpeedProperty, value); }
		}

		/// <summary>
		/// ConsumerId
		/// </summary>
		public Guid ConsumerId
		{
			get { return this.consumerId; }
			set
			{
				this.consumerId = value;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(RefreshThread);
			}

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
			}

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
			ReportCreditCard reportPrototype = dependencyObject as ReportCreditCard;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ReportCreditCard.animationDurations[(Int32)animationSpeed];

		}

		/// <summary>
		/// Handles a change to the CreditCardRow table.
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


			// The content of the report is regenerated in a worker thread when the data related to this report has changed.  When
			// the content is regenerated, it will be sent to the foreground to be presented in the report.
			if (this.isDataChanged)
			{
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(RefreshThread);
				this.isDataChanged = false;
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
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles a change to the CreditCardRow table.
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
					CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;


					if (dataTableCoordiante.DataColumn == DataModel.CreditCard.DebtRuleIdColumn)
					{
						//make sure the value has changed before making a webSvc call
						object newDebtRuleId = comboBox.SelectedValue;
						if (newDebtRuleId == null || (Guid)newDebtRuleId == Guid.Empty)
							newDebtRuleId = DBNull.Value;
						object oldDebtRuleId = DBNull.Value;
						if (creditCardRow.IsDebtRuleIdNull() == false)
							oldDebtRuleId = creditCardRow.DebtRuleId;

						if (object.Equals(oldDebtRuleId, newDebtRuleId) == false)
						{
							FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
								TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { DebtRuleId = newDebtRuleId })));
						}
					}

				}

			}

		}

		private void OnTextChanged(object sender, RoutedEventArgs routedEventArgs)
		{
			TextBox textBox = routedEventArgs.OriginalSource as TextBox;
			ReportCreditCard report = sender as ReportCreditCard;

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
				CreditCardRow creditCardRow = iContent.Key as CreditCardRow;

				if (creditCardRow == null)
					return;

				foreach(ConsumerTrustNegotiationRow consumerTrustNegotiationRow in creditCardRow.GetConsumerTrustNegotiationRows())
					if(creditCardRow.CreditCardId == consumerTrustNegotiationRow.CreditCardId)
					{
						MatchRow matchRow = consumerTrustNegotiationRow.MatchRow;
						if(matchRow != null &&
							matchRow.IsHeatIndexDetailsNull() == false)
						{
							this.matchToolTipContent.SetDetails(matchRow.HeatIndexDetails);
							e.ToolTip.Content = this.matchToolTipContent;
						}

						break;
					}
			}
		}

		/// <summary>
		/// Handler for the textbox class.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="routedEventArgs"></param>
		private void OnTextBoxLostFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			TextBox textBox = routedEventArgs.OriginalSource as TextBox;
			ReportCreditCard report = sender as ReportCreditCard;

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
			ConsumerRow workingOrderRow = dataTableCoordiante.DataRow as ConsumerRow;
			string textBoxString = textBoxTxt;
			Guid workingOrderId = dataTableCoordiante.Association;

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.FirstNameColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { FirstName = textBoxString, WorkingOrderId = workingOrderId })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.LastNameColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { LastName = textBoxString, WorkingOrderId = workingOrderId })));

			if (dataTableCoordiante.DataColumn == DataModel.Consumer.SocialSecurityNumberColumn)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumer(new Consumer(workingOrderRow) { SocialSecurityNumber = textBoxString })));

			if (dataTableCoordiante.DataColumn == DataModel.CreditCard.AccountBalanceColumn)
			{
				CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { AccountBalance = textBoxString })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.CreditCard.OriginalAccountNumberColumn)
			{
				CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { OriginalAccountNumber = textBoxString })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.CreditCard.AccountNumberColumn)
			{
				CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { AccountNumber = textBoxString })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.CreditCard.DebtHolderColumn)
			{
				CreditCardRow creditCardRow = dataTableCoordiante.DataRow as CreditCardRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateCreditCard(new CreditCard(creditCardRow) { DebtHolder = textBoxString })));
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
				CreditCardRow CreditCardRow = dataTableCoordiante.DataRow as CreditCardRow;

			}

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
		/// Handles a change to the CreditCardRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnConsumerRowChanged(object sender, ConsumerRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

		}

		/// <summary>
		/// Handles a change to the MatchRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		void OnMatchRowChanged(object sender, MatchRowChangeEventArgs e)
		{
			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;
		}

		/// <summary>
		/// Handles a change to the CreditCardRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnCreditCardRowChanged(object sender, CreditCardRowChangeEventArgs e)
		{

			// When the merge is completed, this indicates that the document should be refreshed.
			this.isDataChanged = true;

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
				this.SetContent(this.CreditCardHeaderSelector(this.guid), requestGC);

			ThreadPoolHelper.QueueUserWorkItem(RefreshThreadCompleted);

		}

		private void RefreshThreadCompleted(object state)
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(false);

		}

		private Schema.CreditCardHeader.CreditCardHeader CreditCardHeaderSelector(Guid guid)
		{
			Schema.CreditCardHeader.CreditCardHeader CreditCardHeader = new Schema.CreditCardHeader.CreditCardHeader();
			CreditCardHeader.Prefilter = this.prefilter;
			CreditCardHeader.Selector = MatchSelector;
			CreditCardHeader.Filter = this.filter;
			CreditCardHeader.Comparer = this.comparer;
			return CreditCardHeader.Select(guid);
		}

		private Schema.CreditCard.CreditCard MatchSelector(CreditCardRow creditCardRow)
		{
			Schema.CreditCard.CreditCard match = new Schema.CreditCard.CreditCard();
			return match.Select(creditCardRow);
		}


		private bool FilterBlotters(CreditCardRow creditCardRow)
		{
			return creditCardRow.ConsumerId == this.ConsumerId;
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
		/// Handle selection with keyboard
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				if ((this.FocusedCell != null) && (this.FocusedCell.Content is FluidTrade.Guardian.Schema.CreditCard.SelectRow))
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
		/// Delete the selected rows.
		/// </summary>
		public void DeleteRows()
		{
			List<List<FluidTrade.Core.Windows.Controls.ReportRow>> selectedRowBlocks = reportGrid.SelectedRowBlocks;
			List<CreditCardRow> toDeleteRows = new List<CreditCardRow>();

			if ((selectedRowBlocks.Count < 1) && (reportGrid.CurrentReportCell != null))
			{

				if (reportGrid.CurrentReportCell != null)
				{

					FluidTrade.Core.Windows.Controls.ReportRow row = reportGrid.CurrentReportCell.ReportRow;

					if (row != null)
					{

						CreditCardRow workingOrderRow = GetandValidateWorkingRow(row);
						toDeleteRows.Add(workingOrderRow);
						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(DestroyRecords, toDeleteRows);

					}

				}

			}
			else
			{
				//Iterate over collections of selected items
				foreach (var selectedRows in selectedRowBlocks)
				{
					foreach (FluidTrade.Core.Windows.Controls.ReportRow row in selectedRows)
					{
						CreditCardRow workingOrderRow = GetandValidateWorkingRow(row);
						if (workingOrderRow != null)
						{
							toDeleteRows.Add(workingOrderRow);
						}
					}
				}

				if (toDeleteRows.Count > 0)
				{
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(DestroyRecords, toDeleteRows);
				}
			}
		}

		/// <summary>
		/// Safely retrieve workingOrderRow from a reportRow
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		private CreditCardRow GetandValidateWorkingRow(FluidTrade.Core.Windows.Controls.ReportRow row)
		{
			CreditCardRow workingOrderRow = row.NullSafe(datarow => datarow.IContent).NullSafe(Content => Content.Key) as CreditCardRow;

			try
			{
				if (workingOrderRow != null)
				{
					if (workingOrderRow.CreditCardId != null)
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
		/// Delete a list of credit card rows.
		/// </summary>
		/// <param name="state">The list to delete.</param>
		public void DestroyRecords(object state)
		{
			List<CreditCardRow> toDeleteRows = state as List<CreditCardRow>;
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				int recordsPerCall = 100;
				TradingSupportReference.CreditCard[] orders = null;
				int recordTotal = 0;
				int recordIndex = 0;

				foreach (CreditCardRow workingOrderRow in toDeleteRows)
				{

					if (recordIndex == 0)
						orders = new TradingSupportReference.CreditCard[
							toDeleteRows.Count - recordTotal < recordsPerCall ?
							toDeleteRows.Count - recordTotal :
							recordsPerCall];

					orders[recordIndex++] = new TradingSupportReference.CreditCard()
					{
						RowId = workingOrderRow.CreditCardId,
						RowVersion = workingOrderRow.RowVersion
					};

					if (recordIndex == orders.Length)
					{
						
						MethodResponseErrorCode response = tradingSupportClient.DeleteCreditCard(orders);
						if(!response.IsSuccessful)
						{
							ErrorCode error = response.Result;
							if (response.Errors.Length > 0)
								error = response.Errors[0].ErrorCode;
							if(error == ErrorCode.RecordExists)
								throw new IsSettledException(string.Format("{0} is settled", workingOrderRow.OriginalAccountNumber )); 
							else
								throw new Exception(String.Format("Server error {0}", response.Result));
						}
						recordTotal += recordIndex;
						recordIndex = 0;

					}

				}

			}
			catch(IsSettledException exception)
			{
				EventLog.Information("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Cannot delete Credit Card: " + exception.Message, Application.Current.MainWindow.Title)));
			}
			catch(Exception exception)
			{
				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, "Cannot delete Credit Card", Application.Current.MainWindow.Title)));
			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();

			}

		}

	}

}


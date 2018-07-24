namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
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

	/// <summary>Schema.DebtNegotiatorPaymentSummary.PaymentSummary
	/// 
	/// </summary>
	public class ReportDebtNegotiatorPaymentSummary : DynamicReport
	{
		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.PrototypeReport.AnimationSpeed dependency property.
		/// </summary>
		public static readonly DependencyProperty AnimationSpeedProperty;

		// Private Static Fields
		private static Duration[] animationDurations;
		private static Dictionary<String, IComparer<Schema.DebtNegotiatorPaymentSummary.PaymentSummary>> sortMethods;

		// Private Instance Fields
		private Guid consumerTrustSettlementId;
		private Guid blotterId;
		private ComplexComparer<Schema.DebtNegotiatorPaymentSummary.PaymentSummary> comparer;
		private ComplexFilter<Schema.DebtNegotiatorPaymentSummary.PaymentSummary> filter;
		private Guid guid;
		private Boolean isDataChanged;
		private ComplexFilter<ConsumerTrustPaymentRow> prefilter;
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
		static ReportDebtNegotiatorPaymentSummary()
		{

			// AnimationSpeed
			ReportDebtNegotiatorPaymentSummary.AnimationSpeedProperty = DependencyProperty.Register(
				"AnimationSpeed",
				typeof(AnimationSpeed),
				typeof(ReportDebtNegotiatorPaymentSummary),
				new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAnimationSpeedChanged)));

			// These constants control the animation speed.
			ReportDebtNegotiatorPaymentSummary.animationDurations = new Duration[]
			{
				new Duration(TimeSpan.FromMilliseconds(0)),
				new Duration(TimeSpan.FromMilliseconds(250)),
				new Duration(TimeSpan.FromMilliseconds(500)),
				new Duration(TimeSpan.FromMilliseconds(1000))
			};

			ReportDebtNegotiatorPaymentSummary.sortMethods = new Dictionary<string, IComparer<Schema.DebtNegotiatorPaymentSummary.PaymentSummary>>();
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("ActiveFlagColumn", new Schema.DebtNegotiatorPaymentSummary.IsActiveComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("ActualPaymentDateColumn", new Schema.DebtNegotiatorPaymentSummary.ActualPaymentDateComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("ActualPaymentValueColumn", new Schema.DebtNegotiatorPaymentSummary.ActualPaymentValueComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("CheckIdColumn", new Schema.DebtNegotiatorPaymentSummary.CheckIdComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("ClearedDateColumn", new Schema.DebtNegotiatorPaymentSummary.ClearedDateComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("CreatedDateTimeColumn", new Schema.DebtNegotiatorPaymentSummary.CreatedDateTimeComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("CreatedUserIdColumn", new Schema.DebtNegotiatorPaymentSummary.CreatedUserIdComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("EffectivePaymentDateColumn", new Schema.DebtNegotiatorPaymentSummary.EffectivePaymentDateComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("EffectivePaymentValueColumn", new Schema.DebtNegotiatorPaymentSummary.EffectivePaymentValueComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("Fee0Column", new Schema.DebtNegotiatorPaymentSummary.Fee0Comparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("IsClearedColumn", new Schema.DebtNegotiatorPaymentSummary.IsClearedComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("Memo0Column", new Schema.DebtNegotiatorPaymentSummary.Memo0Comparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("Memo1Column", new Schema.DebtNegotiatorPaymentSummary.Memo1Comparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("Memo2Column", new Schema.DebtNegotiatorPaymentSummary.Memo2Comparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("ModifiedDateTimeColumn", new Schema.DebtNegotiatorPaymentSummary.ModifiedDateTimeComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("ModifiedUserIdColumn", new Schema.DebtNegotiatorPaymentSummary.ModifiedUserIdComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("StatusIdColumn", new Schema.DebtNegotiatorPaymentSummary.StatusComparer());
			ReportDebtNegotiatorPaymentSummary.sortMethods.Add("TrackingNumberColumn", new Schema.DebtNegotiatorPaymentSummary.TrackingNumberComparer());

		}

		/// <summary>
		/// This is an example of how to override the MarkThree.Windows.Controls.Report class.
		/// </summary>
		public ReportDebtNegotiatorPaymentSummary()
		{

			// All records in the presentation layer of the report require a unique identifier.  When the report is updated, this
			// identifier is used to map the data to an existing record or to create a new one.  The starting point for the report
			// is the header record which uses this identifier.  The rest of the records in the report will generally use the
			// source DataRow as the unique identifier.
			this.guid = Guid.NewGuid();

			this.reportId = Guid.Empty;

			// These objects are required for sorting, filtering and ordering the report.
			this.prefilter = new ComplexFilter<ConsumerTrustPaymentRow>();
			this.prefilter.Add(this.FilterBlotters);
			this.filter = new ComplexFilter<Schema.DebtNegotiatorPaymentSummary.PaymentSummary>();
			this.comparer = new ComplexComparer<Schema.DebtNegotiatorPaymentSummary.PaymentSummary>();
			this.comparer.Add(new Schema.DebtNegotiatorPaymentSummary.EffectivePaymentDateComparer(), SortOrder.Ascending);

			// This is needed to satisfy the compiler.  In practice, this value is loaded from the user settings and defaulted
			// through the same mechanism.
			this.AnimationSpeed = AnimationSpeed.Off;

			this.CommandBindings.Add(new CommandBinding(FluidTradeCommands.SortReport, OnSortReport));

			// These handlers will update the middle tier in response to changes in the report.
			this.AddHandler(FluidTrade.Actipro.DateTimePicker.DateTimeChangedEvent, new RoutedEventHandler(OnDateTimePicker));
			this.AddHandler(FluidTrade.Guardian.Windows.Controls.StatusComboBox.PersistentSelectedValueChangedEvent, new RoutedEventHandler(OnComboBoxChange));
			this.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnToggleButtonChange));
			this.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnToggleButtonChange));			
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
				IComparer<Schema.DebtNegotiatorPaymentSummary.PaymentSummary> comparer;
				if (ReportDebtNegotiatorPaymentSummary.sortMethods.TryGetValue(sortItem.Column.ColumnId, out comparer))
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

			ThreadPoolHelper.QueueUserWorkItem(LoadSource);

		}

		private void LoadSource(object state)
		{

			try
			{

				lock (DataModel.SyncRoot)
				{

					BlotterConfigurationRow blotterConfigurationRow = DataModel.BlotterConfiguration.BlotterConfigurationKeyBlotterIdReportTypeId.Find(
						this.blotterId,
						DataModel.ReportType.ReportTypeKeyReportTypeCode.Find(ReportType.PaymentSummary).ReportTypeId);

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
			DataModel.ConsumerTrustPayment.ConsumerTrustPaymentRowChanged += new ConsumerTrustPaymentRowChangeEventHandler(OnConsumerTrustPaymentRowChanged);
			DataModel.ConsumerTrustPayment.ConsumerTrustPaymentRowDeleted += new ConsumerTrustPaymentRowChangeEventHandler(OnConsumerTrustPaymentRowDeleted);
			DataModel.EndMerge += new EventHandler(OnEndMerge);

		}

		void OnConsumerTrustPaymentRowDeleted(object sender, ConsumerTrustPaymentRowChangeEventArgs e)
		{
			this.isDataChanged = true;

		}

		void OnConsumerTrustPaymentRowChanged(object sender, ConsumerTrustPaymentRowChangeEventArgs e)
		{
			this.isDataChanged = true;

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
			DataModel.ConsumerTrustPayment.ConsumerTrustPaymentRowChanged -= new ConsumerTrustPaymentRowChangeEventHandler(OnConsumerTrustPaymentRowChanged);
			DataModel.ConsumerTrustPayment.ConsumerTrustPaymentRowDeleted -= new ConsumerTrustPaymentRowChangeEventHandler(OnConsumerTrustPaymentRowDeleted);
			DataModel.EndMerge -= new EventHandler(OnEndMerge);

		}

		/// <summary>
		/// Gets of sets the global animation speed of the application.
		/// </summary>
		public AnimationSpeed AnimationSpeed
		{
			get { return (AnimationSpeed)this.GetValue(ReportDebtNegotiatorPaymentSummary.AnimationSpeedProperty); }
			set { this.SetValue(ReportDebtNegotiatorPaymentSummary.AnimationSpeedProperty, value); }
		}

		/// <summary>
		/// ConsumerTrustSettlementId
		/// </summary>
		public Guid ConsumerTrustSettlementId
		{
			get { return this.consumerTrustSettlementId; }
			set
			{
				this.consumerTrustSettlementId = value;
				ThreadPoolHelper.QueueUserWorkItem(RefreshThread, false);
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
			ReportDebtNegotiatorPaymentSummary reportPrototype = dependencyObject as ReportDebtNegotiatorPaymentSummary;
			AnimationSpeed animationSpeed = (AnimationSpeed)dependencyPropertyChangedEventArgs.NewValue;
			reportPrototype.Duration = ReportDebtNegotiatorPaymentSummary.animationDurations[(Int32)animationSpeed];

		}

		/// <summary>
		/// Handles a change to the PaymentRow table.
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
				ThreadPoolHelper.QueueUserWorkItem(RefreshThread, false);
				this.isDataChanged = false;
			}


		}
		
		private void OnTextChanged(object sender, RoutedEventArgs routedEventArgs)
		{
			TextBox textBox = routedEventArgs.OriginalSource as TextBox;
			ReportDebtNegotiatorPaymentSummary report = sender as ReportDebtNegotiatorPaymentSummary;

			if (textBox == null || report == null)
				return;

			if (InputHelper.IsUserInitiated(textBox, TextBox.TextProperty))
			{
				report.CurrentSelectionChanged = true;
			}

		}

		//private FluidTrade.Guardian.Windows.Controls.MatchPartsUserControl matchToolTipContent;
		
		/// <summary>
		/// Handle the grid requesting tooltip content
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnShowToolTipHandler(object sender, ReportGridtToolTipEventArgs e)
		{			
		}

		/// <summary>
		/// Handler for the textbox class.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="routedEventArgs"></param>
		private void OnTextBoxLostFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			TextBox textBox = routedEventArgs.OriginalSource as TextBox;
			ReportDebtNegotiatorPaymentSummary report = sender as ReportDebtNegotiatorPaymentSummary;

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
		/// Handle sending data down to the server for a text box changed event.
		/// </summary>
		/// <param name="textBoxTxt"></param>
		/// <param name="iContent"></param>
		private void TextBoxValueChanged(string textBoxTxt, IContent iContent)
		{
			DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
			ConsumerRow workingOrderRow = dataTableCoordiante.DataRow as ConsumerRow;
			string textBoxString = textBoxTxt;
			Guid workingOrderId = dataTableCoordiante.Association;

			if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.CheckIdColumn)
			{
				ConsumerTrustPaymentRow consumerTrustPaymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(consumerTrustPaymentRow) { CheckId = textBoxTxt })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.Fee0Column)
			{
				Decimal result;
				bool parsed = Decimal.TryParse(textBoxTxt, out result);
				if (!parsed)
					result = 0.0m;

				ConsumerTrustPaymentRow consumerTrustPaymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(consumerTrustPaymentRow) { Fee0 = result })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.ActualPaymentValueColumn)
			{
				Decimal result;
				bool parsed = Decimal.TryParse(textBoxTxt, out result);
				if (!parsed)
					result = 0.0m;

				ConsumerTrustPaymentRow consumerTrustPaymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(consumerTrustPaymentRow) { ActualPaymentValue = result })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.Memo0Column)
			{
				ConsumerTrustPaymentRow consumerTrustPaymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(consumerTrustPaymentRow) { Memo0 = textBoxTxt })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.Memo1Column)
			{
				ConsumerTrustPaymentRow consumerTrustPaymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(consumerTrustPaymentRow) { Memo1 = textBoxTxt })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.Memo2Column)
			{
				ConsumerTrustPaymentRow consumerTrustPaymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(consumerTrustPaymentRow) { Memo2 = textBoxTxt })));
			}

			if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.TrackingNumberColumn)
			{
				ConsumerTrustPaymentRow consumerTrustPaymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(consumerTrustPaymentRow) { TrackingNumber = textBoxTxt })));
			}

		}


		/// <summary>
		/// Handle sending data down to the server for a combo box changed event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="routedEventArgs"></param>
		private void OnComboBoxChange(object sender, RoutedEventArgs routedEventArgs)
		{
			FluidTrade.Guardian.Windows.Controls.StatusComboBox originalSourceStatusComboBox = routedEventArgs.OriginalSource as FluidTrade.Guardian.Windows.Controls.StatusComboBox;
			if (originalSourceStatusComboBox != null)
			{
				IContent iContent = originalSourceStatusComboBox.DataContext as IContent;
				FluidTrade.Guardian.Windows.Controls.StatusComboBox sourceStatusComboBox = routedEventArgs.Source as FluidTrade.Guardian.Windows.Controls.StatusComboBox;

				if (InputHelper.IsUserInitiated(originalSourceStatusComboBox, FluidTrade.Guardian.Windows.Controls.StatusComboBox.PersistentSelectedValueProperty) &&
					(sourceStatusComboBox != null) &&
					(iContent != null) &&
					(iContent.Key is DataTableCoordinate))
				{
					// At this point, a ComboBox was modified by the user and it is connected to a data model field.  This will 
					// extract the coordinates of the field in the table.  That, in turn, drives the decision about how to update the
					// shared data model.
					DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
					ConsumerTrustPaymentRow paymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
					
					// Update the Payment Status (StatusId) column.
					if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.StatusIdColumn)
					{

					Guid selectedItem = (Guid)sourceStatusComboBox.SelectedValue;
						Guid persistedSelectedItem = (Guid)sourceStatusComboBox.PersistentSelectedValue;

					    FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
					        TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(paymentRow) { StatusId = persistedSelectedItem })));
					}

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
					ConsumerTrustPaymentRow paymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
					
					// Update the Cleared Date column.
					if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.ClearedDateColumn)
					{
						if (sourceDateTimePicker.DateTime != null)
						{
							DateTime selectedItem = (DateTime)sourceDateTimePicker.DateTime;
							FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
								TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(paymentRow) { ClearedDate = selectedItem })));
						}
					}

					// Update the Actual Payment Date column.
					if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.ActualPaymentDateColumn)
					{
						if (sourceDateTimePicker.DateTime != null)
						{
							DateTime selectedItem = (DateTime)sourceDateTimePicker.DateTime;
							FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
								TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(paymentRow) { ActualPaymentDate = selectedItem })));
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
				bool toggleButtonState = toggleButton.IsChecked.GetValueOrDefault();

				// At this point, a ToggleButton was modified by the user and it is connected to a data model field.  This will 
				// extract the coordinates of the field in the table.  That, in turn, drives the decision about how to update the
				// shared data model.
				DataTableCoordinate dataTableCoordiante = iContent.Key as DataTableCoordinate;
				ConsumerTrustPaymentRow paymentRow = dataTableCoordiante.DataRow as ConsumerTrustPaymentRow;
				// Update the IsActive column.
				if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.IsActiveColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(paymentRow) { IsActive = toggleButtonState })));
				// Update the IsCleared column.
				if (dataTableCoordiante.DataColumn == DataModel.ConsumerTrustPayment.IsClearedColumn)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(UpdateField, new Func<MethodResponseErrorCode>(() =>
						TradingSupportWebService.UpdateConsumerTrustPayment(new ConsumerTrustPayment(paymentRow) { IsCleared = toggleButtonState })));

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
		/// Updates the data in the report.
		/// </summary>
		public override void Refresh()
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(true);

			ThreadPoolHelper.QueueUserWorkItem(RefreshThread, false);

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
				this.SetContent(this.PaymentSummaryHeaderSelector(this.guid), requestGC);

			ThreadPoolHelper.QueueUserWorkItem(RefreshThreadCompleted);

		}

		private void RefreshThreadCompleted(object state)
		{

			if (this.reportGrid != null)
				this.reportGrid.UpdateBodyCanvasCursor(false);

		}

		private Schema.DebtNegotiatorPaymentSummaryHeader.PaymentHeader PaymentSummaryHeaderSelector(Guid guid)
		{
			Schema.DebtNegotiatorPaymentSummaryHeader.PaymentHeader paymentSummaryHeader = new Schema.DebtNegotiatorPaymentSummaryHeader.PaymentHeader();
			paymentSummaryHeader.Prefilter = this.prefilter;
			paymentSummaryHeader.Selector = MatchSelector;
			paymentSummaryHeader.Filter = this.filter;
			paymentSummaryHeader.Comparer = this.comparer;
			return paymentSummaryHeader.Select(guid);
		}

		private Schema.DebtNegotiatorPaymentSummary.PaymentSummary MatchSelector(ConsumerTrustPaymentRow paymentRow)
		{
			Schema.DebtNegotiatorPaymentSummary.PaymentSummary match = new Schema.DebtNegotiatorPaymentSummary.PaymentSummary();
			return match.Select(paymentRow);
		}


		private bool FilterBlotters(ConsumerTrustPaymentRow paymentRow)
		{
			return paymentRow.ConsumerTrustSettlementId == this.ConsumerTrustSettlementId;
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
				DeleteRows();
				//e.Handled = true;
			}

			// The base class handles any keys not handled above.
			base.OnPreviewKeyDown(e);
		}

		/// <summary>
		/// Delete the selected rows.
		/// </summary>
		public void DeleteRows()
		{
		}

		/// <summary>
		/// Safely retrieve workingOrderRow from a reportRow
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		private ConsumerTrustPaymentRow GetandValidateWorkingRow(FluidTrade.Core.Windows.Controls.ReportRow row)
		{
			ConsumerTrustPaymentRow workingOrderRow = row.NullSafe(datarow => datarow.IContent).NullSafe(Content => Content.Key) as ConsumerTrustPaymentRow;

			try
			{
				if (workingOrderRow != null)
				{
					if (workingOrderRow.ConsumerTrustPaymentId != null)
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
		
	}

}


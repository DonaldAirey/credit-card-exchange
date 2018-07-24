namespace FluidTrade.Guardian
{
	using System;
	using System.Data;
	using System.Linq;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media.Imaging;
	using System.Windows.Threading;
	using ActiproSoftware.Windows;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;
	using FluidTrade.Guardian.Windows.Controls;

	/// <summary>
	/// Interaction logic for DebtHolderNegotiationConsole.xaml
	/// </summary>
	public partial class DebtHolderNegotiationConsole : UserControl
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty CanRejectProperty; 

		/// <summary>
		/// The primary identifier of this negotiation.
		/// </summary>
		private static DependencyProperty MatchIdProperty;

		/// <summary>
		/// Indicates whether event handlers have been linked into this control or not.
		/// </summary>
		private Boolean isLoaded;

		/// <summary>
		/// The matchId Guid used by the background thread to identify the current matched working order.
		/// </summary>
		private Guid matchId;

		/// <summary>
		/// An image object used by the GUI to identify if an offer condition has been met by both the offer and counter sides.
		/// </summary>
		private BitmapImage offerMetImage;

		/// <summary>
		/// An image object used by the GUI to identify if an offer condition has not been met by both the offer and counter sides.
		/// </summary>
		private BitmapImage offerNotMetImage;

		/// <summary>
		/// Used to synchronize access to critical data items in this object.
		/// </summary>
		private Object syncObj;

		/// <summary>
		/// Delegate used to update the Chat window.
		/// </summary>
		/// <param name="chatItem">An element of the conversation in the 'Chat' window.</param>
		private delegate void ChatItemDelegate(ChatItem chatItem);

		/// <summary>
		/// Delegate used to update the parameters of the negotiation for a Consumer Debt settlement.
		/// </summary>
		/// <param name="negotiationItem">Contains all the elements of a negotiation for a Consumer Debt settlement.</param>
		private delegate void NegotiationItemDelegate(NegotiationItem negotiationItem);

		/// <summary>
		/// Delegate used to update the PaymentMethod object to the GUI.
		/// </summary>
		/// <param name="paymentMethodInfo">The PaymentMethodInfo object.</param>
		private delegate void PaymentMethodDelegate(Guid paymentMethodTypeId);

		/// <summary>
		/// Delegate used to update the SettlementInfo object to the GUI.
		/// </summary>
		/// <param name="settlementItem">The SettlementInfo object.</param>
		private delegate void SettlementInfoDelegate(SettlementItem settlementItem);

		/// <summary>
		/// Delegate used to update the status of the negotiation.
		/// </summary>
		/// <param name="status">The SettlementInfo object.</param>
		private delegate void StatusDelegate(Status status);

		/// <summary>
		/// Creates the static resources used by this type.
		/// </summary>
		static DebtHolderNegotiationConsole()
		{

			// This dependency property indicates the match that is displayed in the console.
			DebtHolderNegotiationConsole.MatchIdProperty = DependencyProperty.Register(
				"MatchId",
				typeof(Guid),
				typeof(DebtHolderNegotiationConsole),
				new PropertyMetadata(DebtHolderNegotiationConsole.OnMatchIdChanged));

			DebtHolderNegotiationConsole.CanRejectProperty = DependencyProperty.Register(
				"CanReject", 
				typeof(bool), 
				typeof(DebtHolderNegotiationConsole));
		}

		/// <summary>
		/// Creates a console used to negotiate a settlement.
		/// </summary>
		public DebtHolderNegotiationConsole()
		{

			// Initialize the object.
			this.isLoaded = false; 
			this.syncObj = new Object();
			this.CanReject = false;

			// The IDE managed resources are initialized here.
			InitializeComponent();

			// These images are used to indicate when there are compatible parameters in the negotiation console.
			this.offerMetImage = new BitmapImage(new Uri(@"/FluidTrade.DebtHolderNegotiationConsole;component/Resources/Check.png", UriKind.Relative));
			this.offerNotMetImage = new BitmapImage(new Uri(@"/FluidTrade.DebtHolderNegotiationConsole;component/Resources/Warning.png", UriKind.Relative));

			// These events allow the user to enter the settlement amount in either percent or market value.
			this.holderSettlementPercentage.ValueChanged += new EventHandler<PropertyChangedRoutedEventArgs<decimal>>(this.OnHolderSettlementPercentageChanged);
			this.holderSettlementMarketValue.ValueChanged += new DependencyPropertyChangedEventHandler(this.OnHolderSettlementMarketValueChanged);
			this.negotiatorSettlementPercentage.ValueChanged += new EventHandler<PropertyChangedRoutedEventArgs<decimal>>(this.OnNegotiatorSettlementPercentChanged);

			// These handlers will take care of installing and removing the data model event handlers used by the background processes.
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

        }

		/// <summary>
		/// The instructions for the Chat window.
		/// </summary>
		public FlowDocument FlowDocument
		{

			get
			{

				// Create the chat window placeholder text resource.
				FlowDocument flowDocument = new FlowDocument();
				flowDocument.Foreground = System.Windows.Media.Brushes.Gray;
				flowDocument.FontSize = 11.0;
				flowDocument.Blocks.Add(new Paragraph(new Bold(new Run("Welcome to the Negotiation Manager"))));
				flowDocument.Blocks.Add(
					new Paragraph(
						new Run("This manager facilitates account settlement negotiation for matched accounts, which are accounts with a valid counterparty account as determined by the matching rules engine. The list of matched accounts is also displayed in the Inventory Manager, which lists all matched and unmatched accounts. The match account rules engine sets the matched state when new accounts are imported or the account is updated in the Inventory Manager.")));
				flowDocument.Blocks.Add(
					new Paragraph(
						new Run("To negotiate a settlement with the Negotiation Manager, select an account, review the account and counterparty settlement terms in the Negotiation pane, update and offer settlement terms, and accept or reject the counterparty terms. Optionally, use the Chat facility for free form negotiation. After both parties accept the settlement terms, the negotiation is complete and the account moves from the Negotiation Manager to the Settlement Manager.")));
				return flowDocument;

			}

		}

		/// <summary>
		/// Ability to reject or not
		/// </summary>
		public bool CanReject
		{
			get { return (bool)GetValue(CanRejectProperty); }
			set { SetValue(CanRejectProperty, value); }
		}
		/// <summary>
		/// MatchId associated to this negotiation session.
		/// </summary>
		public Guid MatchId
		{
			get { return (Guid)this.GetValue(DebtHolderNegotiationConsole.MatchIdProperty); }
			set { this.SetValue(DebtHolderNegotiationConsole.MatchIdProperty, value); }
		}

		/// <summary>
		/// Adds a selected payment method to the offer.
		/// </summary>
		/// <param name="paymentMethodTypeId">The payment method to be removed.</param>
		private void AddHolderPaymentMethod(Guid paymentMethodTypeId)
		{

			// Add the new payment method to the methods shown in the console.  Note that the value may already be selected because the user has just updated 
			// it. In this scenario, the server will get the instruction to update the payment types and that change will propagate back out to the client 
			// where it will trigger this handler.  Alternatively, this may be another user watching the same transaction.  In this scenario, this event
			// handler will not find the payment type already selected in the combo box.
			if (!this.holderPaymentMethods.SelectedValues.Contains(paymentMethodTypeId))
				this.holderPaymentMethods.SelectedValues.Add(paymentMethodTypeId);

			// Indicate whether or not there is a common payment method available.
			ValidatePaymentMethods();

		}

		/// <summary>
		/// Adds a selected payment method to the offer.
		/// </summary>
		/// <param name="paymentMethodTypeId">The payment method to be removed.</param>
		private void AddNegotiatorPaymentMethod(Guid paymentMethodTypeId)
		{

			// Add the new payment method to the methods shown in the console.  Note that the value may already be selected because the user has just updated 
			// it. In this scenario, the server will get the instruction to update the payment types and that change will propagate back out to the client 
			// where it will trigger this handler.  Alternatively, this may be another user watching the same transaction.  In this scenario, this event
			// handler will not find the payment type already selected in the combo box.
			if (!this.negotiatorPaymentMethods.SelectedValues.Contains(paymentMethodTypeId))
				this.negotiatorPaymentMethods.SelectedValues.Add(paymentMethodTypeId);

			// Indicate whether or not there is a common payment method available.
			ValidatePaymentMethods();

		}

		/// <summary>
		/// Removes a selected payment method from the offer.
		/// </summary>
		/// <param name="paymentMethodTypeId">The payment method to be removed.</param>
		private void DeleteHolderPaymentMethod(Guid paymentMethodTypeId)
		{

			// Remove the payment method from the methods shown in the console.  Note that the value may already be unselected because the user has just
			// updated it. In this scenario, the server will get the instruction to remove the payment method types and that change will propagate back out to
			// the client where it will trigger this handler.  Alternatively, this may be another user watching the same transaction.  In this scenario, this
			// event handler will not find the payment type already selected in the combo box.
			if (this.holderPaymentMethods.SelectedValues.Contains(paymentMethodTypeId))
				this.holderPaymentMethods.SelectedValues.Remove(paymentMethodTypeId);

			// Indicate whether or not there is a common payment method available.
			ValidatePaymentMethods();

		}

		/// <summary>
		/// Removes a selected payment method from the offer.
		/// </summary>
		/// <param name="paymentMethodTypeId">The payment method to be removed.</param>
		private void DeleteNegotiatorPaymentMethod(Guid paymentMethodTypeId)
		{

			// Remove the payment method from the methods shown in the console.  Note that the value may already be unselected because the user has just
			// updated it. In this scenario, the server will get the instruction to remove the payment method types and that change will propagate back out to
			// the client where it will trigger this handler.  Alternatively, this may be another user watching the same transaction.  In this scenario, this
			// event handler will not find the payment type already selected in the combo box.
			if (this.negotiatorPaymentMethods.SelectedValues.Contains(paymentMethodTypeId))
				this.negotiatorPaymentMethods.SelectedValues.Remove(paymentMethodTypeId);

			// Indicate whether or not there is a common payment method available.
			ValidatePaymentMethods();

		}

		/// <summary>
		/// Subscribe to DataModel events.
		/// </summary>
		/// <param name="state">The unused thread initialization parameter.</param>
		private void InstallEventHandlers(Object state)
		{

			// This will listen to all the changes made to the tables involved with the negotiations.  When new data is available, these events handlers will
			// drive the new information into the chat and negotiation windows.
	lock (DataModel.SyncRoot)
			{
				DataModel.Chat.ChatRowChanging += OnChatRowChanging;
				DataModel.Match.MatchRowChanged += OnMatchRowChanged;
				DataModel.Match.MatchRowDeleting += OnMatchRowDeleting;
				DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationRowChanged += OnConsumerDebtNegotiationRowChanged;
				DataModel.ConsumerDebtNegotiationOfferPaymentMethod.ConsumerDebtNegotiationOfferPaymentMethodRowChanging +=
					OnConsumerDebtNegotiationOfferPaymentMethodRowChanging;
				DataModel.ConsumerDebtNegotiationCounterPaymentMethod.ConsumerDebtNegotiationCounterPaymentMethodRowChanging +=
					OnConsumerDebtNegotiationCounterPaymentMethodRowChanging;
				DataModel.ConsumerDebtNegotiationOfferPaymentMethod.ConsumerDebtNegotiationOfferPaymentMethodRowDeleting +=
					OnConsumerDebtNegotiationOfferPaymentMethodRowDeleting;
				DataModel.ConsumerDebtNegotiationCounterPaymentMethod.ConsumerDebtNegotiationCounterPaymentMethodRowDeleting +=
					OnConsumerDebtNegotiationCounterPaymentMethodRowDeleting;
			}

		}

		/// <summary>
		/// Handles a single click to the 'Accept' button.
		/// </summary>
		/// <param name="sender">The object that initiated this event.</param>
		/// <param name="eventArgs">The unused event arguments.</param>
		private void OnButtonAcceptClick(Object sender, RoutedEventArgs eventArgs)
		{
			// Show settlement confirmation screen.
			PreviewWindow previewWindow = new PreviewWindow();
			previewWindow.AccountBalance = this.accountBalance.Value;
			previewWindow.SettlementMarketValue = this.negotiatorSettlementMarketValue.Value;
			previewWindow.SettlementPercent = this.negotiatorSettlementPercentage.Value;
			previewWindow.Payments = this.negotiatorPayments.Value;
			previewWindow.StartDateLength = this.negotiatorStartDate.Length;
			previewWindow.StartDateUnit = this.negotiatorStartDate.TimeUnitId;
			previewWindow.Loaded += (s, e) =>
				previewWindow.PaymentMethods = this.holderPaymentMethods.SelectedValues;
			previewWindow.Owner = Application.Current.MainWindow;
			if (Convert.ToBoolean(previewWindow.ShowDialog()))
			{

				// This will construct an offer to match the counter party's offer.  Note that we don't want to change the payment methods.
				OfferItem offerItem = new OfferItem();
				offerItem.MatchId = this.MatchId;
				offerItem.PaymentLength = this.negotiatorPayments.Value;
				offerItem.PaymentStartDateLength = this.negotiatorStartDate.Length;
				offerItem.PaymentStartDateUnitId = this.negotiatorStartDate.TimeUnitId;
				offerItem.SettlementValue = this.negotiatorSettlementPercentage.Value;
				foreach (Guid method in this.holderPaymentMethods.SelectedValues)
				{
					offerItem.PaymentMethods.Add(method);
				}

				// Update the button to be disabled so that it will not send bad calls to the server.
//				UpdateButtons(false, false, true);

				// This sends the dialog item to the background to be passed on to the server.  Communication should never be done from the foreground thread.
				ThreadPoolHelper.QueueUserWorkItem(this.SettleAccount, offerItem);

			}

		}

		/// <summary>
		/// Handles a single click to the 'Offer' button.
		/// </summary>
		/// <param name="sender">The object that initiated this event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnButtonOfferClick(Object sender, RoutedEventArgs e)
		{

			// The dialog elements are extracted here in the foreground and sent to the background where they'll become part of the Web Service parameters that
			// will update the server.
			OfferItem offerItem = new OfferItem();
			offerItem.MatchId = this.MatchId;
			offerItem.PaymentLength = this.holderPayments.Value;
			offerItem.PaymentStartDateLength = this.holderStartDate.Length;
			offerItem.PaymentStartDateUnitId = this.holderStartDate.TimeUnitId;
			offerItem.SettlementValue = this.holderSettlementPercentage.Value;
			offerItem.StatusId = Guid.Empty;

			foreach (Guid method in this.holderPaymentMethods.SelectedValues)
			{
				offerItem.PaymentMethods.Add(method);
			}

			// This sends the dialog offer to the background to be passed on to the server.
			ThreadPoolHelper.QueueUserWorkItem(this.SendOfferInfo, offerItem);

		}

		/// <summary>
		/// Handles a single click to the 'Send' button.
		/// </summary>
		/// <param name="sender">The object that initiated this event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnButtonSendClick(Object sender, RoutedEventArgs e)
		{

			if (this.IsEnabled)
			{

				// Create a new chat element from the user's message.
				ChatItem chatItem = new ChatItem();
				chatItem.MatchId = this.MatchId;
				chatItem.Message = this.chatBox.Text;
				chatItem.CreatedTime = DateTime.UtcNow;

				//Display the message locally before sending it.
				UpdateChat(chatItem);

				// This sends the chat item to the background to be passed on to the server.
				ThreadPoolHelper.QueueUserWorkItem(this.SendChat, chatItem);

				// This initializes the chat controls for the next dialog item.
				this.chatBox.Text = String.Empty;
				string textString = this.Resources["ChatMessageString"].ToString();
				this.WatermarkText.Text = textString;
				this.buttonSend.IsEnabled = false;
				this.Focus();

			}

		}

		/// <summary>
		/// Handles a single click to the 'Reject' button.
		/// </summary>
		/// <param name="sender">The object that initiated this event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnButtonRejectClick(Object sender, RoutedEventArgs e)
		{
			if (this.CanReject)
			{
				// The dialog elements are extracted here in the foreground and sent to the background where they'll become part of the Web Service parameters that
				// will update the server.
				OfferItem offerItem = new OfferItem();
				offerItem.MatchId = this.MatchId;
				offerItem.PaymentLength = this.negotiatorPayments.Value;
				offerItem.PaymentStartDateLength = this.negotiatorStartDate.Length;
				offerItem.PaymentStartDateUnitId = this.negotiatorStartDate.TimeUnitId;
				offerItem.SettlementValue = this.negotiatorSettlementPercentage.Value;
				offerItem.StatusId = Guid.Empty;

				foreach (Guid method in this.negotiatorPaymentMethods.SelectedValues)
				{
					offerItem.PaymentMethods.Add(method);
				}
				// This sends the dialog offer to the background to be passed on to the server.
				ThreadPoolHelper.QueueUserWorkItem(this.RejectNegotiation, offerItem);
			}
		}

		/// <summary>
		/// Handles a change to the percent of the settlement from the Debt Holder's side.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The routed event arguments.</param>
		private void OnHolderSettlementPercentageChanged(object sender, PropertyChangedRoutedEventArgs<decimal> e)
		{

			// The settlement can be negotiated in terms of percentage or as a market value.  If a percentage is entered, the market value is calculated
			// accordingly.
			this.holderSettlementMarketValue.Value = this.holderSettlementPercentage.Value * this.accountBalance.Value;

		}

		/// <summary>
		/// Handles a change to the market value of the settlement from the Debt Holder's side.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The routed event arguments.</param>
		private void OnHolderSettlementMarketValueChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

			// The settlement can be negotiated in terms of percentage or as a market value.  If a market value is entered, the percentage is calculated 
			// accordingly.
			this.holderSettlementPercentage.Value = this.holderSettlementMarketValue.Value / this.accountBalance.Value;

		}

		/// <summary>
        /// Handles a change to the market value of the settlement from the Debt Negotiator's side.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The routed event arguments.</param>
		private void OnNegotiatorSettlementPercentChanged(object sender, PropertyChangedRoutedEventArgs<decimal> e)
        {

			// The settlement can be negotiated in terms of percentage or as a market value.  If a percentage is entered, the market value is calculated
			// accordingly.
			this.negotiatorSettlementMarketValue.Value = this.negotiatorSettlementPercentage.Value * this.accountBalance.Value;

        }

		/// <summary>
		/// Handles a change in the Chat table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="chatRowChangeEventArgs">The RowChanged event parameters.</param>
		private void OnChatRowChanging(Object sender, ChatRowChangeEventArgs chatRowChangeEventArgs)
		{

			// This will examine every committed record that hasn't been deleted for a new piece of the dialog.
			if (chatRowChangeEventArgs.Action == DataRowAction.Commit &&
				chatRowChangeEventArgs.Row.RowState != DataRowState.Detached &&
				chatRowChangeEventArgs.Row.HasVersion(DataRowVersion.Original) == false)
			{

				// Insure that all background fields are protected for this operation.
				lock (this.syncObj)
				{

					// This event handler is only interested in chat items intended for the selected Match.
					if (chatRowChangeEventArgs.Row.MatchId == this.matchId)
					{

						// Construct a single chat item from the newly added dialog that will be passed to the foreground.
						ChatItem chatItem = new ChatItem();
						chatItem.IsReply = chatRowChangeEventArgs.Row.IsReply;
						chatItem.MatchId = this.matchId;
						chatItem.CreatedTime = chatRowChangeEventArgs.Row.CreatedTime;
						chatItem.Message = chatRowChangeEventArgs.Row.Message;

						if (chatItem.IsReply)
						{
							// Dispatch to foreground thread to update the controls.
							this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ChatItemDelegate(this.UpdateChat), chatItem);
						}
					}

				}

			}

		}

		/// <summary>
		/// Handles a change in the ConsumerDebtNegotiationRow table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="chatRowChangeEventArgs">The RowChanged event parameters.</param>
		private void OnConsumerDebtNegotiationRowChanged(
			Object sender,
			ConsumerDebtNegotiationRowChangeEventArgs consumerDebtNegotiationRowChangeEventArgs)
		{

			// Whenever a change has been made to the negotiation record (and it's not a deleted record), the changes will be packed up and sent to the
			// foreground thread to update the controls.
			if (consumerDebtNegotiationRowChangeEventArgs.Action == DataRowAction.Commit && 
				consumerDebtNegotiationRowChangeEventArgs.Row.RowState != DataRowState.Detached)
			{

				// Protect the background fields while the new record is examined.
				lock (syncObj)
				{

					// Extract the modified record from the event arguments.
					ConsumerDebtNegotiationRow consumerDebtTrustNegotiationRow = consumerDebtNegotiationRowChangeEventArgs.Row;

					// Only the negotiation records that are appropriate for the currently viewed match are considered here.
					if (consumerDebtNegotiationRowChangeEventArgs.Row.MatchId == this.matchId)
					{

						// This structure is passed to the foreground so the updated fields can be presented to the user.
						NegotiationItem negotiationItem = new NegotiationItem(consumerDebtTrustNegotiationRow);

						// Dispatch to foreground thread for gui processing.
						this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NegotiationItemDelegate(this.UpdateNegotiation), negotiationItem);

					}

				}

			}

		}

		/// <summary>
		/// Handles a change in the debt-side CounterPaymentMethod table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="rowChangeEventArgs">The RowChanged event parameters.</param>
		private void OnConsumerDebtNegotiationCounterPaymentMethodRowChanging(
			Object sender,
			ConsumerDebtNegotiationCounterPaymentMethodRowChangeEventArgs rowChangeEventArgs)
		{

			// This will examine every record that is added to the list of available payment methods used by the debt holder.
			if (rowChangeEventArgs.Action == DataRowAction.Commit &&
				rowChangeEventArgs.Row.RowState != DataRowState.Detached &&
				rowChangeEventArgs.Row.HasVersion(DataRowVersion.Original) == false)
			{

				// lock foreground thread resources.
				lock (this.syncObj)
				{

					// The console is only interested in changes made to the payment methods associated with the currently displayed match.
					ConsumerDebtNegotiationRow consumerDebtNegotiationRow = DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(
						rowChangeEventArgs.Row.ConsumerDebtNegotiationId);

					// This event handler is only interested in items intended for the selected Match.
					if (consumerDebtNegotiationRow != null && consumerDebtNegotiationRow.MatchId == this.matchId)
					{

						// Dispatch to foreground thread to update the controls.
						this.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new PaymentMethodDelegate(this.AddNegotiatorPaymentMethod),
							rowChangeEventArgs.Row.PaymentMethodTypeId);

					}

				}

			}

		}

		/// <summary>
		/// Handles the event of a datarow is currently being deleted.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="rowDeletingEventArgs">The RowDeleting event parameters</param>
		private void OnConsumerDebtNegotiationCounterPaymentMethodRowDeleting(
			Object sender,
			ConsumerDebtNegotiationCounterPaymentMethodRowChangeEventArgs rowDeletingEventArgs)
		{

			// This will examine every record that is removed from the available payment methods used by the debt holder.
			if (rowDeletingEventArgs.Action == DataRowAction.Delete)
			{

				// This insures the background data will not be corrupted.
				lock (this.syncObj)
				{

					// The negotiation row is needed to determine if the updated payment methods are intended for the current negotiation in the console.
					ConsumerDebtNegotiationRow consumerDebtNegotiationRow = DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(
						rowDeletingEventArgs.Row.ConsumerDebtNegotiationId);

					// This event handler is only interested in items intended for the selected Match.
					if (consumerDebtNegotiationRow != null && consumerDebtNegotiationRow.MatchId == this.matchId)
					{

						// Update the foreground to remove the payment method for the debt negotiator.
						this.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new PaymentMethodDelegate(this.DeleteNegotiatorPaymentMethod),
							rowDeletingEventArgs.Row.PaymentMethodTypeId);

					}

				}

			}

		}

		/// <summary>
		/// Handles a change in the debt-side OfferPaymentMethod table.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="rowChangeEventArgs">The RowChanged event parameters.</param>
		private void OnConsumerDebtNegotiationOfferPaymentMethodRowChanging(
			Object sender,
			ConsumerDebtNegotiationOfferPaymentMethodRowChangeEventArgs rowChangeEventArgs)
		{

			// This will examine every record that is added to the list of acceptable payment methods used by the debt negotiator.
			if (rowChangeEventArgs.Action == DataRowAction.Commit &&
				rowChangeEventArgs.Row.RowState != DataRowState.Detached &&
				rowChangeEventArgs.Row.HasVersion(DataRowVersion.Original) == false)
			{

				// This insures the background data will not be corrupted.
				lock (this.syncObj)
				{

					// The negotiation row is needed to determine if the updated payment methods are intended for the current negotiation in the console.
					ConsumerDebtNegotiationRow consumerDebtNegotiationRow = DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(
						rowChangeEventArgs.Row.ConsumerDebtNegotiationId);

					// This event handler is only interested in items intended for the selected Match displayed in this console.
					if (consumerDebtNegotiationRow != null && consumerDebtNegotiationRow.MatchId == this.matchId)
					{

						// Dispatch to foreground thread to update the controls.
						this.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new PaymentMethodDelegate(this.AddHolderPaymentMethod),
							rowChangeEventArgs.Row.PaymentMethodTypeId);

					}

				}

			}

		}

		/// <summary>
		/// Handles the event of a datarow is currently being deleted.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="rowDeletingEventArgs">The RowDeleting event parameters</param>
		private void OnConsumerDebtNegotiationOfferPaymentMethodRowDeleting(
			Object sender,
			ConsumerDebtNegotiationOfferPaymentMethodRowChangeEventArgs rowDeletingEventArgs)
		{

			// This will examine every record that is removed the list of acceptable payment methods used by the negotiator.
			if (rowDeletingEventArgs.Action == DataRowAction.Delete)
			{

				// This insures the background data will not be corrupted.
				lock (this.syncObj)
				{

					// The console is only interested in changes made to the payment methods associated with the currently displayed match.
					ConsumerDebtNegotiationRow consumerDebtNegotiationRow = DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationKey.Find(
						rowDeletingEventArgs.Row.ConsumerDebtNegotiationId);

					// This event handler is only interested in items intended for the selected Match.
					if (consumerDebtNegotiationRow != null && consumerDebtNegotiationRow.MatchId == this.matchId)
					{

						// This will remove the payment method from the foreground controls.
						this.Dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new PaymentMethodDelegate(this.DeleteHolderPaymentMethod),
							rowDeletingEventArgs.Row.PaymentMethodTypeId);

					}

				}

			}

		}

		/// <summary>
		/// Handles the loading of this control from the user interface.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnLoaded(Object sender, RoutedEventArgs e)
		{

			// Due to an eccentricity with Tabbed Controls, the 'Loaded' event is fired twice.  Once when the object is created and again when the tab is
			// selected.  A strict interpretation of the conditions for firing this event would lead us to believe this is a Microsoft bug.  This gate-keeper
			// works around the bug and stops the event handler from installing the data model event handlers until they have been unloaded.
			if (!this.isLoaded)
			{

				// This will prevent the event handlers from being installed until the console is unloaded.
				this.isLoaded = true;

				// The event handlers must be installed from the background because they access the data model.
				ThreadPoolHelper.QueueUserWorkItem(InstallEventHandlers);

				// Force the GUI to re-synchronize with server in the event that the user moved to a different tab while the negotiation information was begin
				// changed.
				if (this.MatchId != Guid.Empty)
					ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(InitializeData), this.MatchId);

			}

		}

		/// <summary>
		/// Handle changes of the DependencyProperty that is registered with this event handler.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnMatchIdChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			// Extract the target of this property change from the parameters.
			DebtHolderNegotiationConsole negotiationConsoleBase = sender as DebtHolderNegotiationConsole;

			// Reset the state of the negotiation console to reflect the new match id.
			ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(negotiationConsoleBase.InitializeData), (Guid)eventArgs.NewValue);

		}

		/// <summary>
		/// Handle a change to the Match record.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnMatchRowChanged(object sender, MatchRowChangeEventArgs matchRowChangeEventArgs)
		{

			// This event handler will examine every change made to the Match rows looking for a change of state which must be applied to the user interface.
			if (matchRowChangeEventArgs.Action == DataRowAction.Change)
			{

				// This insures that the background fields are not corrupted by other threads.
				lock (this.syncObj)
				{

					// The console is only interested in changes made to the payment methods associated with the currently displayed match.
					MatchRow matchRow = matchRowChangeEventArgs.Row;

					// This event handler is only interested in items intended for the selected Match.
					if (matchRow.MatchId == this.matchId)
					{
						this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new StatusDelegate(this.UpdateStatus), matchRow.StatusRow.StatusCode);
					}

				}
			}

		}

		/// <summary>
		/// Handle a the Match record being deleted.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnMatchRowDeleting(object sender, MatchRowChangeEventArgs matchRowChangeEventArgs)
		{

			lock (this.syncObj)
			{

				// The console is only interested in changes made to the payment methods associated with the currently displayed match.
				MatchRow matchRow = matchRowChangeEventArgs.Row;

				if (matchRow.MatchId == this.matchId)
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementInfoDelegate(this.UpdateConsole), new SettlementItem());

			}

		}

		/// <summary>
		/// Handles the unloading of this control from the user interface.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnUnloaded(Object sender, RoutedEventArgs e)
		{

			// Due to an eccentricity with Tabbed Controls, the 'Loaded' event is fired twice.  Once when the object is created and again when the tab is
			// selected.  A strict interpretation of the conditions for firing this event would lead us to believe this is a Microsoft bug.  This gate-keeper
			// works around the bug and stops the event handler from installing the data model event handlers twice.
			if (this.isLoaded)
			{

				// Clearing this flag allows this object to be initialized again when selected again.
				this.isLoaded = false;

				// The event handlers must be removed from a background thread because they access the data model.
				ThreadPoolHelper.QueueUserWorkItem(UninstallEventHandlers);

				// Disable the GUI because this session is over.
				this.IsEnabled = false;

			}

		}

		/// <summary>
		/// Handles the user pressing the keyboard in the response TextBox.
		/// </summary>
		/// <param name="sender">The object that initiated this event.</param>
		/// <param name="e">The Object that contains the description of the key pressed.</param>
		private void OnChatBoxPreviewKeyUp(Object sender, KeyEventArgs e)
		{

			// The 'Enter' key is handled the same way as hitting the 'Send' button.
			if (e.Key == Key.Return && this.buttonSend.IsEnabled)
			{
				this.OnButtonSendClick(sender, e);
			}

		}

		/// <summary>
		/// Handles changes to the "ResponseBox" to validate the user entries.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnChatBoxTextChanged(Object sender, TextChangedEventArgs e)
		{
			if (this.buttonSend != null)
			{

				// Disable the 'Send' button when there is nothing to send.
				this.buttonSend.IsEnabled = this.IsEnabled && this.chatBox.Text.Length != 0;

			}
		}

		/// <summary>
		/// Initialize the data used by this control.
		/// </summary>
		private void InitializeData(Object parameter)
		{

			// This parameter is used by the background threads to give an identity to this negotiation console.
			this.matchId = (Guid)parameter;

			// This is a list of all the items currently in the chat window.  It is collected in the background and then sent to the foreground to be 
			// displayed.
			SettlementItem settlementItem = new SettlementItem(this.matchId);

			// Dispatch to foreground thread for updating the GUI.
			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SettlementInfoDelegate(this.UpdateConsole), settlementItem);

		}

		/// <summary>
		/// Add settlement related rows to the database depending on the operation type.
		/// </summary>
		/// <param name="state">Input parameter for this callback function.</param>
		private void SendOfferInfo(Object state)
		{

			// Extract the thread parameters.
			OfferItem offerItem = state as OfferItem;

			// This structure forms a contract with the Web Service that will update the negotiation with this new offer.  Some of the information for this
			// contract comes from the user interface and is passed to the background through this data structure.
			ConsumerDebtNegotiationInfo consumerDebtNegotiationInfo = new ConsumerDebtNegotiationInfo();
			consumerDebtNegotiationInfo.PaymentLength = offerItem.PaymentLength;
			consumerDebtNegotiationInfo.PaymentStartDateLength = offerItem.PaymentStartDateLength;
			consumerDebtNegotiationInfo.PaymentStartDateUnitId = offerItem.PaymentStartDateUnitId;
			consumerDebtNegotiationInfo.SettlementValue = offerItem.SettlementValue;
			consumerDebtNegotiationInfo.PaymentMethodTypes = offerItem.PaymentMethods.ToArray();
			consumerDebtNegotiationInfo.StatusId = offerItem.StatusId;

			// The rest of the information for the update to the offer comes from the data model.
			lock (DataModel.SyncRoot)
			{

				try
				{

					if(consumerDebtNegotiationInfo.StatusId == Guid.Empty)
						consumerDebtNegotiationInfo.StatusId = DataModel.Status.StatusKeyStatusCode.Find(Status.Negotiating).StatusId;

					// Some additional information is required for the Web Service that is not supplied by the user interface.  The data model can provide
					// these additional items from a background thread.
					MatchRow matchRow = DataModel.Match.MatchKey.Find(offerItem.MatchId);
					if (matchRow != null)
					{

						// The MatchId is used by the foreground to primarily identify the combination of parties, but it is the negotiation record has the
						// unique identifier for this negotitation.  It also contains the row version which is required for optimistic concurrency checking.
						foreach (ConsumerDebtNegotiationRow consumerDebtNegotiationRow in matchRow.GetConsumerDebtNegotiationRows())
						{

							// These items are needed to identify the negotiation and update it.
							consumerDebtNegotiationInfo.ConsumerDebtNegotiationId = consumerDebtNegotiationRow.ConsumerDebtNegotiationId;
							consumerDebtNegotiationInfo.RowVersion = consumerDebtNegotiationRow.RowVersion;

						}

						// The settlement units are fixed currently to use percentage.  In the future this should be based on a user setting in the UI.
						SettlementUnitRow settlementUnitRow = DataModel.SettlementUnit.SettlementUnitKeySettlementUnitCode.Find(SettlementUnit.Percent);
						consumerDebtNegotiationInfo.SettlementUnitId = settlementUnitRow.SettlementUnitId;


					}

				}
				catch (Exception exception)
				{

					// Any issues trying to communicate to the server are logged.
					EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				}

			}

			// This will create a channel to the middle tier web services.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);
			
			try
			{

				// Update the existing negotiation with the new information.
				tradingSupportClient.UpdateConsumerDebtNegotiation(new ConsumerDebtNegotiationInfo[] { consumerDebtNegotiationInfo });

			}
			catch (FaultException<OptimisticConcurrencyFault>)
			{

				//Supress the message
				//// Present the fault detail to the user in a foreground thread.
				//this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
				//    delegate()
				//    {
				//        AlertMessageBox.Instance.Show(faultException);
				//    }
				//));				

			}
			catch (Exception exception)
			{

				// Present the exception to the user in a foreground thread.
				this.Dispatcher.BeginInvoke(
					new Action(() => MessageBox.Show(Application.Current.MainWindow, exception.Message, Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
		}

		/// <summary>
		/// Send the Chat.
		/// </summary>
		/// <param name="state">Input parameter for this callback function.</param>
		private void SendChat(Object state)
		{

			// Extract the thread parameters.
			ChatItem chatItem = state as ChatItem;

			// This is the structure that will be passed to the web service.
			ChatInfo chatInfo = new ChatInfo();
			chatInfo.MatchId = chatItem.MatchId;
			chatInfo.ChatId = Guid.NewGuid();
			chatInfo.Message = chatItem.Message;

			// This will create a channel to the middle tier web services.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				// Update the existing negotiation with the new information.
				tradingSupportClient.CreateChat(new ChatInfo[] { chatInfo });

			}
			catch (Exception exception)
			{

				// Present the exception to the user in a foreground thread.
				this.Dispatcher.BeginInvoke(
					new Action(() => MessageBox.Show(Application.Current.MainWindow, exception.Message, Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
		}

		/// <summary>
		/// Reject negotiation 
		/// </summary>
		/// <param name="state">Input parameter for this callback function.</param>
		private void RejectNegotiation(Object state)
		{

			// Extract the thread parameters.
			OfferItem offerItem = state as OfferItem;

			// This structure forms a contract with the Web Service that will update the negotiation with this new offer.  Some of the information for this
			// contract comes from the user interface and is passed to the background through this data structure.
			ConsumerDebtNegotiationInfo consumerDebtNegotiationInfo = new ConsumerDebtNegotiationInfo();
			consumerDebtNegotiationInfo.PaymentLength = offerItem.PaymentLength;
			consumerDebtNegotiationInfo.PaymentStartDateLength = offerItem.PaymentStartDateLength;
			consumerDebtNegotiationInfo.PaymentStartDateUnitId = offerItem.PaymentStartDateUnitId;
			consumerDebtNegotiationInfo.SettlementValue = offerItem.SettlementValue;
			consumerDebtNegotiationInfo.PaymentMethodTypes = offerItem.PaymentMethods.ToArray();			

			// The rest of the information for the update to the offer comes from the data model.
			lock (DataModel.SyncRoot)
			{

				try
				{

					// Some additional information is required for the Web Service that is not supplied by the user interface.  The data model can provide
					// these additional items from a background thread.
					MatchRow matchRow = DataModel.Match.MatchKey.Find(offerItem.MatchId);
					if (matchRow != null)
					{

						// The MatchId is used by the foreground to primarily identify the combination of parties, but it is the negotiation record has the
						// unique identifier for this negotitation.  It also contains the row version which is required for optimistic concurrency checking.
						foreach (ConsumerDebtNegotiationRow consumerDebtNegotiationRow in matchRow.GetConsumerDebtNegotiationRows())
						{

							// These items are needed to identify the negotiation and update it.
							consumerDebtNegotiationInfo.ConsumerDebtNegotiationId = consumerDebtNegotiationRow.ConsumerDebtNegotiationId;
							consumerDebtNegotiationInfo.RowVersion = consumerDebtNegotiationRow.RowVersion;

						}

						// The settlement units are fixed currently to use percentage.  In the future this should be based on a user setting in the UI.
						SettlementUnitRow settlementUnitRow = DataModel.SettlementUnit.SettlementUnitKeySettlementUnitCode.Find(SettlementUnit.Percent);
						consumerDebtNegotiationInfo.SettlementUnitId = settlementUnitRow.SettlementUnitId;
						consumerDebtNegotiationInfo.StatusId = DataModel.Status.StatusKeyStatusCode.Find(Status.Rejected).StatusId;
					}

					// This will create a channel to the middle tier web services.
					TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

					try
					{

						// Update the existing negotiation with the new information.
						tradingSupportClient.RejectConsumerDebtNegotiation(new ConsumerDebtNegotiationInfo[] { consumerDebtNegotiationInfo });

					}
					catch (FaultException<OptimisticConcurrencyFault> faultException)
					{

						// Present the fault detail to the user in a foreground thread.
						this.Dispatcher.BeginInvoke(
							new Action(() => MessageBox.Show(
								Application.Current.MainWindow,
								String.Format(
									FluidTrade.Core.Properties.Resources.OptimisticConcurrencyError,									
									faultException.Detail.TableName),
								Application.Current.MainWindow.Title)));

					}
					catch (Exception exception)
					{

						// Present the exception to the user in a foreground thread.
						this.Dispatcher.BeginInvoke(
							new Action(() => MessageBox.Show(Application.Current.MainWindow, exception.Message, Application.Current.MainWindow.Title)));

					}
					finally
					{
						if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
							tradingSupportClient.Close();
					}

				}
				catch (Exception exception)
				{

					// Any issues trying to communicate to the server are logged.
					EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

				}

			}
		}

		
		/// <summary>
		/// Send the settlement request to the server.
		/// </summary>
		/// <param name="state">Input parameter for this callback function.</param>
		private void SettleAccount(Object state)
		{

			// Extract the match identifier from the thread start parameter.
			OfferItem offerItem = (OfferItem)state;
			Guid matchId = offerItem.MatchId;

			// Update the offer to match the counter offer before accepting the settlement.
			lock (DataModel.SyncRoot)
			{

				offerItem.StatusId = DataModel.Status.StatusKeyStatusCode.Find(Status.OfferAccepted).StatusId;
			}

			// Update the offer to match the counter offer before accepting the settlement.
			SendOfferInfo(offerItem);

			// Create a channel to the middle tier.
			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				// The Web Service call will require the ConsumerDebtNegotiationId which must be extracted from the data model.
				Guid consumerDebtNegotiationId = Guid.Empty;

				// The data model must be locked while the negotiation id is extracted from the match.  This identifier is used to settle the trade on the
				// server side.
				lock (DataModel.SyncRoot)
				{
					MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);
					long maxVersion = matchRow.GetConsumerDebtNegotiationRows().Max(negotitiaon => negotitiaon.Version);
					ConsumerDebtNegotiationRow consumerDebtNegotiationRow = matchRow.GetConsumerDebtNegotiationRows().Where(p => p.Version == maxVersion).Single();
					consumerDebtNegotiationId = consumerDebtNegotiationRow.ConsumerDebtNegotiationId;
				}

				// Update the database.
				tradingSupportClient.CreateConsumerDebtSettlement(consumerDebtNegotiationId);

			}
			catch (FaultException<OptimisticConcurrencyFault> faultException)
			{

				// Present the fault detail to the user in a foreground thread.
				this.Dispatcher.BeginInvoke(
					new Action(() => MessageBox.Show(
						Application.Current.MainWindow,
						String.Format(
							FluidTrade.Core.Properties.Resources.OptimisticConcurrencyError,
							faultException.Detail.TableName),
						Application.Current.MainWindow.Title)));

			}
			catch (FaultException<ArgumentFault> faultException)
			{

				// Present the fault detail to the user in a foreground thread.
				this.Dispatcher.BeginInvoke(
					new Action(() => MessageBox.Show(Application.Current.MainWindow, faultException.Detail.Message, Application.Current.MainWindow.Title)));

			}
			catch (FaultException<PaymentMethodFault> faultException)
			{

				// Present the fault detail to the user in a foreground thread.
				this.Dispatcher.BeginInvoke(
					new Action(() => MessageBox.Show(Application.Current.MainWindow, faultException.Detail.Message, Application.Current.MainWindow.Title)));

			}
			catch (Exception exception)
			{

				// Present the exception to the user in a foreground thread.
				this.Dispatcher.BeginInvoke(
					new Action(() => MessageBox.Show(Application.Current.MainWindow, exception.Message, Application.Current.MainWindow.Title)));

			}
			finally
			{
				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}
		}

		/// <summary>
		/// Subscribe to DataModel events.
		/// </summary>
		/// <param name="state">The unused thread initialization parameter.</param>
		private void UninstallEventHandlers(Object state)
		{

			// This will unsubscribe from all the previously subscribed events.
			lock (DataModel.SyncRoot)
			{
				DataModel.Chat.ChatRowChanging -= this.OnChatRowChanging;
				DataModel.ConsumerDebtNegotiation.ConsumerDebtNegotiationRowChanged -= this.OnConsumerDebtNegotiationRowChanged;
				DataModel.ConsumerDebtNegotiationOfferPaymentMethod.ConsumerDebtNegotiationOfferPaymentMethodRowChanging -=
					this.OnConsumerDebtNegotiationOfferPaymentMethodRowChanging;
				DataModel.ConsumerDebtNegotiationCounterPaymentMethod.ConsumerDebtNegotiationCounterPaymentMethodRowChanging -=
					this.OnConsumerDebtNegotiationCounterPaymentMethodRowChanging;
				DataModel.ConsumerDebtNegotiationOfferPaymentMethod.ConsumerDebtNegotiationOfferPaymentMethodRowDeleting -=
					this.OnConsumerDebtNegotiationOfferPaymentMethodRowDeleting;
				DataModel.ConsumerDebtNegotiationCounterPaymentMethod.ConsumerDebtNegotiationCounterPaymentMethodRowDeleting -=
					this.OnConsumerDebtNegotiationCounterPaymentMethodRowDeleting;
			}

		}

		/// <summary>
		/// Displays the next part of the conversation in the 'Chat' window.
		/// </summary>
		/// <param name="chatItem">Information about an item in a conversation.</param>
		private void UpdateChat(ChatItem chatItem)
		{

			// This will clear out the Watermark Instructions when the first chat item is recieved.
			if (this.stackPanelMain.Children.Count != 0 && stackPanelMain.Children[0] is RichTextBox)
				this.stackPanelMain.Children.Clear();

			// Convert universal time into local time.
			DateTime localTime = TimeZoneInfo.ConvertTime(chatItem.CreatedTime, TimeZoneInfo.Utc, TimeZoneInfo.Local);

			// Create a message package object based on the ChatItem object.
			ChatBubble.MessagePackage messagePackage = new ChatBubble.MessagePackage(
				chatItem.IsReply ? ChatBubble.PartyEnum.Contra : ChatBubble.PartyEnum.You,
				chatItem.Message);

			// Create a new chatBubble GUI instance.
			ChatBubble chatBubble = new ChatBubble();
			chatBubble.ChatObject = messagePackage;

			// Add the timestamp.
			TextBlock textBlock = new TextBlock();
			textBlock.Foreground = System.Windows.Media.Brushes.Gray;
			textBlock.FontStyle = FontStyles.Italic;
			textBlock.Text = localTime.ToString(@"MMM d yyyy H:mm tt");
			textBlock.TextAlignment = TextAlignment.Center;

			// Add the dialog item to the panel.
			this.stackPanelMain.Children.Add(textBlock);

			// Add the chatBubble GUI component to to the stackpanel.
			this.stackPanelMain.Children.Add(chatBubble);

			// force the last entry to always be visible.
			this.scrollViewerMain.ScrollToBottom();

		}

		/// <summary>
		/// Display changes to the entire console.
		/// </summary>
		/// <param name="settlementItem"></param>
		private void UpdateConsole(SettlementItem settlementItem)
		{

			// The console is only enabled when it has something to display.  If the associated Negotiation Manager hasn't got an item selected then this
			// control should be inaccessible to the user.
			this.IsEnabled = true;

			// This will initialize all the scalar parameters of the negotiation.
			this.UpdateNegotiation(settlementItem.NegotiationItem);

			// This will clear out and re-populate the vector that holds all the payment methods for the debt holder.
			this.holderPaymentMethods.SelectedItems.Clear();
			this.holderPaymentMethods.SelectedValues.Clear();
			foreach (Guid method in settlementItem.OfferPaymentMethods)
				this.holderPaymentMethods.SelectedValues.Add(method);

			// This will clear out and re-populate the vector that holds all the payment methods for the debt negotiator.
			this.negotiatorPaymentMethods.SelectedItems.Clear();
			this.negotiatorPaymentMethods.SelectedValues.Clear();
			foreach (Guid method in settlementItem.CounterPaymentMethods)
				this.negotiatorPaymentMethods.SelectedValues.Add(method);

			// Indicate whether or not there is a common payment method available.
			ValidatePaymentMethods();

			// Set the state of the GUI depending on the status of the negotiation.
			this.UpdateStatus(settlementItem.Status);

			// Clear out the existing chat.
			this.stackPanelMain.Children.Clear();

			// Update the chat area of the console.
			if (settlementItem.ChatItemList.Count == 0)
			{

				// If there are no chat items to display, then display the help text.
				RichTextBox myRichTextBox = new RichTextBox();
				myRichTextBox.HorizontalAlignment = HorizontalAlignment.Center;
				myRichTextBox.Document = this.FlowDocument;
				myRichTextBox.Width = 250.0;
				// Removed the pre-configured text for the Chat window.
				//this.stackPanelMain.Children.Add(myRichTextBox);
				this.scrollViewerMain.ScrollToTop();

			}
			else
			{

				// Fill the chat up with the entire conversation.
				foreach (ChatItem chatItem in settlementItem.ChatItemList)
				{
					UpdateChat(chatItem);
				}

			}

		}

		/// <summary>
		/// Display any changes to the chat session.
		/// </summary>
		/// <param name="chatRow"></param>
		private void UpdateNegotiation(NegotiationItem negotiationItem)
		{

			if (negotiationItem != null)
			{

				this.IsEnabled = true;

				// This will update all the scalar fields in the negotiation with the data from the event.
				this.accountBalance.Value = negotiationItem.AccountBalance;
				this.holderPayments.Value = negotiationItem.OfferPaymentLength;
				this.holderSettlementMarketValue.Value = negotiationItem.OfferSettlementValue * negotiationItem.AccountBalance;
				this.holderSettlementPercentage.Value = negotiationItem.OfferSettlementValue;
				this.holderStartDate.Length = negotiationItem.OfferPaymentStartDateLength;
				this.holderStartDate.TimeUnitId = negotiationItem.OfferPaymentStartDateUnitId;
				this.negotiatorPayments.Value = negotiationItem.CounterPaymentLength;
				this.negotiatorSettlementMarketValue.Value = negotiationItem.CounterSettlementValue * negotiationItem.AccountBalance;
				this.negotiatorSettlementPercentage.Value = negotiationItem.CounterSettlementValue;
				this.negotiatorStartDate.Length = negotiationItem.CounterPaymentStartDateLength;
				this.negotiatorStartDate.TimeUnitId = negotiationItem.CounterPaymentStartDateUnitId;

				// The images shown next to each of the negotiated values will change to reflect whether there is a match or mismatch.
				this.settlementValueImage.Source = negotiationItem.CounterSettlementValue >= negotiationItem.OfferSettlementValue ?
					this.offerMetImage :
					this.offerNotMetImage;
				this.settlementPercentImage.Source = negotiationItem.CounterSettlementValue >= negotiationItem.OfferSettlementValue ?
					this.offerMetImage :
					this.offerNotMetImage;
				this.paymentsImage.Source = negotiationItem.CounterPaymentLength <= negotiationItem.OfferPaymentLength ?
					this.offerMetImage :
					this.offerNotMetImage;
				this.termLengthImage.Source = negotiationItem.CounterDays <= negotiationItem.OfferDays ? this.offerMetImage : this.offerNotMetImage;

			}
			else
			{

				this.IsEnabled = false;
				
			}

		}

		/// <summary>
		/// Display changes to the entire console.
		/// </summary>
		/// <param name="settlementItem"></param>
		private void UpdateStatus(Status status)
		{

			// Set the state of the GUI depending on the status of the negotiation.
			Boolean isEnabled = status != Status.Pending && status != Status.OfferAccepted && status != Status.Accepted;
			this.holderPaymentMethods.IsEnabled = isEnabled;
			this.holderPayments.IsEnabled = isEnabled;
			this.holderSettlementMarketValue.IsEnabled = isEnabled;
			this.holderSettlementPercentage.IsEnabled = isEnabled;
			this.holderStartDate.IsEnabled = isEnabled;
			this.buttonAccept.IsEnabled = status != Status.Pending && status != Status.Accepted;
			this.buttonOffer.IsEnabled = isEnabled;
			// TODO: Fix this for 1.4 (which we need server side support).
			// Currently only allow Reject in the Pending state for 1.3.8
			this.CanReject = (status == Status.Pending || status == Status.OfferAccepted);
			// This what should be the final checks for version 1.4.0+ ?
			//this.CanReject = (status == Status.Pending || status == Status.OfferAccepted || status == Status.Accepted);
		}

		/// <summary>
		/// Allows you to manually control the enabling or disabling the button on the Negotiation Console.
		/// </summary>
		/// <param name="isEnabledButtonAccept">Is Accept Button enabled</param>
		/// <param name="isEnabledButtonOffer">Is Offer Button enabled</param>
		/// <param name="isEnabledButtonReject">Is Reject Button enabled</param>
		private void UpdateButtons(bool isEnabledButtonAccept, bool isEnabledButtonOffer, bool isEnabledButtonReject)
		{
			this.buttonAccept.IsEnabled = isEnabledButtonAccept;
			this.buttonOffer.IsEnabled = isEnabledButtonOffer;
			this.buttonReject.IsEnabled = isEnabledButtonReject;
		}
	
		/// <summary>
		/// Determines if a compatible payment method exists between the debt holder and debt negotiator.
		/// </summary>
		private void ValidatePaymentMethods()
		{

			// Iterate through each of the payment methods available for both sides of the negotiation and determine if even a single method is common to the
			// two parties.
			Boolean isValid = false;
			foreach (Guid holderPaymentMethodId in this.holderPaymentMethods.SelectedValues)
			{
				foreach (Guid negotiatorPaymentMethodId in this.negotiatorPaymentMethods.SelectedValues)
				{
					if (holderPaymentMethodId == negotiatorPaymentMethodId)
					{
						isValid = true;
					}
				}
			}

			// Display an image indicating whether there is a common payment method available or not.
			this.paymentMethodImage.Source = isValid ? this.offerMetImage : this.offerNotMetImage;

		}

		
		/// <summary>
		/// Handle the chat box losing focus. If there's no text in the in the box, revert to the default text
		/// </summary>
		/// <param name="sender">The chat box.</param>
		/// <param name="e">The event arguments.</param>
		private void OnChatBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			// Had to add this as the Watermark Service is not picking up the lost focus and setting the watermark.
			// We are using this to trigger it back.

			TextBox chat = sender as TextBox;

			if (String.IsNullOrEmpty(chat.Text))
			{

				string textString = this.Resources["ChatMessageString"].ToString();
				this.WatermarkText.Text = textString;

			}
		}

	}

}

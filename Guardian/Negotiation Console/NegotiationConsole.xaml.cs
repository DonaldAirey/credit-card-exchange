namespace FluidTrade.Guardian
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

	/// <summary>
	/// Interaction logic for NegotiationConsole.xaml
	/// </summary>
	public partial class NegotiationConsole : UserControl
	{

		// Private Enums
		enum NegotiationState { None, Counting, Pending, Accepted, Rejected, Done };

		// Private Instance Fields
		private Guid matchId;
		private Guid negotiationId;

		private Decimal quantity;
		private Decimal leavesQuantity;
		private NegotiationState negotiationState;
		private delegate void SetDialogAttributesDelegate(String title, String symbol, String name, String logoSource, Decimal leavesQuantity, NegotiationState negotiationState);
		private delegate void MessageDelegate(String message, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage);
		private delegate void NegotiationStateDelegate(NegotiationState negotiationState);
		private delegate void TimeSpanDelegate(TimeSpan timeLeft);
		private delegate void VoidDelegate();
		private Dictionary<Guid, Int32> countdownTable;
		private Timer timer;

		public NegotiationConsole()
		{

			InitializeComponent();
			this.timer = new Timer(OnTimer);
			this.countdownTable = new Dictionary<Guid, int>();

		}

		public Guid MatchId
		{

			get { return this.matchId; }
			set
			{
				this.matchId = value;
				this.negotiationId = Guid.Empty;

				this.leavesQuantity = 0.0m;
				this.quantity = 100000.0m;

				this.negotiationState = NegotiationState.None;
				this.timer.Change(0, 1000);
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(InitializeData));
			}

		}

		private void ClearKeypad()
		{

			this.labelLeavesQuantity.Content = String.Format("{0:#,##0}", 0.0m);
			this.labelMinimumQuantity.Content = String.Format("{0:#,##0}", 0.0m);
			this.logoImage.Source = null;

		}

		public void DisableKeypad()
		{

			// Clear the keypad just in case
			this.ClearKeypad();

			// Disable all the keypad buttons.
			this.labelLeaves.IsEnabled = false;
			this.labelLeavesQuantity.IsEnabled = false;
			this.labelQuantity.IsEnabled = false;
			this.textBoxQuantity.IsEnabled = false;
			this.labelMinimum.IsEnabled = false;
			this.labelMinimumQuantity.IsEnabled = false;
			this.buttonTrade.IsEnabled = false;
			this.buttonPass.IsEnabled = false;
		}

		private void DeclineTrade(object parameter)
		{

			DataModelClient dataModelClient = new DataModelClient(FluidTrade.Guardian.Properties.Settings.Default.DataModelEndpoint);

			lock (DataModel.SyncRoot)
			{

				// Find the Match record.
				MatchRow matchRow = DataModel.Match.MatchKey.Find(this.matchId);
				if (matchRow == null)
				{
					Guid negotiationId = Guid.NewGuid();
					dataModelClient.CreateNegotiation(
						matchRow.WorkingOrderRow.BlotterId,
						null,
						null,
						matchRow.MatchId,
						negotiationId,
						0.0M,
						DataModel.Status.StatusKeyStatusCode.Find(Status.Declined).StatusId);
				}

			}

		}

		public void EnableKeypad()
		{

			// Enable all the keypad buttons.
			this.labelLeaves.IsEnabled = true;
			this.labelLeavesQuantity.IsEnabled = true;
			this.labelQuantity.IsEnabled = true;
			this.textBoxQuantity.IsEnabled = true;
			this.labelMinimum.IsEnabled = true;
			this.labelMinimumQuantity.IsEnabled = true;
			this.buttonTrade.IsEnabled = true;
			this.buttonPass.IsEnabled = true;

		}

		/// <summary>
		/// Initialize the data used in this application.
		/// </summary>
		private void InitializeData(object parameter)
		{

			String title = String.Empty;
			String symbol = String.Empty;
			String name = String.Empty;
			String logoSource = null;
			Decimal leavesQuantity = 0.0m;
			NegotiationState negotiationState = NegotiationState.None;

			lock (DataModel.SyncRoot)
			{

				// Find the Match record.
				MatchRow matchRow = DataModel.Match.MatchKey.Find(this.matchId);
				WorkingOrderRow workingOrderRow = matchRow.WorkingOrderRow;
				OrderTypeRow orderTypeRow = workingOrderRow.OrderTypeRow;
				SecurityRow securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;

				symbol = securityRow.Symbol;
				name = securityRow.EntityRow.Name;
				logoSource = securityRow.IsLogoNull() ? String.Empty : securityRow.Logo;
				title = String.Format("{0} of {1}", orderTypeRow.Description, symbol);
				leavesQuantity = 0.0M;
				foreach (SourceOrderRow sourceOrderRow in workingOrderRow.GetSourceOrderRows())
					leavesQuantity += sourceOrderRow.OrderedQuantity;
				foreach (DestinationOrderRow destinationOrderRow in workingOrderRow.GetDestinationOrderRows())
					foreach (ExecutionRow executionRow in destinationOrderRow.GetExecutionRows())
						leavesQuantity -= executionRow.ExecutionQuantity;
				leavesQuantity /= securityRow.QuantityFactor;

			}

 			this.Dispatcher.Invoke(DispatcherPriority.Normal, new SetDialogAttributesDelegate(SetDialogAttributes), title, symbol, name, logoSource, leavesQuantity, negotiationState);

		}

		private void OnButtonPassClick(object sender, EventArgs e)
		{

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(DeclineTrade));
			DisableKeypad();

		}

		private void OnButtonTradeClick(object sender, EventArgs e)
		{

			if (this.textBoxQuantity.Text != String.Empty)
			{

				this.quantity = 0.0M;
				if (this.textBoxQuantity.Text != string.Empty)
					this.quantity = Convert.ToDecimal(this.textBoxQuantity.Text);

				if (quantity < 100000.0m)
				{
					MessageBox.Show("Value must meet the minimum quantity.");
					return;
				}

				if (quantity > this.leavesQuantity)
				{
					MessageBox.Show("Value must be less than the quantity leaves.");
					return;
				}

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(NegotiateTrade), this.quantity / 100.0M);

				DisableKeypad();

				this.negotiationId = Guid.Empty;
				this.negotiationState = NegotiationState.None;

			}

		}

		private void NegotiateTrade(object parameter)
		{

			// Extract the thread parameters
			Decimal quantity = (Decimal)parameter;

			DataModelClient dataModelClient = new DataModelClient(Guardian.Properties.Settings.Default.DataModelEndpoint);

			lock (DataModel.SyncRoot)
			{

				// Open up the negotiations.
				MatchRow matchRow = DataModel.Match.MatchKey.Find(this.matchId);
				Guid negotiationId = Guid.NewGuid();
				dataModelClient.CreateNegotiation(
					matchRow.WorkingOrderRow.BlotterId,
					null,
					null,
					matchRow.MatchId,
					negotiationId,
					quantity,
					DataModel.Status.StatusKeyStatusCode.Find(Status.Pending).StatusId);

			}

		}

		private void OnLabelMinimumClick(object sender, EventArgs e)
		{
			this.textBoxQuantity.Text = Convert.ToString(100000.0m);
		}

		private void OnLabelLeavesClick(object sender, EventArgs e)
		{
			this.textBoxQuantity.Text = Convert.ToString(this.labelLeaves.Content);
		}

		private void OnTimer(Object state)
		{

			foreach (KeyValuePair<Guid, int> keyPair in this.countdownTable)
			{
				int counter = keyPair.Value - 1;
				if (counter == 0)
				{

					this.countdownTable.Remove(keyPair.Key);

					if (keyPair.Key == this.matchId)
						this.negotiationState = NegotiationState.Rejected;

					// disable the keypad when the timer is up
					DisableKeypad();

				}
				else
				{
					this.countdownTable[keyPair.Key] = counter;
				}

			}

		}

		private void SetDialogAttributes(String title, String symbol, String name, String logoSource, Decimal leavesQuantity, NegotiationState negotiationState)
		{

			// Submitted Quantity is saved here
			this.leavesQuantity = leavesQuantity;
			this.negotiationState = negotiationState;

			// Initialize the dialog elements with the data retrieved from the data model.
			if (logoSource != String.Empty)
			{
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(logoSource));
				bitmapImage.EndInit();
				this.logoImage.Source = bitmapImage;
			}

			this.textBoxQuantity.Text = Convert.ToString(100000.0m);
			this.labelLeavesQuantity.Content = leavesQuantity.ToString("#,##0");
			this.labelMinimumQuantity.Content = String.Format("{0:#,##0}", 100000.0m);

			// The keypad can now be enabled.
			EnableKeypad();

		}

	}

}

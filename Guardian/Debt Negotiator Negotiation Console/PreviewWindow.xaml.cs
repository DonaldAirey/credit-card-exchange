namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.Collections;
	using System.Windows;

    /// <summary>
    /// Presents the settlement values to the user and gives them the option to accept or reject them.
    /// </summary>
    public partial class PreviewWindow : Window
    {

		/// <summary>
		/// The account balance.
		/// </summary>
		public Decimal AccountBalance
		{
			get { return this.accountBalance.Value; }
			set { this.accountBalance.Value = value; }
		}

		/// <summary>
		/// The proposed methods by which this settlement can be paid.
		/// </summary>
		public IList PaymentMethods
		{
			get { return this.paymentMethod.SelectedValues; }
			set { this.paymentMethod.SelectedValues = value; }
		}

		/// <summary>
		/// The proposed number of payments.
		/// </summary>
		public Decimal Payments
		{
			get { return this.payments.Value; }
			set { this.payments.Value = value; }
		}

		/// <summary>
		/// The proposed settlement amount as a market value.
		/// </summary>
		public Decimal SettlementMarketValue
		{
			get { return this.settlementMarketValue.Value; }
			set { this.settlementMarketValue.Value = value; }
		}

		/// <summary>
		/// The proposed settlement amount as a percentage.
		/// </summary>
		public Decimal SettlementPercent
		{
			get { return this.settlementPercentage.Value; }
			set { this.settlementPercentage.Value = value; }
		}

		/// <summary>
		/// The proposed start date.
		/// </summary>
		public Decimal StartDateLength
		{
			get { return this.startDate.Length; }
			set { this.startDate.Length = value; }
		}

		/// <summary>
		/// The units in which the proposed start date is quoted.
		/// </summary>
		public Guid StartDateUnit
		{
			get { return this.startDate.TimeUnitId; }
			set { this.startDate.TimeUnitId = value; }
		}

        /// <summary>
        /// Create a preview of the settlement terms.
        /// </summary>
        public PreviewWindow()
        {

			// The IDE supported controls are initialized here.
            InitializeComponent();

        }

		/// <summary>
		/// Handles a command to cancel the settlement.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
        private void OnCancel(object sender, RoutedEventArgs e)
        {

			// At this point the user has decide not to accept the settlement.
            DialogResult = false;

        }

		/// <summary>
		/// Handles a command to accept the settlement.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnOK(object sender, RoutedEventArgs e)
        {

			// At ths point the user has decided to accept the settlement.
            DialogResult = true;

        }

    }
}

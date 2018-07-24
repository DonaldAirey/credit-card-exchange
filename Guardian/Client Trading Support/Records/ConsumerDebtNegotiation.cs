namespace FluidTrade.Guardian.Records
{
	/// <summary>
	/// 
	/// </summary>
	public partial class ConsumerDebtNegotiation : FluidTrade.Guardian.Records.BaseRecord
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerDebtNegotiation()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerDebtNegotiation(ConsumerDebtNegotiationRow row)
		{
			this.BlotterId = row.BlotterId;
			this.RowId = row.ConsumerDebtNegotiationId;
			this.RowVersion = row.RowVersion;
		}
	}
}

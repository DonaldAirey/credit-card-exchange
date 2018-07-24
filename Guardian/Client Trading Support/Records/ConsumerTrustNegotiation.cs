namespace FluidTrade.Guardian.Records
{
	/// <summary>
	/// 
	/// </summary>
	public partial class ConsumerTrustNegotiation : FluidTrade.Guardian.Records.BaseRecord
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerTrustNegotiation()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerTrustNegotiation(ConsumerTrustNegotiationRow row)
		{
			this.BlotterId = row.BlotterId;
			this.RowId = row.ConsumerTrustNegotiationId;
			this.RowVersion = row.RowVersion;
		}
	}
}

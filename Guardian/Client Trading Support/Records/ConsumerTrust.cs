namespace FluidTrade.Guardian.TradingSupportReference
{
	
	/// <summary>
	/// 
	/// </summary>
	public partial class ConsumerTrust : BaseRecord
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerTrust()
		{			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerTrust(ConsumerTrustRow row)
		{
			this.RowId = row.ConsumerTrustId;
			this.RowVersion = row.RowVersion;
			this.TenantId = row.TenantId;
		}
	}
}

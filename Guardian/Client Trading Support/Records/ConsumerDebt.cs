namespace FluidTrade.Guardian.TradingSupportReference
{
	
	/// <summary>
	/// 
	/// </summary>
	public partial class ConsumerDebt : BaseRecord
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerDebt()
		{			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public ConsumerDebt(ConsumerDebtRow row)
		{
			this.RowId = row.ConsumerDebtId;
			this.RowVersion = row.RowVersion;
			this.TenantId = row.TenantId;
		}
	}
}

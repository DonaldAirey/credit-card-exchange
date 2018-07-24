namespace FluidTrade.Guardian.TradingSupportReference
{	

	/// <summary>
	/// 
	/// </summary>
	public partial class WorkingOrderRecord : BaseRecord 
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public WorkingOrderRecord()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public WorkingOrderRecord(WorkingOrderRow row)
		{
			this.BlotterId = row.BlotterId;
			this.RowId = row.WorkingOrderId;
			this.RowVersion = row.RowVersion;
		}
	}
}

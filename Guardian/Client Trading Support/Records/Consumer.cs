namespace FluidTrade.Guardian.TradingSupportReference
{

	/// <summary>
	/// 
	/// </summary>
	public partial class Consumer : BaseRecord
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Consumer()
		{			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Consumer(ConsumerRow row)
		{
			this.RowId = row.ConsumerId;
			this.RowVersion = row.RowVersion;
		}

	}

}

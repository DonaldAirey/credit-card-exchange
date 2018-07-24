
namespace FluidTrade.Guardian.TradingSupportReference
{
	
	/// <summary>
	/// 
	/// </summary>
	public partial class Province : BaseRecord
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Province()
		{			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Province(ProvinceRow row)
		{
			this.RowId = row.ProvinceId;
			this.RowVersion = row.RowVersion;
		}
	}
}

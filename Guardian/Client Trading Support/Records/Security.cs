
namespace FluidTrade.Guardian.TradingSupportReference
{	
	/// <summary>
	/// 
	/// </summary>
	public partial class Security : BaseRecord
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Security()
		{
		
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Security(SecurityRow row)
		{
			this.RowId = row.SecurityId;
			this.RowVersion = row.RowVersion;
			this.TenantId = row.TenantId;
		}
	}
}

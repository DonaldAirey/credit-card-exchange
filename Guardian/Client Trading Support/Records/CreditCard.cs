
namespace FluidTrade.Guardian.TradingSupportReference
{
	/// <summary>
	/// 
	/// </summary>
	public partial class CreditCard : BaseRecord
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public CreditCard()
		{			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public CreditCard(CreditCardRow row)
		{
			this.RowId = row.CreditCardId;
			this.RowVersion = row.RowVersion;
		}
	}
}

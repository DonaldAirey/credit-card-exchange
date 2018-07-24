namespace FluidTrade.Guardian.TradingSupportReference
{	
	
	/// <summary>
	/// 
	/// </summary>
	public partial class Negotiation : BaseRecord
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Negotiation()
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Negotiation(NegotiationRow row)
		{
			this.RowId = row.NegotiationId;
			this.RowVersion = row.RowVersion;
		}
	}
}

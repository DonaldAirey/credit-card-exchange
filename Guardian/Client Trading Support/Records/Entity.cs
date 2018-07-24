namespace FluidTrade.Guardian.TradingSupportReference
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Entity : BaseRecord
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Entity()
		{			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="row"></param>
		public Entity(EntityRow row)
		{
			this.RowId = row.EntityId;
			this.RowVersion = row.RowVersion;
			this.TenantId = row.TenantId;
		}

	}
}

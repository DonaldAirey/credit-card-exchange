namespace FluidTrade.Guardian.Windows
{


    /// <summary>
	/// A Trader is a collection of orders.
	/// </summary>
	public class Trader : User
	{

		/// <summary>
		/// Create a new Trader based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the DataModel.</param>
		public Trader(EntityRow entityRow) : base(entityRow)
		{
		
		}

		/// <summary>
		/// Create a new Trader based on an entity.
		/// </summary>
		/// <param name="entity">An entity.</param>
		public Trader(Entity entity)
			: base(entity)
		{

		}

	}

}

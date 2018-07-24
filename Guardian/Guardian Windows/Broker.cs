namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>
	/// Summary description for Broker.
	/// </summary>
	public class Broker : Blotter
	{

		/// <summary>
		/// Create a new Broker based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the DataModel.</param>
		public Broker(EntityRow entityRow) : base(entityRow)
		{
		}

		/// <summary>
		/// Primary Identifier of this object.
		/// </summary>
		public Guid BrokerId { get { return this.EntityId; } }

	}

}

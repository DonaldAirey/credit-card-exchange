namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>A large company that contains a relationship (clearing, brokerage) with other companies.</summary>
	public class ClearingBroker : Blotter
	{

		/// <summary>
		/// A large company that contains a relationship (clearing, brokerage) with other companies.
		/// </summary>
		/// <param name="entityRow">The primary key of the object.</param>
		public ClearingBroker(EntityRow entityRow) : base(entityRow)
		{
		}

		/// <summary>
		/// Primary Identifier of this object.
		/// </summary>
 		public Guid ClearingBrokerId {get {return this.BlotterId;}}

	}

}

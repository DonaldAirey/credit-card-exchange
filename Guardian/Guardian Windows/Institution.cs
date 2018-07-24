namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>A large company that contains a relationship (clearing, brokerage) with other companies.</summary>
	public class Institution : Blotter
	{

		/// <summary>
		/// A large company that contains a relationship (clearing, brokerage) with other companies.
		/// </summary>
		/// <param name="entityRow">The primary key of the object.</param>
		public Institution(EntityRow entityRow) : base(entityRow)
		{
		}

		/// <summary>
		/// Primary Identifier of this object.
		/// </summary>
		public Guid InstitutionId {get {return this.BlotterId;}}

	}

}

namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>A large company that contains a relationship (clearing, brokerage) with other companies.</summary>
	public class ComplianceOfficer : User
	{

		/// <summary>
		/// A large company that contains a relationship (clearing, brokerage) with other companies.
		/// </summary>
		/// <param name="entityRow">The primary key of the object.</param>
		public ComplianceOfficer(EntityRow entityRow) : base(entityRow)
		{
		}

		/// <summary>
		/// Primary Identifier of this object.
		/// </summary>
		public Guid ComplianceOfficerId {get {return this.EntityId;}}

	}

}

namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>
	/// Summary description for Branch.
	/// </summary>
	public class Branch : Blotter
	{

		/// <summary>
		/// Create a new Branch based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the DataModel.</param>
		public Branch(EntityRow entityRow) : base(entityRow)
		{
		}

		/// <summary>
		/// Primary Identifier of this object.
		/// </summary>
		public Guid BranchId {get {return this.EntityId;}}

	}

}

namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>
	/// Summary description for Blotter.
	/// </summary>
	public class DebtBlotter : Blotter
	{

		/// <summary>
		/// Create a new DebtBlotter based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the DataModel.</param>
		public DebtBlotter(EntityRow entityRow) : base(entityRow)
		{
		}

	}

}

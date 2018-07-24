namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>
	/// Summary description for Blotter.
	/// </summary>
	public class EquityBlotter : Blotter
	{

		/// <summary>
		/// Create a new EquityBlotter based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the DataModel.</param>
		public EquityBlotter(EntityRow entityRow) : base(entityRow)
		{
		}

	}

}

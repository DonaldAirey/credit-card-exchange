namespace FluidTrade.Guardian.Windows
{

    /// <summary>
	/// An object use to open up the details of a working order in the blotter.
	/// </summary>
	public class BlotterWorkingOrderDetail
	{

		/// <summary>
		/// The Blotter in which the match is contained.
		/// </summary>
		public Blotter Blotter;

		/// <summary>
		/// An array of working orders which are to be displayed in the detail view.
		/// </summary>
		public WorkingOrder[] WorkingOrders;

		/// <summary>
		/// Create a description of a detail window.
		/// </summary>
		/// <param name="blotter">The blotter in which the detail is to be displayed.</param>
		/// <param name="workingOrders">An array of working orders to be displayed in the detail pane.</param>
		public BlotterWorkingOrderDetail(Blotter blotter, WorkingOrder[] workingOrders)
		{

			// Initialize the object
			this.Blotter = blotter;
			this.WorkingOrders = workingOrders;

		}

	}

}

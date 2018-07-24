namespace FluidTrade.Guardian.Windows
{


    /// <summary>
	/// An object use to open up the details of a security match in the blotter.
	/// </summary>
	public class BlotterMatchDetail
	{

		/// <summary>
		/// The Blotter in which the match is contained.
		/// </summary>
		public Blotter Blotter;

		/// <summary>
		/// The record of the match which is to be displayed.
		/// </summary>
		public Match[] Matches;

		/// <summary>
		/// Create an object that is used to open up a given match in a blotter.
		/// </summary>
		/// <param name="blotter">The blotter</param>
		/// <param name="matches"></param>
		public BlotterMatchDetail(Blotter blotter, Match[] matches)
		{

			// Initialize the object
			this.Blotter = blotter;
			this.Matches = matches;

		}

	}

}

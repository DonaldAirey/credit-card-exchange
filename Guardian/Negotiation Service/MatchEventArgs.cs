namespace FluidTrade.Guardian
{

	using System;

	/// <summary>
	/// Handles a match oppotunity.
	/// </summary>
	/// <param name="sender">The object that orignated the event.</param>
	/// <param name="matchEventArgs">The event arguments.</param>
	internal delegate void MatchEventHandler(object sender, MatchEventArgs matchEventArgs);

	/// <summary>
	/// Used to pass the identifier of a match opportunity.
	/// </summary>
	internal class MatchEventArgs : EventArgs
	{

		/// <summary>
		/// The identifier of the match opportunity.
		/// </summary>
		public Guid MatchId;

		/// <summary>
		/// Create an event argument to notify a handler about a matching oppotunity.
		/// </summary>
		/// <param name="matchId"></param>
		public MatchEventArgs(Guid matchId)
		{

			// Initialize the object
			this.MatchId = matchId;

		}

	}

}

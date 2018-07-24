namespace FluidTrade.Guardian.Windows
{

	using System;

    /// <summary>
	/// Summary description for Match.
	/// </summary>
	public class Match
	{

		private Guid matchId;

		/// <summary>
		/// Create a new match object.
		/// </summary>
		/// <param name="matchId">The MatchId of the match.</param>
		public Match(Guid matchId)
		{

			this.matchId = matchId;

		}

		/// <summary>The Primary Identifier of this object.</summary>
		public Guid MatchId {get {return this.matchId;}}

	}

}

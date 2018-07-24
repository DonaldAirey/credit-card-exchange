namespace FluidTrade.Guardian
{

    using FluidTrade.Guardian;

	// Intenral Delegates
	internal delegate void MatchClientDelegate(DataModelClient dataModelClient, MatchRow matchRow, object value);

	/// <summary>
	/// Used to carry out an update action on a Working Order record.
	/// </summary>
	internal class MatchChange
	{

		// Public Fields
		public MatchClientDelegate Handler;
		public MatchRow MatchRow;
		public object Value;

		/// <summary>
		/// Describes a handler and the data used to change a Match row in the middle tier.
		/// </summary>
		/// <param name="handler">A handler to update the Working Order record.</param>
		/// <param name="matchRow">The current Working Order record.</param>
		/// <param name="value">The new value of an element of the Working Order record.</param>
		public MatchChange(MatchClientDelegate handler, MatchRow matchRow, object value)
		{
			this.Handler = handler;
			this.MatchRow = matchRow;
			this.Value = value;
		}

	}

}

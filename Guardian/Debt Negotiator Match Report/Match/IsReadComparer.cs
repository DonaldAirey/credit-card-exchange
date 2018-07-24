namespace FluidTrade.Guardian.Schema.DebtNegotiatorMatch
{
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two Match.Match records when sorting a list.
	/// </summary>
	public class IsReadComparer : System.Collections.Generic.IComparer<Match>
	{

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(Match operand1, Match operand2)
		{
			bool operand1Read = false;
			bool operand2Read = false;

			if (operand1.NullSafe(p => p.IsRead).NullSafe(c => c.Read) is bool)
				operand1Read = (bool)operand1.IsRead.Read;

			if (operand2.NullSafe(p => p.IsRead).NullSafe(c => c.Read) is bool)
				operand2Read = (bool)operand2.IsRead.Read;
			
			return operand1Read.CompareTo(operand2Read);
		}

	}

}

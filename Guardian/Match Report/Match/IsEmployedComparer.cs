namespace FluidTrade.Guardian.Schema.Match
{
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two Match.Match records when sorting a list.
	/// </summary>
	public class IsEmployedComparer : System.Collections.Generic.IComparer<Match>
	{

		/// <summary>
		/// Compares two Match records when sorting a list.
		/// </summary>
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(Match operand1, Match operand2)
		{
			bool operand1Employed = false;
			bool operand2Employed = false;

			if (operand1.NullSafe(p => p.IsEmployed).NullSafe(c => c.Employed) is bool)
				operand1Employed = (bool)operand1.IsEmployed.Employed;

			if (operand2.NullSafe(p => p.IsEmployed).NullSafe(c => c.Employed) is bool)
				operand2Employed = (bool)operand2.IsEmployed.Employed;
			
			return operand1Employed.CompareTo(operand2Employed);
		}

	}

}

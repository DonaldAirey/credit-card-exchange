namespace FluidTrade.Guardian.Schema.DebtHolderMatch
{

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
    public class DebtClassComparer : IComparer<Match>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
		{
			return operand1.Blotter.Name.CompareTo(operand2.Blotter.Name);
		}

	}

}

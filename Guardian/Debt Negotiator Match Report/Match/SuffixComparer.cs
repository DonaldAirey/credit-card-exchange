namespace FluidTrade.Guardian.Schema.DebtNegotiatorMatch
{

    using System;
    using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two WorkingOrder.WorkingOrder records when sorting a list.
    /// </summary>
    public class SuffixComparer : IComparer<Match>
    {

        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
		{
			String operand1Suffix = String.Empty;
			String operand2Suffix = String.Empty;

			if (operand1.NullSafe(p => p.Suffix).NullSafe(p => p.Name) is String)
				operand1Suffix = (String)operand1.Suffix.Name;

			if (operand2.NullSafe(p => p.Suffix).NullSafe(p => p.Name) is String)
				operand2Suffix = (String)operand2.Suffix.Name;

			if (String.IsNullOrEmpty(operand1Suffix) == true && String.IsNullOrEmpty(operand2Suffix) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1Suffix) == false && String.IsNullOrEmpty(operand2Suffix) == true)
				return -1;

			return operand1Suffix.CompareTo(operand2Suffix);
		}

    }

}

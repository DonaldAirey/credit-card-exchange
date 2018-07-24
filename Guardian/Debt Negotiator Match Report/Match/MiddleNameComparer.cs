namespace FluidTrade.Guardian.Schema.DebtNegotiatorMatch
{

    using System;
    using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two WorkingOrder.WorkingOrder records when sorting a list.
    /// </summary>
    public class MiddleNameComparer : IComparer<Match>
    {

        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
		{
			String operand1MiddleName = String.Empty;
			String operand2MiddleName = String.Empty;

			if (operand1.NullSafe(p => p.MiddleName).NullSafe(p => p.Name) is String)
				operand1MiddleName = (String)operand1.MiddleName.Name;

			if (operand2.NullSafe(p => p.MiddleName).NullSafe(p => p.Name) is String)
				operand2MiddleName = (String)operand2.MiddleName.Name;

			if (String.IsNullOrEmpty(operand1MiddleName) == true && String.IsNullOrEmpty(operand2MiddleName) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1MiddleName) == false && String.IsNullOrEmpty(operand2MiddleName) == true)
				return -1;

			return operand1MiddleName.CompareTo(operand2MiddleName);
		}

    }

}

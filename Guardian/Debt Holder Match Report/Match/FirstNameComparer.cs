namespace FluidTrade.Guardian.Schema.DebtHolderMatch
{
    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>
    public class FirstNameComparer : System.Collections.Generic.IComparer<Match>
    {

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {
			String operand1FirstName = String.Empty;
			String operand2FirstName = String.Empty;

			if (operand1.NullSafe(p => p.FirstName).NullSafe(FirstName => FirstName.Name) is String)
				operand1FirstName = (String)operand1.FirstName.Name;

			if (operand2.NullSafe(p => p.FirstName).NullSafe(FirstName => FirstName.Name) is String)
				operand2FirstName = (String)operand2.FirstName.Name;

			if (String.IsNullOrEmpty(operand1FirstName) == true && String.IsNullOrEmpty(operand2FirstName) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1FirstName) == false && String.IsNullOrEmpty(operand2FirstName) == true)
				return -1;

			return operand1FirstName.CompareTo(operand2FirstName);
        }

    }

}

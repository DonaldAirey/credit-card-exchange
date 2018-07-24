namespace FluidTrade.Guardian.Schema.DebtHolderMatch
{
    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>	
	public class DebtHolderNameComparer : System.Collections.Generic.IComparer<Match>
    {

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {
			String operand1IssuerName = String.Empty;
			String operand2IssuerName = String.Empty;

			if (operand1.NullSafe(p => p.DebtHolderName).NullSafe(IssuerName => IssuerName.Text) is String)
				operand1IssuerName = (String)operand1.DebtHolderName.Text;

			if (operand2.NullSafe(p => p.DebtHolderName).NullSafe(IssuerName => IssuerName.Text) is String)
				operand2IssuerName = (String)operand2.DebtHolderName.Text;

			if (String.IsNullOrEmpty(operand1IssuerName) == true && String.IsNullOrEmpty(operand2IssuerName) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1IssuerName) == false && String.IsNullOrEmpty(operand2IssuerName) == true)
				return -1;

			return operand1IssuerName.CompareTo(operand2IssuerName);
        }

    }

}

namespace FluidTrade.Guardian.Schema.Match
{

    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>
    public class SavingsBalanceComparer : System.Collections.Generic.IComparer<Match>
    {

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// </param name="operand1">The first row to be compared.</param>
        /// </param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {
			Decimal operand1Balance = 0;
			Decimal operand2Balance = 0;

			if (operand1.NullSafe(p => p.SavingsBalance).NullSafe(p => p.Balance) is Decimal)
				operand1Balance = (Decimal)operand1.SavingsBalance.Balance;

			if (operand2.NullSafe(p => p.SavingsBalance).NullSafe(p => p.Balance) is Decimal)
				operand2Balance = (Decimal)operand2.SavingsBalance.Balance;

			return operand1Balance.CompareTo(operand2Balance);      

       }

    }

}

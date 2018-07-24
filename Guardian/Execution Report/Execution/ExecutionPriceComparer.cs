namespace FluidTrade.Guardian.Schema.Execution
{

    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>
	public class ExecutionPriceComparer : System.Collections.Generic.IComparer<Execution>
    {

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(Execution operand1, Execution operand2)
        {
			Decimal operand1Balance = 0;
			Decimal operand2Balance = 0;

			if (operand1.NullSafe(p => p.ExecutionPrice).NullSafe(p => p.Text) is Decimal)
				operand1Balance = (Decimal)operand1.ExecutionPrice.Text;

			if (operand2.NullSafe(p => p.ExecutionPrice).NullSafe(p => p.Text) is Decimal)
				operand2Balance = (Decimal)operand2.ExecutionPrice.Text;

			return operand1Balance.CompareTo(operand2Balance);      

       }

    }

}

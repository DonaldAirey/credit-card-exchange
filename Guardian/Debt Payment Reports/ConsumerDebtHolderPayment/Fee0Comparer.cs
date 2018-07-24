namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{

    using System;
    using System.Collections.Generic;
    using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match records when sorting a list.
    /// </summary>
	public class Fee0Comparer : IComparer<PaymentSummary>
    {

        /// <summary>
		/// Compares two PaymentSummary records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
			Decimal operand1Fee0 = 0;
			Decimal operand2Fee0 = 0;

            if (operand1.NullSafe(p => p.Fee0).NullSafe(p => p.Text) is Decimal)
				operand1Fee0 = (Decimal)operand1.Fee0.Text;

			if (operand2.NullSafe(p => p.Fee0).NullSafe(p => p.Text) is Decimal)
				operand2Fee0 = (Decimal)operand2.Fee0.Text;

			return operand1Fee0.CompareTo(operand2Fee0);
            
        }

    }

}

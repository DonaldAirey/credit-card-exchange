namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{
    using System;
    using System.Collections.Generic;
    using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	class Memo2Comparer : IComparer<PaymentSummary>
    {

        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
            String operand1Memo2 = String.Empty;
            String operand2Memo2 = String.Empty;

            if (operand1.NullSafe(p => p.Memo2).NullSafe(Memo2 => Memo2.Text) is String)
				operand1Memo2 = (String)operand1.Memo2.Text;

			if (operand2.NullSafe(p => p.Memo2).NullSafe(Memo2 => Memo2.Text) is String)
				operand2Memo2 = (String)operand2.Memo2.Text;

            if (String.IsNullOrEmpty(operand1Memo2) == true && String.IsNullOrEmpty(operand2Memo2) == false)
                return 1;
            if (String.IsNullOrEmpty(operand1Memo2) == false && String.IsNullOrEmpty(operand2Memo2) == true)
                return -1;

            return operand1Memo2.CompareTo(operand2Memo2);
        }
    }
}

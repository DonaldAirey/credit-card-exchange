﻿namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{
    using System;
    using System.Collections.Generic;
    using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	class Memo0Comparer : IComparer<PaymentSummary>
    {

        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
            String operand1Memo0 = String.Empty;
            String operand2Memo0 = String.Empty;

            if (operand1.NullSafe(p => p.Memo0).NullSafe(Memo0 => Memo0.Text) is String)
				operand1Memo0 = (String)operand1.Memo0.Text;

			if (operand2.NullSafe(p => p.Memo0).NullSafe(Memo0 => Memo0.Text) is String)
				operand2Memo0 = (String)operand2.Memo0.Text;

            if (String.IsNullOrEmpty(operand1Memo0) == true && String.IsNullOrEmpty(operand2Memo0) == false)
                return 1;
            if (String.IsNullOrEmpty(operand1Memo0) == false && String.IsNullOrEmpty(operand2Memo0) == true)
                return -1;

            return operand1Memo0.CompareTo(operand2Memo0);
        }
    }
}
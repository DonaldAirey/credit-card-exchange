namespace FluidTrade.Guardian.Schema.DebtNegotiatorPaymentSummary
{
    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>
	public class CreatedUserIdComparer : System.Collections.Generic.IComparer<PaymentSummary>
    {

        /// <summary>
		/// Compares two PaymentSummary when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
			String operand1CreatedUserId = String.Empty;
			String operand2CreatedUserId = String.Empty;

			if (operand1.NullSafe(p => p.CreatedUserId).NullSafe(CreatedUserId => CreatedUserId.Name) is String)
				operand1CreatedUserId = (String)operand1.CreatedUserId.Name;

			if (operand2.NullSafe(p => p.CreatedUserId).NullSafe(CreatedUserId => CreatedUserId.Name) is String)
				operand2CreatedUserId = (String)operand2.CreatedUserId.Name;

			if (String.IsNullOrEmpty(operand1CreatedUserId) == true && String.IsNullOrEmpty(operand2CreatedUserId) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1CreatedUserId) == false && String.IsNullOrEmpty(operand2CreatedUserId) == true)
				return -1;

			return operand1CreatedUserId.CompareTo(operand2CreatedUserId);
        }

    }

}

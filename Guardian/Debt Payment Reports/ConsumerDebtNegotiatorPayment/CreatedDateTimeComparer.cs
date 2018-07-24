namespace FluidTrade.Guardian.Schema.DebtNegotiatorPaymentSummary
{
	using System;

    /// <summary>
    /// Compares two WorkingOrder records when sorting a list.
    /// </summary>
	public class CreatedDateTimeComparer : System.Collections.Generic.IComparer<PaymentSummary>
    {

        /// <summary>
		/// Compares two PaymentSummary when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
			return ((DateTime)operand1.CreatedDateTime.Text).CompareTo((DateTime)operand2.CreatedDateTime.Text);
        }

    }

}

namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{
	using System;

    /// <summary>
    /// Compares two WorkingOrder records when sorting a list.
    /// </summary>
	public class ActualPaymentDateComparer : System.Collections.Generic.IComparer<PaymentSummary>
    {

        /// <summary>
		/// Compares two PaymentSummary when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
			//return ((DateTime)operand1.ActualPaymentDate.Text).CompareTo((DateTime)operand2.ActualPaymentDate.Text);

			// Convert from datetime that is stored as a string to DateTime to perform date comparisions.
			String operand1ActualPaymentDateString = (String)operand1.ActualPaymentDate.Text;
			String operand2ActualPaymentDateString = (String)operand2.ActualPaymentDate.Text;

			DateTime operand1ActualPaymentDate = DateTime.MaxValue;
			DateTime operand2ActualPaymentDate = DateTime.MaxValue;

			if (!String.IsNullOrEmpty((String)operand1.ActualPaymentDate.Text))
				operand1ActualPaymentDate = DateTime.Parse(operand1ActualPaymentDateString);

			if (!String.IsNullOrEmpty((String)operand2.ActualPaymentDate.Text))
				operand2ActualPaymentDate = DateTime.Parse(operand2ActualPaymentDateString);

			return operand1ActualPaymentDate.CompareTo(operand2ActualPaymentDate);

        }

    }

}

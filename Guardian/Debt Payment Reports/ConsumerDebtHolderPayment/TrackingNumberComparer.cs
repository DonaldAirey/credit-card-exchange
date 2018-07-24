namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{
    using System;
    using System.Collections.Generic;
    using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two PaymentSummary records when sorting a list.
	/// </summary>
	class TrackingNumberComparer : IComparer<PaymentSummary>
    {

        /// <summary>
		/// Compares two PaymentSummary records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
            String operand1TrackingNumber = String.Empty;
            String operand2TrackingNumber = String.Empty;

            if (operand1.NullSafe(p => p.TrackingNumber).NullSafe(TrackingNumber => TrackingNumber.Text) is String)
				operand1TrackingNumber = (String)operand1.TrackingNumber.Text;

			if (operand2.NullSafe(p => p.TrackingNumber).NullSafe(TrackingNumber => TrackingNumber.Text) is String)
				operand2TrackingNumber = (String)operand2.TrackingNumber.Text;

            if (String.IsNullOrEmpty(operand1TrackingNumber) == true && String.IsNullOrEmpty(operand2TrackingNumber) == false)
                return 1;
            if (String.IsNullOrEmpty(operand1TrackingNumber) == false && String.IsNullOrEmpty(operand2TrackingNumber) == true)
                return -1;

            return operand1TrackingNumber.CompareTo(operand2TrackingNumber);
        }
    }
}

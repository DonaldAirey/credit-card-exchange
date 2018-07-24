namespace FluidTrade.Guardian.Schema.DebtNegotiatorPaymentSummary
{

    using System;
    using System.Collections.Generic;
    using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match records when sorting a list.
    /// </summary>
	public class EffectivePaymentValueComparer : IComparer<PaymentSummary>
    {

        /// <summary>
		/// Compares two PaymentSummary records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
        {
			Decimal operand1EffectivePaymentValue = 0;
			Decimal operand2EffectivePaymentValue = 0;

            if (operand1.NullSafe(p => p.EffectivePaymentValue).NullSafe(p => p.Text) is Decimal)
				operand1EffectivePaymentValue = (Decimal)operand1.EffectivePaymentValue.Text;

			if (operand2.NullSafe(p => p.EffectivePaymentValue).NullSafe(p => p.Text) is Decimal)
				operand2EffectivePaymentValue = (Decimal)operand2.EffectivePaymentValue.Text;

			return operand1EffectivePaymentValue.CompareTo(operand2EffectivePaymentValue);
            
        }

    }

}

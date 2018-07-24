namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Compares two PaymentSummary records when sorting a list.
	/// </summary>
	public class EffectivePaymentDateComparer : IComparer<PaymentSummary>
	{

		/// <summary>
		/// Compares two Credit Card records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
		{

			//HACK - Change this to DateTime so we don't have to convert
			DateTime operand1Date = DateTime.Parse(operand1.EffectivePaymentDate.Text.ToString());
			DateTime operand2Date = DateTime.Parse(operand2.EffectivePaymentDate.Text.ToString());
			return operand1Date.CompareTo(operand2Date);

		}
	}
}

namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two PaymentSummary records when sorting a list.
	/// </summary>
	public class CheckIdComparer : IComparer<PaymentSummary>
	{

		/// <summary>
		/// Compares two PaymentSummary records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
		{

			String operand1CheckIdText = String.Empty;
			String operand2CheckIdText = String.Empty;

			if (operand1.NullSafe(p => p.CheckId).NullSafe(p => p.Text) is String)
				operand1CheckIdText = (String)operand1.CheckId.Text;

			if (operand2.NullSafe(p => p.CheckId).NullSafe(p => p.Text) is String)
				operand2CheckIdText = (String)operand2.CheckId.Text;

			if (String.IsNullOrEmpty(operand1CheckIdText) == true && String.IsNullOrEmpty(operand2CheckIdText) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1CheckIdText) == false && String.IsNullOrEmpty(operand2CheckIdText) == true)
				return -1;

			return operand1CheckIdText.CompareTo(operand2CheckIdText);    


		}

	}

}

namespace FluidTrade.Guardian.Schema.DebtNegotiatorPaymentSummary
{
	using System;
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two Match.Match records when sorting a list.
	/// </summary>
	public class ModifiedUserIdComparer : System.Collections.Generic.IComparer<PaymentSummary>
	{

		/// <summary>
		/// Compares two PaymentSummary when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
		{
			String operand1ModifiedUserId = String.Empty;
			String operand2ModifiedUserId = String.Empty;

			if (operand1.NullSafe(p => p.ModifiedUserId).NullSafe(ModifiedUserId => ModifiedUserId.Name) is String)
				operand1ModifiedUserId = (String)operand1.ModifiedUserId.Name;

			if (operand2.NullSafe(p => p.ModifiedUserId).NullSafe(ModifiedUserId => ModifiedUserId.Name) is String)
				operand2ModifiedUserId = (String)operand2.ModifiedUserId.Name;

			if (String.IsNullOrEmpty(operand1ModifiedUserId) == true && String.IsNullOrEmpty(operand2ModifiedUserId) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1ModifiedUserId) == false && String.IsNullOrEmpty(operand2ModifiedUserId) == true)
				return -1;

			return operand1ModifiedUserId.CompareTo(operand2ModifiedUserId);
		}

	}

}

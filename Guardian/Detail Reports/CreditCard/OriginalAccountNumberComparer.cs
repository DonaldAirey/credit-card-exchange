namespace FluidTrade.Guardian.Schema.CreditCard
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two CreditCard.CreditCard records when sorting a list.
	/// </summary>
	public class OriginalAccountNumberComparer : IComparer<CreditCard>
	{

		/// <summary>
		/// Compares two CreditCard records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(CreditCard operand1, CreditCard operand2)
		{
			String operand1OriginalAccountNumber = String.Empty;
			String operand2OriginalAccountNumber = String.Empty;

			if (operand1.NullSafe(p => p.OriginalAccountNumber).NullSafe(p => p.Text) is String)
				operand1OriginalAccountNumber = (String)operand1.OriginalAccountNumber.Text;

			if (operand2.NullSafe(p => p.OriginalAccountNumber).NullSafe(p => p.Text) is String)
				operand2OriginalAccountNumber = (String)operand2.OriginalAccountNumber.Text;

			if (String.IsNullOrEmpty(operand1OriginalAccountNumber) == true && String.IsNullOrEmpty(operand2OriginalAccountNumber) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1OriginalAccountNumber) == false && String.IsNullOrEmpty(operand2OriginalAccountNumber) == true)
				return -1;

			return operand1OriginalAccountNumber.CompareTo(operand2OriginalAccountNumber);    
			
		}

	}

}

namespace FluidTrade.Guardian.Schema.CreditCard
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two CreditCard.CreditCard records when sorting a list.
	/// </summary>
	public class AccountNumberComparer : IComparer<CreditCard>
	{

		/// <summary>
		/// Compares two CreditCard records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(CreditCard operand1, CreditCard operand2)
		{
			String operand1AccountNumber = String.Empty;
			String operand2AccountNumber = String.Empty;

			if (operand1.NullSafe(p => p.AccountNumber).NullSafe(p => p.Text) is String)
				operand1AccountNumber = (String)operand1.AccountNumber.Text;

			if (operand2.NullSafe(p => p.AccountNumber).NullSafe(p => p.Text) is String)
				operand2AccountNumber = (String)operand2.AccountNumber.Text;

			if (String.IsNullOrEmpty(operand1AccountNumber) == true && String.IsNullOrEmpty(operand2AccountNumber) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1AccountNumber) == false && String.IsNullOrEmpty(operand2AccountNumber) == true)
				return -1;

			return operand1AccountNumber.CompareTo(operand2AccountNumber);    
			
		}

	}

}

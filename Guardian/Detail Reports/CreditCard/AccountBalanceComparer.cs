namespace FluidTrade.Guardian.Schema.CreditCard
{

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Compares two CreditCard.CreditCard records when sorting a list.
	/// </summary>
	public class AccountBalanceComparer : IComparer<CreditCard>
	{

		/// <summary>
		/// Compares two Credit Card records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(CreditCard operand1, CreditCard operand2)
		{
			if (operand1.AccountBalance == null && operand2.AccountBalance == null)
				return 0;
			if (operand1.AccountBalance == null && operand2.AccountBalance != null)
				return 1;
			if (operand1.AccountBalance != null && operand2.AccountBalance == null)
				return -1;
			return ((Decimal)operand1.AccountBalance.Text).CompareTo((Decimal)operand2.AccountBalance.Text);
		}

	}

}

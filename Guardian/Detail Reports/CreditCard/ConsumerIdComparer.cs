namespace FluidTrade.Guardian.Schema.CreditCard
{

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Compares two CreditCard records when sorting a list.
	/// </summary>
	public class ConsumerIdComparer : IComparer<CreditCard>
	{

		/// <summary>
		/// Compares two CreditCard records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(CreditCard operand1, CreditCard operand2)
		{

			Guid operand1Guid = (Guid)operand1.ConsumerId.Text;
			Guid operand2Guid = (Guid)operand2.ConsumerId.Text;

			return operand1Guid.CompareTo(operand2Guid);

		}

	}

}
namespace FluidTrade.Guardian.Schema.DebtNegotiatorPaymentSummary
{
	using System;
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two Match.Match records when sorting a list.
	/// </summary>
	public class StatusComparer : System.Collections.Generic.IComparer<PaymentSummary>
	{

		/// <summary>
		/// Compares two PaymentSummary records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
		{
			int operand1Status = 0;
			int operand2Status = 0;

			if (operand1.NullSafe(p => p.StatusId).NullSafe(StatusId => StatusId.StatusSortOrder) is String)
			{
				try
				{
					operand1Status = Convert.ToInt32(operand1.StatusId.StatusSortOrder);
				}
				catch (FormatException fe)
				{
					Console.WriteLine("Input string is not a sequence of digits." + fe.Message);
				}
				catch (OverflowException oe)
				{
					Console.WriteLine("The number cannot fit in an Int32." + oe.Message);
				}
			}

			if (operand2.NullSafe(p => p.StatusId).NullSafe(Status => Status.StatusSortOrder) is String)
			{
				try
				{
					operand2Status = Convert.ToInt32(operand2.StatusId.StatusSortOrder);
				}
				catch (FormatException fe)
				{
					Console.WriteLine("Input string is not a sequence of digits." + fe.Message);
				}
				catch (OverflowException oe)
				{
					Console.WriteLine("The number cannot fit in an Int32." + oe.Message);
				}
			}

			return operand1Status.CompareTo(operand2Status);
		}

	}

}

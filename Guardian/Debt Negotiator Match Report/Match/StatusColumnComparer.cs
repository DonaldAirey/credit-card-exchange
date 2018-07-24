namespace FluidTrade.Guardian.Schema.DebtNegotiatorMatch
{
	using System;
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two Match.Match records when sorting a list.
	/// </summary>
	public class StatusColumnComparer : System.Collections.Generic.IComparer<Match>
	{

		/// <summary>
		/// Compares two Match records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(Match operand1, Match operand2)
		{
			int operand1Status = 0;
			int operand2Status = 0;

			if (operand1.NullSafe(p => p.Status).NullSafe(Status => Status.StatusSortOrder) is String)
			{
				try
				{
					operand1Status = Convert.ToInt32(operand1.Status.StatusSortOrder);
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

			if (operand2.NullSafe(p => p.Status).NullSafe(Status => Status.StatusSortOrder) is String)
			{
				try
				{
					operand2Status = Convert.ToInt32(operand2.Status.StatusSortOrder);
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

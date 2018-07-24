namespace FluidTrade.Guardian.Schema.DebtHolderWorkingOrder
{
	using System;
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class StatusComparer : System.Collections.Generic.IComparer<WorkingOrder>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{

			String operand1Status = String.Empty;
			String operand2Status = String.Empty;

            // TODO: Comment if Mnemonic is added in the future.
            //if (operand1.NullSafe(p => p.MatchStatus).NullSafe(Status => MatchStatus.Mnemonic) is String)
            //    operand1Status = (String)operand1.MatchStatus.Text;

            //if (operand2.NullSafe(p => p.MatchStatus).NullSafe(Status => MatchStatus.Mnemonic) is String)
            //    operand2Status = (String)operand2.MatchStatus.Mnemonic;

			if (String.IsNullOrEmpty(operand1Status) == true && String.IsNullOrEmpty(operand2Status) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1Status) == false && String.IsNullOrEmpty(operand2Status) == true)
				return -1;

			return operand1Status.CompareTo(operand2Status);

		}

	}

}

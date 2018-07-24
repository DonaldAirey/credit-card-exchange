namespace FluidTrade.Guardian.Schema.Match
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
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(Match operand1, Match operand2)
		{
			String operand1Status = String.Empty;
			String operand2Status = String.Empty;

			if (operand1.NullSafe(p => p.Status).NullSafe(Status => Status.Mnemonic) is String)
				operand1Status = (String)operand1.Status.Mnemonic;

			if (operand2.NullSafe(p => p.Status).NullSafe(Status => Status.Mnemonic) is String)
				operand2Status = (String)operand2.Status.Mnemonic;

			if (String.IsNullOrEmpty(operand1Status) == true && String.IsNullOrEmpty(operand2Status) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1Status) == false && String.IsNullOrEmpty(operand2Status) == true)
				return -1;

			return operand1Status.CompareTo(operand2Status);
		}

	}

}

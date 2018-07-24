namespace FluidTrade.Guardian.Schema.DebtNegotiatorMatch
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two Match records when sorting a list.
	/// </summary>
	public class ProvinceComparer : IComparer<Match>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
		{

			String operand1ProvinceText = String.Empty;
			String operand2ProvinceText = String.Empty;

			if (operand1.NullSafe(p => p.Province).NullSafe(p => p.Text) is String)
				operand1ProvinceText = (String)operand1.Province.Text;

			if (operand2.NullSafe(p => p.Province).NullSafe(p => p.Text) is String)
				operand2ProvinceText = (String)operand2.Province.Text;

			if (String.IsNullOrEmpty(operand1ProvinceText) == true && String.IsNullOrEmpty(operand2ProvinceText) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1ProvinceText) == false && String.IsNullOrEmpty(operand2ProvinceText) == true)
				return -1;

			return operand1ProvinceText.CompareTo(operand2ProvinceText);    

        }

	}

}

namespace FluidTrade.Guardian.Schema.DebtHolderMatch
{
    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>
    public class ModifiedByComparer : System.Collections.Generic.IComparer<Match>
    {

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {
			String operand1ModifiedBy = String.Empty;
			String operand2ModifiedBy = String.Empty;

			if (operand1.NullSafe(p => p.ModifiedBy).NullSafe(ModifiedBy => ModifiedBy.Name) is String)
				operand1ModifiedBy = (String)operand1.ModifiedBy.Name;

			if (operand2.NullSafe(p => p.ModifiedBy).NullSafe(ModifiedBy => ModifiedBy.Name) is String)
				operand2ModifiedBy = (String)operand2.ModifiedBy.Name;

			if (String.IsNullOrEmpty(operand1ModifiedBy) == true && String.IsNullOrEmpty(operand2ModifiedBy) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1ModifiedBy) == false && String.IsNullOrEmpty(operand2ModifiedBy) == true)
				return -1;

			return operand1ModifiedBy.CompareTo(operand2ModifiedBy);
        }

    }

}

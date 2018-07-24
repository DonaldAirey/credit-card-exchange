namespace FluidTrade.Guardian.Schema.DebtHolderMatch
{
    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>
    public class CreatedByComparer : System.Collections.Generic.IComparer<Match>
    {

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {
			String operand1CreatedBy = String.Empty;
			String operand2CreatedBy = String.Empty;

			if (operand1.NullSafe(p => p.CreatedBy).NullSafe(CreatedBy => CreatedBy.Name) is String)
				operand1CreatedBy = (String)operand1.CreatedBy.Name;

			if (operand2.NullSafe(p => p.CreatedBy).NullSafe(CreatedBy => CreatedBy.Name) is String)
				operand2CreatedBy = (String)operand2.CreatedBy.Name;

			if (String.IsNullOrEmpty(operand1CreatedBy) == true && String.IsNullOrEmpty(operand2CreatedBy) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1CreatedBy) == false && String.IsNullOrEmpty(operand2CreatedBy) == true)
				return -1;

			return operand1CreatedBy.CompareTo(operand2CreatedBy);
        }

    }

}

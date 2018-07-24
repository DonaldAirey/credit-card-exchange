namespace FluidTrade.Guardian.Schema.Match
{

    using System;

    class SecurityNameComparer : System.Collections.Generic.IComparer<Match>
    {
        #region IComparer<Match> Members

        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {
            if (operand1.SecurityName == null && operand2.SecurityName == null)
                return 0;
            if (operand1.SecurityName == null && operand2.SecurityName != null)
                return 1;
            if (operand1.SecurityName != null && operand2.SecurityName == null)
                return -1;
            return ((String)operand1.SecurityName.Name).CompareTo((String)operand2.SecurityName.Name);
        }

        #endregion
    }
}

﻿namespace FluidTrade.Guardian.Schema.DebtNegotiatorMatch
{

    using System;

    class AccountNumberComparer : System.Collections.Generic.IComparer<Match>
    {
        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {
            if (operand1.AccountNumber == null && operand2.AccountNumber == null)
                return 0;
            if (operand1.AccountNumber == null && operand2.AccountNumber != null)
                return 1;
            if (operand1.AccountNumber != null && operand2.AccountNumber == null)
                return -1;
            return ((String)operand1.AccountNumber.Text).CompareTo((String)operand2.AccountNumber.Text);
        }
    }
}
namespace FluidTrade.Guardian.Schema.DebtNegotiatorWorkingOrder
{
    using System;
    using System.Collections.Generic;
    using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two WorkingOrder records when sorting a list.
    /// </summary>
    /// </param name="operand1">The first row to be compared.</param>
    /// </param name="operand2">The second row to be compared.</param>
    /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
    class Address1Comparer : IComparer<WorkingOrder>
    {
        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
            String operand1Address1 = String.Empty;
            String operand2Address1 = String.Empty;

            if (operand1.NullSafe(p => p.Address1).NullSafe(Address1 => Address1.Text) is String)
                operand1Address1 = (String)operand1.Address1.Text;

            if (operand2.NullSafe(p => p.Address1).NullSafe(Address1 => Address1.Text) is String)
                operand2Address1 = (String)operand2.Address1.Text;

            if (String.IsNullOrEmpty(operand1Address1) == true && String.IsNullOrEmpty(operand2Address1) == false)
                return 1;
            if (String.IsNullOrEmpty(operand1Address1) == false && String.IsNullOrEmpty(operand2Address1) == true)
                return -1;

            return operand1Address1.CompareTo(operand2Address1);
        }
    }
}

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
    class Address2Comparer : IComparer<WorkingOrder>
    {
        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
            String operand1Address2 = String.Empty;
            String operand2Address2 = String.Empty;

            if (operand1.NullSafe(p => p.Address2).NullSafe(Address2 => Address2.Text) is String)
                operand1Address2 = (String)operand1.Address2.Text;

            if (operand2.NullSafe(p => p.Address2).NullSafe(Address2 => Address2.Text) is String)
                operand2Address2 = (String)operand2.Address2.Text;

            if (String.IsNullOrEmpty(operand1Address2) == true && String.IsNullOrEmpty(operand2Address2) == false)
                return 1;
            if (String.IsNullOrEmpty(operand1Address2) == false && String.IsNullOrEmpty(operand2Address2) == true)
                return -1;

            return operand1Address2.CompareTo(operand2Address2);
        }
    }
}

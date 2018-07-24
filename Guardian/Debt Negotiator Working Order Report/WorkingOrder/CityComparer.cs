using System;
using System.Collections.Generic;

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
    class CityComparer : IComparer<WorkingOrder>
    {

        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
            String operand1City = String.Empty;
            String operand2City = String.Empty;

            if (operand1.NullSafe(p => p.City).NullSafe(City => City.Text) is String)
                operand1City = (String)operand1.City.Text;

            if (operand2.NullSafe(p => p.City).NullSafe(City => City.Text) is String)
                operand2City = (String)operand2.City.Text;

            if (String.IsNullOrEmpty(operand1City) == true && String.IsNullOrEmpty(operand2City) == false)
                return 1;
            if (String.IsNullOrEmpty(operand1City) == false && String.IsNullOrEmpty(operand2City) == true)
                return -1;

            return operand1City.CompareTo(operand2City);
        }

    }
}

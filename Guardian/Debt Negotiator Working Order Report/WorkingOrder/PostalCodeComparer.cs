namespace FluidTrade.Guardian.Schema.DebtNegotiatorWorkingOrder
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two WorkingOrder.WorkingOrder records when sorting a list.
    /// </summary>
    public class PostalCodeComparer : IComparer<WorkingOrder>
    {

        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// </param name="operand1">The first row to be compared.</param>
        /// </param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
			String operand1PostalCode = String.Empty;
			String operand2PostalCode = String.Empty;

			if (operand1.NullSafe(p => p.PostalCode).NullSafe(p => p.Code) is String)
				operand1PostalCode = (String)operand1.PostalCode.Code;

			if (operand2.NullSafe(p => p.PostalCode).NullSafe(p => p.Code) is String)
				operand2PostalCode = (String)operand2.PostalCode.Code;

			if (String.IsNullOrEmpty(operand1PostalCode) == true && String.IsNullOrEmpty(operand2PostalCode) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1PostalCode) == false && String.IsNullOrEmpty(operand2PostalCode) == true)
				return -1;

			return operand1PostalCode.CompareTo(operand2PostalCode);
		}

    }

}



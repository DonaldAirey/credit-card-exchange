namespace FluidTrade.Guardian.Schema.DebtNegotiatorWorkingOrder
{

	using System;
	using System.Collections.Generic;
    using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class SalutationComparer : IComparer<WorkingOrder>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{
            String operand1Salutation = String.Empty;
            String operand2Salutation = String.Empty;

            if (operand1.NullSafe(p => p.Salutation).NullSafe(Salutation => Salutation.Name) is String)
                operand1Salutation = (String)operand1.Salutation.Name;

            if (operand2.NullSafe(p => p.Salutation).NullSafe(Salutation => Salutation.Name) is String)
                operand2Salutation = (String)operand2.Salutation.Name;

            if (String.IsNullOrEmpty(operand1Salutation) == true && String.IsNullOrEmpty(operand2Salutation) == false)
                return 1;
            if (String.IsNullOrEmpty(operand1Salutation) == false && String.IsNullOrEmpty(operand2Salutation) == true)
                return -1;

            return operand1Salutation.CompareTo(operand2Salutation);

		}

	}

}

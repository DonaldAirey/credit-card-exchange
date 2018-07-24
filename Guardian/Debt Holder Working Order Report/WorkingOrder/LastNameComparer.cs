namespace FluidTrade.Guardian.Schema.DebtHolderWorkingOrder
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class LastNameComparer : IComparer<WorkingOrder>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{
			String operand1LastName = String.Empty;
			String operand2LastName = String.Empty;

			if (operand1.NullSafe(p => p.LastName).NullSafe(p => p.Name) is String)
				operand1LastName = (String)operand1.LastName.Name;

			if (operand2.NullSafe(p => p.LastName).NullSafe(p => p.Name) is String)
				operand2LastName = (String)operand2.LastName.Name;

			if (String.IsNullOrEmpty(operand1LastName) == true && String.IsNullOrEmpty(operand2LastName) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1LastName) == false && String.IsNullOrEmpty(operand2LastName) == true)
				return -1;

			return operand1LastName.CompareTo(operand2LastName);    
			
		}

	}

}

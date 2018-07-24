namespace FluidTrade.Guardian.Schema.DebtHolderWorkingOrder
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class SecurityNameComparer : IComparer<WorkingOrder>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{
			String operand1SecurityName = String.Empty;
			String operand2SecurityName = String.Empty;

			if (operand1.NullSafe(p => p.SecurityName).NullSafe(p => p.Name) is String)
				operand1SecurityName = (String)operand1.SecurityName.Name;

			if (operand2.NullSafe(p => p.SecurityName).NullSafe(p => p.Name) is String)
				operand2SecurityName = (String)operand2.SecurityName.Name;

			if (String.IsNullOrEmpty(operand1SecurityName) == true && String.IsNullOrEmpty(operand2SecurityName) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1SecurityName) == false && String.IsNullOrEmpty(operand2SecurityName) == true)
				return -1;

			return operand1SecurityName.CompareTo(operand2SecurityName);    
			
		}

	}

}

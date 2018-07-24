namespace FluidTrade.Guardian.Schema.DebtHolderWorkingOrder
{

	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class SocialSecurityNumberComparer : IComparer<WorkingOrder>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{
			String operand1SocialSecurityNumber = String.Empty;
			String operand2SocialSecurityNumber = String.Empty;

			if (operand1.NullSafe(p => p.SocialSecurityNumber).NullSafe(p => p.Number) is String)
				operand1SocialSecurityNumber = (String)operand1.SocialSecurityNumber.Number;

			if (operand2.NullSafe(p => p.SocialSecurityNumber).NullSafe(p => p.Number) is String)
				operand2SocialSecurityNumber = (String)operand2.SocialSecurityNumber.Number;

			if (String.IsNullOrEmpty(operand1SocialSecurityNumber) == true && String.IsNullOrEmpty(operand2SocialSecurityNumber) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1SocialSecurityNumber) == false && String.IsNullOrEmpty(operand2SocialSecurityNumber) == true)
				return -1;

			return operand1SocialSecurityNumber.CompareTo(operand2SocialSecurityNumber);

			
		}

	}

}

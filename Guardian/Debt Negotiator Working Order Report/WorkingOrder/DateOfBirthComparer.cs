namespace FluidTrade.Guardian.Schema.DebtNegotiatorWorkingOrder
{

	using System;

    /// <summary>
    /// Compares two WorkingOrder records when sorting a list.
	/// </summary>
    public class DateOfBirthComparer : System.Collections.Generic.IComparer<WorkingOrder>
	{

		/// <summary>
        /// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// <param name="operand1">The first row to be compared.</param>
		/// <param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{

			// Convert from datetime that is stored as a string to DateTime to perform date comparisions.
			DateTime operand1DateOfBirth = DateTime.MaxValue;
			DateTime operand2DateOfBirth = DateTime.MaxValue;

			if ((operand1 != null) && (operand1.DateOfBirth.DateTime != null))
				operand1DateOfBirth = (DateTime)operand1.DateOfBirth.DateTime;

			if ((operand2 != null) && (operand2.DateOfBirth.DateTime != null))
				operand2DateOfBirth = (DateTime)operand2.DateOfBirth.DateTime;

			return operand1DateOfBirth.CompareTo(operand2DateOfBirth);

		}

	}

}

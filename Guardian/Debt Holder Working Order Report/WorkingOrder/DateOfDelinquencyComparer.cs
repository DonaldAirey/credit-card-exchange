namespace FluidTrade.Guardian.Schema.DebtHolderWorkingOrder
{

	using System;

    /// <summary>
    /// Compares two WorkingOrder records when sorting a list.
	/// </summary>
    public class DateOfDelinquencyComparer : System.Collections.Generic.IComparer<WorkingOrder>
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
			String operand1DateOfDelinquencyString = (String)operand1.DateOfDelinquency.DateTime;
			String operand2DateOfDelinquencyString = (String)operand2.DateOfDelinquency.DateTime;

			DateTime operand1DateOfDelinquency = DateTime.MaxValue;
			DateTime operand2DateOfDelinquency = DateTime.MaxValue;

			if (!String.IsNullOrEmpty((String)operand1.DateOfDelinquency.DateTime))
				operand1DateOfDelinquency = DateTime.Parse(operand1DateOfDelinquencyString);

			if (!String.IsNullOrEmpty((String)operand2.DateOfDelinquency.DateTime))
				operand2DateOfDelinquency = DateTime.Parse(operand2DateOfDelinquencyString);

			return operand1DateOfDelinquency.CompareTo(operand2DateOfDelinquency);

		}

	}

}

namespace FluidTrade.Guardian.Schema.ConsumerTrustWorkingOrder
{


    /// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class ExecutionQuantityComparer : System.Collections.Generic.IComparer<WorkingOrder>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{
			return operand1.ExecutionQuantity.Quantity.CompareTo(operand2.ExecutionQuantity.Quantity);
		}

	}

}

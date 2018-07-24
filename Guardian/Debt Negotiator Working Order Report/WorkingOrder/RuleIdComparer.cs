namespace FluidTrade.Guardian.Schema.DebtNegotiatorWorkingOrder
{

	using System.Collections.Generic;
	using System;

	/// <summary>
	/// Compares two WorkingOrder.WorkingOrder records when sorting a list.
	/// </summary>
	public class RuleIdComparer : IComparer<WorkingOrder>
	{

		/// <summary>
		/// Compares two WorkingOrder records when sorting a list.
		/// </summary>
		/// </param name="operand1">The first row to be compared.</param>
		/// </param name="operand2">The second row to be compared.</param>
		/// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(WorkingOrder operand1, WorkingOrder operand2)
		{
            // TODO: Neither of these works - Need to find a solution.
            //Guid operand1Guid = (operand1.RuleId.DebtRuleId);
            //Guid operand2Guid = (operand2.RuleId.DebtRuleId); 

            //return operand1Guid.CompareTo(operand2Guid);
            return operand1.RuleId.DebtRuleId.CompareTo(operand2.RuleId.DebtRuleId);

		}

	}

}

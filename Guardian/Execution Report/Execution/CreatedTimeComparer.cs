namespace FluidTrade.Guardian.Schema.Execution
{

    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
    /// Compares two Match.Match records when sorting a list.
    /// </summary>
	public class CreatedTimeComparer : System.Collections.Generic.IComparer<Execution>
    {

        /// <summary>
        /// Compares two Match records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(Execution operand1, Execution operand2)
        {
			DateTime operand1Balance = DateTime.MinValue;
			DateTime operand2Balance = DateTime.MinValue;

			if (operand1.NullSafe(p => p.CreatedTime).NullSafe(p => p.Text) is DateTime)
				operand1Balance = (DateTime)operand1.CreatedTime.Text;

			if (operand2.NullSafe(p => p.CreatedTime).NullSafe(p => p.Text) is DateTime)
				operand2Balance = (DateTime)operand2.CreatedTime.Text;

			return operand1Balance.CompareTo(operand2Balance);      

       }

    }

}

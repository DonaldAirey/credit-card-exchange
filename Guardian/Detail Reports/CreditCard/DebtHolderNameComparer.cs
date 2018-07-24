namespace FluidTrade.Guardian.Schema.CreditCard
{
    using System;
	using FluidTrade.Core.Windows.Utilities;

    /// <summary>
	/// Compares two CreditCard records when sorting a list.
    /// </summary>
	public class DebtHolderNameComparer : System.Collections.Generic.IComparer<CreditCard>
    {

        /// <summary>
		/// Compares two CreditCard records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(CreditCard operand1, CreditCard operand2)
        {
			String operand1DebtHolderName = String.Empty;
			String operand2DebtHolderName = String.Empty;

			if (operand1.NullSafe(p => p.DebtHolderName).NullSafe(DebtHolderName => DebtHolderName.Text) is String)
				operand1DebtHolderName = (String)operand1.DebtHolderName.Text;

			if (operand2.NullSafe(p => p.DebtHolderName).NullSafe(DebtHolderName => DebtHolderName.Text) is String)
				operand2DebtHolderName = (String)operand2.DebtHolderName.Text;

			if (String.IsNullOrEmpty(operand1DebtHolderName) == true && String.IsNullOrEmpty(operand2DebtHolderName) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1DebtHolderName) == false && String.IsNullOrEmpty(operand2DebtHolderName) == true)
				return -1;

			return operand1DebtHolderName.CompareTo(operand2DebtHolderName);
        }

    }

}

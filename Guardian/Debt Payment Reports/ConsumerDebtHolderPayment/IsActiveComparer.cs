namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two Match.Match records when sorting a list.
	/// </summary>
    public class IsActiveComparer : System.Collections.Generic.IComparer<PaymentSummary>
	{

        /// <summary>
		/// Compares two PaymentSummary records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
		{
			bool operand1Active = false;
			bool operand2Active = false;

			if (operand1.NullSafe(p => p.ActiveFlag).NullSafe(c => c.Active))
				operand1Active = (bool)operand1.ActiveFlag.Active;

			if (operand2.NullSafe(p => p.ActiveFlag).NullSafe(c => c.Active))
				operand2Active = (bool)operand2.ActiveFlag.Active;
			
			return operand1Active.CompareTo(operand2Active);
		}

	}

}

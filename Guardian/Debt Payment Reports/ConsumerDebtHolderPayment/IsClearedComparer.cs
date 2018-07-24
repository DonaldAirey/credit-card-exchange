namespace FluidTrade.Guardian.Schema.DebtHolderPaymentSummary
{
	using FluidTrade.Core.Windows.Utilities;

	/// <summary>
	/// Compares two Match.Match records when sorting a list.
	/// </summary>
    public class IsClearedComparer : System.Collections.Generic.IComparer<PaymentSummary>
	{

        /// <summary>
		/// Compares two PaymentSummary records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(PaymentSummary operand1, PaymentSummary operand2)
		{
			bool operand1Cleared = false;
			bool operand2Cleared = false;

			if (operand1.NullSafe(p => p.IsCleared).NullSafe(c => c.Cleared))
				operand1Cleared = (bool)operand1.IsCleared.Cleared;

			if (operand2.NullSafe(p => p.IsCleared).NullSafe(c => c.Cleared))
				operand2Cleared = (bool)operand2.IsCleared.Cleared;
			
			return operand1Cleared.CompareTo(operand2Cleared);
		}

	}

}

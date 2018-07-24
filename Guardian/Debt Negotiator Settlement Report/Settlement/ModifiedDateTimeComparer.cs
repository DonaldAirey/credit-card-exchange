namespace FluidTrade.Guardian.Schema.DebtNegotiatorSettlement
{


    /// <summary>
    /// Compares two WorkingOrder records when sorting a list.
    /// </summary>
    public class ModifiedDateTimeComparer : System.Collections.Generic.IComparer<Settlement>
    {

        /// <summary>
		/// Compares two Settlement rows when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
		public int Compare(Settlement operand1, Settlement operand2)
        {
            return operand1.ModifiedDateTime.DateTime.CompareTo(operand2.ModifiedDateTime.DateTime);
        }

    }

}

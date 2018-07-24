namespace FluidTrade.Guardian.Schema.DebtHolderMatch
{

    using System;
	using FluidTrade.Core.Windows.Utilities;

    class Address2Comparer : System.Collections.Generic.IComparer<Match>
    {
        #region IComparer<Match> Members

        /// <summary>
        /// Compares two WorkingOrder records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(Match operand1, Match operand2)
        {

			String operand1Address = String.Empty;
			String operand2Address = String.Empty;

			if (operand1.NullSafe(p => p.Address2).NullSafe(address => address.Text) is String)
				operand1Address = (String)operand1.Address2.Text;

			if (operand2.NullSafe(p => p.Address2).NullSafe(address => address.Text) is String)
				operand2Address = (String)operand2.Address2.Text;

			if (String.IsNullOrEmpty(operand1Address) == true && String.IsNullOrEmpty(operand2Address) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1Address) == false && String.IsNullOrEmpty(operand2Address) == true)
				return -1;

			return operand1Address.CompareTo(operand2Address);          
        }

        #endregion
    }
}

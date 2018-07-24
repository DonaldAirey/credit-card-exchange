namespace FluidTrade.Guardian.Schema.CreditCard
{
	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    class FirstNameComparer : IComparer<CreditCard>
    {

        #region IComparer<CreditCard> Members

        /// <summary>
        /// Compares two CreditCard records when sorting a list.
        /// </summary>
        /// <param name="operand1">The first row to be compared.</param>
        /// <param name="operand2">The second row to be compared.</param>
        /// <returns>1 if operand1 is greater than operand2, -1 if operand1 is less than operand2, 0 if they are equal.</returns>
        public int Compare(CreditCard operand1, CreditCard operand2)
        {

			String operand1FirstName = String.Empty;
			String operand2FirstName = String.Empty;

			if (operand1.NullSafe(p => p.FirstName).NullSafe(FirstName => FirstName.Text) is String)
				operand1FirstName = (String)operand1.FirstName.Text;

			if (operand2.NullSafe(p => p.FirstName).NullSafe(FirstName => FirstName.Text) is String)
				operand2FirstName = (String)operand2.FirstName.Text;

			if (String.IsNullOrEmpty(operand1FirstName) == true && String.IsNullOrEmpty(operand2FirstName) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1FirstName) == false && String.IsNullOrEmpty(operand2FirstName) == true)
				return -1;

			return operand1FirstName.CompareTo(operand2FirstName);            
        }
         
        #endregion
    }
}

namespace FluidTrade.Guardian.Schema.DebtHolderWorkingOrder
{
    using System;
    using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    class Address1Comparer : IComparer<WorkingOrder>
    {
        #region IComparer<WorkingOrder> Members

        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
			String operand1Address = String.Empty;
			String operand2Address = String.Empty;

			if (operand1.NullSafe(p => p.Address1).NullSafe(address => address.Text) is String)
				operand1Address =  (String)operand1.Address1.Text;

			if (operand2.NullSafe(p => p.Address1).NullSafe(address => address.Text) is String)
				operand2Address = (String)operand2.Address1.Text;

			if (String.IsNullOrEmpty(operand1Address) == true && String.IsNullOrEmpty(operand2Address) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1Address) == false && String.IsNullOrEmpty(operand2Address) == true)
				return -1;

			return operand1Address.CompareTo(operand2Address);
        }

        #endregion
    }
}

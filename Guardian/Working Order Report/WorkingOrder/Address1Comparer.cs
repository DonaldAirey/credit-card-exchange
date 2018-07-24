using System;
using System.Collections.Generic;

namespace FluidTrade.Guardian.Schema.ConsumerTrustWorkingOrder
{
    class Address1Comparer : IComparer<WorkingOrder>
    {
        #region IComparer<WorkingOrder> Members

        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
            if (operand1.Address1 == null && operand2.Address1 == null)
                return 0;
            if (operand1.Address1 == null && operand2.Address1 != null)
                return 1;
            if (operand1.Address1 != null && operand2.Address1 == null)
                return -1;
            return ((String)operand1.Address1.Text).CompareTo((String)operand2.Address1.Text);
        }

        #endregion
    }
}

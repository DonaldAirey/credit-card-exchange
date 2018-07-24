using System;
using System.Collections.Generic;

namespace FluidTrade.Guardian.Schema.ConsumerTrustWorkingOrder
{
    class CityComparer : IComparer<WorkingOrder>
    {

        #region IComparer<WorkingOrder> Members

        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
            if (operand1.City == null && operand2.City == null)
                return 0;
            if (operand1.City == null && operand2.City != null)
                return 1;
            if (operand1.City != null && operand2.City == null)
                return -1;
            return ((String)operand1.City.Text).CompareTo((String)operand2.City.Text);
        }

        #endregion
    }
}

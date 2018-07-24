namespace FluidTrade.Guardian.Schema.DebtHolderWorkingOrder
{
	using System;
	using System.Collections.Generic;
	using FluidTrade.Core.Windows.Utilities;

    class CityComparer : IComparer<WorkingOrder>
    {

        #region IComparer<WorkingOrder> Members

        public int Compare(WorkingOrder operand1, WorkingOrder operand2)
        {
			String operand1City = String.Empty;
			String operand2City = String.Empty;

			if (operand1.NullSafe(p => p.City).NullSafe(city => city.Text) is String)
				operand1City = (String)operand1.City.Text;

			if (operand2.NullSafe(p => p.City).NullSafe(city => city.Text) is String)
				operand2City = (String)operand2.City.Text;

			if (String.IsNullOrEmpty(operand1City) == true && String.IsNullOrEmpty(operand2City) == false)
				return 1;
			if (String.IsNullOrEmpty(operand1City) == false && String.IsNullOrEmpty(operand2City) == true)
				return -1;

			return operand1City.CompareTo(operand2City);            
        }

        #endregion
    }
}

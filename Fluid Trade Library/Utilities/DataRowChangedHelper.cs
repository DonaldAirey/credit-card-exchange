using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FluidTrade.Core.Utilities
{
	/// <summary>
	/// helper class/method for checking for changes on a dataRow
	/// 
	/// </summary>
	public static class DataRowChangedHelper
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="row">row to check for changes</param>
		/// <param name="columnList">list of columns to include or exclude when checking for change</param>
		/// <param name="columnListIncludeExclude">true if columns param is a list of columns to check for
		/// pending changes, otherwise the columns param is a list of columns to exclude when 
		/// checking for changes</param>
		/// <returns>true if row has changes. if the row is in an added state then this will return false</returns>
		public static bool HasRowValuesChanged(DataRow row, List<System.Data.DataColumn> columnList, bool columnListIncludeExclude)
		{
			if(row.HasVersion(DataRowVersion.Current)  == false ||
				row.HasVersion(DataRowVersion.Original) == false)
				return false;

			if(columnListIncludeExclude == true)
			{
				foreach(DataColumn dc in columnList)
					if(false == object.Equals(row[dc, DataRowVersion.Original], row[dc, DataRowVersion.Current]))
						return true;

				return false;
			}

			HashSet<DataColumn> dataColumnSet = new HashSet<DataColumn>(columnList);
			foreach(DataColumn dc in row.Table.Columns)
			{
				if(dataColumnSet.Contains(dc))
					continue;

				if(false == object.Equals(row[dc, DataRowVersion.Original], row[dc, DataRowVersion.Current]))
					return true;
			}
			return false;
		}
	}
}

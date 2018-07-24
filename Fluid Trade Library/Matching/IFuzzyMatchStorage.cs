using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FluidTrade.Core;

namespace FluidTrade.Core.Matching
{
	public interface IFuzzyMatchStorage
	{
		SupportedLookupType LookupTypes { get; }

		List<MatchInfo> FindExact(object value, IRowLockingWrapper valueDataRow, 
										IDataModelTransaction dataModelTransaction);

		Dictionary<IRow, MatchInfo> FindProbable(object value, IRowLockingWrapper valueDataRow,
													bool useValidateForStrength, IDataModelTransaction dataModelTransaction);

		#region Not Implemented SearchForProbableUsingValidate and SearchForProbableUsingValidate
		//Dictionary<DataRow, MatchInfo> SearchForProbableUsingDataMasks(object value, IExtendedStorageRow extendedStorageRow, bool useValidateForStrength);

		//List<MatchInfo> SearchForProbableUsingValidate(object value);
		#endregion

		MatchInfo Validate(object value, IRowLockingWrapper targetValidationRow, IDataModelTransaction dataModelTransaction);

		void AddTableRowsToMap(IDataModelTransaction dataModelTransaction);
		void AddRowToMap(IRowLockingWrapper rowWrapper);

		void RemoveRowFromMap(IRow row);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FluidTrade.Core.Matching
{
	public class MatcherCreateArgs
	{
		public MatcherCreateArgs()
		{
			this.MinMatchPercent = 51;
		}

		public MatcherCreateArgs(int numberOfCharsInData, DataCharType dataChars,
								SupportedLookupType lookupTypes,
								ExtendedStorageTypes extendedStorageUsage, DataColumn sourceColumn)
		{
			this.MinMatchPercent = 51;
			
			this.NumberCharatersInData = numberOfCharsInData;
			this.DataChars = dataChars;
			this.LookupTypes = lookupTypes;
			this.ExtendedStorageUsage = extendedStorageUsage;
			this.SourceColumn = sourceColumn;
		}

		public int NumberCharatersInData { get; set; }
		public int MinMatchPercent { get; set; }
		public DataCharType DataChars { get; set; }
		public SupportedLookupType LookupTypes { get; set; }
		public ExtendedStorageTypes ExtendedStorageUsage { get; set; }
		public Predicate<IRowLockingWrapper> RowValidForMatcherMap { get; set; }
		public DataColumn SourceColumn { get; set; }
	}

}

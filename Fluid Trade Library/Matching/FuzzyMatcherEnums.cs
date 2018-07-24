using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Core.Matching
{
	public enum DataCharType
	{
		DigitsOnly,
		AlphaNumericString,
		String
	}

	[Flags]
	public enum SupportedLookupType
	{
		ValidateOnly = 0,
		ExactLookup = 0x01,
		UseDataMasksForProbable = 0x02
	}


	[Flags]
	public enum ExtendedStorageTypes
	{
		None = 0,
		ConvertToValue = 0x01,
		ConvertToValueList = 0x02,
		GetDataMasks = 0x04
	}
}

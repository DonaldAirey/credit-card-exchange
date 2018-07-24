using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FluidTrade.Core.Matching
{
	public class DateMatcher : ColumnFuzzyMatcher
	{
		public DateMatcher(MatcherCreateArgs createArgs)
			: base(createArgs)
		{
		}

		protected override IFuzzyMatchStorage CreateStorage(MatcherCreateArgs createArgs)
		{
			return new DateStorage(createArgs);
		}

		protected class DateStorage : NineDigitOrLessMatcher.UIntStorage
		{
			public DateStorage(MatcherCreateArgs createArgs)
				: base(createArgs)
			{
			}

			protected override bool ConvertToValueInternal(object value, bool allowExtraCharacters,
								out bool isInputValueComplete, out UInt32 convertedValue,
								out String overflowString)
			{
				return base.ConvertToValueInternal(DateObjectToInt(value, false), allowExtraCharacters, out isInputValueComplete, out convertedValue, out overflowString);
			}

			protected override List<UInt32> ConvertToValueListInternal(object value, out decimal factor)
			{
				List<UInt32> retList = base.ConvertToValueListInternal(DateObjectToInt(value, false), out factor);
				List<UInt32> retListSwap = base.ConvertToValueListInternal(DateObjectToInt(value, true), out factor);

				if (retList != null)
				{
					if (retListSwap != null)
						retList.AddRange(retListSwap);
				}
				else
				{
					retList = retListSwap;
				}

				return retList;
			}

			private object DateObjectToInt(object val, bool swapMonthDay)
			{
				DateTime dateVal;
				if (val is string)
				{
					if (DateTime.TryParse((string)val, out dateVal) == false)
						return null;
				}
				else
				{
					dateVal = (DateTime)val;
				}

				if (swapMonthDay)
					return dateVal.Day * 100000000 +
									dateVal.Month * 100000 +
									dateVal.Year;

				return dateVal.Month * 100000000 +
								dateVal.Day * 100000 +
								dateVal.Year;
			}
		}
	}
}

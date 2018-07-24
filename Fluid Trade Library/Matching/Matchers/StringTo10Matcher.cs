using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Runtime.ConstrainedExecution;

namespace FluidTrade.Core.Matching
{

	public class StringTo10Matcher : ColumnFuzzyMatcher
	{
		public StringTo10Matcher(MatcherCreateArgs createArgs)
			: base(createArgs)
		{
		}

		protected override IFuzzyMatchStorage CreateStorage(MatcherCreateArgs createArgs)
		{
			return new StringTo10Storage(createArgs);
		}

		protected class StringTo10Storage : FuzzyMatchStorage<String, UInt32>
		{
			protected int probableMatchSetsCount = 16;
			//protected readonly int[,] probableMatchSets = new int[,] {
			//                                        { 1, -1, -1, 0, 4, -1},
			//                                        { 1, 2, -1, 2, Int32.MaxValue, 78 },
			//                                        { 1, 2, 6, 6, Int32.MaxValue, 78 },
			//                                        { 1, 3, -1, 6, Int32.MaxValue, 78 }

			//                                        { 1, 5, -1, 9, Int32.MaxValue, 78 },
			//                                        { 1, 5, 6, 9, Int32.MaxValue, 78 },
			//                                        { 1, 6, -1, 2, 8, 78 },
			//                                        { 3, -1, -1, 12, Int32.MaxValue, 78 },
			//                                        { 3, 4, -1, 12, Int32.MaxValue, 78 },

			//                                        { 6, -1, -1, 0, 4, -1},
			//                                        { 6, 5, -1, 2, Int32.MaxValue, 78 },
			//                                        { 6, 4, -1, 6, Int32.MaxValue, 78 },
			//                                        { 6, 2, -1, 9, Int32.MaxValue, 78 },
			//                                        { 4, -1, -1, 12, Int32.MaxValue, 78 }
			//                                        };

			protected readonly StringPartDescriptor[] getSets = new StringPartDescriptor[]{
														new StringPartDescriptor0(),
														new StringPartDescriptor0_1(),
														new StringPartDescriptor0_1_5(),
														new StringPartDescriptor0_2(),
														new StringPartDescriptor0_4(),
														new StringPartDescriptor0_4_5(),
														new StringPartDescriptor0_5(),
														new StringPartDescriptor1(),
														new StringPartDescriptor1_4(),
														new StringPartDescriptor1_5(),
														new StringPartDescriptor2(),
														
														new StringPartDescriptor3(),
														new StringPartDescriptor2_3(),
														new StringPartDescriptor3_5(),
														new StringPartDescriptor5(),
														new StringPartDescriptor4_5(),
														};





			public StringTo10Storage(MatcherCreateArgs createArgs)
				: base(createArgs)
			{
			}

			protected override int InitializeAvailableDataMasks(int minMatchPercent)
			{
				return 16;
			}


			protected override Decimal GetMaskedDataMatchStrength(String curVal, int chunkIndex)
			{
				return getSets[chunkIndex].GetMaskedDataMatchStrength(curVal);
			}

			protected override string ValueToString(string value)
			{
				return value.ToString();
				//return value;
			}

			protected override bool ConvertToValueInternal(object value, bool allowExtraCharacters,
								out bool isInputValueComplete, out String convertedValue,
								out String overflowString)
			{
				overflowString = null;
				//!!!RM need to step through the string matching again.
				isInputValueComplete = false;

				if (value == null)
					convertedValue = string.Empty;
				else
					convertedValue = value.ToString().ToLowerInvariant();

				return true;
			}

			protected override List<String> ConvertToValueListInternal(object value, out decimal factor)
			{
				factor = 1M;
				List<String> retList = new List<String>();
				if (value == null)
					retList.Add(string.Empty);
				else
					retList.Add(value.ToString().ToLowerInvariant());

				return retList;
			}

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			protected override void FillMaskedDataArray(String curVal, UInt32[] chunkArray, out bool[] validAr)
			{
				int len = curVal.Length;
				validAr = new bool[probableMatchSetsCount];
				unsafe
				{
					fixed (char* src = curVal)
					{

						for (int i = 0; i < probableMatchSetsCount; i++)
						{
							int strIndex = 0;
							StringPartDescriptor sg = getSets[i];
							if (sg.IsValid(len) == false)
							{
								validAr[i] = false;
								continue;
							}
							validAr[i] = true;

							int hash1 = 5381;
							int hash2 = hash1;

							int c;
							char* s = src;
							while ((c = s[0]) != 0)
							{
								if (sg.IsInSet(curVal, strIndex))
								{
									hash1 = ((hash1 << 5) + hash1) ^ c;
									c = s[1];
									if (c == 0)
										break;
									hash2 = ((hash2 << 5) + hash2) ^ c;
								}
								s += 2;
								strIndex++;
							}

							chunkArray[i] = (UInt32)(hash1 + (hash2 * 1566083941));
						}
					}
				}

			}
		}
	}
}

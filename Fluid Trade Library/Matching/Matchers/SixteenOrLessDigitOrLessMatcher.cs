using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace FluidTrade.Core.Matching
{
	public class SixteenOrLessDigitOrLessMatcher : ColumnFuzzyMatcher
	{
		public SixteenOrLessDigitOrLessMatcher(MatcherCreateArgs createArgs)
			: base(createArgs)
		{
		}

		protected override IFuzzyMatchStorage CreateStorage(MatcherCreateArgs createArgs)
		{
			return new ULong16Storage(createArgs);
		}

		protected class ULong16Storage : FuzzyMatchStorage<UInt64, UInt64>
		{
			protected int probableMatchSetsCount = 15;
			private const ulong TensDigits			= 1000000000000000;
			private const ulong TensDigitsPlusOne	= 10000000000000000;
			private const int digitCount = 16;

			//!!!RM % are based on 18 not 16 ...adjust
			protected readonly int[,] probableMatchSets = new int[,] {
														{4, 3, -1, 0, 89},

														{ 1, 2, 6, 1, 78 },{1, 5, 6, 1, 78},
														{ 2, 3, -1, 1, 78 }, {4, 5, -1, 1, 78},
 
														{ 1, 3, -1, 2, 67 }, { 4, 6, -1, 2, 67 },

														{ 1, 2, -1, 3, 56 },{ 1, 5, -1, 3, 56 },
														{ 2, 5, -1, 3, 56 },{ 2, 6, -1, 3, 56 }, 
														{ 5, 6, -1, 3, 56 },

														{ 1, 6, -1, 4, 44 },{ 3, -1, -1, 4, 44 },
														{ 4, -1, -1, 4, 44 }};


			//protected static readonly UInt32[] probableChunkMask = new UInt32[] 
			//{     
			/*
			999919999 = MOD(MOD(S1, 10000)+(ROUND((S1/100000),0) *100000), 1000000000)
			119999999 = MOD(S2,10000000)
			991199999 = MOD(S3, 100000)+(ROUND((S3/10000000),0) *10000000)
			999991199 = MOD(S4, 100) +(ROUND((S4/10000),0) *10000)
			999999911 = ROUND(S5/100,0)*100
			991119999 = MOD(S6, 10000)+(ROUND((S6/10000000),0) *10000000)
			999911199 = MOD(S7, 100)+(ROUND((S7/100000),0) *100000)
			111199999 = MOD(S8,100000)
			119991199 = MOD(S9, 100) + ( ROUND((MOD(S9, 10000000))/10000, 0) *10000)
			119999911 = ROUND((MOD(S10, 10000000))/100,0)*100
			991199911 = (MOD(ROUND(S11/100,0), 1000) *100) +(ROUND((S11/10000000),0) *10000000)
			999991111 = ROUND(S12/10000,0)*10000
			991111199 = MOD(S13, 100) + ROUND(S13/10000000, 0)*10000000
			*/
			//};

			public ULong16Storage(MatcherCreateArgs createArgs)
				: base(createArgs)
			{
			}

			protected override int InitializeAvailableDataMasks(int minMatchPercent)
			{
				int maskedDataArrayLength = 0;
				if (this.createArgs.NumberCharatersInData > digitCount ||
					this.createArgs.NumberCharatersInData <= 0)
					this.createArgs.NumberCharatersInData = digitCount;
				else if (this.createArgs.NumberCharatersInData < 3)
					this.createArgs.NumberCharatersInData = 3;

				for (int i = 0; i < probableMatchSetsCount; i++)
				{
					if (probableMatchSets[i, 4] >= minMatchPercent)
						maskedDataArrayLength++;
					else
						break;
				}

				return maskedDataArrayLength;
			}


			protected override Decimal GetMaskedDataMatchStrength(UInt64 curVal, int chunkIndex)
			{
				return ((decimal)probableMatchSets[chunkIndex, 4]) / 100M;
			}

			protected override string ValueToString(ulong value)
			{
				return value.ToString();
				//return value.ToString("0000000000000000");
			}

			protected override bool ConvertToValueInternal(object value, bool allowExtraCharacters,
								out bool isInputValueComplete, out UInt64 convertedValue,
								out String overflowString)
			{
				overflowString = null;
				isInputValueComplete = false;
				convertedValue = default(UInt64);
				if (value == null)
					return false;


				if (value is Int32)
				{
					UInt64 intVal = (UInt64)value;
					if (intVal < TensDigitsPlusOne)
					{
						isInputValueComplete = true;
						convertedValue = intVal;
						return true;
					}
				}

				string valString = value.ToString();
				List<UInt64> retList = new List<UInt64>();
				StringBuilder intsSb = new StringBuilder(valString.Length);
				foreach (char c in valString)
				{
					if (char.IsDigit(c))
						intsSb.Append(c);
				}

				if (intsSb.Length == 0)
					return false;

				if (intsSb.Length <= this.createArgs.NumberCharatersInData)
				{
					convertedValue = UInt64.Parse(intsSb.ToString());
					isInputValueComplete = intsSb.Length == this.createArgs.NumberCharatersInData;
					return true;
				}
				if(allowExtraCharacters == true)
				{
					overflowString = intsSb.ToString();
				}

				return false;
			}

			protected override List<UInt64> ConvertToValueListInternal(object value, out decimal factor)
			{
				factor = 0M;
				if (value == null)
					return null;

				if (value is UInt64)
				{
					UInt64 intVal = (UInt64)value;
					if (intVal < TensDigitsPlusOne)
					{
						List<UInt64> single = new List<UInt64>();
						single.Add(intVal);
						return single;
					}
				}

				string valString = value.ToString();
				List<UInt64> retList = new List<UInt64>();
				StringBuilder intsSb = new StringBuilder(valString.Length);
				foreach (char c in valString)
				{
					if (char.IsDigit(c))
						intsSb.Append(c);
				}
				int digitCount = intsSb.Length;
				if (digitCount == this.createArgs.NumberCharatersInData)
				{
					factor = 1M;
					retList.Add(UInt64.Parse(intsSb.ToString()));
					return retList;
				}

				factor = (decimal)(this.createArgs.NumberCharatersInData - (Math.Abs(this.createArgs.NumberCharatersInData - digitCount))) / (decimal)this.createArgs.NumberCharatersInData;

				List<string> strings = new List<string>();
				//if bigger or smaller.. there could be a bunch of matches
				//!!!RM this can slow things down. maybe want to turn it off 
				//or as a differnt pass
				if (digitCount > this.createArgs.NumberCharatersInData)
				{
					//ColumnFuzzyMatcher.GetStringWithRemoved(intsSb, strings, 0, 0, digitCount - this.createArgs.NumberCharatersInData);
					ColumnFuzzyMatcher.GetStringWithRemovedMultiple(intsSb, strings, digitCount - this.createArgs.NumberCharatersInData);
				}
				else
				{
					//ColumnFuzzyMatcher.GetStringWithAdded(intsSb, strings, 0, 0, this.createArgs.NumberCharatersInData - digitCount);
					ColumnFuzzyMatcher.GetStringWithAddedMultiple(intsSb, strings, this.createArgs.NumberCharatersInData - digitCount);
				}

				foreach (string s in strings)
					retList.Add(UInt64.Parse(s));

				return retList;
			}

			protected override void FillMaskedDataArray(UInt64 curVal, UInt64[] chunkArray, out bool[] validAr)
			{
				validAr = null;
				/*					
							999919999 = MOD(MOD(S1, 10000)+(ROUND((S1/100000),0) *100000), 1000000000)
							119999999 = MOD(S2,10000000)
							991199999 = MOD(S3, 100000)+(ROUND((S3/10000000),0) *10000000)
							999991199 = MOD(S4, 100) +(ROUND((S4/10000),0) *10000)
							999999911 = ROUND(S5/100,0)*100
							991119999 = MOD(S6, 10000)+(ROUND((S6/10000000),0) *10000000)
							999911199 = MOD(S7, 100)+(ROUND((S7/100000),0) *100000)
							111199999 = MOD(S8,100000)
							119991199 = MOD(S9, 100) + ( ROUND((MOD(S9, 10000000))/10000, 0) *10000)
							119999911 = ROUND((MOD(S10, 10000000))/100,0)*100
							991199911 = (MOD(ROUND(S11/100,0), 1000) *100) +(ROUND((S11/10000000),0) *10000000)
							999991111 = ROUND(S12/10000,0)*10000
							991111199 = MOD(S13, 100) + ROUND(S13/10000000, 0)*10000000

			*/
				chunkArray[0] = (((curVal % 100000000) + ((curVal / 10000000000) * 10000000000)) % 100000000000000000) / 10;
				chunkArray[1] = ((curVal % 100000000000000) % 100000000000000000) / 10;
				chunkArray[2] = (((curVal % 10000000000) + ((curVal / 100000000000000) * 100000000000000)) % 100000000000000000) / 10;
				chunkArray[3] = (((curVal % 10000) + ((curVal / 100000000) * 100000000)) % 100000000000000000) / 10;
				chunkArray[4] = (((curVal / 10000) * 10000) % 100000000000000000) / 10;
				chunkArray[5] = (((curVal % 100000000) + ((curVal / 100000000000000) * 100000000000000)) % 100000000000000000) / 10;
				chunkArray[6] = (((curVal % 10000) + ((curVal / 10000000000) * 10000000000)) % 100000000000000000) / 10;
				chunkArray[7] = ((curVal % 10000000000) % 100000000000000000) / 10;
				chunkArray[8] = (((curVal % 10000) + (((curVal % 100000000000000) / 100000000) * 100000000)) % 100000000000000000) / 10;
				chunkArray[9] = ((((curVal % 100000000000000) / 10000) * 10000) % 100000000000000000) / 10;
				chunkArray[10] = (((((curVal / 10000) % 1000000) * 10000) + ((curVal / 100000000000000) * 100000000000000)) % 100000000000000000) / 10;
				chunkArray[11] = (((curVal / 100000000) * 100000000) % 100000000000000000) / 10;
				//chunkArray[12] = ((curVal % 10000) + ((curVal / 100000000000000) * 100000000000000)) % 1000000000000000000;
			}

			protected override bool ValidateInternal(UInt64 valA, IRowLockingWrapper valAStorage, string valAStr,
														UInt64 valB, IRowLockingWrapper valBStorage, string valBStr,
														out decimal strength)
			{
				if(valAStr != null ||
					valBStr != null)
					return base.ValidateInternal(valA, valAStorage, valAStr,
													valB, valBStorage, valBStr,
													out strength);

				if(valA >= TensDigitsPlusOne)
					valA -= TensDigitsPlusOne;
				if(valB >= TensDigitsPlusOne)
					valB -= TensDigitsPlusOne;

				if(valB == valA)
				{
					strength = 1M;
					return true;
				}

				int pow10A = EightteenOrLessDigitOrLessMatcher.ULongStorage.PowOfTen(valA);
				int pow10B = EightteenOrLessDigitOrLessMatcher.ULongStorage.PowOfTen(valB);
				int sameDigits = 0;

				if (pow10A == pow10B)
				{
					sameDigits = Compare(valA, valB);

				}
				else if (pow10A > pow10B)
				{
					sameDigits = Compare(valA, valB, (uint)(pow10A - pow10B));
				}
				else
				{
					sameDigits = Compare(valB, valA, (uint)(pow10B - pow10A));
				}

				int numer = sameDigits;
				int denom = digitCount;
				if(denom == 0 ||
					numer == 0)
				{
					strength = 0M;
				}
				else if(numer == denom)
				{
					strength = 1M;
				}
				else
				{
					strength = (decimal)numer /
							(decimal)denom;
				}

				return true;
			}

			private static int Compare(ulong valA, ulong valB)
			{
				ulong cur10Factor = TensDigits;
				int rlMatchCount = 0;
				ulong matchBits = 0;

				for (int i = 0; i < digitCount; i++)
				{
					ulong convValDigit = (valA % (cur10Factor * 10)) / cur10Factor;
					ulong convRowValDigit = (valB % (cur10Factor * 10)) / cur10Factor;

					matchBits <<= 1;
					if (convValDigit == convRowValDigit)
					{
						rlMatchCount++; //
						matchBits++;
					}
					cur10Factor /= 10;
				}
				return rlMatchCount;
			}

		
			/// <summary>
			/// a should be bigger
			/// </summary>
			/// <param name="valA"></param>
			/// <param name="valB"></param>
			/// <param name="diffAPow10_BPow10"></param>
			/// <returns></returns>
			private static int Compare(ulong valA, ulong valB, uint diffAPow10_BPow10)
			{
				ulong cur10Factor = TensDigits;
				int rlMatchCount = 0;
				ulong rlMatchBits = 0;

				for (int i = 0; i < digitCount; i++)
				{
					ulong convValDigit = (valA % (cur10Factor * 10)) / cur10Factor;
					ulong convRowValDigit = (valB % (cur10Factor * 10)) / cur10Factor;

					rlMatchBits <<= 1;
					if (convValDigit == convRowValDigit)
					{
						rlMatchCount++; //
						rlMatchBits++;
					}
					cur10Factor /= 10;
				}

				if(rlMatchCount > digitCount - 1)
				{
					return rlMatchCount;
				}


				while (diffAPow10_BPow10 > 0)
				{
					valB *= 10;
					diffAPow10_BPow10--;
				}

				uint lrMatchBits = 0;
				cur10Factor = TensDigits;
				for (int i = 0; i < digitCount; i++)
				{
					ulong convValDigit = (valA % (cur10Factor * 10)) / cur10Factor;
					ulong convRowValDigit = (valB % (cur10Factor * 10)) / cur10Factor;

					lrMatchBits <<= 1;
					if (convValDigit == convRowValDigit)
						lrMatchBits++; //

					cur10Factor /= 10;
				}

				rlMatchBits |= lrMatchBits;

				lrMatchBits = 0;
				for (int i = 0; i < digitCount; i++)
				{
					if ((rlMatchBits & 0x1) == 1)
						lrMatchBits++;

					rlMatchBits >>= 1;
				}

				return (int)lrMatchBits;
			}
		}
	}

}

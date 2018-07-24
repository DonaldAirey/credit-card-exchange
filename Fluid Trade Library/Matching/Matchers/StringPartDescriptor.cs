using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace FluidTrade.Core.Matching
{
	/// <summary>
	/// !!!RM  currently hard coded for string length of 10.. using first 2 chars and next 3 and
	/// then growing the last block.  this works ok up to 10 not sure about larger because after 10 digits the
	/// variable length block becomes more and more significant. 
	/// </summary>
	public abstract class StringPartDescriptor
	{
		public abstract bool IsInSet(string curVal, int index);
		//decimal Strength(string curVal);

		public abstract bool IsValid(int len);

		protected abstract int First { get; }
		protected abstract int Second { get; }
		
		protected abstract int Third { get; }

		public virtual decimal GetMaskedDataMatchStrength(string curVal)
		{
			if(curVal == null)
				return 0M;

			return GetFactor(curVal.Length, this.First, this.Second, this.Third);
		}


		private static Dictionary<string, decimal> partsFactorMap;
		static StringPartDescriptor()
		{
			partsFactorMap = GenPartsTemp();
		}

		public static decimal GetFactor(int numChars, int first, int second, int third)
		{
			string key;
			if (second < 0)
				key = string.Format("{0}.{1}", numChars, first);
			else if (third < 0)
				key = string.Format("{0}.{1}_{2}", numChars, first, second);
			else
				key =  string.Format("{0}.{1}_{2}_{3}", numChars, first, second, third);

			decimal retVal;

			if (partsFactorMap.TryGetValue(key, out retVal) == false)
				return 0M;

			return retVal;
		}

		private static Dictionary<string, decimal> GenPartsTemp()
		{
			Decimal factor = 0.01M;

			Dictionary<string, decimal> factorMap = new Dictionary<string, decimal>();
			for (int len = 2; len < 24; len++)
			{
				Rectangle[] rects = new Rectangle[6];
				rects[0] = new Rectangle(0, 0, 2, 1);
				rects[1] = new Rectangle(2, 0, (len < 5) ? -1 : 3, 1);
				rects[2] = new Rectangle(5, 0, len - 5, 1);

				rects[3] = new Rectangle(0, 0, len - 5, 1);
				rects[4] = new Rectangle(len - 5, 0, 3, 1);
				rects[5] = new Rectangle(len - 2, 0, 2, 1);

				for (int i = 0; i < 6; i++)
				{
					if (rects[i].X < 0 ||
						rects[i].Width <= 0)
						continue;

					decimal factorI = (decimal)(rects[i].Width) / (decimal)len;
					if (factorI > factor &&
						factorI < 1M)
					{
						factorMap[string.Format("{0}.{1}", len, i)] = factorI;
					}

					Rectangle tmpI = rects[i];

					for (int j = i + 1; j < 6; j++)
					{
						if (rects[j].X < 0 ||
							rects[j].Width <= 0)
							continue;

						Rectangle tmpI2 = tmpI;
						Rectangle tmpJ = rects[j];
						Rectangle unionIJ;

						decimal factorJ;
						if (tmpJ.IntersectsWith(tmpI2))
						{
							tmpI2.Intersect(tmpJ);
							if (tmpI2 == tmpJ)
							{
								//I comsumes J
								continue;
							}
							tmpI2 = tmpI;
							tmpJ.Intersect(tmpI2);
							if (tmpJ == tmpI2)
							{
								//J comsumes I
								continue;
							}

							unionIJ = Rectangle.Union(tmpI, rects[j]);
							factorJ = (decimal)(unionIJ).Width / (decimal)len;
						}
						else
						{
							unionIJ = Rectangle.Empty;
							factorJ = (decimal)(rects[i].Width + rects[j].Width) / (decimal)len;
						}

						if (factorJ > factor &&
							factorJ < 1M)
							factorMap[string.Format("{0}.{1}_{2}", len, i, j)] = factorJ;


						for (int k = j + 1; k < 6; k++)
						{
							if (rects[k].X < 0 ||
							rects[k].Width <= 0)
								continue;

							Rectangle tmpI3 = rects[i];
							Rectangle tmpJ2 = rects[j];
							Rectangle tmpK = rects[k];

							if (tmpK.IntersectsWith(tmpI3))
							{
								tmpI3.Intersect(tmpK);
								if (tmpI3 == tmpK)
								{
									//I comsumes K
									continue;
								}
								tmpI3 = tmpI2;
								tmpK.Intersect(tmpI3);
								if (tmpK == tmpI3)
								{
									//K comsumes I
									continue;
								}
							}

							if (tmpK.IntersectsWith(tmpJ2))
							{
								tmpJ2.Intersect(tmpK);
								if (tmpJ2 == tmpK)
								{
									//K comsumes J
									continue;
								}
								tmpJ2 = tmpJ;
								tmpK.Intersect(tmpJ2);
								if (tmpK == tmpJ2)
								{
									//J comsumes K
									continue;
								}
							}

							decimal factorK;
							if (unionIJ != Rectangle.Empty &&
								tmpK.IntersectsWith(unionIJ))
							{
								factorK = (decimal)(Rectangle.Union(unionIJ, rects[k])).Width / (decimal)len;
							}
							else if (tmpK.IntersectsWith(rects[i]) &&
								tmpK.IntersectsWith(rects[j]))
							{

								factorK = (decimal)
									Rectangle.Union(Rectangle.Union(rects[i], rects[k]), rects[j]).Width / (decimal)len;
							}
							else if (tmpK.IntersectsWith(rects[i]))
							{
								factorK = (decimal)
									Rectangle.Union(rects[i], rects[k]).Width / (decimal)len;
							}
							else if (tmpK.IntersectsWith(rects[j]))
							{
								factorK = (decimal)
									Rectangle.Union(rects[j], rects[k]).Width / (decimal)len;
							}
							else
							{
								if (unionIJ == Rectangle.Empty)
									factorK = (decimal)(rects[i].Width + rects[j].Width + rects[k].Width) / (decimal)len;
								else
									factorK = (decimal)(unionIJ.Width + rects[k].Width) / (decimal)len;
							}

							if (factorK > factor &&
								factorK < 1M)
								factorMap[string.Format("{0}.{1}_{2}_{3}", len, i, j, k)] = factorK;
						}
					}
				}

			}

			return factorMap;
		}
	
	}
	public class StringPartDescriptor0 : StringPartDescriptor
	{
		public StringPartDescriptor0()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return (index < 2);
		}

		public override bool IsValid(int len)
		{
			return len < 4;
		}

		protected override int  First
		{
			get { return 0; }
		}

		protected override int  Second
		{
			get { return -1; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor0_1 : StringPartDescriptor
	{
		public StringPartDescriptor0_1()
		{
		}


		public override bool IsInSet(string curVal, int index)
		{
			return (index < 6);
		}

		public override bool IsValid(int len)
		{
			return len >2;
		}

		
		protected override int First
		{
			get { return 0; }
		}

		protected override int Second
		{
			get { return 1; }
		}

		protected override int Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor0_1_5 : StringPartDescriptor
	{
		public StringPartDescriptor0_1_5()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return (index < 6) ||
						index >= (curVal.Length - 2);
		}

		public override bool IsValid(int len)
		{
			return len > 6;
		}

		protected override int First
		{
			get { return 0; }
		}

		protected override int Second
		{
			get { return 1; }
		}

		protected override int Third
		{
			get { return 5; }
		}
	}

	public class StringPartDescriptor0_2 : StringPartDescriptor
	{
		public StringPartDescriptor0_2()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index < 2 ||
						index >= 6;
		}

		public override bool IsValid(int len)
		{
			return len > 6;
		}

		protected override int First
		{
			get { return 0; }
		}

		protected override int Second
		{
			get { return 2; }
		}

		protected override int Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor0_4 : StringPartDescriptor
	{
		public StringPartDescriptor0_4()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index < 2 ||
						(index >= (curVal.Length - 6) && index < (curVal.Length - 2));
		}

		public override bool IsValid(int len)
		{
			return len > 9;
		}

		protected override int First
		{
			get { return 0; }
		}

		protected override int Second
		{
			get { return -4; }
		}

		protected override int Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor0_4_5 : StringPartDescriptor
	{
		public StringPartDescriptor0_4_5()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index < 2 ||
						index >= (curVal.Length - 6);
		}

		public override bool IsValid(int len)
		{
			return len > 9;
		}

		protected override int  First
		{
			get { return 0; }
		}

		protected override int  Second
		{
			get { return 4; }
		}

		protected override int  Third
		{
			get { return 5; }
		}
	}

	public class StringPartDescriptor0_5 : StringPartDescriptor
	{
		public StringPartDescriptor0_5()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index < 2 ||
						index >= (curVal.Length - 2);
		}

		public override bool IsValid(int len)
		{
			return len > 2 && len <=8;
		}

		protected override int  First
		{
			get { return 0; }
		}

		protected override int  Second
		{
			get { return 5; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor1 : StringPartDescriptor
	{
		public StringPartDescriptor1()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index >= 2 &&
						index < 6;
		}

		public override bool IsValid(int len)
		{
			return len > 2 && len <= 8;
		}

		protected override int  First
		{
			get { return 1; }
		}

		protected override int  Second
		{
			get { return -1; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor1_2 : StringPartDescriptor
	{
		public StringPartDescriptor1_2()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index >= 2;
		}

		public override bool IsValid(int len)
		{
			return len > 6;
		}

		protected override int  First
		{
			get { return 1; }
		}

		protected override int  Second
		{
			get { return 2; }
		}

		protected override int  Third
		{
			get { return -1; }
		}	
	}

	public class StringPartDescriptor1_4 : StringPartDescriptor
	{
		public StringPartDescriptor1_4()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return (index >= 2 &&
						index < 6) ||
						(index >= (curVal.Length - 6) && index < (curVal.Length - 2));

		}

		public override bool IsValid(int len)
		{
			return len > 8;
		}

		protected override int  First
		{
			get { return 1; }
		}

		protected override int  Second
		{
			get { return 4; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor1_5 : StringPartDescriptor
	{
		public StringPartDescriptor1_5()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return (index >= 2 &&
						index < 6) ||
						(index >= (curVal.Length - 2));

		}

		public override bool IsValid(int len)
		{
			return len > 2;
		}

		protected override int  First
		{
			get { return 1; }
		}

		protected override int  Second
		{
			get { return 5; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor2 : StringPartDescriptor
	{
		public StringPartDescriptor2()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index >= 6;

		}

		public override bool IsValid(int len)
		{
			return len > 10;
		}

		protected override int  First
		{
			get { return 2; }
		}

		protected override int  Second
		{
			get { return -1; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}
	
	public class StringPartDescriptor2_3 : StringPartDescriptor
	{
		public StringPartDescriptor2_3()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index >= 6 ||
					index < curVal.Length -6;

		}

		public override bool IsValid(int len)
		{
			return len > 10;
		}

		protected override int  First
		{
			get { return 2; }
		}

		protected override int  Second
		{
			get { return 3; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor3 : StringPartDescriptor
	{
		public StringPartDescriptor3()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index < curVal.Length -6;

		}
		public override bool IsValid(int len)
		{
			return len > 10;
		}

		protected override int  First
		{
			get { return 3; }
		}

		protected override int  Second
		{
			get { return -1; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	
	public class StringPartDescriptor3_4 : StringPartDescriptor
	{
		public StringPartDescriptor3_4()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index < curVal.Length - 2;

		}

		public override bool IsValid(int len)
		{
			return len > 6;
		}

		protected override int  First
		{
			get { return 3; }
		}

		protected override int  Second
		{
			get { return 4; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor3_5 : StringPartDescriptor
	{
		public StringPartDescriptor3_5()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return index < curVal.Length - 6 ||
						(index >= (curVal.Length - 2));

		}

		public override bool IsValid(int len)
		{
			return len > 6;
		}

		protected override int  First
		{
			get { return 3; }
		}

		protected override int  Second
		{
			get { return 5; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}


	public class StringPartDescriptor4_5 : StringPartDescriptor
	{
		public StringPartDescriptor4_5()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return (index >= (curVal.Length - 6));
		}

		public override bool IsValid(int len)
		{
			return len > 2;
		}

		protected override int  First
		{
			get { return 4; }
		}

		protected override int  Second
		{
			get { return 5; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}

	public class StringPartDescriptor5 : StringPartDescriptor
	{
		public StringPartDescriptor5()
		{
		}

		public override bool IsInSet(string curVal, int index)
		{
			return (index >= (curVal.Length - 2));
		}

		public override bool IsValid(int len)
		{
			return len < 4;
		}

		protected override int  First
		{
			get { return 5; }
		}

		protected override int  Second
		{
			get { return -1; }
		}

		protected override int  Third
		{
			get { return -1; }
		}
	}


}

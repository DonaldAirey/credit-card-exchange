namespace FluidTrade.ExpressionEvaluation
{

	using System.IO;

	//* 
	// * A regular expression combination element. This element matches 
	// * two consecutive elements. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class CombineElement : Element
	{

		//* 
		// * The first element. 
		// 

		private Element elem1;

		//* 
		// * The second element. 
		// 

		private Element elem2;

		//* 
		// * Creates a new combine element. 
		// * 
		// * @param first the first element 
		// * @param second the second element 
		// 

		public CombineElement(Element first, Element second)
		{
			elem1 = first;
			elem2 = second;
		}

		//* 
		// * Creates a copy of this element. The copy will be an 
		// * instance of the same class matching the same strings. 
		// * Copies of elements are necessary to allow elements to cache 
		// * intermediate results while matching strings without 
		// * interfering with other threads. 
		// * 
		// * @return a copy of this element 
		// 

		public override object Clone()
		{
			return new CombineElement(elem1, elem2);
		}

		//* 
		// * Returns the length of a matching string starting at the 
		// * specified position. The number of matches to skip can also be 
		// * specified, but numbers higher than zero (0) cause a failed 
		// * match for any element that doesn't attempt to combine other 
		// * elements. 
		// * 
		// * @param m the matcher being used 
		// * @param input the input character stream to match 
		// * @param start the starting position 
		// * @param skip the number of matches to skip 
		// * 
		// * @return the length of the longest matching string, or 
		// * -1 if no match was found 
		// * 
		// * @throws IOException if an I/O error occurred 
		// 

		public override int Match(Matcher m, LookAheadReader input, int start, int skip)
		{

			int length1 = -1;
			int length2 = 0;
			int skip1 = 0;
			int skip2 = 0;

			while (skip >= 0)
			{
				length1 = elem1.Match(m, input, start, skip1);
				if (length1 < 0)
				{
					return -1;
				}
				length2 = elem2.Match(m, input, start + length1, skip2);
				if (length2 < 0)
				{
					skip1 += 1;
					skip2 = 0;
				}
				else
				{
					skip2 += 1;
					skip -= 1;
				}
			}

			return length1 + length2;
		}

		//* 
		// * Prints this element to the specified output stream. 
		// * 
		// * @param output the output stream to use 
		// * @param indent the current indentation 
		// 

		public override void PrintTo(TextWriter output, string indent)
		{
			elem1.PrintTo(output, indent);
			elem2.PrintTo(output, indent);
		}
	}
}
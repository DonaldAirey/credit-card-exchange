﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.IO;

	//* 
	// * A regular expression alternative element. This element matches 
	// * the longest alternative element. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class AlternativeElement : Element
	{

		//* 
		// * The first alternative element. 
		// 

		private Element elem1;

		//* 
		// * The second alternative element. 
		// 

		private Element elem2;

		//* 
		// * Creates a new alternative element. 
		// * 
		// * @param first the first alternative 
		// * @param second the second alternative 
		// 

		public AlternativeElement(Element first, Element second)
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
			return new AlternativeElement(elem1, elem2);
		}

		//* 
		// * Returns the length of a matching string starting at the 
		// * specified position. The number of matches to skip can also 
		// * be specified, but numbers higher than zero (0) cause a 
		// * failed match for any element that doesn't attempt to 
		// * combine other elements. 
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

			int length = 0;
			int length1 = -1;
			int length2 = -1;
			int skip1 = 0;
			int skip2 = 0;

			while (length >= 0 && skip1 + skip2 <= skip)
			{
				length1 = elem1.Match(m, input, start, skip1);
				length2 = elem2.Match(m, input, start, skip2);
				if (length1 >= length2)
				{
					length = length1;
					skip1 += 1;
				}
				else
				{
					length = length2;
					skip2 += 1;
				}
			}
			return length;
		}

		//* 
		// * Prints this element to the specified output stream. 
		// * 
		// * @param output the output stream to use 
		// * @param indent the current indentation 
		// 

		public override void PrintTo(TextWriter output, string indent)
		{
			output.WriteLine(indent + "Alternative 1");
			elem1.PrintTo(output, indent + " ");
			output.WriteLine(indent + "Alternative 2");
			elem2.PrintTo(output, indent + " ");
		}
	}
}
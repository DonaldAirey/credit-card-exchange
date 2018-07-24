﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.IO;

	//* 
	// * A regular expression element repeater. The element repeats the 
	// * matches from a specified element, attempting to reach the 
	// * maximum repetition count. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class RepeatElement : Element
	{

		//* 
		// * The repeat type constants. 
		// 

		public enum RepeatType
		{

			//* 
			// * The greedy repeat type constant. 
			// 

			GREEDY = 1,

			//* 
			// * The reluctant repeat type constant. 
			// 

			RELUCTANT = 2,

			//* 
			// * The possesive repeat type constant. 
			// 

			POSSESSIVE = 3
		}

		//* 
		// * The element to repeat. 
		// 

		private Element elem;

		//* 
		// * The minimum number of repetitions. 
		// 

		private int min;

		//* 
		// * The maximum number of repetitions. 
		// 

		private int max;

		//* 
		// * The repeat type. 
		// 

		private RepeatType type;

		//* 
		// * The start position of the last set of matches. 
		// 

		private int matchStart;

		//* 
		// * A set with all matches starting at matchStart. A match with 
		// * a specific length is reported by a non-zero bit in the bit 
		// * array. 
		// 

		private BitArray matches;

		//* 
		// * Creats a new element repeater. 
		// * 
		// * @param elem the element to repeat 
		// * @param min the minimum count 
		// * @param max the maximum count 
		// * @param type the repeat type constant 
		// 

		public RepeatElement(Element elem, int min, int max, RepeatType type)
		{

			this.elem = elem;
			this.min = min;
			if (max <= 0)
			{
				this.max = Int32.MaxValue;
			}
			else
			{
				this.max = max;
			}
			this.type = type;
			this.matchStart = -1;
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
			return new RepeatElement((Element)elem.Clone(), min, max, type);
		}

		//* 
		// * Returns the length of a matching string starting at the 
		// * specified position. The number of matches to skip can also be 
		// * specified. 
		// * 
		// * @param m the matcher being used 
		// * @param input the input character stream to match 
		// * @param start the starting position 
		// * @param skip the number of matches to skip 
		// * 
		// * @return the length of the matching string, or 
		// * -1 if no match was found 
		// * 
		// * @throws IOException if an I/O error occurred 
		// 

		public override int Match(Matcher m, LookAheadReader input, int start, int skip)
		{

			if (skip == 0)
			{
				matchStart = -1;
				matches = null;
			}
			switch (type)
			{
			case RepeatType.GREEDY:
				return MatchGreedy(m, input, start, skip);
			case RepeatType.RELUCTANT:
				return MatchReluctant(m, input, start, skip);
			case RepeatType.POSSESSIVE:
				if (skip == 0)
				{
					return MatchPossessive(m, input, start, 0);
				}

				break;
			}
			return -1;
		}

		//* 
		// * Returns the length of the longest possible matching string 
		// * starting at the specified position. The number of matches 
		// * to skip can also be specified. 
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

		private int MatchGreedy(Matcher m, LookAheadReader input, int start, int skip)
		{

			// Check for simple case 
			if (skip == 0)
			{
				return MatchPossessive(m, input, start, 0);
			}

			// Find all matches 
			if (matchStart != start)
			{
				matchStart = start;
				matches = new BitArray(10);
				FindMatches(m, input, start, 0, 0, 0);
			}
			for (int i = matches.Count - 1; i >= 0; i += -1)
			{

				// Find first non-skipped match 
				if (matches[i])
				{
					if (skip == 0)
					{
						return i;
					}
					skip -= 1;
				}
			}
			return -1;
		}

		//* 
		// * Returns the length of the shortest possible matching string 
		// * starting at the specified position. The number of matches to 
		// * skip can also be specified. 
		// * 
		// * @param m the matcher being used 
		// * @param input the input character stream to match 
		// * @param start the starting position 
		// * @param skip the number of matches to skip 
		// * 
		// * @return the length of the shortest matching string, or 
		// * -1 if no match was found 
		// * 
		// * @throws IOException if an I/O error occurred 
		// 

		private int MatchReluctant(Matcher m, LookAheadReader input, int start, int skip)
		{

			// Find all matches 
			if (matchStart != start)
			{
				matchStart = start;
				matches = new BitArray(10);
				FindMatches(m, input, start, 0, 0, 0);
			}
			for (int i = 0; i <= matches.Count - 1; i++)
			{

				// Find first non-skipped match 
				if (matches[i])
				{
					if (skip == 0)
					{
						return i;
					}
					skip -= 1;
				}
			}
			return -1;
		}

		//* 
		// * Returns the length of the maximum number of elements matching 
		// * the string starting at the specified position. This method 
		// * allows no backtracking, i.e. no skips.. 
		// * 
		// * @param m the matcher being used 
		// * @param input the input character stream to match 
		// * @param start the starting position 
		// * @param count the start count, normally zero (0) 
		// * 
		// * @return the length of the longest matching string, or 
		// * -1 if no match was found 
		// * 
		// * @throws IOException if an I/O error occurred 
		// 

		private int MatchPossessive(Matcher m, LookAheadReader input, int start, int count)
		{

			int length = 0;
			int subLength = 1;

			// Match as many elements as possible 
			while (subLength > 0 && count < max)
			{
				subLength = elem.Match(m, input, start + length, 0);
				if (subLength >= 0)
				{
					count += 1;
					length += subLength;
				}
			}

			// Return result 
			if (min <= count && count <= max)
			{
				return length;
			}
			else
			{
				return -1;
			}
		}

		//* 
		// * Finds all matches and adds the lengths to the matches set. 
		// * 
		// * @param m the matcher being used 
		// * @param input the input character stream to match 
		// * @param start the starting position 
		// * @param length the match length at the start position 
		// * @param count the number of sub-elements matched 
		// * @param attempt the number of match attempts here 
		// * 
		// * @throws IOException if an I/O error occurred 
		// 

		private void FindMatches(Matcher m, LookAheadReader input, int start, int length, int count, int attempt)
		{

			int subLength = 0;

			// Check match ending here 
			if (count > max)
			{
				return;
			}
			if (min <= count && attempt == 0)
			{
				if (matches.Length <= length)
				{
					matches.Length = length + 10;
				}
				matches[length] = true;
			}

			// Check element match 
			subLength = elem.Match(m, input, start, attempt);
			if (subLength < 0)
			{
				return;
			}
			else if (subLength == 0)
			{
				if (min == count + 1)
				{
					if (matches.Length <= length)
					{
						matches.Length = length + 10;
					}
					matches[length] = true;
				}
				return;
			}

			// Find alternative and subsequent matches 
			FindMatches(m, input, start, length, count, attempt + 1);
			FindMatches(m, input, start + subLength, length + subLength, count + 1, 0);
		}

		//* 
		// * Prints this element to the specified output stream. 
		// * 
		// * @param output the output stream to use 
		// * @param indent the current indentation 
		// 

		public override void PrintTo(TextWriter output, string indent)
		{
			output.Write(indent + "Repeat (" + min + "," + max + ")");
			if (type == RepeatType.RELUCTANT)
			{
				output.Write("?");
			}
			else if (type == RepeatType.POSSESSIVE)
			{
				output.Write("+");
			}
			output.WriteLine();
			elem.PrintTo(output, indent + " ");
		}
	}
}
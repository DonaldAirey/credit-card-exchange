﻿namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections;
	using System.IO;
	using System.Globalization;
	using System.Text;

	//* 
	// * A regular expression. This class creates and holds an internal 
	// * data structure representing a regular expression. It also 
	// * allows creating matchers. This class is thread-safe. Multiple 
	// * matchers may operate simultanously on the same regular 
	// * expression. 
	// * 
	// * @author Per Cederberg, <per at percederberg dot net> 
	// * @version 1.5 
	// 

	internal class RegExp
	{

		//* 
		// * The base regular expression element. 
		// 

		private Element element;

		//* 
		// * The regular expression pattern. 
		// 

		private string pattern;

		//* 
		// * The character case ignore flag. 
		// 

		private bool ignoreCase;

		//* 
		// * The current position in the pattern. This variable is used by 
		// * the parsing methods. 
		// 

		private int pos;

		//* 
		// * Creates a new case-sensitive regular expression. 
		// * 
		// * @param pattern the regular expression pattern 
		// * 
		// * @throws RegExpException if the regular expression couldn't be 
		// * parsed correctly 
		// 

		public RegExp(string pattern)
			: this(pattern, false)
		{
		}

		//* 
		// * Creates a new regular expression. The regular expression 
		// * can be either case-sensitive or case-insensitive. 
		// * 
		// * @param pattern the regular expression pattern 
		// * @param ignoreCase the character case ignore flag 
		// * 
		// * @throws RegExpException if the regular expression couldn't be 
		// * parsed correctly 
		// * 
		// * @since 1.5 
		// 

		public RegExp(string pattern, bool ignoreCase)
		{
			this.pattern = pattern;
			this.ignoreCase = ignoreCase;
			this.element = ParseExpr();
			if (pos < pattern.Length)
			{
				throw new RegExpException(RegExpException.ErrorType.UNEXPECTED_CHARACTER, pos, pattern);
			}
		}

		//* 
		// * Creates a new matcher for the specified string. 
		// * 
		// * @param str the string to work with 
		// * 
		// * @return the regular expresion matcher 
		// 

		public Matcher Matcher(string str)
		{
			return Matcher(new StringReader(str));
		}

		//* 
		// * Creates a new matcher for the specified character input 
		// * stream. 
		// * 
		// * @param input the character input stream 
		// * 
		// * @return the regular expresion matcher 
		// * 
		// * @since 1.5 
		// 

		public Matcher Matcher(TextReader input)
		{
			if (input is LookAheadReader)
			{
				return Matcher((LookAheadReader)input);
			}
			else
			{
				return Matcher(new LookAheadReader(input));
			}
		}

		//* 
		// * Creates a new matcher for the specified look-ahead 
		// * character input stream. 
		// * 
		// * @param input the character input stream 
		// * 
		// * @return the regular expresion matcher 
		// * 
		// * @since 1.5 
		// 

		private Matcher Matcher(LookAheadReader input)
		{
			return new Matcher((Element)element.Clone(), input, ignoreCase);
		}

		//* 
		// * Returns a string representation of the regular expression. 
		// * 
		// * @return a string representation of the regular expression 
		// 

		public override string ToString()
		{
			StringWriter str = default(StringWriter);

			str = new StringWriter();
			str.WriteLine("Regular Expression");
			str.WriteLine(" Pattern: " + pattern);
			str.Write(" Flags:");
			if (ignoreCase)
			{
				str.Write(" caseignore");
			}
			str.WriteLine();
			str.WriteLine(" Compiled:");
			element.PrintTo(str, " ");
			return str.ToString();
		}

		//* 
		// * Parses a regular expression. This method handles the Expr 
		// * production in the grammar (see regexp.grammar). 
		// * 
		// * @return the element representing this expression 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseExpr()
		{
			Element first = default(Element);
			Element second = default(Element);

			first = ParseTerm();
			if (PeekChar(0) != Convert.ToInt32('|'))
			{
				return first;
			}
			else
			{
				ReadChar('|');
				second = ParseExpr();
				return new AlternativeElement(first, second);
			}
		}

		//* 
		// * Parses a regular expression term. This method handles the 
		// * Term production in the grammar (see regexp.grammar). 
		// * 
		// * @return the element representing this term 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseTerm()
		{
			ArrayList list = new ArrayList();

			list.Add(ParseFact());
			while (true)
			{
				int i = PeekChar(0);

				if (i == -1)
				{
					return CombineElements(list);
				}

				char c = Convert.ToChar(i);

				switch (c)
				{
				case ')':
				case ']':
				case '{':
				case '}':
				case '?':
				case '+':
				case '|':
					return CombineElements(list);
				default:
					list.Add(ParseFact());
					break;
				}
			}
		}

		//* 
		// * Parses a regular expression factor. This method handles the 
		// * Fact production in the grammar (see regexp.grammar). 
		// * 
		// * @return the element representing this factor 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseFact()
		{
			Element elem = default(Element);

			elem = ParseAtom();

			int i = PeekChar(0);

			if (i == -1)
			{
				return elem;
			}

			char c = Convert.ToChar(i);

			switch (c)
			{
			case '?':
			case '*':
			case '+':
			case '{':
				return ParseAtomModifier(elem);
			default:
				return elem;
			}
		}

		//* 
		// * Parses a regular expression atom. This method handles the 
		// * Atom production in the grammar (see regexp.grammar). 
		// * 
		// * @return the element representing this atom 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseAtom()
		{
			Element elem = default(Element);

			int i = PeekChar(0);

			if (i == -1)
			{
				throw new RegExpException(RegExpException.ErrorType.UNEXPECTED_CHARACTER, pos, pattern);
			}

			char c = Convert.ToChar(i);

			switch (c)
			{
			case '.':
				ReadChar('.');
				return CharacterSetElement.DOT;
			case '(':
				ReadChar('(');
				elem = ParseExpr();
				ReadChar(')');
				return elem;
			case '[':
				ReadChar('[');
				elem = ParseCharSet();
				ReadChar(']');
				return elem;
			case ')':
			case ']':
			case '{':
			case '}':
			case '?':
			case '*':
			case '+':
			case '|':
				throw new RegExpException(RegExpException.ErrorType.UNEXPECTED_CHARACTER, pos, pattern);
			default:
				return ParseChar();
			}
		}

		//* 
		// * Parses a regular expression atom modifier. This method handles 
		// * the AtomModifier production in the grammar (see regexp.grammar). 
		// * 
		// * @param elem the element to modify 
		// * 
		// * @return the modified element 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseAtomModifier(Element elem)
		{
			int min = 0;
			int max = -1;
			RepeatElement.RepeatType type = default(RepeatElement.RepeatType);
			int firstPos = 0;

			// Read min and max 
			type = RepeatElement.RepeatType.GREEDY;
			switch (ReadChar())
			{
			case '?':
				min = 0;
				max = 1;
				break;
			case '*':
				min = 0;
				max = -1;
				break;
			case '+':
				min = 1;
				max = -1;
				break;
			case '{':
				firstPos = pos - 1;
				min = ReadNumber();
				max = min;
				if (PeekChar(0) == Convert.ToInt32(','))
				{
					ReadChar(',');
					max = -1;
					if (PeekChar(0) != Convert.ToInt32('}'))
					{
						max = ReadNumber();
					}
				}

				ReadChar('}');
				if (max == 0 || (max > 0 && min > max))
				{
					throw new RegExpException(RegExpException.ErrorType.INVALID_REPEAT_COUNT, firstPos, pattern);
				}

				break;
			default:
				throw new RegExpException(RegExpException.ErrorType.UNEXPECTED_CHARACTER, pos - 1, pattern);
			}

			// Read operator mode 
			if (PeekChar(0) == Convert.ToInt32('?'))
			{
				ReadChar('?');
				type = RepeatElement.RepeatType.RELUCTANT;
			}
			else if (PeekChar(0) == Convert.ToInt32('+'))
			{
				ReadChar('+');
				type = RepeatElement.RepeatType.POSSESSIVE;
			}

			return new RepeatElement(elem, min, max, type);
		}

		//* 
		// * Parses a regular expression character set. This method handles 
		// * the contents of the '[...]' construct in a regular expression. 
		// * 
		// * @return the element representing this character set 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseCharSet()
		{
			CharacterSetElement charset = default(CharacterSetElement);
			Element elem = default(Element);
			bool repeat = true;
			char start = '\0';
			char end = '\0';

			if (PeekChar(0) == Convert.ToInt32('^'))
			{
				ReadChar('^');
				charset = new CharacterSetElement(true);
			}
			else
			{
				charset = new CharacterSetElement(false);
			}

			while (PeekChar(0) > 0 && repeat)
			{
				start = Convert.ToChar(PeekChar(0));
				switch (start)
				{
				case ']':
					repeat = false;
					break;
				case '\\':
					elem = ParseEscapeChar();
					if (elem is StringElement)
					{
						charset.AddCharacters((StringElement)elem);
					}
					else
					{
						charset.AddCharacterSet((CharacterSetElement)elem);
					}

					break;
				default:
					ReadChar(start);
					if (PeekChar(0) == Convert.ToInt32('-') && PeekChar(1) > 0 && PeekChar(1) != Convert.ToInt32(']'))
					{

						ReadChar('-');
						end = ReadChar();
						charset.AddRange(FixChar(start), FixChar(end));
					}
					else
					{
						charset.AddCharacter(FixChar(start));
					}

					break;
				}
			}

			return charset;
		}

		//* 
		// * Parses a regular expression character. This method handles 
		// * a single normal character in a regular expression. 
		// * 
		// * @return the element representing this character 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseChar()
		{
			char c = Convert.ToChar(PeekChar(0));

			switch (c)
			{
			case '\\':
				return ParseEscapeChar();
			case '^':
			case '$':
				throw new RegExpException(RegExpException.ErrorType.UNSUPPORTED_SPECIAL_CHARACTER, pos, pattern);
			default:
				return new StringElement(FixChar(ReadChar()));
			}
		}

		//* 
		// * Parses a regular expression character escape. This method 
		// * handles a single character escape in a regular expression. 
		// * 
		// * @return the element representing this character escape 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private Element ParseEscapeChar()
		{
			char c = '\0';
			string str = null;
			int value = 0;

			ReadChar('\\');
			c = ReadChar();
			switch (c)
			{
			case '0':
				c = ReadChar();
				if (c < '0' || c > '3')
				{
					throw new RegExpException(RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER, pos - 3, pattern);
				}

				value = Convert.ToInt32(c) - Convert.ToInt32('0');
				c = Convert.ToChar(PeekChar(0));
				if ('0' <= c && c <= '7')
				{
					value *= 8;
					value += Convert.ToInt32(ReadChar()) - Convert.ToInt32('0');
					c = Convert.ToChar(PeekChar(0));
					if ('0' <= c && c <= '7')
					{
						value *= 8;
						value += Convert.ToInt32(ReadChar()) - Convert.ToInt32('0');
					}
				}

				return new StringElement(FixChar(Convert.ToChar(value)));
			case 'x':
				str = ReadChar().ToString() + ReadChar().ToString();
				try
				{
					value = Int32.Parse(str, NumberStyles.AllowHexSpecifier);
					return new StringElement(FixChar(Convert.ToChar(value)));
				}
				catch (FormatException)
				{
					throw new RegExpException(RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER, pos - str.Length - 2, pattern);
				}
			case 'u':
				str = ReadChar().ToString() + ReadChar().ToString() + ReadChar().ToString() + ReadChar().ToString();
				try
				{
					value = Int32.Parse(str, NumberStyles.AllowHexSpecifier);
					return new StringElement(FixChar(Convert.ToChar(value)));
				}
				catch (FormatException)
				{
					throw new RegExpException(RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER, pos - str.Length - 2, pattern);
				}
			case 't':
				return new StringElement(Convert.ToChar(9));
			case 'n':
				return new StringElement(Convert.ToChar(10));
			case 'r':
				return new StringElement(Convert.ToChar(13));
			case 'f':
				return new StringElement(Convert.ToChar(12));
			case 'a':
				return new StringElement(Convert.ToChar(7));
			case 'e':
				return new StringElement(Convert.ToChar(27));
			case 'd':
				return CharacterSetElement.DIGIT;
			case 'D':
				return CharacterSetElement.NON_DIGIT;
			case 's':
				return CharacterSetElement.WHITESPACE;
			case 'S':
				return CharacterSetElement.NON_WHITESPACE;
			case 'w':
				return CharacterSetElement.WORD;
			case 'W':
				return CharacterSetElement.NON_WORD;
			default:
				if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
				{
					throw new RegExpException(RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER, pos - 2, pattern);
				}
				return new StringElement(FixChar(c));
			}
		}

		//* 
		// * Adjusts a character for inclusion in a string or character 
		// * set element. For case-insensitive regular expressions, this 
		// * transforms the character to lower-case. 
		// * 
		// * @param c the input character 
		// * 
		// * @return the adjusted character 
		// 

		private char FixChar(char c)
		{
			return (ignoreCase ? Char.ToLower(c) : c);
		}

		//* 
		// * Reads a number from the pattern. If the next character isn't a 
		// * numeric character, an exception is thrown. This method reads 
		// * several consecutive numeric characters. 
		// * 
		// * @return the numeric value read 
		// * 
		// * @throws RegExpException if an error was encountered in the 
		// * pattern string 
		// 

		private int ReadNumber()
		{
			StringBuilder buf = new StringBuilder();
			int c = 0;

			c = PeekChar(0);
			while (Convert.ToInt32('0') <= c && c <= Convert.ToInt32('9'))
			{
				buf.Append(ReadChar());
				c = PeekChar(0);
			}
			if (buf.Length <= 0)
			{
				throw new RegExpException(RegExpException.ErrorType.UNEXPECTED_CHARACTER, pos, pattern);
			}
			return Int32.Parse(buf.ToString());
		}

		//* 
		// * Reads the next character in the pattern. If no next character 
		// * exists, an exception is thrown. 
		// * 
		// * @return the character read 
		// * 
		// * @throws RegExpException if no next character was available in 
		// * the pattern string 
		// 

		private char ReadChar()
		{
			int c = PeekChar(0);

			if (c < 0)
			{
				throw new RegExpException(RegExpException.ErrorType.UNTERMINATED_PATTERN, pos, pattern);
			}
			else
			{
				pos += 1;
				return Convert.ToChar(c);
			}
		}

		//* 
		// * Reads the next character in the pattern. If the character 
		// * wasn't the specified one, an exception is thrown. 
		// * 
		// * @param c the character to read 
		// * 
		// * @return the character read 
		// * 
		// * @throws RegExpException if the character read didn't match the 
		// * specified one, or if no next character was 
		// * available in the pattern string 
		// 

		private char ReadChar(char c)
		{
			if (c != ReadChar())
			{
				throw new RegExpException(RegExpException.ErrorType.UNEXPECTED_CHARACTER, pos - 1, pattern);
			}
			return c;
		}

		//* 
		// * Returns a character that has not yet been read from the 
		// * pattern. If the requested position is beyond the end of the 
		// * pattern string, -1 is returned. 
		// * 
		// * @param count the preview position, from zero (0) 
		// * 
		// * @return the character found, or 
		// * -1 if beyond the end of the pattern string 
		// 

		private int PeekChar(int count)
		{
			if (pos + count < pattern.Length)
			{
				return Convert.ToInt32(pattern[pos + count]);
			}
			else
			{
				return -1;
			}
		}

		//* 
		// * Combines a list of elements. This method takes care to always 
		// * concatenate adjacent string elements into a single string 
		// * element. 
		// * 
		// * @param list the list with elements 
		// * 
		// * @return the combined element 
		// 

		private Element CombineElements(ArrayList list)
		{
			Element prev = default(Element);
			Element elem = default(Element);
			string str = null;
			int i = 0;

			// Concatenate string elements 
			prev = (Element)list[0];
			for (i = 1; i <= list.Count - 2; i++)
			{
				elem = (Element)list[i];
				if (prev is StringElement && elem is StringElement)
				{

					str = ((StringElement)prev).GetString() + ((StringElement)elem).GetString();
					elem = new StringElement(str);
					list.RemoveAt(i);
					list[i - 1] = elem;
					i -= 1;
				}
				prev = elem;
			}

			// Combine all remaining elements 
			elem = (Element)list[list.Count - 1];
			for (i = list.Count - 2; i >= 0; i += -1)
			{
				prev = (Element)list[i];
				elem = new CombineElement(prev, elem);
			}

			return elem;
		}
	}
}
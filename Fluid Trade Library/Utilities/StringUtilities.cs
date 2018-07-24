using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FluidTrade.Core
{
	public static class StringUtilities
	{
		/// <summary>
		/// returns a string that contains only the numbers from the input string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string CleanUpNumberString(string input)
		{
			if(string.IsNullOrEmpty(input))
				return input;

			return Regex.Replace(input, @"[^0-9]", "");
		}

		/// <summary>
		/// returns a string that contains only the alpha-numeric characters from the input string
		/// Alpha chars are A-Z, a-z so no multibyte chars are included
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string CleanUpAlphaNumericString(string input)
		{
			if(string.IsNullOrEmpty(input))
				return input;

			return Regex.Replace(input, @"[^a-zA-Z0-9]", "");
		}
	}
}

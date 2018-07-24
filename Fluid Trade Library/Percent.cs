using System;
using System.Text.RegularExpressions;

namespace FluidTrade.Core
{

	/// <summary>
	/// Class for parsing various inputs.
	/// </summary>
	public class Percent
	{

		/// <summary>
		/// Parses a decimal percent from a string that may or may not contain the percent sign.
		/// </summary>
		/// <param name="stringPercent">Contains the text that is parsed into a decimal value.</param>
		/// <returns>The text string converted into a decimal value.</returns>
		public static decimal Parse(string stringPercent)
		{

			Match match;

			// Attempt to match the input string against a decimal number with a percent sign '%' after it.  If a match 
			// is found, convert the numeric portion of the string to a decimal number and return it after applying the
			// percent factor.  This matching should be exactly the same as Microsoft Excel's input handler for percent
			// formatted cells.
			match = Regex.Match(stringPercent, @"(?<percent>\d*\.?\d*)%");
			if (match.Success)
				return Convert.ToDecimal(match.Groups["percent"].Value) / 100.0M;

			// A decimal number with a leading zero is used without the implied divisor.
			match = Regex.Match(stringPercent, @"(?<percent>\0+\.\d*)");
			if (match.Success)
				return Convert.ToDecimal(match.Groups["percent"].Value);

			// If we reached here, we know that the number doesn't have a leading zero.  We'll interpret numbers without a
			// percent sign the same as with, as long as they don't have a leading zero.  Again, this should be the same
			// method of handling as Microsoft Excel.
			match = Regex.Match(stringPercent, @"(?<percent>\d*\.?\d*)");
			if (match.Success)
				return Convert.ToDecimal(match.Groups["percent"].Value) / 100.0M;

			// If the input string didn't match any of the above pattersn, return zero as a default value.
			return 0.0M;

		}

	}

}

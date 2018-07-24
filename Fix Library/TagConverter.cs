namespace FluidTrade.Core
{
    using System;
    using System.Globalization;

	/// <summary>
	/// Type Converter for FIX Tags.
	/// </summary>
	public class TagConverter
	{

		/// <summary>
		/// Converts a string representation of a FIX tag to a Tag. Only unsigned, positive integers are allowed as Tags.
		/// </summary>
		/// <param name="value">The FIX string representation of a Tag.</param>
		/// <returns>A Tag value.</returns>
		public static Tag ConvertFrom(string value) {return (Tag)Int32.Parse(value, NumberStyles.None);}

		/// <summary>
		/// Converts a Tag to a string representation of a FIX tag.
		/// </summary>
		/// <param name="value">A Tag value.</param>
		/// <returns>The FIX string representation of a Tag.</returns>
		public static string ConvertTo(Tag tag) {return Convert.ToInt32(tag).ToString();}

	}

}


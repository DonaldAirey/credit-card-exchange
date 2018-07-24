namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX OptionType Field
	/// </summary>
	public class OptionTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{OptionType.Put, "0"},
				{OptionType.Call, "1"},
		};

		/// <summary>
		/// Initializes the shared members of a OptionTypeConverter.
		/// </summary>
		static OptionTypeConverter()
		{

			// Initialize the mapping of strings to Type.
			OptionTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OptionTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			OptionTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OptionTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a OptionType.
		/// </summary>
		/// <param name="value">The FIX string representation of a OptionType.</param>
		/// <returns>A OptionType value.</returns>
		public static OptionType ConvertFrom(string value) {return (OptionType)OptionTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a OptionType to a string.
		/// </summary>
		/// <param name="value">A OptionType value.</param>
		/// <returns>The FIX string representation of a OptionType.</returns>
		public static string ConvertTo(OptionType enumType) {return (string)OptionTypeConverter.toTable[enumType];}

	}

}


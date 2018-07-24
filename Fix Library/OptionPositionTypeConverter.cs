namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX OptionPositionType Field
	/// </summary>
	public class OptionPositionTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{OptionPositionType.Open, "O"},
				{OptionPositionType.Close, "C"},
			};

			/// <summary>
			/// Initializes the shared members of a OptionPositionTypeConverter.
			/// </summary>
			static OptionPositionTypeConverter()
			{

				// Initialize the mapping of strings to Type.
				OptionPositionTypeConverter.fromTable = new Hashtable();
				for (int element = 0; element < pairs.GetLength(0); element++)
					OptionPositionTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

				// Initialize the mapping of Type to strings.
				OptionPositionTypeConverter.toTable = new Hashtable();
				for (int element = 0; element < pairs.GetLength(0); element++)
					OptionPositionTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

			}

		/// <summary>
		/// Converts a string to a OptionPositionType.
		/// </summary>
		/// <param name="value">The FIX string representation of a OptionPositionType.</param>
		/// <returns>A OptionPositionType value.</returns>
		public static OptionPositionType ConvertFrom(string value) {return (OptionPositionType)OptionPositionTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a OptionPositionType to a string.
		/// </summary>
		/// <param name="value">A OptionPositionType value.</param>
		/// <returns>The FIX string representation of a OptionPositionType.</returns>
		public static string ConvertTo(OptionPositionType enumType) {return (string)OptionPositionTypeConverter.toTable[enumType];}

	}

}


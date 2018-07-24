namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX SecurityType Field
	/// </summary>
	public class SecurityTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{SecurityType.Future, "FUT"},
				{SecurityType.Option, "OPT"},
		};

		/// <summary>
		/// Initializes the shared members of a SecurityTypeConverter.
		/// </summary>
		static SecurityTypeConverter()
		{

			// Initialize the mapping of strings to Type.
			SecurityTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				SecurityTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			SecurityTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				SecurityTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a SecurityType.
		/// </summary>
		/// <param name="value">The FIX string representation of a SecurityType.</param>
		/// <returns>A SecurityType value.</returns>
		public static SecurityType ConvertFrom(string value) {return (SecurityType)SecurityTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a SecurityType to a string.
		/// </summary>
		/// <param name="value">A SecurityType value.</param>
		/// <returns>The FIX string representation of a SecurityType.</returns>
		public static string ConvertTo(SecurityType enumType) {return (string)SecurityTypeConverter.toTable[enumType];}

	}

}


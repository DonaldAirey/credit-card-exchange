namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX CommType Field
	/// </summary>
	public class CommTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{CommType.PerShare, "1"},
				{CommType.Percentage, "2"},
				{CommType.Absolute, "3"},
		};

		/// <summary>
		/// Initializes the shared members of a CommTypeConverter.
		/// </summary>
		static CommTypeConverter()
		{

			// Initialize the mapping of strings to Type.
			CommTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				CommTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			CommTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				CommTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a CommType.
		/// </summary>
		/// <param name="value">The FIX string representation of a CommType.</param>
		/// <returns>A CommType value.</returns>
		public static CommType ConvertFrom(string value) {return (CommType)CommTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a CommType to a string.
		/// </summary>
		/// <param name="value">A CommType value.</param>
		/// <returns>The FIX string representation of a CommType.</returns>
		public static string ConvertTo(CommType enumType) {return (string)CommTypeConverter.toTable[enumType];}

	}

}


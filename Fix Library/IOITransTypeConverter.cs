namespace FluidTrade.Core
{

    using System.Collections;

    /// <summary>
	/// Type Converter for FIX IOITransType Field
	/// </summary>
	public class IOITransTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{IOITransType.New, "N"},
			{IOITransType.Cancel, "C"},
			{IOITransType.Replace, "R"}
		};

		/// <summary>
		/// Initializes the shared members of a IOITransTypeConverter.
		/// </summary>
		static IOITransTypeConverter()
		{

			// Initialize the mapping of strings to IOITransType.
			IOITransTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				IOITransTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of IOITransType to strings.
			IOITransTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				IOITransTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a IOITransType.
		/// </summary>
		/// <param name="value">The FIX string representation of a IOITransType.</param>
		/// <returns>A IOITransType value.</returns>
		public static IOITransType ConvertFrom(string value) {return (IOITransType)IOITransTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a IOITransType to a string.
		/// </summary>
		/// <returns>A IOITransType value.</returns>
		/// <param name="value">The FIX string representation of a IOITransType.</param>
		public static string ConvertTo(IOITransType value) {return (string)IOITransTypeConverter.toTable[value];}

	}

}

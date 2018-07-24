namespace FluidTrade.Core
{

    using System.Collections;

    /// <summary>
	/// Type Converter for FIX IOIQltyInd Field
	/// </summary>
	public class IOIQltyIndConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{IOIQltyInd.Low, "L"},
			{IOIQltyInd.Medium, "M"},
			{IOIQltyInd.High, "H"}
		};

		/// <summary>
		/// Initializes the shared members of a IOIQltyIndConverter.
		/// </summary>
		static IOIQltyIndConverter()
		{

			// Initialize the mapping of strings to IOIQltyInd.
			IOIQltyIndConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				IOIQltyIndConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of IOIQltyInd to strings.
			IOIQltyIndConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				IOIQltyIndConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a IOIQltyInd.
		/// </summary>
		/// <param name="value">The FIX string representation of a IOIQltyInd.</param>
		/// <returns>A IOIQltyInd value.</returns>
		public static IOIQltyInd ConvertFrom(string value) {return (IOIQltyInd)IOIQltyIndConverter.fromTable[value];}

		/// <summary>
		/// Converts a IOIQltyInd to a string.
		/// </summary>
		/// <returns>A IOIQltyInd value.</returns>
		/// <param name="value">The FIX string representation of a IOIQltyInd.</param>
		public static string ConvertTo(IOIQltyInd value) {return (string)IOIQltyIndConverter.toTable[value];}

	}

}

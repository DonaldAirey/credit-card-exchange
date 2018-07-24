namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX CxlRejResponseTo Field
	/// </summary>
	public class CxlRejResponseToConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{CxlRejResponseTo.CancelRequest, "1"},
				{CxlRejResponseTo.CancelReplaceRequest, "2"}
			};

			/// <summary>
			/// Initializes the shared members of a CxlRejResponseToConverter.
			/// </summary>
			static CxlRejResponseToConverter()
			{

				// Initialize the mapping of strings to Type.
				CxlRejResponseToConverter.fromTable = new Hashtable();
				for (int element = 0; element < pairs.GetLength(0); element++)
					CxlRejResponseToConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

				// Initialize the mapping of Type to strings.
				CxlRejResponseToConverter.toTable = new Hashtable();
				for (int element = 0; element < pairs.GetLength(0); element++)
					CxlRejResponseToConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

			}

		/// <summary>
		/// Converts a string to a CxlRejResponseTo.
		/// </summary>
		/// <param name="value">The FIX string representation of a CxlRejResponseTo.</param>
		/// <returns>A CxlRejResponseTo value.</returns>
		public static CxlRejResponseTo ConvertFrom(string value) {return (CxlRejResponseTo)CxlRejResponseToConverter.fromTable[value];}

		/// <summary>
		/// Converts a CxlRejResponseTo to a string.
		/// </summary>
		/// <param name="value">A CxlRejResponseTo value.</param>
		/// <returns>The FIX string representation of a CxlRejResponseTo.</returns>
		public static string ConvertTo(CxlRejResponseTo enumType) {return (string)CxlRejResponseToConverter.toTable[enumType];}

	}

}


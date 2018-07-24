namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX CxlType Field
	/// </summary>
	public class CxlTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
				{CxlType.Full, "F"},
				{CxlType.Partial, "P"},
		};

		/// <summary>
		/// Initializes the shared members of a CxlTypeConverter.
		/// </summary>
		static CxlTypeConverter()
		{

			// Initialize the mapping of strings to Type.
			CxlTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				CxlTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			CxlTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				CxlTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a CxlType.
		/// </summary>
		/// <param name="value">The FIX string representation of a CxlType.</param>
		/// <returns>A CxlType value.</returns>
		public static CxlType ConvertFrom(string value) {return (CxlType)CxlTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a CxlType to a string.
		/// </summary>
		/// <param name="value">A CxlType value.</param>
		/// <returns>The FIX string representation of a CxlType.</returns>
		public static string ConvertTo(CxlType enumType) {return (string)CxlTypeConverter.toTable[enumType];}

	}

}


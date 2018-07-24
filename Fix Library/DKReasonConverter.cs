namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX DKReason Field
	/// </summary>
	public class DKReasonConverter
	{

		/*
		 * A = Unknown symbol
		 * B = Wrong side
		 * C = Quantity exceeds order
		 * D = No matching order
		 * E = Price exceeds limit
		 * Z = Other 
		 */
		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{DKReason.UnknownSymbol, "A"},
				{DKReason.WrongSide, "B"},
				{DKReason.QuantityExceedsOrder, "C"},
				{DKReason.NoMatchingOrder, "D"},
				{DKReason.PriceExceedsLimit, "E"},
				{DKReason.Other, "Z"},
		};

		/// <summary>
		/// Initializes the shared members of a CxlRejReasonConverter.
		/// </summary>
		static DKReasonConverter()
		{

			// Initialize the mapping of strings to Type.
			DKReasonConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				DKReasonConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			DKReasonConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				DKReasonConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a DKReason.
		/// </summary>
		/// <param name="value">The FIX string representation of a DKReason.</param>
		/// <returns>A DKReason value.</returns>
		public static DKReason ConvertFrom(string value) {return (DKReason)DKReasonConverter.fromTable[value];}

		/// <summary>
		/// Converts a DKReason to a string.
		/// </summary>
		/// <param name="value">A DKReason value.</param>
		/// <returns>The FIX string representation of a CxlRejReason.</returns>
		public static string ConvertTo(DKReason enumType) {return (string)DKReasonConverter.toTable[enumType];}

	}

}


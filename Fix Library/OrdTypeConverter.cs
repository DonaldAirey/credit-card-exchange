namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX OrdType Field
	/// </summary>
	public class OrdTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{OrderType.Market, "1"},
			{OrderType.Limit, "2"},
			{OrderType.Stop, "3"},
			{OrderType.StopLimit, "4"},
			{OrderType.MarketOnClose, "5"},
			{OrderType.WithOrWithout, "6"},
			{OrderType.LimitOrBetter, "7"},
			{OrderType.LimitWithOrWithout, "8"},
			{OrderType.OnBasis, "9"},
			{OrderType.OnClose, "A"},
			{OrderType.LimitOnClose, "B"},
			{OrderType.PreviouslyQuoted, "D"},
			{OrderType.PreviouslyIndicated, "E"},
			{OrderType.ForexLimit, "F"},
			{OrderType.ForexSwap, "G"},
			{OrderType.ForexPreviouslyIndicated, "H"},
			{OrderType.Funarii, "I"},
			{OrderType.MarketIfTouched, "J"},
			{OrderType.MarketWithLeftoverAsLimit, "K"},
			{OrderType.PreviousFundValuationPoint, "L"},
			{OrderType.NextFundValuationPoint, "M"},
			{OrderType.Pegged, "P"},
		};

		/// <summary>
		/// Initializes the shared members of a OrdTypeConverter.
		/// </summary>
		static OrdTypeConverter()
		{

			// Initialize the mapping of strings to OrdType.
			OrdTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OrdTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of OrdType to strings.
			OrdTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OrdTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a OrdType.
		/// </summary>
		/// <param name="value">The FIX string representation of a OrdType.</param>
		/// <returns>A OrdType value.</returns>
		public static OrderType ConvertFrom(string value) {return (OrderType)OrdTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a OrdType to a string.
		/// </summary>
		/// <returns>A OrdType value.</returns>
		/// <param name="value">The FIX string representation of a OrdType.</param>
		public static string ConvertTo(OrderType messageType) {return (string)OrdTypeConverter.toTable[messageType];}

	}

}

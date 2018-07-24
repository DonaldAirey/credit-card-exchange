namespace FluidTrade.Core
{
    using System.Collections;

	/// <summary>
	/// Type Converter for FIX TimeInForce Field
	/// </summary>
	public class TimeInForceConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{TimeInForce.Day, "0"},
			{TimeInForce.GoodTillCancel, "1"},
			{TimeInForce.AtTheOpening, "2"},
			{TimeInForce.ImmediateOrCancel, "3"},
			{TimeInForce.FillOrKill, "4"},
			{TimeInForce.GoodTillCrossing, "5"},
			{TimeInForce.GoodTillDate, "6"},
			{TimeInForce.AtTheClose , "7"}
		};

		/// <summary>
		/// Initializes the shared members of a TimeInForceConverter.
		/// </summary>
		static TimeInForceConverter()
		{

			// Initialize the mapping of strings to TimeInForce.
			TimeInForceConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				TimeInForceConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of TimeInForce to strings.
			TimeInForceConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				TimeInForceConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a TimeInForce.
		/// </summary>
		/// <param name="value">The FIX string representation of a TimeInForce.</param>
		/// <returns>A TimeInForce value.</returns>
		public static TimeInForce ConvertFrom(string value) {return (TimeInForce)TimeInForceConverter.fromTable[value];}

		/// <summary>
		/// Converts a TimeInForce to a string.
		/// </summary>
		/// <returns>A TimeInForce value.</returns>
		/// <param name="value">The FIX string representation of a TimeInForce.</param>
		public static string ConvertTo(TimeInForce messageType) {return (string)TimeInForceConverter.toTable[messageType];}

	}

}

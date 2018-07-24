namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX Side Field
	/// </summary>
	public class SideConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{Side.Buy, "1"},
			{Side.Sell, "2"},
			{Side.BuyMinus, "3"},
			{Side.SellPlus, "4"},
			{Side.SellShort, "5"},
			{Side.SellShortExempt, "6"},
			{Side.Undisclosed, "7"},
			{Side.Cross, "8"},
			{Side.CrossShort, "9"},
			{Side.CrossShortExempt, "A"},
			{Side.AsDefined, "B"},
			{Side.Opposite, "C"},
			{Side.Subscribe, "D"},
			{Side.Redeem, "E"},
			{Side.Lend, "F"},
			{Side.Borrow, "G"}
		};

		/// <summary>
		/// Initializes the shared members of a SideConverter.
		/// </summary>
		static SideConverter()
		{

			// Initialize the mapping of strings to Side.
			SideConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				SideConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Side to strings.
			SideConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				SideConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a Side.
		/// </summary>
		/// <param name="value">The FIX string representation of a Side.</param>
		/// <returns>A Side value.</returns>
		public static Side ConvertFrom(string value) {return (Side)SideConverter.fromTable[value];}

		/// <summary>
		/// Converts a Side to a string.
		/// </summary>
		/// <returns>A Side value.</returns>
		/// <param name="value">The FIX string representation of a Side.</param>
		public static string ConvertTo(Side messageType) {return (string)SideConverter.toTable[messageType];}

	}

}

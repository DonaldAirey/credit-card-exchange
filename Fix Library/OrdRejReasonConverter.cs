namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX OrdRejReason Field
	/// </summary>
	public class OrdRejReasonConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{OrdRejReason.BrokerExchangeOption, "0"},
			{OrdRejReason.UnknownSymbol, "1"},
			{OrdRejReason.ExchangeClosed, "2"},
			{OrdRejReason.OrderExceedsLimit, "3"},
			{OrdRejReason.TooLateToEnter, "4"},
			{OrdRejReason.UnknownOrder, "5"},
			{OrdRejReason.DuplicateOrder, "6"},
			{OrdRejReason.DuplicateOfVerballyCommunicatedOrder, "7"},
			{OrdRejReason.StaleOrder, "8"},
			{OrdRejReason.TradeAlongRequired, "9"},
			{OrdRejReason.InvalidInvestorID, "10"},
			{OrdRejReason.UnsupportedOrderCharacteristic, "11"},
			{OrdRejReason.SurveillenceOption, "12"},
			{OrdRejReason.IncorrectQuantity, "13"},
			{OrdRejReason.IncorrectAllocatedQuantity, "14"},
			{OrdRejReason.UnknownAccount, "15"},
			{OrdRejReason.Other, "99"},
		};

		/// <summary>
		/// Initializes the shared members of a OrdRejReasonConverter.
		/// </summary>
		static OrdRejReasonConverter()
		{

			// Initialize the mapping of strings to OrdRejReason.
			OrdRejReasonConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OrdRejReasonConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of OrdRejReason to strings.
			OrdRejReasonConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OrdRejReasonConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a OrdRejReason.
		/// </summary>
		/// <param name="value">The FIX string representation of a OrdRejReason.</param>
		/// <returns>A OrdRejReason value.</returns>
		public static OrdRejReason ConvertFrom(string value) {return (OrdRejReason)OrdRejReasonConverter.fromTable[value];}

		/// <summary>
		/// Converts a OrdRejReason to a string.
		/// </summary>
		/// <returns>A OrdRejReason value.</returns>
		/// <param name="value">The FIX string representation of a OrdRejReason.</param>
		public static string ConvertTo(OrdRejReason messageType) {return (string)OrdRejReasonConverter.toTable[messageType];}

	}

}

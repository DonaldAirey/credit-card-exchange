namespace FluidTrade.Core
{

    using System.Collections;

    /// <summary>
	/// Type Converter for FIX ExecType Field
	/// </summary>
	public class ExecTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{ExecType.New, "0"},
			{ExecType.PartialFill, "1"},
			{ExecType.Fill, "2"},
			{ExecType.DoneForDay, "3"},
			{ExecType.Canceled, "4"},
			{ExecType.Replace, "5"},
			{ExecType.PendingCancel, "6"},
			{ExecType.Stopped, "7"},
			{ExecType.Rejected, "8"},
			{ExecType.Suspended, "9"},
			{ExecType.PendingNew, "A"},
			{ExecType.Calculated, "B"},
			{ExecType.Expired, "C"},
			{ExecType.Restated, "D"},
			{ExecType.PendingReplace, "E"},
			{ExecType.Trade, "F"},
			{ExecType.TradeCorrect, "G"},
			{ExecType.TradeCancel, "H"},
			{ExecType.OrderStatus, "I"},
		};

		/// <summary>
		/// Initializes the shared members of a ExecTypeConverter.
		/// </summary>
		static ExecTypeConverter()
		{

			// Initialize the mapping of strings to ExecType.
			ExecTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				ExecTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of ExecType to strings.
			ExecTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				ExecTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a ExecType.
		/// </summary>
		/// <param name="value">The FIX string representation of a ExecType.</param>
		/// <returns>A ExecType value.</returns>
		public static ExecType ConvertFrom(string value) {return (ExecType)ExecTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a ExecType to a string.
		/// </summary>
		/// <returns>A ExecType value.</returns>
		/// <param name="value">The FIX string representation of a ExecType.</param>
		public static string ConvertTo(ExecType messageType) {return (string)ExecTypeConverter.toTable[messageType];}

	}

}

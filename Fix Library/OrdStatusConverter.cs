namespace FluidTrade.Core
{

    using System.Collections;
	
	/// <summary>
	/// Type Converter for FIX OrdStatus Field
	/// </summary>
	public class OrdStatusConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{OrdStatus.New, "0"},
			{OrdStatus.PartiallyFilled, "1"},
			{OrdStatus.Filled, "2"},
			{OrdStatus.DoneForDay, "3"},
			{OrdStatus.Canceled, "4"},
			{OrdStatus.Replaced, "5"},
			{OrdStatus.PendingCancel, "6"},
			{OrdStatus.Stopped, "7"},
			{OrdStatus.Rejected, "8"},
			{OrdStatus.Suspended, "9"},
			{OrdStatus.PendingNew, "A"},
			{OrdStatus.Calculated, "B"},
			{OrdStatus.Expired, "C"},
			{OrdStatus.AcceptedForBidding, "D"},
			{OrdStatus.PendingReplace, "E"},
		};

		/// <summary>
		/// Initializes the shared members of a OrdStatusConverter.
		/// </summary>
		static OrdStatusConverter()
		{

			// Initialize the mapping of strings to OrdStatus.
			OrdStatusConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OrdStatusConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of OrdStatus to strings.
			OrdStatusConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				OrdStatusConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a OrdStatus.
		/// </summary>
		/// <param name="value">The FIX string representation of a OrdStatus.</param>
		/// <returns>A OrdStatus value.</returns>
		public static OrdStatus ConvertFrom(string value) {return (OrdStatus)OrdStatusConverter.fromTable[value];}

		/// <summary>
		/// Converts a OrdStatus to a string.
		/// </summary>
		/// <returns>A OrdStatus value.</returns>
		/// <param name="value">The FIX string representation of a OrdStatus.</param>
		public static string ConvertTo(OrdStatus messageType) {return (string)OrdStatusConverter.toTable[messageType];}

	}

}

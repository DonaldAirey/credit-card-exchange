namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX MsgType Field
	/// </summary>
	public class MsgTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{MsgType.Heartbeat, "0"},
			{MsgType.TestRequest, "1"},
			{MsgType.ResendRequest, "2"},
			{MsgType.Reject, "3"},
			{MsgType.SequenceReset, "4"},
			{MsgType.Logout, "5"},
			{MsgType.IndicationOfInterest, "6"},
			{MsgType.Advertisement, "7"},
			{MsgType.ExecutionReport, "8"},
			{MsgType.OrderCancelReject, "9"},
			{MsgType.Login, "A"},
			{MsgType.News, "B"},
			{MsgType.Email, "C"},
			{MsgType.Order, "D"},
			{MsgType.OrderCancelRequest, "F"},
			{MsgType.OrderCancelReplaceRequest, "G"},
			{MsgType.OrderStatusRequest, "H"},
			{MsgType.Allocation, "J"},
			{MsgType.AllocationAck, "P"},
			{MsgType.DontKnowTrade, "Q"},
			{MsgType.BusinessMessageReject, "j"}
};

		/// <summary>
		/// Initializes the shared members of a MsgTypeConverter.
		/// </summary>
		static MsgTypeConverter()
		{

			// Initialize the mapping of strings to MsgType.
			MsgTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				MsgTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of MsgType to strings.
			MsgTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				MsgTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a MsgType.
		/// </summary>
		/// <param name="value">The FIX string representation of a MsgType.</param>
		/// <returns>A MsgType value.</returns>
		public static MsgType ConvertFrom(string value) {return (MsgType)(MsgTypeConverter.fromTable[value]!=null?MsgTypeConverter.fromTable[value]:MsgType.NullMsgType);}

		/// <summary>
		/// Converts a MsgType to a string.
		/// </summary>
		/// <returns>A MsgType value.</returns>
		/// <param name="value">The FIX string representation of a MsgType.</param>
		public static string ConvertTo(MsgType messageType) {return (string)MsgTypeConverter.toTable[messageType];}

	}

}

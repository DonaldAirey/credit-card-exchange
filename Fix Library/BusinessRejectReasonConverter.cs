namespace FluidTrade.Core
{


    using System.Collections;

	/// <summary>
	/// Type Converter for FIX BusinessRejectReason Field
	/// </summary>
	public class BusinessRejectReasonConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{BusinessRejectReason.Other, "0"},
			{BusinessRejectReason.UnkownID, "1"},
			{BusinessRejectReason.UnknownSecurity, "2"},
			{BusinessRejectReason.UnsupportedMessageType, "3"},
			{BusinessRejectReason.ApplicationNotAvailable, "4"},
			{BusinessRejectReason.ConditionallyRequiredFieldMissing, "5"}
		};

		/// <summary>
		/// Initializes the shared members of a BusinessRejectReasonConverter.
		/// </summary>
		static BusinessRejectReasonConverter()
		{

			// Initialize the mapping of strings to BusinessRejectReason.
			BusinessRejectReasonConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				BusinessRejectReasonConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of BusinessRejectReason to strings.
			BusinessRejectReasonConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				BusinessRejectReasonConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a BusinessRejectReason.
		/// </summary>
		/// <param name="value">The FIX string representation of a BusinessRejectReason.</param>
		/// <returns>A BusinessRejectReason value.</returns>
		public static BusinessRejectReason ConvertFrom(string value) {return (BusinessRejectReason)BusinessRejectReasonConverter.fromTable[value];}

		/// <summary>
		/// Converts a BusinessRejectReason to a string.
		/// </summary>
		/// <returns>A BusinessRejectReason value.</returns>
		/// <param name="value">The FIX string representation of a BusinessRejectReason.</param>
		public static string ConvertTo(BusinessRejectReason messageType) {return (string)BusinessRejectReasonConverter.toTable[messageType];}

	}

}

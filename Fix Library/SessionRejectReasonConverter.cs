namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX SessionRejectReason Field
	/// </summary>
	public class SessionRejectReasonConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{

				{SessionRejectReason.InvalidTagNumber, "0"},
				{SessionRejectReason.RequiredTagMissing, "1"},
				{SessionRejectReason.TagNotDefinedForMessageType, "2"},
				{SessionRejectReason.UndefinedTag, "3"},
				{SessionRejectReason.TagSpecifiedWithoutValue, "4"},
				{SessionRejectReason.ValueOutOfRange, "5"},
				{SessionRejectReason.IncorrectDataFormat, "6"},
				{SessionRejectReason.DecryptionProblem, "7"},
				{SessionRejectReason.SignatureProblem, "8"},
				{SessionRejectReason.CompIdProblem, "9"},
				{SessionRejectReason.SendingTimeAccuracyProblem, "10"} ,
				{SessionRejectReason.InvalidMsgType , "11"}

		};

		/// <summary>
		/// Initializes the shared members of a SessionRejectReasonConverter.
		/// </summary>
		static SessionRejectReasonConverter()
		{

			// Initialize the mapping of strings to Type.
			SessionRejectReasonConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				SessionRejectReasonConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			SessionRejectReasonConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				SessionRejectReasonConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a SessionRejectReason.
		/// </summary>
		/// <param name="value">The FIX string representation of a SessionRejectReason.</param>
		/// <returns>A SessionRejectReason value.</returns>
		public static SessionRejectReason ConvertFrom(string value) {return (SessionRejectReason)SessionRejectReasonConverter.fromTable[value];}

		/// <summary>
		/// Converts a SessionRejectReason to a string.
		/// </summary>
		/// <param name="value">A SessionRejectReason value.</param>
		/// <returns>The FIX string representation of a SessionRejectReason.</returns>
		public static string ConvertTo(SessionRejectReason enumType) {return (string)SessionRejectReasonConverter.toTable[enumType];}

	}

}


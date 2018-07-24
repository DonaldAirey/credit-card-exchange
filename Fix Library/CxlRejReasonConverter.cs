namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX CxlRejReason Field
	/// </summary>
	public class CxlRejReasonConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{CxlRejReason.TooLateToCancel, "0"},
				{CxlRejReason.UnknownOrder, "1"},
				{CxlRejReason.BrokerOption, "2"},
				{CxlRejReason.AlreadyPending, "3"},
		};

		/// <summary>
		/// Initializes the shared members of a CxlRejReasonConverter.
		/// </summary>
		static CxlRejReasonConverter()
		{

			// Initialize the mapping of strings to Type.
			CxlRejReasonConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				CxlRejReasonConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			CxlRejReasonConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				CxlRejReasonConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a CxlRejReason.
		/// </summary>
		/// <param name="value">The FIX string representation of a CxlRejReason.</param>
		/// <returns>A CxlRejReason value.</returns>
		public static CxlRejReason ConvertFrom(string value) {return (CxlRejReason)CxlRejReasonConverter.fromTable[value];}

		/// <summary>
		/// Converts a CxlRejReason to a string.
		/// </summary>
		/// <param name="value">A CxlRejReason value.</param>
		/// <returns>The FIX string representation of a CxlRejReason.</returns>
		public static string ConvertTo(CxlRejReason enumType) {return (string)CxlRejReasonConverter.toTable[enumType];}

	}

}


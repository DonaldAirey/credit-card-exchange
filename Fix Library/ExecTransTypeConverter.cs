namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX ExecTransType Field
	/// </summary>
	public class ExecTransTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{ExecTransType.New, "0"},
			{ExecTransType.Cancel, "1"},
			{ExecTransType.Correct, "2"},
			{ExecTransType.Status, "3"}
		};

		/// <summary>
		/// Initializes the shared members of a ExecTransTypeConverter.
		/// </summary>
		static ExecTransTypeConverter()
		{

			// Initialize the mapping of strings to ExecTransType.
			ExecTransTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				ExecTransTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of ExecTransType to strings.
			ExecTransTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				ExecTransTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a ExecTransType.
		/// </summary>
		/// <param name="value">The FIX string representation of a ExecTransType.</param>
		/// <returns>A ExecTransType value.</returns>
		public static ExecTransType ConvertFrom(string value) {return (ExecTransType)ExecTransTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a ExecTransType to a string.
		/// </summary>
		/// <returns>A ExecTransType value.</returns>
		/// <param name="value">The FIX string representation of a ExecTransType.</param>
		public static string ConvertTo(ExecTransType messageType) {return (string)ExecTransTypeConverter.toTable[messageType];}

	}

}

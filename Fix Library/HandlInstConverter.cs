namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX HandlInst Field
	/// </summary>
	public class HandlInstConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{HandlInst.Automatic, "1"},
			{HandlInst.Broker, "2"},
			{HandlInst.Manual, "3"}
		};

		/// <summary>
		/// Initializes the shared members of a HandlInstConverter.
		/// </summary>
		static HandlInstConverter()
		{

			// Initialize the mapping of strings to HandlInst.
			HandlInstConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				HandlInstConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of HandlInst to strings.
			HandlInstConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				HandlInstConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a HandlInst.
		/// </summary>
		/// <param name="value">The FIX string representation of a HandlInst.</param>
		/// <returns>A HandlInst value.</returns>
		public static HandlInst ConvertFrom(string value) {return (HandlInst)HandlInstConverter.fromTable[value];}

		/// <summary>
		/// Converts a HandlInst to a string.
		/// </summary>
		/// <returns>A HandlInst value.</returns>
		/// <param name="value">The FIX string representation of a HandlInst.</param>
		public static string ConvertTo(HandlInst messageType) {return (string)HandlInstConverter.toTable[messageType];}

	}

}

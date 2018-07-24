namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX LastCapacity Field
	/// </summary>
	public class LastCapacityConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{LastCapacity.Agent, "1"},
				{LastCapacity.CrossAsAgent, "2"},
				{LastCapacity.CrossAsPrincipal, "3"},
				{LastCapacity.Principal, "4"}
			};

		/// <summary>
		/// Initializes the shared members of a LastCapacityConverter.
		/// </summary>
		static LastCapacityConverter()
		{

			// Initialize the mapping of strings to Type.
			LastCapacityConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				LastCapacityConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of Type to strings.
			LastCapacityConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				LastCapacityConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a LastCapacity.
		/// </summary>
		/// <param name="value">The FIX string representation of a LastCapacity.</param>
		/// <returns>A LastCapacity value.</returns>
		public static LastCapacity ConvertFrom(string value) {return (LastCapacity)LastCapacityConverter.fromTable[value];}

		/// <summary>
		/// Converts a LastCapacity to a string.
		/// </summary>
		/// <param name="value">A LastCapacity value.</param>
		/// <returns>The FIX string representation of a LastCapacity.</returns>
		public static string ConvertTo(LastCapacity enumType) {return (string)LastCapacityConverter.toTable[enumType];}

	}

}


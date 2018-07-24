namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX RoutingType Field.
	/// </summary>
	public class RoutingTypeConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{RoutingType.TargetFirm, "1"},
			{RoutingType.TargetList, "2"},
			{RoutingType.BlockFirm, "3"},
			{RoutingType.BlockList, "4"}
		};

		/// <summary>
		/// Initializes the shared members of a RoutingTypeConverter.
		/// </summary>
		static RoutingTypeConverter()
		{

			// Initialize the mapping of strings to RoutingType.
			RoutingTypeConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				RoutingTypeConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of RoutingType to strings.
			RoutingTypeConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				RoutingTypeConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a RoutingType.
		/// </summary>
		/// <param name="value">The FIX string representation of a RoutingType.</param>
		/// <returns>A RoutingType value.</returns>
		public static RoutingType ConvertFrom(string value) {return (RoutingType)RoutingTypeConverter.fromTable[value];}

		/// <summary>
		/// Converts a RoutingType to a string.
		/// </summary>
		/// <returns>A RoutingType value.</returns>
		/// <param name="value">The FIX string representation of a RoutingType.</param>
		public static string ConvertTo(RoutingType routingType) {return (string)RoutingTypeConverter.toTable[routingType];}

	}

}

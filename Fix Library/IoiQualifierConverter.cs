namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX IOIQualifier Field.
	/// </summary>
	public class IoiQualifierConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{IOIQualifier.AllOrNone, "A"},
			{IOIQualifier.AtTheClose, "C"},
			{IOIQualifier.InTouchWith, "I"},
			{IOIQualifier.Limit, "L"},
			{IOIQualifier.MoreBehind, "M"},
			{IOIQualifier.AtTheOpen, "O"},
			{IOIQualifier.TakingPosition, "P"},
			{IOIQualifier.AtTheMarket, "Q"},
			{IOIQualifier.ReadyToTrade, "R"},
			{IOIQualifier.PortfolioShown, "S"},
			{IOIQualifier.ThroughTheDay, "T"},
			{IOIQualifier.Versus, "V"},
			{IOIQualifier.WorkingAway, "W"},
			{IOIQualifier.CrossingOpportunity, "X"},
			{IOIQualifier.AtTheMidpoint, "Y"},
			{IOIQualifier.PreOpen , "Z"}
		};

		/// <summary>
		/// Initializes the shared members of a IoiQualifierConverter.
		/// </summary>
		static IoiQualifierConverter()
		{

			// Initialize the mapping of strings to IOIQualifier.
			IoiQualifierConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				IoiQualifierConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of IOIQualifier to strings.
			IoiQualifierConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				IoiQualifierConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a IOIQualifier.
		/// </summary>
		/// <param name="value">The FIX string representation of a IOIQualifier.</param>
		/// <returns>A IOIQualifier value.</returns>
		public static IOIQualifier ConvertFrom(string value) {return (IOIQualifier)IoiQualifierConverter.fromTable[value];}

		/// <summary>
		/// Converts a IOIQualifier to a string.
		/// </summary>
		/// <returns>A IOIQualifier value.</returns>
		/// <param name="value">The FIX string representation of a IOIQualifier.</param>
		public static string ConvertTo(IOIQualifier ioiQualifier) {return (string)IoiQualifierConverter.toTable[ioiQualifier];}

	}

}

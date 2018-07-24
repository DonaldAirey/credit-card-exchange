namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX DiscretionInst Field
	/// </summary>
	public class DiscretionInstConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
			{
				{DiscretionInst.Displayed, "0"},
				{DiscretionInst.Market, "1"},
				{DiscretionInst.Primary, "2"},
				{DiscretionInst.LocalPrimary, "3"},
				{DiscretionInst.Midpoint, "4"},
				{DiscretionInst.LastTrade, "5"}
			};

			/// <summary>
			/// Initializes the shared members of a DiscretionInstConverter.
			/// </summary>
			static DiscretionInstConverter()
			{

				// Initialize the mapping of strings to Type.
				DiscretionInstConverter.fromTable = new Hashtable();
				for (int element = 0; element < pairs.GetLength(0); element++)
					DiscretionInstConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

				// Initialize the mapping of Type to strings.
				DiscretionInstConverter.toTable = new Hashtable();
				for (int element = 0; element < pairs.GetLength(0); element++)
					DiscretionInstConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

			}

		/// <summary>
		/// Converts a string to a DiscretionInst.
		/// </summary>
		/// <param name="value">The FIX string representation of a DiscretionInst.</param>
		/// <returns>A DiscretionInst value.</returns>
		public static DiscretionInst ConvertFrom(string value) {return (DiscretionInst)DiscretionInstConverter.fromTable[value];}

		/// <summary>
		/// Converts a DiscretionInst to a string.
		/// </summary>
		/// <param name="value">A DiscretionInst value.</param>
		/// <returns>The FIX string representation of a DiscretionInst.</returns>
		public static string ConvertTo(DiscretionInst enumType) {return (string)DiscretionInstConverter.toTable[enumType];}

	}

}


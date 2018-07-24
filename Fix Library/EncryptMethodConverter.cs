namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Type Converter for FIX EncryptMethod Field
	/// </summary>
	public class EncryptMethodConverter
	{

		// Private Members
		private static Hashtable fromTable;
		private static Hashtable toTable;
		private static object[,] pairs =
		{
			{EncryptMethod.None, "0"},
			{EncryptMethod.PKCS, "1"},
			{EncryptMethod.DES, "2"},
			{EncryptMethod.PKCSDES, "3"},
			{EncryptMethod.PGPDES, "4"},
			{EncryptMethod.PGPDESMD5, "5"},
			{EncryptMethod.PEMDESMD5, "6"}
		};

		/// <summary>
		/// Initializes the shared members of a EncryptMethodConverter.
		/// </summary>
		static EncryptMethodConverter()
		{

			// Initialize the mapping of strings to EncryptMethod.
			EncryptMethodConverter.fromTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				EncryptMethodConverter.fromTable.Add(pairs[element, 1], pairs[element, 0]);

			// Initialize the mapping of EncryptMethod to strings.
			EncryptMethodConverter.toTable = new Hashtable();
			for (int element = 0; element < pairs.GetLength(0); element++)
				EncryptMethodConverter.toTable.Add(pairs[element, 0], pairs[element, 1]);

		}

		/// <summary>
		/// Converts a string to a EncryptMethod.
		/// </summary>
		/// <param name="value">The FIX string representation of a EncryptMethod.</param>
		/// <returns>A EncryptMethod value.</returns>
		public static EncryptMethod ConvertFrom(string value) {return (EncryptMethod)EncryptMethodConverter.fromTable[value];}

		/// <summary>
		/// Converts a EncryptMethod to a string.
		/// </summary>
		/// <returns>A EncryptMethod value.</returns>
		/// <param name="value">The FIX string representation of a EncryptMethod.</param>
		public static string ConvertTo(EncryptMethod messageType) {return (string)EncryptMethodConverter.toTable[messageType];}

	}

}

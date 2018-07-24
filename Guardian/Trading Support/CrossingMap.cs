namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

	/// <summary>
	/// Converts the CrossingMap constants to internal identifiers.
	/// </summary>
	class CrossingMap
	{

		// Static private fields
		private static Dictionary<Guid, Crossing> idDictionary;
		private static Dictionary<Crossing, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static CrossingMap()
		{

			// Initialize the object
			CrossingMap.idDictionary = new Dictionary<Guid, Crossing>();
			CrossingMap.enumDictionary = new Dictionary<Crossing, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (CrossingRow crossingRow in DataModel.Crossing)
				{
					CrossingMap.idDictionary.Add((Guid)crossingRow[DataModel.Crossing.CrossingIdColumn], (Crossing)crossingRow[DataModel.Crossing.CrossingCodeColumn]);
					CrossingMap.enumDictionary.Add((Crossing)crossingRow[DataModel.Crossing.CrossingCodeColumn], (Guid)crossingRow[DataModel.Crossing.CrossingIdColumn]);
				}

			}
			finally
			{

				// The data model can be used by other threads now.
				DataModel.DataLock.ExitReadLock();

			}

		}

		/// <summary>
		/// Gets an internal database identifier based on the Enumerated value.
		/// </summary>
		/// <param name="key">The strongly typed enumerated value.</param>
		/// <returns>The equivalent record identifier from the database.</returns>
		public static Guid FromCode(Crossing key)
		{
			return CrossingMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static Crossing FromId(Guid key)
		{
			return CrossingMap.idDictionary[key];
		}

	}

}

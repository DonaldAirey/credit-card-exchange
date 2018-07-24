namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

	/// <summary>
	/// Converts the TimeUnitMap constants to internal identifiers.
	/// </summary>
	class TimeUnitMap
	{

		// Static private fields
		private static Dictionary<Guid, TimeUnit> idDictionary;
		private static Dictionary<TimeUnit, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static TimeUnitMap()
		{

			// Initialize the object
			TimeUnitMap.idDictionary = new Dictionary<Guid, TimeUnit>();
			TimeUnitMap.enumDictionary = new Dictionary<TimeUnit, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (TimeUnitRow timeUnitRow in DataModel.TimeUnit)
				{
					TimeUnitMap.idDictionary.Add((Guid)timeUnitRow[DataModel.TimeUnit.TimeUnitIdColumn], (TimeUnit)timeUnitRow[DataModel.TimeUnit.TimeUnitCodeColumn]);
					TimeUnitMap.enumDictionary.Add((TimeUnit)timeUnitRow[DataModel.TimeUnit.TimeUnitCodeColumn], (Guid)timeUnitRow[DataModel.TimeUnit.TimeUnitIdColumn]);
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
		public static Guid FromCode(TimeUnit key)
		{
			return TimeUnitMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static TimeUnit FromId(Guid key)
		{
			return TimeUnitMap.idDictionary[key];
		}

	}

}

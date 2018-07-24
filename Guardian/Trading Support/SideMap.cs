namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

	/// <summary>
	/// Converts the SideMap constants to internal identifiers.
	/// </summary>
	class SideMap
	{

		// Static private fields
		private static Dictionary<Guid, Side> idDictionary;
		private static Dictionary<Side, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static SideMap()
		{

			// Initialize the object
			SideMap.idDictionary = new Dictionary<Guid, Side>();
			SideMap.enumDictionary = new Dictionary<Side, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (SideRow sideRow in DataModel.Side)
				{
					SideMap.idDictionary.Add((Guid)sideRow[DataModel.Side.SideIdColumn], (Side)sideRow[DataModel.Side.SideCodeColumn]);
					SideMap.enumDictionary.Add((Side)sideRow[DataModel.Side.SideCodeColumn], (Guid)sideRow[DataModel.Side.SideIdColumn]);
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
		public static Guid FromCode(Side key)
		{
			return SideMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static Side FromId(Guid key)
		{
			return SideMap.idDictionary[key];
		}

	}

}

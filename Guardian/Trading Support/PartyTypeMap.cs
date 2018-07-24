namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

	/// <summary>
	/// Converts the PartyTypeMap constants to internal identifiers.
	/// </summary>
	class PartyTypeMap
	{

		// Static private fields
		private static Dictionary<Guid, PartyType> idDictionary;
		private static Dictionary<PartyType, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static PartyTypeMap()
		{

			// Initialize the object
			PartyTypeMap.idDictionary = new Dictionary<Guid, PartyType>();
			PartyTypeMap.enumDictionary = new Dictionary<PartyType, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (PartyTypeRow partyTypeRow in DataModel.PartyType)
				{
					PartyTypeMap.idDictionary.Add((Guid)partyTypeRow[DataModel.PartyType.PartyTypeIdColumn], (PartyType)partyTypeRow[DataModel.PartyType.PartyTypeCodeColumn]);
					PartyTypeMap.enumDictionary.Add((PartyType)partyTypeRow[DataModel.PartyType.PartyTypeCodeColumn], (Guid)partyTypeRow[DataModel.PartyType.PartyTypeIdColumn]);
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
		public static Guid FromCode(PartyType key)
		{
			return PartyTypeMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static PartyType FromId(Guid key)
		{
			return PartyTypeMap.idDictionary[key];
		}

	}

}

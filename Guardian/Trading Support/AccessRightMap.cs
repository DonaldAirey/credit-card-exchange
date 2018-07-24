namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

	/// <summary>
	/// Converts the StatusMap constants to internal identifiers.
	/// </summary>
	public class AccessRightMap
	{

		// Static private fields
		private static Dictionary<Guid, AccessRight> idDictionary;
		private static Dictionary<AccessRight, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static AccessRightMap()
		{

			// Initialize the object
			AccessRightMap.idDictionary = new Dictionary<Guid, AccessRight>();
			AccessRightMap.enumDictionary = new Dictionary<AccessRight, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (AccessRightRow accessRightRow in DataModel.AccessRight)
				{
					AccessRightMap.idDictionary.Add(
						(Guid)accessRightRow[DataModel.AccessRight.AccessRightIdColumn],
						(AccessRight)accessRightRow[DataModel.AccessRight.AccessRightCodeColumn]);
					AccessRightMap.enumDictionary.Add(
						(AccessRight)accessRightRow[DataModel.AccessRight.AccessRightCodeColumn],
						(Guid)accessRightRow[DataModel.AccessRight.AccessRightIdColumn]);
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
		public static Guid FromCode(AccessRight key)
		{
			return AccessRightMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static AccessRight FromId(Guid key)
		{
			return AccessRightMap.idDictionary[key];
		}

	}

}

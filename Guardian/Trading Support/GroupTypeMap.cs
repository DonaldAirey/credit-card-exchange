namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;

	class GroupTypeMap
	{

		// Static private fields
		private static Dictionary<Guid, GroupType> idDictionary;
		private static Dictionary<GroupType, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static GroupTypeMap()
		{

			// Initialize the object
			GroupTypeMap.idDictionary = new Dictionary<Guid, GroupType>();
			GroupTypeMap.enumDictionary = new Dictionary<GroupType, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (GroupTypeRow groupTypeRow in DataModel.GroupType)
				{
					GroupTypeMap.idDictionary.Add(
							(Guid)groupTypeRow[DataModel.GroupType.GroupTypeIdColumn],
							(GroupType)groupTypeRow[DataModel.GroupType.GroupTypeCodeColumn]);
					GroupTypeMap.enumDictionary.Add(
							(GroupType)groupTypeRow[DataModel.GroupType.GroupTypeCodeColumn],
							(Guid)groupTypeRow[DataModel.GroupType.GroupTypeIdColumn]);
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
		public static Guid FromCode(GroupType key)
		{
			return GroupTypeMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static GroupType FromId(Guid key)
		{
			return GroupTypeMap.idDictionary[key];
		}
	
	}
}

namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

	/// <summary>
	/// Converts the VolumeCategoryMap constants to internal identifiers.
	/// </summary>
	class VolumeCategoryMap
	{

		// Static private fields
		private static Dictionary<Guid, VolumeCategory> idDictionary;
		private static Dictionary<VolumeCategory, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static VolumeCategoryMap()
		{

			// Initialize the object
			VolumeCategoryMap.idDictionary = new Dictionary<Guid, VolumeCategory>();
			VolumeCategoryMap.enumDictionary = new Dictionary<VolumeCategory, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (VolumeCategoryRow volumeCategoryRow in DataModel.VolumeCategory)
				{
					VolumeCategoryMap.idDictionary.Add((Guid)volumeCategoryRow[DataModel.VolumeCategory.VolumeCategoryIdColumn], (VolumeCategory)volumeCategoryRow[DataModel.VolumeCategory.VolumeCategoryCodeColumn]);
					VolumeCategoryMap.enumDictionary.Add((VolumeCategory)volumeCategoryRow[DataModel.VolumeCategory.VolumeCategoryCodeColumn], (Guid)volumeCategoryRow[DataModel.VolumeCategory.VolumeCategoryIdColumn]);
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
		public static Guid FromCode(VolumeCategory key)
		{
			return VolumeCategoryMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static VolumeCategory FromId(Guid key)
		{
			return VolumeCategoryMap.idDictionary[key];
		}

	}

}

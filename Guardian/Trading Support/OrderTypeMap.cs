namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian;

	/// <summary>
	/// Converts the OrderTypeMap constants to internal identifiers.
	/// </summary>
	class OrderTypeMap
	{

		// Static private fields
		private static Dictionary<Guid, OrderType> idDictionary;
		private static Dictionary<OrderType, Guid> enumDictionary;

		/// <summary>
		/// Creates a mapping between internal constants and database identifiers.
		/// </summary>
		static OrderTypeMap()
		{

			// Initialize the object
			OrderTypeMap.idDictionary = new Dictionary<Guid, OrderType>();
			OrderTypeMap.enumDictionary = new Dictionary<OrderType, Guid>();

			// A transaction is required to lock the records while the table is read.
			try
			{

				// Lock the whole data model before reading the table.
				DataModel.DataLock.EnterReadLock();

				// This will read each of the values into a hash table.  This hash table can be used in the code without having to lock the tables because each
				// of these values is constant and doesn't change after the system has been started.
				foreach (OrderTypeRow orderTypeRow in DataModel.OrderType)
				{
					OrderTypeMap.idDictionary.Add((Guid)orderTypeRow[DataModel.OrderType.OrderTypeIdColumn], (OrderType)orderTypeRow[DataModel.OrderType.OrderTypeCodeColumn]);
					OrderTypeMap.enumDictionary.Add((OrderType)orderTypeRow[DataModel.OrderType.OrderTypeCodeColumn], (Guid)orderTypeRow[DataModel.OrderType.OrderTypeIdColumn]);
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
		public static Guid FromCode(OrderType key)
		{
			return OrderTypeMap.enumDictionary[key];
		}

		/// <summary>
		/// Gets the strongly typed enumerated value from the internal database identifier.
		/// </summary>
		/// <param name="key">The internal database identifier.</param>
		/// <returns>The strongly typed enumerated value.</returns>
		public static OrderType FromId(Guid key)
		{
			return OrderTypeMap.idDictionary[key];
		}

	}

}

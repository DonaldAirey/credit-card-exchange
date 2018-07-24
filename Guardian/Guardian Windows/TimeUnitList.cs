namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using FluidTrade.Core;

	/// <summary>
	/// Represents a list of TimeInForceItems that is bound to the client data model.
	/// </summary>
	public class TimeUnitList : DataBoundList<TimeUnitItem>
	{

		/// <summary>
		/// The global TimeUnitList
		/// </summary>
		public static readonly TimeUnitList Default = new TimeUnitList();

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static TimeUnitList.CompareItems comparer = new TimeUnitList.CompareItems();

		/// <summary>
		/// Create a new time-unit list.
		/// </summary>
		private TimeUnitList()
		{

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<TimeUnitItem> Comparer
		{
			get { return TimeUnitList.comparer; }
		}

		/// <summary>
		/// The TimeUnit table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.TimeUnit; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<TimeUnitItem>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(TimeUnitItem left, TimeUnitItem right)
			{

				return left.Name.CompareTo(right.Name);

			}

		}

		/// <summary>
		/// Finds a TimeUnitItem that corresponds to a TimeUnit.
		/// </summary>
		/// <param name="timeUnitCode"></param>
		/// <returns>The item that was find.</returns>
		public TimeUnitItem Find(TimeUnit timeUnitCode)
		{

			return this.Find(row => row.TimeUnitCode == timeUnitCode);

		}

		/// <summary>
		/// Finds a TimeUnitItem that corresponds to a time unit id.
		/// </summary>
		/// <param name="timeUnitId"></param>
		/// <returns>The item that was find.</returns>
		public TimeUnitItem Find(Guid timeUnitId)
		{

			return this.Find(row => row.TimeUnitId == timeUnitId);

		}

		/// <summary>
		/// Create a new item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the time-unit table.</param>
		/// <returns>A new TimeUnitItem object.</returns>
		protected override TimeUnitItem New(DataRow dataRow)
		{

			TimeUnitItem item;
			TimeUnitRow baseRow = dataRow as TimeUnitRow;

			item = new TimeUnitItem()
			{
				Name = baseRow.Name,
				TimeUnitCode = baseRow.TimeUnitCode,
				TimeUnitId = baseRow.TimeUnitId,
			};

			return item;

		}

		/// <summary>
		/// Update a TimeUnitItem object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(TimeUnitItem old, TimeUnitItem update)
		{

			old.Name = update.Name;
			old.TimeUnitCode = update.TimeUnitCode;
			old.TimeUnitId = update.TimeUnitId;

		}

	}

}

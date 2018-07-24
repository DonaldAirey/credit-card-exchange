namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using FluidTrade.Core;

	/// <summary>
	/// Represents a list of SettlementUnitItems that is bound to the client data model.
	/// </summary>
	public class SettlementUnitList : DataBoundList<SettlementUnitItem>
	{

		/// <summary>
		/// The global SettlementUnitList
		/// </summary>
		public static readonly SettlementUnitList Default = new SettlementUnitList();

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static SettlementUnitList.CompareItems comparer = new SettlementUnitList.CompareItems();

		/// <summary>
		/// Create a new settlement-unit list.
		/// </summary>
		private SettlementUnitList()
		{

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<SettlementUnitItem> Comparer
		{
			get { return SettlementUnitList.comparer; }
		}

		/// <summary>
		/// The SettlementUnit table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.SettlementUnit; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<SettlementUnitItem>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(SettlementUnitItem left, SettlementUnitItem right)
			{

				return left.Name.CompareTo(right.Name);

			}

		}

		/// <summary>
		/// Finds a SettlementUnitItem that corresponds to a SettlementUnit.
		/// </summary>
		/// <param name="settlementUnitCode"></param>
		/// <returns></returns>
		public SettlementUnitItem Find(SettlementUnit settlementUnitCode)
		{

			return this.Find(row => row.SettlementUnitCode == settlementUnitCode);

		}

		/// <summary>
		/// Finds a SettlementUnitItem that corresponds to a time unit id.
		/// </summary>
		/// <param name="settlementUnitId"></param>
		/// <returns></returns>
		public SettlementUnitItem Find(Guid settlementUnitId)
		{

			return this.Find(row => row.SettlementUnitId == settlementUnitId);

		}

		/// <summary>
		/// Create a new item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the settlement-unit table.</param>
		/// <returns>A new SettlementUnitItem object.</returns>
		protected override SettlementUnitItem New(DataRow dataRow)
		{

			SettlementUnitItem item;
			SettlementUnitRow baseRow = dataRow as SettlementUnitRow;

			item = new SettlementUnitItem()
			{
				Name = baseRow.Name,
				SettlementUnitCode = baseRow.SettlementUnitCode,
				SettlementUnitId = baseRow.SettlementUnitId,
			};

			return item;

		}

		/// <summary>
		/// Update a SettlementUnitItem object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(SettlementUnitItem old, SettlementUnitItem update)
		{

			old.Name = update.Name;
			old.SettlementUnitCode = update.SettlementUnitCode;
			old.SettlementUnitId = update.SettlementUnitId;

		}

	}

}

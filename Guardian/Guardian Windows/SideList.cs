namespace FluidTrade.Guardian.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;
    using FluidTrade.Core.Windows;

	/// <summary>
	/// Represents a list of SideItems that is bound to the client data model.
	/// </summary>
	public class SideList : DataBoundList<SideItem>
	{

		/// <summary>
		/// The global SideList
		/// </summary>
		public static readonly SideList Default = new SideList();

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static SideList.CompareItems comparer = new SideList.CompareItems();

		/// <summary>
		/// Create a new side list.
		/// </summary>
		private SideList()
		{

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<SideItem> Comparer
		{
			get { return SideList.comparer; }
		}

		/// <summary>
		/// The Side table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.Side; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<SideItem>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(SideItem left, SideItem right)
			{

				return left.Mnemonic.CompareTo(right.Mnemonic);

			}

		}

		/// <summary>
		/// Create a new item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the side table.</param>
		/// <returns>A new SideItem object.</returns>
		protected override SideItem New(DataRow dataRow)
		{

			SideItem item;
			SideRow baseRow = dataRow as SideRow;

			item = new SideItem()
			{
				Description = baseRow.Description,
				DisabledImage = baseRow.DisabledImage,
				EnabledImage = baseRow.EnabledImage,
				Mnemonic = baseRow.Mnemonic,
				SideCode = baseRow.SideCode,
				SortOrder = baseRow.SortOrder,
			};

			return item;

		}

		/// <summary>
		/// Update a SideItem object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(SideItem old, SideItem update)
		{

			old.Description = update.Description;
			old.DisabledImage = update.DisabledImage;
			old.EnabledImage = update.EnabledImage;
			old.Mnemonic = update.Mnemonic;
			old.SideCode = update.SideCode;
			old.SortOrder = update.SortOrder;

		}

	}

}

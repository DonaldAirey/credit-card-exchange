namespace FluidTrade.Guardian
{

	using System.Collections.Generic;
	using System.Data;
	using System;
	
	/// <summary>
	/// A live list of provinces in a particular country.
	/// </summary>
	public class ProvinceList : DataBoundList<Province>
	{

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static ProvinceList.CompareItems comparer = new ProvinceList.CompareItems();

		private Guid countryId;

		/// <summary>
		/// Create a new province list.
		/// </summary>
		public ProvinceList(Guid countryId)
		{

			this.countryId = countryId;
			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<Province> Comparer
		{
			get { return ProvinceList.comparer; }
		}

		/// <summary>
		/// The Province table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.Province; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<Province>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(Province left, Province right)
			{

				return left.Abbreviation.CompareTo(right.Abbreviation);

			}

		}

		/// <summary>
		/// Filter what rows are included in the list based on Filter.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>The return value of Filter.</returns>
		protected override bool Filter(DataRow row)
		{

			return (row as ProvinceRow).CountryId == this.countryId;

		}

		/// <summary>
		/// Create a new item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the province table.</param>
		/// <returns>A new Province object.</returns>
		protected override Province New(DataRow dataRow)
		{

			Province item;
			ProvinceRow baseRow = dataRow as ProvinceRow;

			item = new Province(baseRow);

			return item;

		}

		/// <summary>
		/// Update a Province object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(Province old, Province update)
		{

			old.Update(update);

		}

	}

}

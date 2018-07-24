namespace FluidTrade.Guardian.Windows
{

    using System;
	using System.Collections.Generic;
	using System.Data;

	/// <summary>
	/// Represents a list of BlotterItems that is bound to the client data model.
	/// </summary>
	public class BlotterList : DataBoundList<BlotterItem>
	{

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static BlotterList.CompareItems comparer = new BlotterList.CompareItems();

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<BlotterItem> Comparer
		{
			get { return BlotterList.comparer; }
		}

		/// <summary>
		/// The Blotter table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.Blotter; }
		}

		/// <summary>
		/// A comparer to compare two blotters by name.
		/// </summary>
		public class CompareItems : IComparer<BlotterItem>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the items.</returns>
			public int Compare(BlotterItem left, BlotterItem right)
			{

				return left.Name.CompareTo(right.Name);

			}

		}

		/// <summary>
		/// Create a new blotter list.
		/// </summary>
		public BlotterList()
		{

			this.InitializeList();

		}

		/// <summary>
		/// Filter what rows are included in the list based on Filter.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>The return value of Filter.</returns>
		protected override bool Filter(DataRow row)
		{

			BlotterRow blotterRow = null;

			if (row is BlotterRow)
				blotterRow = row as BlotterRow;
			else if (row is EntityRow)
			{
				EntityRow entityRow = row as EntityRow;
				if (entityRow.GetBlotterRows().Length != 0)
					blotterRow = entityRow.GetBlotterRows()[0];
			}
			else
				throw new RowNotHandledException("row isn't the right kind of row");

			if (blotterRow == null)
				return false;

			return true;

		}

		/// <summary>
		/// Create a new blotter item from a table row.
		/// </summary>
		/// <param name="row">A row from the blotter table.</param>
		/// <returns>A new Blotter object.</returns>
		protected override BlotterItem New(DataRow row)
		{

			BlotterItem blotterItem;
			EntityRow entityRow;

			if (row is BlotterRow)
				entityRow = (row as BlotterRow).EntityRow;
			else if (row is EntityRow)
				entityRow = row as EntityRow;
			else
				throw new RowNotHandledException("row isn't the right kind of row");

			blotterItem = new BlotterItem() {
				BlotterId = entityRow.EntityId,
				Description = entityRow.IsDescriptionNull()? null : entityRow.Description,
				Name = entityRow.Name,
				TypeId = entityRow.TypeId
			};

			return blotterItem;

		}

		/// <summary>
		/// Update a BlotterItem object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(BlotterItem old, BlotterItem update)
		{

			old.Description = update.Description;
			old.Name = update.Name;
			old.TypeId = update.TypeId;

		}

	}

}

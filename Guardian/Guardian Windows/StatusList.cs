namespace FluidTrade.Guardian.Windows
{

	using System.Collections.Generic;
	using System.Data;

	/// <summary>
	/// Represents a list of StatusItems that is bound to the client data model.
	/// </summary>
	public class StatusList : DataBoundList<StatusItem>
	{

		/// <summary>
		/// The global StatusList
		/// </summary>
		public static readonly StatusList Default = new StatusList();

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static StatusList.CompareItems comparer = new StatusList.CompareItems();

		/// <summary>
		/// Create a new status list.
		/// </summary>
		private StatusList()
		{

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<StatusItem> Comparer
		{
			get { return StatusList.comparer; }
		}

		/// <summary>
		/// The Status table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.Status; }
		}

		/// <summary>
		/// A comparer to compare two items by mnemonic.
		/// </summary>
		private class CompareItems : IComparer<StatusItem>
		{

			/// <summary>
			/// Compare two items.
			/// </summary>
			/// <param name="left">The left item.</param>
			/// <param name="right">The right item.</param>
			/// <returns>The relative order of the item.</returns>
			public int Compare(StatusItem left, StatusItem right)
			{

				return left.Mnemonic.CompareTo(right.Mnemonic);

			}

		}

		/// <summary>
		/// Create a new item from a table row.
		/// </summary>
		/// <param name="dataRow">A row from the status table.</param>
		/// <returns>A new StatusItem object.</returns>
		protected override StatusItem New(DataRow dataRow)
		{

			StatusItem item;
			StatusRow baseRow = dataRow as StatusRow;

			item = new StatusItem()
			{
				Description = baseRow.Description,
				DisabledImage = baseRow.DisabledImage,
				EnabledImage = baseRow.EnabledImage,
				Mnemonic = baseRow.Mnemonic,
				StatusCode = baseRow.StatusCode,
				StatusId = baseRow.StatusId,
			};

			return item;

		}

		/// <summary>
		/// Update a StatusItem object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(StatusItem old, StatusItem update)
		{

			old.Description = update.Description;
			old.DisabledImage = update.DisabledImage;
			old.EnabledImage = update.EnabledImage;
			old.Mnemonic = update.Mnemonic;
			old.StatusCode = update.StatusCode;
			old.StatusId = update.StatusId;

		}

	}

}

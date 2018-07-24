namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Data;

	/// <summary>
	/// A live list of available rights holders.
	/// </summary>
	public class RightsHolderList : DataBoundList<RightsHolder>
	{

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static RightsHolderList.CompareItems comparer = new RightsHolderList.CompareItems();

		private Boolean includeRemovedUsers;
		private Guid? tenant;

		/// <summary>
		/// Create a new list.
		/// </summary>
		public RightsHolderList()
		{

			this.tenant = null;
			this.includeRemovedUsers = false;

			this.InitializeList();

		}

		/// <summary>
		/// Create a new list.
		/// </summary>
		public RightsHolderList(Boolean includeRemovedUsers)
		{

			this.includeRemovedUsers = includeRemovedUsers;

			this.InitializeList();

		}

		/// <summary>
		/// Create a new list.
		/// </summary>
		public RightsHolderList(Boolean includeRemovedUsers, Guid tenant)
		{

			this.includeRemovedUsers = includeRemovedUsers;
			this.tenant = tenant;

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<RightsHolder> Comparer
		{
			get { return RightsHolderList.comparer; }
		}

		/// <summary>
		/// The RightsHolder table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.RightsHolder; }
		}

		/// <summary>
		/// The tenant id of the organization the users are in.
		/// </summary>
		public Guid? Tenant
		{
			get { return this.tenant; }
		}

		/// <summary>
		/// Get whether we should include deleted users in the list.
		/// </summary>
		protected Boolean IncludeRemovedUsers
		{
			get { return this.includeRemovedUsers; }
		}

		/// <summary>
		/// A comparer to compare two users by name.
		/// </summary>
		private class CompareItems : IComparer<RightsHolder>
		{

			/// <summary>
			/// Compare two rights holders.
			/// </summary>
			/// <param name="left">The left rights holder.</param>
			/// <param name="right">The right rights holder.</param>
			/// <returns>The relative order of the rights holders.</returns>
			public int Compare(RightsHolder left, RightsHolder right)
			{

				return left.Name.CompareTo(right.Name);

			}

		}

		/// <summary>
		/// Filter what rows are included in the list based on the value of includeRemovedUsers.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>False if IncludeRemovedUsers is false and the user has been removed; true otherwise.</returns>
		protected override bool Filter(DataRow row)
		{

			RightsHolderRow rightsHolderRow = null;
			UserRow[] userRow;

			if (row is RightsHolderRow)
			{

				rightsHolderRow = row as RightsHolderRow;

			}
			else if (row is EntityRow)
			{

				EntityRow entityRow = row as EntityRow;

				if (entityRow.GetRightsHolderRows().Length != 0)
					rightsHolderRow = entityRow.GetRightsHolderRows()[0];

			}
			else
			{

				throw new RowNotHandledException("row isn't the right kind of row");

			}

			userRow = rightsHolderRow.GetUserRows();

			return this.includeRemovedUsers || userRow.Length == 0 || !userRow[0].IsRemoved;

		}

		/// <summary>
		/// Create a new rights holder from a table row.
		/// </summary>
		/// <param name="row">A row from the rights holder table.</param>
		/// <returns>A new RightsHolder object.</returns>
		protected override RightsHolder New(DataRow row)
		{

			if (row is RightsHolderRow)
				return Entity.New((row as RightsHolderRow).EntityRow) as RightsHolder;
			else if (row is EntityRow)
				return new User(row as EntityRow);
			else
				throw new RowNotHandledException("row isn't the right kind of row");

		}

		/// <summary>
		/// Update a RightsHolder object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(RightsHolder old, RightsHolder update)
		{

			old.Update(update);

		}

	}

}

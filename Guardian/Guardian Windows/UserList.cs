namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Data;
	using FluidTrade.Guardian.Utilities;

	/// <summary>
	/// A live list of available users.
	/// </summary>
	public class UserList : RightsHolderList
	{

		/// <summary>
		/// Create a new list.
		/// </summary>
		public UserList()
		{

		}

		/// <summary>
		/// Create a new list.
		/// </summary>
		public UserList(Boolean includeRemovedUsers, Guid tenant)
			: base(includeRemovedUsers, tenant)
		{

		}

		/// <summary>
		/// The User table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.User; }
		}

		/// <summary>
		/// Filter what rows are included in the list based on the value of includeRemovedUsers.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>False if includeRemovedUsers is false and the user has been removed; true otherwise.</returns>
		protected override bool Filter(DataRow row)
		{

			UserRow userRow = null;
			Boolean include;
			
			if (row is UserRow)
				userRow = row as UserRow;
			else if (row is RightsHolderRow && this.Any(r => r.EntityId == (row as RightsHolderRow).RightsHolderId))
				userRow = (row as RightsHolderRow).GetUserRows()[0];
			else if (row is EntityRow && this.Any(r => r.EntityId == (row as EntityRow).EntityId))
				userRow = (row as EntityRow).GetRightsHolderRows()[0].GetUserRows()[0];
			else
				throw new RowNotHandledException("row isn't the right kind of row");
			
			include = (this.IncludeRemovedUsers || !userRow.IsRemoved) &&
				(this.Tenant == null || (this.Tenant.Value == userRow.RightsHolderRow.TenantId));

			return include;
				

		}

		/// <summary>
		/// Create a new user from a table row.
		/// </summary>
		/// <param name="row">A row from the user table.</param>
		/// <returns>A new User object.</returns>
		protected override RightsHolder New(DataRow row)
		{

			if (row is UserRow)
				return new User((row as UserRow).RightsHolderRow.EntityRow);
			else if (row is RightsHolderRow)
				return new User((row as RightsHolderRow).EntityRow);
			else if (row is EntityRow)
				return new User(row as EntityRow);
			else
				throw new RowNotHandledException("row isn't the right kind of row");

		}

	}

}

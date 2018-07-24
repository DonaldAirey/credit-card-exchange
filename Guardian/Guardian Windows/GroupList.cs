namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Utilities;
	using System.Data;

	/// <summary>
	/// A live list of groups.
	/// </summary>
	public class GroupList : RightsHolderList
	{

		/// <summary>
		/// Create a new list.
		/// </summary>
		public GroupList()
		{

		}

		/// <summary>
		/// Create a new list for groups inside a particular tenant.
		/// </summary>
		public GroupList(Guid tenant)
			: base(false, tenant)
		{

		}

		/// <summary>
		/// The User table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.Group; }
		}

		/// <summary>
		/// Filter what rows are included in the list based on the value of includeRemovedUsers.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>False if includeRemovedUsers is false and the user has been removed; true otherwise.</returns>
		protected override bool Filter(DataRow row)
		{

			GroupRow groupRow = null;
			Boolean include;
			
			if (row is GroupRow)
				groupRow = row as GroupRow;
			else if (row is RightsHolderRow && this.Any(r => r.EntityId == (row as RightsHolderRow).RightsHolderId))
				groupRow = (row as RightsHolderRow).GetGroupRows()[0];
			else if (row is EntityRow && this.Any(r => r.EntityId == (row as EntityRow).EntityId))
				groupRow = (row as EntityRow).GetRightsHolderRows()[0].GetGroupRows()[0];
			
			include =
				(this.Tenant == null || (this.Tenant.Value == groupRow.RightsHolderRow.TenantId));

			return include;
				

		}

		/// <summary>
		/// Create a new group from a table row.
		/// </summary>
		/// <param name="row">A row from the group table.</param>
		/// <returns>A new Group object.</returns>
		protected override RightsHolder New(DataRow row)
		{

			if (row is GroupRow)
				return new Group((row as GroupRow).RightsHolderRow.EntityRow);
			else if (row is RightsHolderRow && this.Any(r => r.EntityId == (row as RightsHolderRow).RightsHolderId))
				return new Group((row as RightsHolderRow).EntityRow);
			else if (row is EntityRow && this.Any(r => r.EntityId == (row as EntityRow).EntityId))
				return new Group(row as EntityRow);
			else
				throw new Exception("row isn't the right kind of row");

		}

	}

}

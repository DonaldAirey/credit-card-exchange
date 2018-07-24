namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Data;

	/// <summary>
	/// A live list of available users.
	/// </summary>
	public class TenantList : DataBoundList<Tenant>
	{

		/// <summary>
		/// Keep a single comparer for all lists.
		/// </summary>
		private static TenantList.CompareItems comparer = new TenantList.CompareItems();

		private Guid? organization = null;

		/// <summary>
		/// Create a new list.
		/// </summary>
		public TenantList()
		{

			this.organization = null;

			this.InitializeList();

		}

		/// <summary>
		/// Create a new list.
		/// </summary>
		public TenantList(Guid organization)
		{

			this.organization = organization;

			this.InitializeList();

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected override IComparer<Tenant> Comparer
		{
			get { return TenantList.comparer; }
		}

		/// <summary>
		/// The Organization table.
		/// </summary>
		protected override DataTable Table
		{
			get { return DataModel.Tenant; }
		}

		/// <summary>
		/// A comparer to compare two organizations by name.
		/// </summary>
		private class CompareItems : IComparer<Tenant>
		{

			/// <summary>
			/// Compare two users.
			/// </summary>
			/// <param name="left">The left organization.</param>
			/// <param name="right">The right organization.</param>
			/// <returns>The relative order of the organizations.</returns>
			public int Compare(Tenant left, Tenant right)
			{

				return left.Name.CompareTo(right.Name);

			}

		}

		/// <summary>
		/// Filter what rows are included in the list.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>False if the organization isn't under the parent organization; true otherwise.</returns>
		protected override bool Filter(DataRow row)
		{

			TenantRow tenantRow = null;
			Boolean include = false;

			if (row is TenantRow)
				tenantRow = row as TenantRow;
			else
				throw new RowNotHandledException("row isn't the right kind of row");

			if (this.organization == null)
				include = true;
			else
				foreach (TenantTreeRow tenantTreeRow in tenantRow.GetTenantTreeRowsByFK_Tenant_TenantTree_ChildId())
					if (this.organization == null || tenantTreeRow.ParentId == this.organization)
						include = true;

			return include;

		}

		/// <summary>
		/// Create a new tenant from a table row.
		/// </summary>
		/// <param name="row">A row from the tenant table.</param>
		/// <returns>A new Tenant object.</returns>
		protected override Tenant New(DataRow row)
		{

			if (row is TenantRow)
				return new Tenant(row as TenantRow);
			else
				throw new RowNotHandledException("row isn't the right kind of row");

		}

		/// <summary>
		/// Update an Tenant object with another.
		/// </summary>
		/// <param name="old">The original object to update.</param>
		/// <param name="update">The new object to update with</param>
		protected override void Update(Tenant old, Tenant update)
		{

			old.Update(update);

		}

	}

}

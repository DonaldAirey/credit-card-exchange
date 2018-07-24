namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Object representing an organization.
	/// </summary>
	public class Tenant : GuardianObject
	{

		private String name;
		private Guid tenantId;
		private MergedList<GuardianObject> organizationsAndUsers = null;
		private GroupList groups = null;
		private UserList users = null;

		/// <summary>
		/// Create a new Organization based on an entity row.
		/// </summary>
		/// <param name="tenantRow">An entity row from the DataModel.</param>
		public Tenant(TenantRow tenantRow) : base()
		{

			this.tenantId = tenantRow.TenantId;
			this.name = tenantRow.Name;

		}

		/// <summary>
		/// Create a new organization based on an existing entity.
		/// </summary>
		/// <param name="tenant">The entity.</param>
		public Tenant(Tenant tenant)
			: base()
		{

			this.tenantId = tenant.tenantId;
			this.name = tenant.Name;

		}

		/// <summary>
		/// The list of groups that are owned by this tenant.
		/// </summary>
		public GroupList Groups
		{

			get
			{

				if (this.groups == null)
					this.groups = new GroupList(this.TenantId);

				return this.groups;

			}

		}

		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		public String Name
		{
			get { return this.name; }
			set
			{
				if (this.name != value)
				{
					this.name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		/// <summary>
		/// The organizations and users that are beneath this organization.
		/// </summary>
		public MergedList<GuardianObject> OrganizationsAndUsers
		{

			get
			{

				if (this.organizationsAndUsers == null)
				{

					this.organizationsAndUsers = new MergedList<GuardianObject>();

					this.organizationsAndUsers.AddList(new TenantList(this.TenantId));
					this.organizationsAndUsers.AddList(new UserList(true, this.TenantId));

				}

				return this.organizationsAndUsers;

			}

		}

		/// <summary>
		/// Primary Identifier of this object.
		/// </summary>
		public Guid TenantId { get { return this.tenantId; } }

		/// <summary>
		/// The list of users owned by the tenant.
		/// </summary>
		public UserList Users
		{

			get
			{

				if (this.users == null)
					this.users = new UserList(false, this.TenantId);

				return this.users;

			}

		}

		/// <summary>
		/// Commit changes to the tenant. Current unsupported.
		/// </summary>
		public override void Commit()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Populate a tenant record with Tenant informat. Currently unsupported.
		/// </summary>
		/// <param name="record">The record.</param>
		protected override void PopulateRecord(FluidTrade.Guardian.TradingSupportReference.BaseRecord record)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Return a string representation of the object.
		/// </summary>
		/// <returns>The name of the tenant.</returns>
		public override string ToString()
		{

			return this.Name;

		}

		/// <summary>
		/// Update this object based on another.
		/// </summary>
		/// <param name="obj">The object to draw updates from.</param>
		public override void Update(GuardianObject obj)
		{

			Tenant tenant = obj as Tenant;

			if (!this.Modified)
			{

				this.Name = tenant.Name;

			}

		}

	}

}

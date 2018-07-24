namespace FluidTrade.Guardian.Windows
{

	using System;

	/// <summary>
	/// An entity that can have access rights to other entities.
	/// </summary>
	public class RightsHolder : Entity
	{

		private Tenant tenant;

		/// <summary>
		/// Create an empty rights holder.
		/// </summary>
		public RightsHolder() : base()
		{

			
		}

		/// <summary>
		/// Create a new rights holder base on an EntityRow
		/// </summary>
		/// <param name="entityRow">The entity to base the rights holder on.</param>
		public RightsHolder(EntityRow entityRow) : base(entityRow)
		{


		}

		/// <summary>
		/// Create a new rights holder based on another one.
		/// </summary>
		/// <param name="entity">The entity to base the new one on.</param>
		public RightsHolder(Entity entity) : base(entity)
		{

			RightsHolder source = entity as RightsHolder;
			
			this.tenant = source.Tenant;

		}
		/// <summary>
		/// The fully qualified name of the rights holder.
		/// </summary>
		public String QualifiedName
		{

			get { return String.Format("{0}@{1}", this.Name, this.Tenant.Name); }
		}

		/// <summary>
		/// Gets the unique identifier of the rights holder.
		/// </summary>
		public Guid RightsHolderId
		{

			get { return this.EntityId; }

		}

		
		/// <summary>
		/// Gets the organization this rights holder belongs to.
		/// </summary>
		public Tenant Tenant
		{

			get { return this.tenant; }

		}

		/// <summary>
		/// Update this entity with contents of another one.
		/// </summary>
		/// <param name="entity">The entity to update from.</param>
		public override void Copy(GuardianObject entity)
		{

			base.Copy(entity);
			
			this.tenant = (entity as RightsHolder).Tenant;

		}

		/// <summary>
		/// Load rights holder specific information.
		/// </summary>
		protected override void FinishLoad()
		{

			RightsHolderRow rightsHolderRow = DataModel.RightsHolder.RightsHolderKey.Find(this.EntityId);

			base.FinishLoad();
			
			tenant = new Tenant(DataModel.Tenant.TenantKey.Find(this.TenantId));		
		}

	}

}

namespace FluidTrade.Guardian.Windows
{

    using System;
    using FluidTrade.Core;
	using System.ServiceModel;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
    /// A user's level of access to another entity.
    /// </summary>
    public class AccessControl
    {

        // Private Instance Fields
		private Guid accessControlId;
        private RightsHolder user;
        private Entity entity;
		private Guid tenantId;
        private AccessRight accessRight;
        private long rowVersion;
        private Boolean dirty;
        private Boolean deleted;
        private Boolean newRights;

        /// <summary>
        /// Create an new AccessControl from a AccessControlRow.
        /// </summary>
        /// <param name="accessControlRow">The row to base this object on.</param>
        public AccessControl(AccessControlRow accessControlRow)
        {

			this.accessControlId = accessControlRow.AccessControlId;
            this.user = Entity.New(accessControlRow.RightsHolderRow.EntityRow) as RightsHolder;
            this.entity = new Entity(accessControlRow.EntityRow);
			this.tenantId = accessControlRow.TenantRow.TenantId;
            this.accessRight = accessControlRow.AccessRightRow.AccessRightCode;
            this.rowVersion = accessControlRow.RowVersion;
            this.dirty = false;
            this.deleted = false;
            this.newRights = false;

        }

		/// <summary>
		/// Create an new AccessControl from a AccessControl.
		/// </summary>
		/// <param name="access">The source object.</param>
		public AccessControl(AccessControl access)
		{

			this.accessControlId = access.AccessControlId;
			this.user = access.User.Clone() as RightsHolder;
			this.entity = new Entity(access.Entity);
			this.tenantId = access.TenantId;
			this.accessRight = access.AccessRight;
			this.rowVersion = access.RowVersion;
			this.dirty = access.Dirty;
			this.deleted = access.Deleted;
			this.newRights = access.newRights;

		}

        /// <summary>
        /// Create a new AccessControl. This AccessControl is automatically dirty.
        /// </summary>
        /// <param name="user">The user who (will) have access.</param>
        /// <param name="entity">The entity that the user has access to.</param>
        /// <param name="accessRight">The rights the user has.</param>
		/// /// <param name="tenant">The tenant id of this user.</param>
        public AccessControl(RightsHolder user, Entity entity, AccessRight accessRight, Guid tenant)
        {

            this.user = user;
            this.entity = entity;
            this.accessRight = accessRight;
            this.dirty = true;
            this.deleted = false;
            this.newRights = true;
			this.tenantId = tenant;

        }

		/// <summary>
		/// The AccessControlId of the row this object is based on.
		/// </summary>
		public Guid AccessControlId
		{

			get { return this.accessControlId; }

		}

		/// <summary>
		/// The rights the user has to the entity.
		/// </summary>
		public AccessRight AccessRight
		{

			get { return this.accessRight; }

		}

		/// <summary>
		/// Whether the user has browse access to the entity.
		/// </summary>
		public Boolean Browse
		{

			get { return (this.accessRight & AccessRight.Browse) == AccessRight.Browse; }
			set
			{

				if (this.Browse != value)
				{

					if (value)
						this.accessRight |= AccessRight.Browse;
					else
						this.accessRight &= ~AccessRight.Browse;
					this.dirty = true;

				}

			}

		}

		/// <summary>
		/// True if the access control is marked for deletion, false otherwise.
		/// </summary>
		public Boolean Deleted
		{

			get { return this.deleted; }
			set { this.deleted = value; }

		}

		/// <summary>
		/// Whether the rights information is dirty. True if there are changes that need to be committed, false if not.
		/// </summary>
		public Boolean Dirty
		{

			get { return this.dirty; }

		}

		/// <summary>
		/// The Entity these access rights refer to.
		/// </summary>
		public Entity Entity
		{

			get { return this.entity; }

		}


		/// <summary>
		/// The Entity these access rights refer to.
		/// </summary>
		public Guid TenantId
		{

			get { return this.tenantId; }

		}


        /// <summary>
        /// Whether the user has execute access to the entity.
        /// </summary>
        public Boolean Execute
        {

            get { return (this.accessRight & AccessRight.Execute) == AccessRight.Execute; }
            set
            {

                if (this.Execute != value)
                {

                    if (value)
                        this.accessRight |= AccessRight.Execute;
                    else
                        this.accessRight &= ~AccessRight.Execute;
                    this.dirty = true;

                }

            }

        }

        /// <summary>
        /// Whether the user has all access rights to the entity.
        /// </summary>
        public Boolean HasFullControl
        {

            get { return this.accessRight == AccessRight.FullControl; }

        }

        /// <summary>
        /// True if this object refers new, uncommit access rights.
        /// </summary>
        public Boolean New
        {

            get { return this.newRights; }

        }

        /// <summary>
        /// Whether the user has read access to the entity.
        /// </summary>
        public Boolean Read
        {

            get { return (this.accessRight & AccessRight.Read) == AccessRight.Read; }
            set
            {

                if (this.Read != value)
                {

                    if (value)
                        this.accessRight |= AccessRight.Read;
                    else
                        this.accessRight &= ~AccessRight.Read;
                    this.dirty = true;

                }

            }

        }

		/// <summary>
		/// The version this object corresponds to.
		/// </summary>
		public long RowVersion
		{

			get { return this.rowVersion; }

		}

		/// <summary>
		/// Whether the user has write access to the entity.
		/// </summary>
		public Boolean Write
		{

			get { return (this.accessRight & AccessRight.Write) == AccessRight.Write; }
			set
			{

				if (this.Write != value)
				{

					if (value)
						this.accessRight |= AccessRight.Write;
					else
						this.accessRight &= ~AccessRight.Write;
					this.dirty = true;

				}

			}

		}

		/// <summary>
		/// The user who has these rights.
		/// </summary>
		public RightsHolder User
		{

			get { return this.user; }

		}

		/// <summary>
		/// Commit any changes to the access control.
		/// </summary>
		public void Commit()
		{

			TradingSupportClient client = new TradingSupportClient(FluidTrade.Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{
				if (this.Deleted)
				{

					// Could be the user added and then deleted a user to the list, so makes sure the row actually exists.
					if (!this.newRights)
					{

						MethodResponseErrorCode response;

						response = client.RevokeAccess(this.User.EntityId, this.Entity.EntityId);

						if (response.Result != ErrorCode.Success)
							if (response.Errors.Length > 0)
								GuardianObject.ThrowErrorInfo(response.Errors[0]);
							else
								GuardianObject.ThrowErrorInfo(response.Result);

					}

				}
				else if (this.Dirty)
				{

					Guid accessRightId;
					MethodResponseguid response;

					lock (DataModel.SyncRoot)
						accessRightId = DataModel.AccessRight.AccessRightKeyAccessRightCode.Find(this.AccessRight).AccessRightId;

					response = client.GrantAccess(this.User.EntityId, this.Entity.EntityId, accessRightId);

					if (response.Errors.Length > 0)
						GuardianObject.ThrowErrorInfo(response.Errors[0]);

					this.accessControlId = response.Result;

					this.newRights = false;
					this.dirty = false;

				}
			}
			finally
			{
				if (client != null && client.State == CommunicationState.Opened)
					client.Close();
			}
		}

		/// <summary>
		/// Determine if this object equal to another AccessControl object.
		/// </summary>
		/// <param name="obj">The other AccessControl.</param>
		/// <returns>True if the two objects are the access rights for the same Entity/User pair.</returns>
		public override Boolean Equals(object obj)
		{

			if (obj is AccessControl)
			{

				AccessControl access = obj as AccessControl;

				return this.User == access.User && this.Entity == access.Entity;

			}

			return false;

		}

		/// <summary>
		/// Get the hash code for this AccessControl object.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{

			return this.User.GetHashCode() ^ this.Entity.GetHashCode();

		}

		/// <summary>
		/// Determine whether a rights holder has particular rights on an entity.
		/// </summary>
		/// <param name="rightsHolder">The rights holder</param>
		/// <param name="entity">The entity.</param>
		/// <param name="right">The required rights.</param>
		/// <returns>True if the rights holder has the specified rights to the entity.</returns>
		public static Boolean HasAccess(Guid rightsHolder, Guid entity, AccessRight right)
		{
			// TODO: Remove this method? Current there are no calls to this method.
			// NOTE: This must be used in a background thread.
			lock (DataModel.SyncRoot)
				return AccessControl.HasAccess(
					DataModel.RightsHolder.RightsHolderKey.Find(rightsHolder),
					DataModel.Entity.EntityKey.Find(entity),
					right);

		}

		/// <summary>
		/// Determine whether a rights holder has particular rights on an entity.
		/// </summary>
		/// <param name="rightsHolderRow">The rights holder</param>
		/// <param name="entity">The entity.</param>
		/// <param name="right">The required rights.</param>
		/// <returns>True if the rights holder has the specified rights to the entity.</returns>
		public static Boolean HasAccess(RightsHolderRow rightsHolderRow, EntityRow entity, AccessRight right)
		{
			// TODO: Remove this method? Current there are no calls to this method.
			// NOTE: This must be used in a background thread.

			Boolean grantedAccess = false;

			lock (DataModel.SyncRoot)
			{
				UserRow userRow = DataModel.User.UserKey.Find(rightsHolderRow.RightsHolderId);

				// See if the rights holder themself has access.
				foreach (AccessControlRow accessControlRow in rightsHolderRow.GetAccessControlRows())
					if (accessControlRow.EntityId == entity.EntityId &&
						(accessControlRow.AccessRightRow.AccessRightCode & right) == right)
						grantedAccess = true;

				if (userRow != null)
					foreach (GroupUsersRow groupUsersRow in userRow.GetGroupUsersRows())
						if (AccessControl.HasAccess(groupUsersRow.GroupRow.RightsHolderRow, entity, right))
							grantedAccess = true;
			}

			return grantedAccess;

		}

        /// <summary>
        /// The AccessControl as represented as a string.
        /// </summary>
        /// <returns>The user's name.</returns>
        public override String ToString()
        {

            return this.user.Name;

        }

        /// <summary>
        /// Update the rights with the rights in the original AccessControlRow. If our rights have been changed, do nothing.
        /// </summary>
        /// <param name="accessControlRow">The row to copy the rights from.</param>
        public void Update(AccessControlRow accessControlRow)
        {

            if (accessControlRow.RowVersion != this.rowVersion
                && accessControlRow.RightsHolderRow.RightsHolderId == user.EntityId
                && accessControlRow.EntityRow.EntityId == entity.EntityId
                && !this.dirty)
            {
                this.accessRight = accessControlRow.AccessRightRow.AccessRightCode;
            }

            this.rowVersion = accessControlRow.RowVersion;

        }

        /// <summary>
        /// Update the rights with the rights in an existing AccessControl. If our rights have been changed, do nothing.
        /// </summary>
        /// <param name="access">The row to copy the rights from.</param>
        public void Update(AccessControl access)
        {

            if (access.RowVersion != this.rowVersion
                && access.User == user
                && access.Entity == entity
                && !this.dirty)
            {

                this.accessRight = access.AccessRight;

            }

            this.rowVersion = access.RowVersion;

        }

    }

}

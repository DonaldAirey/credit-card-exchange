namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.ComponentModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.AdminSupportReference;
	using FluidTrade.Guardian.Properties;
	using System.Threading;
	using System.Windows;
	using System.Windows.Threading;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;

    /// <summary>
	/// A User is a collection of orders.
	/// </summary>
	public class User : RightsHolder
	{

		delegate void SetAdInformationDelegate(Boolean accountDisabled, String emailAddress, Boolean isPasswordExpired, DateTime passwordExpires);

		Boolean accountDisabled;
		Boolean isRemoved;
		String emailAddress;
		String identityName;
		Boolean isPasswordExpired;
		DateTime passwordExpires;
		long rowVersion;
		ObservableCollection<Group> groups;

		/// <summary>
		/// Create a new User object based on an entity row.
		/// </summary>
		/// <param name="entityRow">An entity row from the DataModel.</param>
		public User(EntityRow entityRow) : base(entityRow)
		{

			if (this.groups == null)
				this.groups = new ObservableCollection<Group>();

			this.groups.CollectionChanged += this.OnGroupsChanged;
		
		}

		/// <summary>
		/// Create a new User object based on an existing user entity.
		/// </summary>
		/// <param name="entity">An entity to base the user on.</param>
		public User(Entity entity)
			: base(entity)
		{

			User user = entity as User;

			this.accountDisabled = user.AccountDisabled;
			this.emailAddress = user.EmailAddress;
			this.isRemoved = user.IsRemoved;
			this.identityName = user.IdentityName;
			this.isPasswordExpired = user.IsPasswordExpired;
			this.passwordExpires = user.PasswordExpires;
			this.rowVersion = user.RowVersion;
			this.groups = new ObservableCollection<Group>();

			foreach (Group group in user.Groups)
				this.groups.Add(group);

			this.groups.CollectionChanged += this.OnGroupsChanged;

		}

		/// <summary>
		/// If true, this user account is disabled, and the user cannot login.
		/// </summary>
		public Boolean AccountDisabled
		{

			get { return this.accountDisabled; }
			set
			{

				if (this.accountDisabled != value)
				{

					this.accountDisabled = value;
					this.OnPropertyChanged(new PropertyChangedEventArgs("AccountDisabled"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the email address registered for this user.
		/// </summary>
		public String EmailAddress
		{

			get { return this.emailAddress; }
			set
			{

				if (this.emailAddress != value)
				{

					this.emailAddress = value;
					this.OnPropertyChanged(new PropertyChangedEventArgs("EmailAddress"));

				}

			}

		}

		/// <summary>
		/// The groups that this user belongs to.
		/// </summary>
		public ObservableCollection<Group> Groups
		{

			get { return this.groups; }

		}

		/// <summary>
		/// The default group for this user.
		/// </summary>
		public Group DefaultGroup
		{

			get
			{

				if (this.groups.Count > 0)
					return this.groups[0];
				else
					return null;

			}
			set
			{

				if (this.groups.Count == 0 || this.groups[0] != value && value != null)
				{
	
					if (this.groups.Count > 0)
						this.groups.RemoveAt(0);
					this.groups.Insert(0, value);
					this.OnPropertyChanged(new PropertyChangedEventArgs("DefaultGroup"));

				}

			}

		}

		/// <summary>
		/// If true, the user has been "deleted".
		/// </summary>
		public Boolean IsRemoved
		{

			get { return this.isRemoved; }

		}

		/// <summary>
		/// Gets or sets the identity of the user.
		/// </summary>
		public string IdentityName
		{
			get { return this.identityName; }
			set
			{
				if (this.identityName != value)
				{
					this.identityName = value;
					this.OnPropertyChanged(new PropertyChangedEventArgs("IdentityName"));
				}
			}
		}

		/// <summary>
		/// If true, the user's password is expired and must be reset.
		/// </summary>
		public Boolean IsPasswordExpired
		{

			get { return this.isPasswordExpired; }

		}

		/// <summary>
		/// The date the user's password will expire.
		/// </summary>
		public DateTime PasswordExpires
		{

			get { return this.passwordExpires; }
			set
			{
				if (this.passwordExpires != value)
				{
					this.passwordExpires = value;
					this.OnPropertyChanged(new PropertyChangedEventArgs("PasswordExpires"));
				}
			}
		}

		/// <summary>
		/// The row version of this user.
		/// </summary>
		public new long RowVersion
		{

			get { return this.rowVersion; }

		}

		/// <summary>
		/// The userId of this user.
		/// </summary>
		public Guid UserId
		{
		
			get { return this.EntityId; }

		}

		/// <summary>
		/// Commit any changes to this user to the server.
		/// </summary>
		public override void Commit()
		{

			AdminSupportClient client = new AdminSupportClient(Guardian.Properties.Settings.Default.AdminSupportEndpoint);
			AdminSupportReference.User user = new AdminSupportReference.User();
			MethodResponseErrorCode response;

			this.PopulateRecord(user);

			if (this.Deleted)
			{

				response = client.DeleteUserAccount(user.LookupId);

				if (this.GetFirstErrorCode(response) == ErrorCode.RecordNotFound)
					throw new UserNotFoundException(this, "User not found");

			}
			else
			{

				response = client.UpdateUser(new AdminSupportReference.User[] { user });

				if (this.GetFirstErrorCode(response) == ErrorCode.RecordNotFound)
					throw new UserNotFoundException(this, "User not found");

				if (response.IsSuccessful)
				{

					if (this.AccountDisabled)
						response = client.DisableUserAccount(this.IdentityName);

				}

				if (response.IsSuccessful)
				{

					lock(DataModel.SyncRoot)
					{

						List<Group> newGroups = this.Groups.ToList();
						List<Guid> add = new List<Guid>();
						List<Guid> del = new List<Guid>();
						GroupUsersRow[] oldGroups = DataModel.User.UserKey.Find(this.UserId).GetGroupUsersRows();
						ErrorCode firstError;

						foreach (GroupUsersRow groupUsersRow in oldGroups)
						{

							Group group = newGroups.FirstOrDefault(g => g.GroupId == groupUsersRow.GroupId);

							if (group == null)
							{

								del.Add(groupUsersRow.GroupId);

							}
							else
							{

								if (group.Deleted)
									del.Add(group.GroupId);
								newGroups.Remove(group);

							}

						}

						foreach (Group group in newGroups)
						{

							response = client.AddUserToGroup(this.IdentityName, group.GroupId, this.TenantId);

							firstError = this.GetFirstErrorCode(response);

							if (firstError == ErrorCode.RecordNotFound)
								throw new GroupNotFoundException(this.DefaultGroup, "Group not found");
							else if (firstError != ErrorCode.Success)
								break;

						}

						foreach (Guid group in del)
						{

							response = client.RemoveUserFromGroup(this.IdentityName, group);

							firstError = this.GetFirstErrorCode(response);

							if (firstError != ErrorCode.RecordNotFound && firstError != ErrorCode.Success)
								break;

						}

					}

				}

			}

			if (!response.IsSuccessful)
				GuardianObject.ThrowErrorInfo(response.Errors[0]);

			client.Close();

			this.Modified = false;

		}

		/// <summary>
		/// Update this entity with contents of another one.
		/// </summary>
		/// <param name="entity">The entity to update from.</param>
		public override void Copy(GuardianObject entity)
		{

			User user = entity as User;

			base.Copy(entity);

			this.accountDisabled = user.AccountDisabled;
			this.EmailAddress = user.EmailAddress;
			this.IdentityName = user.IdentityName;
			this.isPasswordExpired = user.IsPasswordExpired;
			this.isRemoved = user.IsRemoved;
			this.passwordExpires = user.PasswordExpires;
			this.rowVersion = user.RowVersion;

			this.groups.Clear();

			foreach (Group group in user.Groups)
				this.groups.Add(group);

		}

		/// <summary>
		/// Create the properties window appropriate for this type.
		/// </summary>
		/// <returns>The properties window.</returns>
		protected override WindowEntityProperties CreatePropertiesWindow()
		{

			return new WindowUserProperties();

		}

		/// <summary>
		/// Load up the user-specific information.
		/// </summary>
		protected override void FinishLoad()
		{
	
			UserRow userRow = DataModel.User.UserKey.Find(this.EntityId);

			base.FinishLoad();

			this.isRemoved = userRow.IsRemoved;
			this.identityName = userRow.IdentityName;
			this.rowVersion = userRow.RowVersion;

			if (this.groups == null)
			{

				this.groups = new ObservableCollection<Group>();
				this.groups.CollectionChanged += this.OnGroupsChanged;
			
			}
			
			foreach (GroupUsersRow groupRow in userRow.GetGroupUsersRows())
				this.groups.Add(Entity.New(groupRow.GroupRow.RightsHolderRow.EntityRow) as Group);

			// The information kept in AD takes a really long time to come back, so we'll grab it in another thread.
			ThreadPoolHelper.QueueUserWorkItem(data => this.LoadAdInformation(data as String), this.IdentityName);

		}

		/// <summary>
		/// Retrieve "extra" information from the server.
		/// </summary>
		/// <param name="identityName">The identity of the user.</param>
		private void LoadAdInformation(String identityName)
		{

			try
			{

				AdminSupportClient client = new AdminSupportClient(Settings.Default.AdminSupportEndpoint);
				MethodResponseUserContextData response = client.FindUserByName(this.identityName);

				if (response.IsSuccessful)
				{

					Application.Current.Dispatcher.BeginInvoke(
							new SetAdInformationDelegate(this.SetAdInformation),
							DispatcherPriority.Normal,
							response.Result.AccountDisabled,
							response.Result.EmailAddress,
							response.Result.IsPasswordExpired,
							response.Result.PasswordExpires);

				}

				client.Close();

			}
			catch (Exception exception)
			{

				EventLog.Information("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// If there's no user row yet, we need to finish loading up the object later.
		/// </summary>
		/// <returns>True if we can't find a user row for the entityId.</returns>
		protected override Boolean NeedLateLoad()
		{

			return DataModel.User.UserKey.Find(this.EntityId) == null;

		}

		/// <summary>
		/// Notify our listeners that our groups changed.
		/// </summary>
		/// <param name="sender">The group list.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnGroupsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
		{

				this.OnPropertyChanged("Groups");

		}

		/// <summary>
		/// Populate a user record with information about this user.
		/// </summary>
		/// <param name="record">The record to populate.</param>
		protected void PopulateRecord(AdminSupportReference.User record)
		{

			record.Description = this.Description;
			record.EmailAddress = this.EmailAddress;
			record.FullName = this.Name;
			record.LookupId = this.IdentityName;
			record.Organization = this.TenantId;
			if (this.DefaultGroup != null)
				record.GroupId = this.DefaultGroup.EntityId;
			record.UserId = this.UserId;

		}

		/// <summary>
		/// Set information from gotten from the Admin call.
		/// </summary>
		/// <param name="accountDisabled">Whether the account is disabled.</param>
		/// <param name="emailAddress">The account's email address.</param>
		/// <param name="isPasswordExpired">Whether the password is expired.</param>
		/// <param name="passwordExpires">When the password expires.</param>
		void SetAdInformation(Boolean accountDisabled, String emailAddress, Boolean isPasswordExpired, DateTime passwordExpires)
		{

			this.accountDisabled = accountDisabled;
			this.emailAddress = emailAddress;
			this.isPasswordExpired = isPasswordExpired;
			this.passwordExpires = passwordExpires;

		}

		/// <summary>
		/// Update the User based on another User.
		/// </summary>
		/// <param name="entity">The Entity to update from.</param>
		public override void Update(GuardianObject entity)
		{

			User user = entity as User;

			base.Update(entity);

			if (!this.Modified && user.EntityId == this.EntityId)
			{

				this.accountDisabled = user.AccountDisabled;
				this.EmailAddress = user.EmailAddress;
				this.IdentityName = user.IdentityName;
				this.isPasswordExpired = user.IsPasswordExpired;
				this.isRemoved = user.IsRemoved;
				this.passwordExpires = user.PasswordExpires;

				this.Groups.Clear();
				foreach (Group group in user.Groups)
					this.groups.Add(group);

			}

			this.rowVersion = user.RowVersion;

		}

		/// <summary>
		/// Update the User based on an EntityRow.
		/// </summary>
		/// <param name="entityRow">The row to update from.</param>
		public override void Update(EntityRow entityRow)
		{

			UserRow userRow = DataModel.User.UserKey.Find(entityRow.EntityId);

			base.Update(entityRow);

			if (userRow != null)
			{

				if (!this.Modified && entityRow.EntityId == this.EntityId)
				{

					this.IdentityName = userRow.IdentityName;
					this.isRemoved = userRow.IsRemoved;

					this.groups.Clear();
					foreach (GroupUsersRow groupRow in userRow.GetGroupUsersRows())
						this.groups.Add(Entity.New(groupRow.GroupRow.RightsHolderRow.EntityRow) as Group);

					// The information kept in AD takes a really long time to come back, so we'll grab it in another thread.
					ThreadPoolHelper.QueueUserWorkItem(data => this.LoadAdInformation(data as String), this.IdentityName);

				}

				this.rowVersion = userRow.RowVersion;
	
			}

		}

	}

}

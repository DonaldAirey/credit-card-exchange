namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Guardian.AdminSupportReference;
	using System.Collections;
	using System.ServiceModel.Security;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// Interaction logic for WindowUserAccounts.xaml
	/// </summary>
	public partial class WindowUserAccounts : Window
	{

		/// <summary>
		/// Indicates the IsExchangeAdmin dependency property.
		/// </summary>
		public static readonly DependencyProperty IsExchangeAdminProperty =
			DependencyProperty.Register("IsExchangeAdminProperty", typeof(Boolean), typeof(WindowUserAccounts), new PropertyMetadata(true));

		private Tenant tenant = null;
		private Boolean canUpdateUser = false;
		private Boolean canApply = false;

		/// <summary>
		/// Create a new user accounts window.
		/// </summary>
		public WindowUserAccounts()
		{

			InitializeComponent();
			this.Loaded += this.OnLoaded;
			this.Unloaded += this.OnUnloaded;

			ThreadPoolHelper.QueueUserWorkItem(data => this.CheckAdminStatus());

		}

		/// <summary>
		/// Determin which users need to be committed shoot them to the background Apply.
		/// </summary>
		public Boolean IsExchangeAdmin
		{
			get { return (Boolean)this.GetValue(WindowUserAccounts.IsExchangeAdminProperty); }
			set { this.SetValue(WindowUserAccounts.IsExchangeAdminProperty, value); }
		}

		/// <summary>
		/// Determin which users need to be committed shoot them to the background Apply.
		/// </summary>
		private void Apply()
		{

			List<User> users = new List<User>();

			this.GetUpdatedUsers(users, this.users.ItemsSource);

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Apply(data as List<User>), users);

		}

		/// <summary>
		/// Apply changes to users.
		/// </summary>
		/// <param name="users">The list of users to commit.</param>
		private void Apply(List<User> users)
		{

			foreach (User user in users)
				try
				{

					user.Commit();

				}
				catch
				{

					this.BackgroundFail(user);

					// Rather than innundate the user with spurious failures and the like, just bail.
					break;

				}

		}

		/// <summary>
		/// Alert the user to any failures and reenabled the apply button.
		/// </summary>
		/// <param name="user">Any exception that should be brought to the user's attention.</param>
		protected void BackgroundFail(User user)
		{

			this.Dispatcher.BeginInvoke(new WaitCallback(delegate(object name)
			{

				MessageBox.Show(
					Application.Current.MainWindow,
					String.Format(FluidTrade.Guardian.Properties.Resources.DeleteFailed, name as String),
					Application.Current.MainWindow.Title);
				this.canApply = true;

			}), DispatcherPriority.Normal, user.Name);

		}

		/// <summary>
		/// Determine whether we can update/edit the selected user.
		/// </summary>
		/// <param name="sender">The originator of the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanUpdateUser(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.canUpdateUser;

		}

		/// <summary>
		/// Determine whether the dialog can be applied or not.
		/// </summary>
		/// <param name="sender">The originator of the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanApply(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.canApply;

		}

		/// <summary>
		/// Determine whether the dialog can be applied or not.
		/// </summary>
		/// <param name="sender">The originator of the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanCreateOrganization(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.addOrganization.Visibility == Visibility.Visible;

		}

		/// <summary>
		/// Determine whether the current user is an exchange administrator.
		/// </summary>
		private void CheckAdminStatus()
		{

			User user = new User(DataModel.Entity.EntityKey.Find(UserContext.Instance.UserId));
			Boolean isAdmin = false;

			foreach (Group group in user.Groups)
				if (group.GroupType == GroupType.ExchangeAdmin)
				{

					isAdmin = true;
					break;

				}
				else if (group.GroupType == GroupType.FluidTradeAdmin)
				{

					isAdmin = true;
					break;

				}

			this.Dispatcher.BeginInvoke(
				new Action(() =>
					this.addOrganization.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed),
				DispatcherPriority.Normal);

		}

		/// <summary>
		/// Determine what level of access (read-only or write) we have to the selected user.
		/// </summary>
		/// <param name="user">The userId of the selected user.</param>
		private void DetermineAccess(Guid user)
		{

			Guid organization;

			lock (DataModel.SyncRoot)
				organization = DataModel.RightsHolder.RightsHolderKey.Find(user).TenantId;

			//Boolean canWrite = AccessControl.HasAccess(Information.UserId, organization, AccessRight.Write);
			Boolean canWrite = true;

			this.Dispatcher.BeginInvoke(new Action(() => this.SetAccess(canWrite)), DispatcherPriority.Normal);

		}

		/// <summary>
		/// Recursively dispose of the databound lists that make up the tenant/user tree.
		/// </summary>
		/// <param name="list">The list to dispose.</param>
		private void DisposeList(MergedList<GuardianObject> list)
		{

			foreach (GuardianObject item in list)
			{

				Tenant tenant = item as Tenant;

				if (tenant != null)
					this.DisposeList(tenant.OrganizationsAndUsers);

			}

			list.Dispose();

		}

		/// <summary>
		/// Get a list of modified users.
		/// </summary>
		/// <param name="users"></param>
		/// <param name="list"></param>
		private void GetUpdatedUsers(List<User> users, IEnumerable list)
		{

			foreach (GuardianObject obj in list)
				if (obj is Tenant)
					GetUpdatedUsers(users, (obj as Tenant).OrganizationsAndUsers);
				else if (obj is User && obj.Deleted)
					users.Add(obj as User);

		}

		/// <summary>
		/// Populate the dialog box for the first time.
		/// </summary>
		/// <param name="userId">The userId of the current user.</param>
		private void Initialize(Guid userId)
		{

			Tenant organization = null;

			lock (DataModel.SyncRoot)
			{

				UserRow userRow = DataModel.User.UserKey.Find(userId);
				organization = new Tenant(userRow.RightsHolderRow.TenantRow);

			}

			this.Dispatcher.BeginInvoke(new Action(delegate()
			{
				organization.OrganizationsAndUsers.Initialized += (s, e) =>
					this.Dispatcher.BeginInvoke(new Action(() => this.Cursor = Cursors.Arrow), DispatcherPriority.Normal);
				this.tenant = organization;
				this.users.ItemsSource = this.tenant.OrganizationsAndUsers;
			}),
				DispatcherPriority.Normal);

		}

		/// <summary>
		/// Handle the Apply command.
		/// </summary>
		/// <param name="sender">The Apply button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnApply(object sender, EventArgs eventArgs)
		{

			if (this.canApply)
				this.Apply();
			this.canApply = false;

		}

		/// <summary>
		/// Handle the Advanced command.
		/// </summary>
		/// <param name="sender">The Advanced button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnAdvanced(object sender, EventArgs eventArgs)
		{

			WindowUserManager window = new WindowUserManager();

			window.Show();

		}

		/// <summary>
		/// Handle the Cancel command.
		/// </summary>
		/// <param name="sender">The Cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCancel(object sender, EventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Bring up the create organization dialog.
		/// </summary>
		/// <param name="sender">The "Create organization" button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCreateOrganization(object sender, EventArgs eventArgs)
		{

			WindowAddOrganization dialog = new WindowAddOrganization();

			dialog.ShowDialog();

		}

		/// <summary>
		/// Handle the Delete command. Remove the selected user.
		/// </summary>
		/// <param name="sender">The Remove button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnDelete(object sender, EventArgs eventArgs)
		{

			(this.users.SelectedItem as User).Deleted = true;
			this.canApply = true;

		}

		/// <summary>
		/// Handle the loaded event.
		/// </summary>
		/// <param name="sender">This dialog box.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs eventArgs)
		{

			this.Cursor = Cursors.AppStarting;
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Initialize(UserContext.Instance.UserId));

		}

		/// <summary>
		/// Handle the New command.
		/// </summary>
		/// <param name="sender">The Add button.</param>
		/// <param name="e">The event arguments.</param>
		private void OnNew(object sender, RoutedEventArgs e)
		{

			WindowAddUser addUser = new WindowAddUser();

			if (this.users.SelectedItem is User)
				addUser.Tenant = (this.users.SelectedItem as User).TenantId;
			else if (this.users.SelectedItem is Tenant)
				addUser.Tenant = (this.users.SelectedItem as Tenant).TenantId;
			else
				addUser.Tenant = this.tenant.TenantId;
			addUser.Owner = this;
			addUser.ShowDialog();

		}

		/// <summary>
		/// Handle the Okay command.
		/// </summary>
		/// <param name="sender">The OK button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOkay(object sender, EventArgs eventArgs)
		{

			if (this.canApply)
				this.Apply();
			this.canApply = false;
			this.Close();

		}

		/// <summary>
		/// Handle the Properties command.
		/// </summary>
		/// <param name="sender">The Properties button.</param>
		/// <param name="e">The event arguments.</param>
		private void OnProperties(object sender, RoutedEventArgs e)
		{

			WindowBasicUserProperties properties = new WindowBasicUserProperties();

			properties.User = this.users.SelectedItem as User;
			properties.Owner = this;
			properties.ShowDialog();

		}

		/// <summary>
		/// Handle the ResetPassword command.
		/// </summary>
		/// <param name="sender">The Reset Password button.</param>
		/// <param name="e">The event arguments.</param>
		private void OnResetPassword(object sender, RoutedEventArgs e)
		{

			WindowResetPassword reset = new WindowResetPassword();

			reset.User = (this.users.SelectedItem as User);
			reset.Owner = this;
			reset.ShowDialog();

		}

		/// <summary>
		/// Handle the Unloaded event.
		/// </summary>
		/// <param name="sender">This dialog box.</param>
		/// <param name="e">The event arguments.</param>
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{

			if (this.users.ItemsSource != null)
				this.DisposeList(this.users.ItemsSource as MergedList<GuardianObject>);

		}

		/// <summary>
		/// Handle the selected user changing.
		/// </summary>
		/// <param name="sender">The users list.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnUsersSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> eventArgs)
		{

			// Disable the editing buttons until we know we can use them.
			this.SetAccess(false);

			if (eventArgs.NewValue is User)
			{

				this.password.DataContext = eventArgs.NewValue;
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.DetermineAccess((Guid)data), (eventArgs.NewValue as User).UserId);

			}
			else
			{

				this.password.DataContext = null;

			}

		}

		/// <summary>
		/// Retrieve the list of users to populate the users list with.
		/// </summary>
		private void Populate()
		{

			IEnumerable<UserRow> organizationUsers = DataModel.User;
			List<User> users = organizationUsers.Select(row => new User(row.RightsHolderRow.EntityRow)).ToList();

			this.Dispatcher.BeginInvoke(new Action(() => this.Populate(users)), DispatcherPriority.Normal);

		}

		/// <summary>
		/// When changes to the DataModel occur, re-populate the users list.
		/// </summary>
		/// <param name="sender">The DataModel.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void Populate(object sender, EventArgs eventArgs)
		{

			this.Populate();

		}

		/// <summary>
		/// Populate the users list with the list of available users.
		/// </summary>
		/// <param name="users">The list of users.</param>
		private void Populate(List<User> users)
		{

			this.userIcon.Source = null;
			this.users.ItemsSource = users;

			if (users.Count > 0)
			{

				//this.users.SelectedItem = this.users.Items[0];
				this.userIcon.SetBinding(Image.SourceProperty, new Binding("SelectedItem.ImageSource") { Source = this.users });

			}

			this.Cursor = Cursors.Arrow;
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.DetermineAccess((Guid)data), (this.users.SelectedItem as User).UserId);

		}

		/// <summary>
		/// Remove a user.
		/// </summary>
		/// <param name="user">The user to remove.</param>
		private void RemoveUser(User user)
		{

			try
			{

				AdminSupportClient adminSupportClient = new AdminSupportClient(Guardian.Properties.Settings.Default.AdminSupportEndpoint);

				adminSupportClient.DisableUserAccount(user.IdentityName);
				adminSupportClient.Close();

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(this, String.Format(Properties.Resources.DeleteUserFailed, user), this.Title)));

			}

		}

		/// <summary>
		/// Set whether we can make updates to the selected user.
		/// </summary>
		/// <param name="canWrite">True if we can update the user, false otherwise.</param>
		private void SetAccess(Boolean canWrite)
		{

			this.canUpdateUser = canWrite;

			CommandManager.InvalidateRequerySuggested();

		}

	}

}

namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Threading;
	using System.Windows;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Guardian.TradingSupportReference;
	using System.Windows.Data;
	using System.ServiceModel.Security;

	/// <summary>
	/// Interaction logic for WindowUserProperties.xaml
	/// </summary>
	public partial class WindowBasicUserProperties : Window
	{

		/// <summary>
		/// Indicates the User dependency property.
		/// </summary>
		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register("User", typeof(User), typeof(WindowBasicUserProperties), new PropertyMetadata(null, OnUserChanged, CoerceUser));
		/// <summary>
		/// Indicates the CanUpdateUser dependency property.
		/// </summary>
		public static readonly DependencyPropertyKey CanUpdateUserProperty =
			DependencyProperty.RegisterReadOnly("CanUpdateUser", typeof(Boolean), typeof(WindowBasicUserProperties), new PropertyMetadata(false));

		private Boolean canUpdateUser = false;
		private Boolean canApply = false;

		/// <summary>
		/// Create a new user properties window.
		/// </summary>
		public WindowBasicUserProperties()
		{
	
			InitializeComponent();
			this.Loaded += OnLoaded;
//			this.SetBinding(WindowUserProperties.RootProperty, new Binding("SelectedValue") { Source = this.selector });
		
		}

		/// <summary>
		/// Whether we have permission to write to this user.
		/// </summary>
		public Boolean CanUpdateUser
		{

			get { return true; }
			//get { return this.canUpdateUser; }

		}

		/// <summary>
		/// The user whose properties we're displaying/editing.
		/// </summary>
		public User User
		{

			get { return this.GetValue(WindowBasicUserProperties.UserProperty) as User; }
			set { this.SetValue(WindowBasicUserProperties.UserProperty, value); }

		}

		/// <summary>
		/// Apply changes to the user.
		/// </summary>
		/// <param name="user">The user to commit.</param>
		private void Apply(User user)
		{

			try
			{

				Guid folderId = Guid.Empty;

				user.Commit();

			}
			catch (Exception exception)
			{

				this.BackgroundFail(exception);

			}

		}

		/// <summary>
		/// Alert the user to any failures and reenabled the apply button.
		/// </summary>
		/// <param name="exception">Any exception that should be brought to the user's attention.</param>
		private void BackgroundFail(Exception exception)
		{

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
			{

				if (exception is UserNotFoundException)
					MessageBox.Show(
						this,
						String.Format(Properties.Resources.UserNotFound, (exception as UserNotFoundException).User),
						this.Title);
				else if (exception is GroupNotFoundException)
					MessageBox.Show(
						this,
						String.Format(Properties.Resources.GroupNotFound, (exception as GroupNotFoundException).Group),
						this.Title);
				else if (exception is SecurityAccessDeniedException)
					MessageBox.Show(
						this,
						String.Format(Properties.Resources.UpdateUserFailedAccessDenied, this.User),
						this.Title);
				else
					MessageBox.Show(
						this,
						String.Format(Properties.Resources.UpdateUserFailed, this.User),
						this.Title);
				this.canApply = true;

			}));

		}

		/// <summary>
		/// Determine whether the apply button can be pressed.
		/// </summary>
		/// <param name="sender">The originator of the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void CanApply(object sender, CanExecuteRoutedEventArgs eventArgs)
		{

			eventArgs.CanExecute = this.canApply;

		}

		/// <summary>
		/// Make sure we're using a seperate user than our owner.
		/// </summary>
		/// <param name="sender">The properties window.</param>
		/// <param name="newUser">The value of user set by the owning window.</param>
		/// <returns></returns>
		private static object CoerceUser(DependencyObject sender, object newUser)
		{

			return (newUser as User).Clone();

		}

		/// <summary>
		/// Determine what level of access (read-only or write) we have to the selected user.
		/// </summary>
		/// <param name="userId">The userId of the selected user.</param>
		private void Populate(Guid userId)
		{

			lock (DataModel.SyncRoot)
			{

				Boolean canWrite = false;
				AccessControlRow accessControlRow = DataModel.AccessControl.AccessControlKeyRightsHolderIdEntityId.Find(UserContext.Instance.UserId, userId);
				UserRow userRow = DataModel.User.UserKey.Find(userId);
				List<Group> groups = new List<Group>();
				Group admin = null;
				Group user = null;

				if (accessControlRow != null)
				{
					AccessRightRow accessRightRow = accessControlRow.AccessRightRow;
					canWrite = (accessRightRow.AccessRightCode & AccessRight.Write) == AccessRight.Write;
				}

				foreach (GroupRow groupRow in DataModel.Group.Where(row => row.RightsHolderRow.TenantId == userRow.TenantId))
				{

					Group group = Entity.New(groupRow.RightsHolderRow.EntityRow) as Group;

					if (group.GroupType == GroupType.FluidTradeAdmin ||
							(admin == null || admin.GroupType == GroupType.SiteAdmin) && group.GroupType == GroupType.ExchangeAdmin ||
							admin == null && group.GroupType == GroupType.SiteAdmin)
						admin = group;

					if (group.GroupType == GroupType.User)
						user = group;

					groups.Add(group);

				}
	
				this.Dispatcher.BeginInvoke(new Action(() => this.Populate(canWrite, admin, user)), DispatcherPriority.Normal);

			}

		}

		/// <summary>
		/// Set whether we can make updates to the selected user.
		/// </summary>
		/// <param name="canWrite">True if we can update the user, false otherwise.</param>
		/// <param name="admin">The admin group available for this user.</param>
		/// <param name="user">The users group available for this user.</param>
		private void Populate(Boolean canWrite, Group admin, Group user)
		{

			this.canUpdateUser = canWrite;
			this.SetValue(WindowBasicUserProperties.CanUpdateUserProperty, this.canUpdateUser);

		}

		/// <summary>
		/// Handle the Apply command.
		/// </summary>
		/// <param name="sender">The Apply button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnApply(object sender, RoutedEventArgs eventArgs)
		{

			if (this.canApply)
			{

				User user = this.User.Clone() as User;

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Apply(user));

			}
			this.canApply = false;

		}

		/// <summary>
		/// Handle the Cancel command. We don't need to do anything, really. Just close the window.
		/// </summary>
		/// <param name="sender">The Cancel button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnCancel(object sender, RoutedEventArgs eventArgs)
		{

			this.Close();

		}

		/// <summary>
		/// Handle the loaded event.
		/// </summary>
		/// <param name="sender">This dialog box.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs eventArgs)
		{

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Populate((Guid)data), this.User.UserId);

		}

		/// <summary>
		/// Handle the Okay command.
		/// </summary>
		/// <param name="sender">The Okay button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnOkay(object sender, RoutedEventArgs eventArgs)
		{

			this.OnApply(sender, eventArgs);
			this.Close();

		}

		/// <summary>
		/// Allow apply/okay after the root changes.
		/// </summary>
		/// <param name="sender">The properties window whose Root value changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnRootChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			(sender as WindowBasicUserProperties).canApply = true;

		}

		/// <summary>
		/// Handle the user changing.
		/// </summary>
		/// <param name="sender">The window whose user changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnUserChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			(sender as WindowBasicUserProperties).OnUserChanged(eventArgs);

		}

		/// <summary>
		/// Handle the user changing.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnUserChanged(DependencyPropertyChangedEventArgs eventArgs)
		{

			if (this.User != null)
				this.User.PropertyChanged -= this.OnUserPropertyChanged;
			this.DataContext = this.User;
			this.User.PropertyChanged += this.OnUserPropertyChanged;

		}

		/// <summary>
		/// Handle a change in the user.
		/// </summary>
		/// <param name="sender">The user.</param>
		/// <param name="e">The event arguments.</param>
		void OnUserPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{

			this.canApply = true;

		}

	}

}

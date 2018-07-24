namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Data;
	using FluidTrade.Core;
	using System.Windows.Controls;
	using System.Windows.Data;
	using FluidTrade.Actipro;
	using FluidTrade.Guardian.Windows.Controls;
	using System.Windows.Input;
	using System.Collections.Specialized;
	using System.Diagnostics;

	/// <summary>
	/// Properties for users.
	/// </summary>
	public class WindowUserProperties : WindowEntityProperties
	{

		/// <summary>
		/// Identifies the User dependency property.
		/// </summary>
		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register("User", typeof(User), typeof(WindowUserProperties), new PropertyMetadata(null));

		private RightsHolderListBox groups = new RightsHolderListBox();
		
		// Background Fields:
		private Guid entityId;

        /// <summary>
        /// Create a new properties dialog box.
        /// </summary>
		public WindowUserProperties()
            : base()
        {

			this.SetBinding(WindowUserProperties.UserProperty,
				new Binding("Entity") { Source = this, Converter = new IdentityConverter() });
			this.SetBinding(WindowUserProperties.DataContextProperty, new Binding("User") { Source = this });

			this.BuildCustomizeGroup();
			this.BuildMemberOfTab();

			this.Loaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				this.groups.ItemsSource = (this.Entity as User).Groups;
				DataModel.RightsHolder.RowChanging += this.FilterRow;
				DataModel.User.RowChanging += this.FilterRow;
				DataModel.GroupUsers.RowChanging += this.FilterRow;
				DataModel.GroupUsers.RowDeleting += this.FilterRow;
			};
			this.Unloaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				DataModel.RightsHolder.RowChanging -= this.FilterRow;
				DataModel.User.RowChanging -= this.FilterRow;
				DataModel.GroupUsers.RowChanging -= this.FilterRow;
				DataModel.GroupUsers.RowDeleting -= this.FilterRow;
			};

        }

		/// <summary>
		/// The user whose properties we will display.
		/// </summary>
		public User User
		{

			get { return this.GetValue(WindowUserProperties.UserProperty) as User; }
			set { this.SetValue(WindowUserProperties.UserProperty, value); }

		}

		/// <summary>
		/// Build the user customization box.
		/// </summary>
		private void BuildCustomizeGroup()
		{

			GroupBox box = new GroupBox() { Header = "Acount Information", Padding = new Thickness(5) };
			StackPanel panel = new StackPanel();
			DockPanel expiresPanel = new DockPanel();
			CheckBox mustChangePassword = new CheckBox() { Content = "User must change password at next logon", IsEnabled = false, Margin = new Thickness(0,3,0,3) };
			TextBlock expiresText = new TextBlock() { Text = "Password expires:", Margin = new Thickness(0,0,3,0) };
			DialogDatePicker passwordExpires = new DialogDatePicker() { Margin = new Thickness(0,3,0,3) };
			CheckBox accountDisabled = new CheckBox() { Content = "Account is disabled", Margin = new Thickness(0, 3, 0, 3) };

			mustChangePassword.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsPasswordExpired") { Mode = BindingMode.OneWay });
			passwordExpires.SetBinding(DialogDatePicker.DateTimeProperty, new Binding("PasswordExpires") { Mode=BindingMode.TwoWay });
			accountDisabled.SetBinding(CheckBox.IsCheckedProperty, new Binding("AccountDisabled"));
			expiresText.SetValue(DockPanel.DockProperty, Dock.Left);
			passwordExpires.SetValue(DockPanel.DockProperty, Dock.Right);
			expiresPanel.Children.Add(expiresText);
			expiresPanel.Children.Add(passwordExpires);

			panel.Children.Add(expiresPanel);
			panel.Children.Add(mustChangePassword);
			panel.Children.Add(accountDisabled);

			box.Content = panel;

			this.customizePanel.Children.Add(box);

		}

		/// <summary>
		/// Create the tab containing the groups this user is a member of.
		/// </summary>
		private void BuildMemberOfTab()
		{

			UserMembersTab memberOf = new UserMembersTab() { Owner = this };

			memberOf.SetBinding(UserMembersTab.UserProperty, new Binding("User") { Source = this });
			this.tabControl.Items.Add(memberOf);

		}

		/// <summary>
		/// Determine whether a row is one of the rows being displayed by the window.
		/// </summary>
		/// <param name="sender">The table that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void FilterRow(object sender, EventArgs eventArgs)
		{

			DataRowAction action = eventArgs is DataTableNewRowEventArgs? DataRowAction.Add : (eventArgs as DataRowChangeEventArgs).Action;
			DataRow row = eventArgs is DataRowChangeEventArgs ? (eventArgs as DataRowChangeEventArgs).Row : (eventArgs as DataTableNewRowEventArgs).Row;

			if (!(row.RowState == DataRowState.Detached || row.RowState == DataRowState.Deleted))
				if (row is UserRow && (row as UserRow).UserId == this.entityId)
					this.MustRedisplay = true;
				else if (row is RightsHolderRow && (row as RightsHolderRow).RightsHolderId == this.entityId)
					this.MustRedisplay = true;
				else if (row is GroupUsersRow && (row as GroupUsersRow).UserId == this.entityId)
					this.MustRedisplay = true;

		}

		/// <summary>
		/// Handle the Entity changing.
		/// </summary>
		protected override void OnEntityChanged()
		{

			base.OnEntityChanged();

			(this.Entity as User).Groups.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs eventArgs)
			{
				if (!this.Populating)
					this.CanApply = true;
			};

			ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
			{
				lock (DataModel.SyncRoot)
					this.entityId = (Guid)data;
			},
				this.Entity.EntityId);

		}

	}

}

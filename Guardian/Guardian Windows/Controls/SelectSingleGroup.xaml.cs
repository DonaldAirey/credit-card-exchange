namespace FluidTrade.Guardian.Windows.Controls
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Threading;
	using FluidTrade.Core;

	/// <summary>
	/// Interaction logic for SelectSingleGroup.xaml
	/// </summary>
	public partial class SelectSingleGroup : UserControl
	{

		/// <summary>
		/// The command that fires when the User group is selected.
		/// </summary>
		public static readonly RoutedCommand UserCommand = new RoutedCommand("UserCommand", typeof(SelectSingleGroup));
		/// <summary>
		/// The command that fires when the Admin group is selected.
		/// </summary>
		public static readonly RoutedCommand AdminCommand = new RoutedCommand("AdminCommand", typeof(SelectSingleGroup));
		/// <summary>
		/// The command that fires when an Other group is selected.
		/// </summary>
		public static readonly RoutedCommand OtherCommand = new RoutedCommand("OtherCommand", typeof(SelectSingleGroup));

		/// <summary>
		/// Indicates the Group dependency property.
		/// </summary>
		public static readonly DependencyProperty GroupProperty = DependencyProperty.Register("Group", typeof(Group), typeof(SelectSingleGroup), new PropertyMetadata(OnGroupChanged));
		/// <summary>
		/// Indicates the Tenant dependency property.
		/// </summary>
		public static readonly DependencyProperty TenantProperty = DependencyProperty.Register(
			"Tenant", typeof(Guid),
			typeof(SelectSingleGroup),
			new PropertyMetadata(SelectSingleGroup.OnTenantChanged));

		/// <summary>
		/// Create a new group selection control.
		/// </summary>
		public SelectSingleGroup()
		{

			InitializeComponent();
			this.Unloaded += OnUnloaded;

		}

		/// <summary>
		/// The user whose properties we're displaying/editing.
		/// </summary>
		public Group Group
		{

			get { return this.GetValue(SelectSingleGroup.GroupProperty) as Group; }
			set { this.SetValue(SelectSingleGroup.GroupProperty, value); }

		}

		/// <summary>
		/// The entity id of the organization whose users to manage.
		/// </summary>
		public Guid Tenant
		{

			get { return (Guid)this.GetValue(SelectSingleGroup.TenantProperty); }
			set { this.SetValue(SelectSingleGroup.TenantProperty, value); }

		}

		/// <summary>
		/// Handle the Admin command. Find the relevent admin group and select it.
		/// </summary>
		/// <param name="sender">The Administrator radio button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnAdminCommand(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			foreach (Group group in this.otherGroups.Items)
				if (group.GroupType == GroupType.ExchangeAdmin || group.GroupType == GroupType.FluidTradeAdmin || group.GroupType == GroupType.SiteAdmin)
				{

					this.otherGroups.PersistentSelectedValue = group.GroupId;
					break;

				}

		}

		/// <summary>
		/// Handle the group changing.
		/// </summary>
		/// <param name="sender">The SelectSingleGroup whose Group has changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnGroupChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			SelectSingleGroup select = sender as SelectSingleGroup;

			if (select.Group != null)
				if (select.Group.GroupType == GroupType.User && select.Group.TenantId == select.Tenant)
					select.standardUser.IsChecked = true;
				else if (select.Group.GroupType != GroupType.Custom && select.Group.TenantId == select.Tenant)
					select.administrator.IsChecked = true;
				else
					select.other.IsChecked = true;

		}

		/// <summary>
		/// Handle the organization changing by repopulating the list of available groups.
		/// </summary>
		/// <param name="sender">This.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnTenantChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			SelectSingleGroup group = sender as SelectSingleGroup;
			GroupList groupList = new GroupList((Guid)eventArgs.NewValue);

			groupList.Initialized += (s, e) =>
				group.Populate(groupList);

			if (group.otherGroups.ItemsSource != null)
				(group.otherGroups.ItemsSource as GroupList).Dispose();

			group.otherGroups.ItemsSource = groupList;

		}

		/// <summary>
		/// Dispose of disposable things on unload.
		/// </summary>
		/// <param name="sender">The control.</param>
		/// <param name="e">The event arguments.</param>
		private void OnUnloaded(object sender, RoutedEventArgs e)
		{

			GroupList groupList = this.otherGroups.ItemsSource as GroupList;

			if (groupList != null)
				groupList.Dispose();

		}

		/// <summary>
		/// Handle the User command. Find the relevent user group and select it.
		/// </summary>
		/// <param name="sender">The User radio button.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnUserCommand(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			foreach (Group group in this.otherGroups.Items)
				if (group.GroupType == GroupType.User)
				{

					this.otherGroups.PersistentSelectedValue = group.GroupId;
					break;

				}

		}

		/// <summary>
		/// Populate the other groups dropdown and select the relevent User group.
		/// </summary>
		/// <param name="groups">The available groups.</param>
		private void Populate(GroupList groups)
		{

//			this.otherGroups.ItemsSource = groups;
			if (this.Group == null)
				this.otherGroups.PersistentSelectedValue = groups.FirstOrDefault(g => (g as Group).GroupType == GroupType.User).EntityId;
			else
				if (this.Group.GroupType == GroupType.User && this.Group.TenantId == this.Tenant)
					this.standardUser.IsChecked = true;
				else if (this.Group.GroupType != GroupType.Custom && this.Group.TenantId == this.Tenant)
					this.administrator.IsChecked = true;
				else
					this.other.IsChecked = true;

		}

	}

}

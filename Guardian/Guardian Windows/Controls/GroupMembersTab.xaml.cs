namespace FluidTrade.Guardian.Windows.Controls
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using FluidTrade.Guardian.Utilities;
	using FluidTrade.Core;

	/// <summary>
	/// Interaction logic for GroupMembersTab.xaml
	/// </summary>
	public partial class GroupMembersTab : TabItem
	{

		/// <summary>
		/// Indicates the Group dependency property.
		/// </summary>
		public static readonly DependencyProperty GroupProperty =
			DependencyProperty.Register("Group", typeof(Group), typeof(GroupMembersTab), new PropertyMetadata(null, OnGroupChanged));
		/// <summary>
		/// Indicates the Owner dependency property.
		/// </summary>
		public static readonly DependencyProperty OwnerProperty =
			DependencyProperty.Register("Owner", typeof(Window), typeof(GroupMembersTab), new PropertyMetadata(null));

		private Guid groupId;

		/// <summary>
		/// Create a new Member Of tab.
		/// </summary>
		public GroupMembersTab()
		{

			InitializeComponent();

		}

		/// <summary>
		/// The Group whose properties we're displaying/editing.
		/// </summary>
		public Group Group
		{

			get { return this.GetValue(GroupMembersTab.GroupProperty) as Group; }
			set { this.SetValue(GroupMembersTab.GroupProperty, value); }

		}

		/// <summary>
		/// The window that owns the tab.
		/// </summary>
		public Window Owner
		{

			get { return this.GetValue(GroupMembersTab.OwnerProperty) as Window; }
			set { this.SetValue(GroupMembersTab.OwnerProperty, value); }

		}

		/// <summary>
		/// Handle the Delete command being executed.
		/// </summary>
		/// <param name="sender">The object the properties command was executed by.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnDelete(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			//foreach (object item in this.groups.SelectedItems)
			//	(item as Group).Deleted = true;

		}

		/// <summary>
		/// When the find-user window closes, grab the rights holder that was selected and add it.
		/// </summary>
		/// <param name="sender">The find window.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnFindUserClose(object sender, EventArgs eventArgs)
		{

			WindowFindUser dialog = sender as WindowFindUser;

			if (dialog.SelectedUser != null)
			{
/*
				if (this.User.Groups.Contains(dialog.SelectedUser as Group))
					this.User.Groups.FirstOrDefault(g => g.Equals(dialog.SelectedUser)).Deleted = false;
				else
					this.User.Groups.Add(dialog.SelectedUser as Group);
*/
			}

			Win32Interop.EnableWindow(this.Owner);
			this.Owner.Activate();

		}

		/// <summary>
		/// Handle the New command being executed.
		/// </summary>
		/// <param name="sender">The object the properties command was executed by.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnNew(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			WindowFindUser dialog = new WindowFindUser() { IncludeGroups = false };

			dialog.Owner = this.Owner;
			dialog.Closed += this.OnFindUserClose;
			dialog.Show();

			Win32Interop.DisableWindow(this.Owner);

		}

		/// <summary>
		/// Handle the Group changing.
		/// </summary>
		/// <param name="sender">The window whose Group changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnGroupChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			(sender as GroupMembersTab).OnGroupChanged(eventArgs);

		}

		/// <summary>
		/// Handle the Group changing.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnGroupChanged(DependencyPropertyChangedEventArgs eventArgs)
		{

			ThreadPoolHelper.QueueUserWorkItem(delegate(object guid)
				{
					lock (DataModel.SyncRoot)
						this.groupId = (Guid)guid;
				}, this.Group.GroupId);

			this.users.ItemsSource = new UserList() { FilterMethod = u => (u as User).Groups.Any(g => g.GroupId == this.groupId) };

		}

	}

}

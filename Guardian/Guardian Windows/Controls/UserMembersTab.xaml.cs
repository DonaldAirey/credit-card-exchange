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

	/// <summary>
	/// Interaction logic for UserMembersTab.xaml
	/// </summary>
	public partial class UserMembersTab : TabItem
	{

		/// <summary>
		/// Indicates the User dependency property.
		/// </summary>
		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register("User", typeof(User), typeof(UserMembersTab), new PropertyMetadata(null, OnUserChanged));
		/// <summary>
		/// Indicates the Owner dependency property.
		/// </summary>
		public static readonly DependencyProperty OwnerProperty =
			DependencyProperty.Register("Owner", typeof(Window), typeof(UserMembersTab), new PropertyMetadata(null));

		/// <summary>
		/// Create a new Member Of tab.
		/// </summary>
		public UserMembersTab()
		{
			InitializeComponent();
		}

		/// <summary>
		/// The user whose properties we're displaying/editing.
		/// </summary>
		public User User
		{

			get { return this.GetValue(UserMembersTab.UserProperty) as User; }
			set { this.SetValue(UserMembersTab.UserProperty, value); }

		}

		/// <summary>
		/// The window that owns the tab.
		/// </summary>
		public Window Owner
		{

			get { return this.GetValue(UserMembersTab.OwnerProperty) as Window; }
			set { this.SetValue(UserMembersTab.OwnerProperty, value); }

		}

		/// <summary>
		/// Handle the Delete command being executed.
		/// </summary>
		/// <param name="sender">The object the properties command was executed by.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnDelete(object sender, ExecutedRoutedEventArgs eventArgs)
		{

			while(this.groups.SelectedItems.Count > 0)
				this.User.Groups.Remove(this.groups.SelectedItems[0] as Group);

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

				if (this.User.Groups.Contains(dialog.SelectedUser as Group))
					this.User.Groups.FirstOrDefault(g => g.Equals(dialog.SelectedUser)).Deleted = false;
				else
					this.User.Groups.Add(dialog.SelectedUser as Group);

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

			WindowFindUser dialog = new WindowFindUser() { IncludeUsers = false };

			dialog.Owner = this.Owner;
			dialog.Closed += this.OnFindUserClose;
			dialog.Show();

			Win32Interop.DisableWindow(this.Owner);

		}

		/// <summary>
		/// Handle the user changing.
		/// </summary>
		/// <param name="sender">The window whose user changed.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnUserChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			(sender as UserMembersTab).OnUserChanged(eventArgs);

		}

		/// <summary>
		/// Handle the user changing.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnUserChanged(DependencyPropertyChangedEventArgs eventArgs)
		{

			//this.groups.ItemsSource = this.User.Groups;

		}

	}

}

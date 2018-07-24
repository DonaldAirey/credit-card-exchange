namespace FluidTrade.Guardian
{

    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using FluidTrade.Core;

	/// <summary>
	/// Quick and dirty manager for roles that includes Certificates.
	/// </summary>
	public partial class WindowMain : Window
	{

		// Private Instance Fields
		private Root root;
		private WebServiceRoleProvider webServiceRoleProvider;

		/// <summary>
		/// Create a quick and dirty manager for roles that includes Certificates.
		/// </summary>
		public WindowMain()
		{

			// The Visual Studio managed components are created here.
			InitializeComponent();

			// Create a Web Service Role Provider that can handle the special requirements of certificates.
			this.webServiceRoleProvider = new WebServiceRoleProvider();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection.Add("connectionStringName", "AuthorizationServices");
			nameValueCollection.Add("applicationName", "Fluid Trade");
			this.webServiceRoleProvider.Initialize("My Provider", nameValueCollection);

			// This collection is used to display the hierarchy of roles in a TreeView.
			ObservableCollection<Root> roots = new ObservableCollection<Root>();
			this.root = new Root(this.webServiceRoleProvider.ApplicationName);
			roots.Add(this.root);
			this.treeView.ItemsSource = roots;

			// Convert each of the roles found in the current metadatabase into top level elements that can be managed by the
			// TreeView.
			foreach (string roleName in this.webServiceRoleProvider.GetAllRoles())
			{

				// Add the Role to the root of the observable collection.
				Role role = new Role(roleName);
				this.root.Roles.Add(role);

				// Add each of the users to the Role.
				foreach (string userName in this.webServiceRoleProvider.GetUsersInRole(role.Name))
					role.Users.Add(new User(role, userName));

			}

		}

		/// <summary>
		/// Indicates whether the selected item can be cut.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		void CanExecuteCut(object sender, CanExecuteRoutedEventArgs e)
		{

			// Not implemented yet.
			e.CanExecute = false;

		}

		/// <summary>
		/// Cuts the selected item to the clipboard.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		private void ExecuteCut(object target, ExecutedRoutedEventArgs e)
		{

		}

		/// <summary>
		/// Indicates whether data can be copied from the currently selected element.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
		{

			// Only a selected user has data that can be copied to the clipboard.
			e.CanExecute = this.treeView.SelectedItem is User;

		}

		/// <summary>
		/// Copies the selected item to the clipboard.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		private void ExecuteCopy(object target, ExecutedRoutedEventArgs e)
		{

			// When a user is selected, the identity of the user can be copied to the clipboard.
			if (this.treeView.SelectedItem is User)
			{
				User user = this.treeView.SelectedItem as User;
				Clipboard.SetText(user.Name);
			}

		}

		/// <summary>
		/// Indicates whether data can be pasted from the clipboard.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		void CanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
		{

			// Not implemented yet.
			e.CanExecute = false;

		}

		/// <summary>
		/// Pastes the selected item from the clipboard.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		private void ExecutePaste(object target, ExecutedRoutedEventArgs e)
		{

		}

		/// <summary>
		/// Indicates whether a role or user can be added.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		void CanExecuteNew(object sender, CanExecuteRoutedEventArgs e)
		{

			// A role can be added to the root and a user can be added to a role.
			e.CanExecute = this.treeView.SelectedItem is Root || this.treeView.SelectedItem is Role;

		}

		/// <summary>
		/// Creates a new role or user.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		private void ExecuteNew(object target, ExecutedRoutedEventArgs e)
		{

			// This adds a Role to the application root.
			if (this.treeView.SelectedItem is Root)
			{
				Root root = this.treeView.SelectedItem as Root;
				WindowRole windowRole = new WindowRole();
				if (windowRole.ShowDialog() == true)
				{
					this.webServiceRoleProvider.CreateRole(windowRole.RoleName);
					root.Roles.Add(new Role(windowRole.RoleName));
				}
			}

			// This adds a User to a Role.
			if (this.treeView.SelectedItem is Role)
			{
				Role role = this.treeView.SelectedItem as Role;
				WindowUser windowUser = new WindowUser();
				if (windowUser.ShowDialog() == true)
				{
					this.webServiceRoleProvider.AddUsersToRoles(new string[] { windowUser.UserName }, new string[] { role.Name });
					role.Users.Add(new User(role, windowUser.UserName));
				}
			}

		}

		/// <summary>
		/// Indicates whether a role or user can be deleted
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		void CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
		{

			// Roles and users can be deleted.  Roots can not.
			e.CanExecute = this.treeView.SelectedItem is Role || this.treeView.SelectedItem is User;

		}

		/// <summary>
		/// Deletes a Role or User.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		private void ExecuteDelete(object target, ExecutedRoutedEventArgs e)
		{

			// This deletes a Role from an application Root.
			if (this.treeView.SelectedItem is Role)
			{
				Role role = this.treeView.SelectedItem as Role;
				User[] users = role.Users.ToArray();
				foreach (User user in users)
				{
					role.Users.Remove(user);
					this.webServiceRoleProvider.RemoveUsersFromRoles(new string[] { user.Name }, new string[] { role.Name });
				}
				this.webServiceRoleProvider.DeleteRole(role.Name, true);
				this.root.Roles.Remove(role);
			}

			// This deletes a User from a Role.
			if (this.treeView.SelectedItem is User)
			{
				User user = this.treeView.SelectedItem as User;
				this.webServiceRoleProvider.RemoveUsersFromRoles(new string[] { user.Name }, new string[] { user.Role.Name });
				user.Role.Users.Remove(user);
			}

		}

		/// <summary>
		/// Indicates whether a role or user can be edited.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		void CanExecuteOpen(object sender, CanExecuteRoutedEventArgs e)
		{

			// Not implemented yet.
			e.CanExecute = false;

		}
		/// <summary>
		/// Edits an existing role or user.
		/// </summary>
		/// <param name="sender">The object that originated the command.</param>
		/// <param name="e">The event arguments.</param>
		private void ExecuteOpen(object target, ExecutedRoutedEventArgs e)
		{

		}

	}

}

namespace FluidTrade.Core
{

    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Web.Security;

	/// <summary>
	/// Provides role management services.
	/// </summary>
	public class WebServiceRoleProvider : RoleProvider
	{

		// Private Instance Fields
		private DataSetRoles dataSetRoles;
		private DataView userView;
		private String fileName;
		private String applicationName;

		/// <summary>
		/// Gets or sets the scope of the role provider.
		/// </summary>
		public override string ApplicationName
		{
			get { return this.applicationName; }
			set { this.applicationName = value; }
		}

		/// <summary>
		/// Initializes the role provider.
		/// </summary>
		/// <param name="name">The friendly name of the provider.</param>
		/// <param name="config">A collection of name/value pairs representing the provider specified attributes specifing the
		/// attributes specified in the configuration file for this provider.</param>
		public override void Initialize(string name, NameValueCollection config)
		{

			// This dataset holds the in-memory database for the role provider.  It is a fast, light weight method of mapping a 
			// given IIdentity to an application specific role.
			this.dataSetRoles = new DataSetRoles();

			// This view provides a quick lookup of the user and all the roles they can have.
			this.userView = new DataView(this.dataSetRoles.User, string.Empty, "User", DataViewRowState.CurrentRows);

			// This will open the persistent database based on the configuration setting that points to a connection string.
			if (!string.IsNullOrEmpty(config["connectionStringName"]))
			{
				ConnectionStringSettings connectionStringSetting =
					ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
				Uri uri = new Uri(connectionStringSetting.ConnectionString);
				this.fileName = uri.LocalPath;
                string directoryPath = Path.GetDirectoryName(this.fileName);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                if (File.Exists(this.fileName))
                    this.dataSetRoles.ReadXml(this.fileName);
			}

			// This sets the scope of the database operations to include only the roles for the given application.
			this.applicationName = string.IsNullOrEmpty(config["applicationName"]) ?
				System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath :
				config["applicationName"];

			// Allow the base class to process any other configuration settings.
			base.Initialize(name, config);

		}

		/// <summary>
		/// Add the specified users to the specified roles for the configured application name.
		/// </summary>
		/// <param name="usernames">A string array of names to be added to the roles.</param>
		/// <param name="roleNames">A string array of role names to which the the users names are added.</param>
		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{

			// Add each of the user names to each of the roles.
			foreach (string roleName in roleNames)
				foreach (string userName in usernames)
					this.dataSetRoles.User.AddUserRow(this.applicationName, roleName, userName);

			// Commit the changes to the database.
			this.dataSetRoles.WriteXml(this.fileName);

		}

		/// <summary>
		/// Adds a new role to the data source for the configured application name.
		/// </summary>
		/// <param name="roleName">The name of the role to create.</param>
		public override void CreateRole(string roleName)
		{

			// Add the role if it doesn't already exist.
			DataSetRoles.RoleRow roleRow = this.dataSetRoles.Role.FindByApplicationRole(this.applicationName, roleName);
			if (roleRow == null)
			{

				// Add the new role.
				this.dataSetRoles.Role.AddRoleRow(this.applicationName, roleName);

				// Commit the changes to the database.
				this.dataSetRoles.WriteXml(this.fileName);

			}

		}

		/// <summary>
		/// Removes a role from the data source for the specified application name.
		/// </summary>
		/// <param name="roleName">The name of the role to delete.</param>
		/// <param name="throwOnPopulatedRole">If true, throw an exception if the role has more than one member and do not
		/// delete the role.</param>
		/// <returns>true if the role was deleted successfully.</returns>
		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{

			// Use the in-memory database to find the role for the specified application name.
			DataSetRoles.RoleRow roleRow = this.dataSetRoles.Role.FindByApplicationRole(this.applicationName, roleName);
			if (roleRow != null)
			{

				// This allows an exception to be thrown if there are still members in this role.
				if (roleRow.GetUserRows().Length != 0 && throwOnPopulatedRole)
					throw new Exception(string.Format("The {0} role is still populated with users", roleName));

				// Removing the role at this point will also remove all the members from the role.
				this.dataSetRoles.Role.RemoveRoleRow(roleRow);

				// Commit the changes to the database.
				this.dataSetRoles.WriteXml(this.fileName);

			}

			// This indicates to the caller whether the role was found or not.
			return roleRow != null;

		}

		/// <summary>
		/// Gets an array of user names in a role where the user name contains the specified user name.
		/// </summary>
		/// <param name="roleName">The name of the role to search.</param>
		/// <param name="usernameToMatch">The user name to be searched for.</param>
		/// <returns>An array of user names in the role containing the specified user name.</returns>
		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{

			// This creates an array of users in the given role that match the name given as a search criteria.
			List<string> users = new List<string>();
			DataSetRoles.RoleRow roleRow = this.dataSetRoles.Role.FindByApplicationRole(this.applicationName, roleName);
			if (roleRow != null)
				foreach (DataSetRoles.UserRow userRow in roleRow.GetUserRows())
					if (userRow.User.Contains(usernameToMatch))
						users.Add(userRow.User);
			return users.ToArray();

		}

		/// <summary>
		/// Gets a list of all roles for the configured application name.
		/// </summary>
		/// <returns>A list of all roles for the configured application name.</returns>
		public override string[] GetAllRoles()
		{

			// This creates an array of all the roles belonging to the configured application name.
			List<string> roles = new List<string>();
			foreach (DataSetRoles.RoleRow roleRow in this.dataSetRoles.Role)
				if (roleRow.Application == this.applicationName)
					roles.Add(roleRow.Role);
			return roles.ToArray();
			
		}

		/// <summary>
		/// Gets a list of roles in which a user participates for the configured application name.
		/// </summary>
		/// <param name="username">A user for which to return the list of roles.</param>
		/// <returns>The list of roles to which a user name belongs.</returns>
		public override string[] GetRolesForUser(string username)
		{

			// Create an array of roles to which the specified user name belongs using the DataView created for this purpose.
			List<string> roles = new List<string>();
			foreach (DataRowView dataRowView in this.userView.FindRows(username))
			{
				DataSetRoles.UserRow userRow = dataRowView.Row as DataSetRoles.UserRow;
				roles.Add(userRow.RoleRowParent.Role);
			}
			return roles.ToArray();

		}

		/// <summary>
		/// Gets a list of users in the specified role for the configured application name.
		/// </summary>
		/// <param name="roleName">The name of the role to which the users belong.</param>
		/// <returns>An array of users belonging to the specified role.</returns>
		public override string[] GetUsersInRole(string roleName)
		{

			// Create an array of user names belonging to the specified role.
			List<string> users = new List<string>();
			DataSetRoles.RoleRow roleRow = this.dataSetRoles.Role.FindByApplicationRole(this.applicationName, roleName);
			foreach (DataSetRoles.UserRow userRow in roleRow.GetUserRows())
				users.Add(userRow.User);
			return users.ToArray();

		}

		/// <summary>
		/// Gets a value indicating whether the specified user is in the specified role for the configured application name.
		/// </summary>
		/// <param name="username">The user name to be found.</param>
		/// <param name="roleName">The role in which to search.</param>
		/// <returns>true indicates that the specified user belongs to the specified rule.</returns>
		public override bool IsUserInRole(string username, string roleName)
		{

			// This check determines whether the user belongs to the specified role for the configured application.
			return this.dataSetRoles.User.FindByApplicationRoleUser(this.applicationName, roleName, username) != null;

		}

		/// <summary>
		/// Removes the specified user names from the specified roles for the configured application name.
		/// </summary>
		/// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
		/// <param name="roleNames">A string array or roles from which the users are removed.</param>
		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{

			// Remove each user name from each role in the configured application name.
			foreach (string roleName in roleNames)
				foreach (string userName in usernames)
				{
					DataSetRoles.UserRow userRow =
						this.dataSetRoles.User.FindByApplicationRoleUser(this.applicationName, roleName, userName);
					if (userRow != null)
						this.dataSetRoles.User.RemoveUserRow(userRow);
				}

			// Commit the changes to the database.
			this.dataSetRoles.WriteXml(this.fileName);

		}

		/// <summary>
		/// Gets a value which indicates whether the specified role exists for the configured application name.
		/// </summary>
		/// <param name="roleName">The name of the role to search for in the data source.</param>
		/// <returns>true if the specified role exists in the data source.</returns>
		public override bool RoleExists(string roleName)
		{

			// This simple check determines whether the given role exists for the configured application name.
			return this.dataSetRoles.Role.FindByApplicationRole(this.applicationName, roleName) != null;

		}

	}

}

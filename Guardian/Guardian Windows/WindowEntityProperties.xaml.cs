namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using System.Transactions;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media.Imaging;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using System.ComponentModel;
	using FluidTrade.Guardian.Utilities;
	using System.ServiceModel;
	using System.Data;
	using System.ServiceModel.Security;

    /// <summary>
    /// Generic properties for any entity.
    /// </summary>
    public partial class WindowEntityProperties : Window
    {

		/// <summary>
		/// Command sent when the "Add..." security button is clicked.
		/// </summary>
		public static readonly RoutedUICommand AddPermissionsGroup = new RoutedUICommand("AddPermissionsGroup", "Add...", typeof(WindowEntityProperties));
		/// <summary>
		/// Command sent when the "Remove" security button is clicked.
		/// </summary>
		public static readonly RoutedUICommand RemovePermissionsGroup = new RoutedUICommand("RemovePermissionsGroup", "Remove", typeof(WindowEntityProperties));
		/// <summary>
        /// Identifies the CanApply dependency property.
        /// </summary>
        public static DependencyProperty CanApplyProperty =
            DependencyProperty.Register("CanApply", typeof(Boolean), typeof(WindowEntityProperties));
		/// <summary>
		/// Identifies the Entity dependency property.
		/// </summary>
		public static readonly DependencyProperty EntityProperty =
			DependencyProperty.Register("Entity", typeof(Entity), typeof(WindowEntityProperties), new PropertyMetadata(null, WindowEntityProperties.OnEntityChanged, WindowEntityProperties.CoerceEntity));

        // Private Instance Fields
		private Boolean isEntityDeleted = false;
        private Boolean isEntityDirty;
        private long entityRowVersion;
        private Boolean populating;
        private Guid treeRootEntityId;
		private ObservableCollection<AccessControl> roles = new ObservableCollection<AccessControl>();
		
		// Private background fields
		private Guid entityId;

        private delegate void populate(Boolean enabled, Entity entity, string type, string location, string contains, List<AccessControl> roles);

        /// <summary>
        /// Create a new properties dialog box.
        /// </summary>
        public WindowEntityProperties()
        {

            InitializeComponent();

			this.RolesAndUsers.ItemsSource = roles;
			this.entityRowVersion = 0;

			this.Loaded += this.OnLoaded;
			this.Unloaded += delegate(object sender, RoutedEventArgs eventArgs)
			{
				DataModel.EndMerge -= this.OnEndMerge;
				DataModel.Entity.RowChanging -= this.FilterRow;
				DataModel.Entity.RowDeleting -= this.FilterRow;
				DataModel.AccessControl.RowChanging -= this.FilterRow;
				DataModel.AccessControl.RowDeleting -= this.FilterRow;
				DataModel.AccessControl.TableNewRow -= this.FilterRow;
			};

        }

        /// <summary>
        /// True if there are changes that can be 'applied', false otherwise.
        /// </summary>
        public Boolean CanApply
        {

            get { return (Boolean)this.GetValue(WindowEntityProperties.CanApplyProperty); }
            set
            {

                if (!this.IsEntityDeleted && !this.Populating)
                    this.SetValue(WindowEntityProperties.CanApplyProperty, value);

            }

        }

        /// <summary>
        /// The entity whose properties we will display.
        /// </summary>
        public Entity Entity
        {

            get { return this.GetValue(WindowEntityProperties.EntityProperty) as Entity; }
			set { this.SetValue(WindowEntityProperties.EntityProperty, value); }

        }

		/// <summary>
		/// True if the underlying entity has been deleted.
		/// </summary>
		protected Boolean IsEntityDeleted
		{
			get { return this.isEntityDeleted; }
		}

		/// <summary>
		/// When set, the underlying objects should be updated on the next EndMerge.
		/// </summary>
		protected Boolean MustRedisplay
		{
			get;
			set;
		}

        /// <summary>
        /// The TabControl that contains all of the property tabs.
        /// </summary>
        protected TabControl TabControl
        {

            get { return tabControl; }

        }

		/// <summary>
		/// Apply any changes to the entity.
		/// </summary>
		private void Apply(Entity entity, List<AccessControl> roles)
		{

			try
			{

				entity.Commit();
				this.ApplySecurityTab(roles);

			}
			catch (SecurityAccessDeniedException)
			{

				this.Dispatcher.BeginInvoke(new Action(delegate()
					{

						MessageBox.Show(this, Properties.Resources.CommitFailedAccessDenied, this.Title);
						this.CanApply = true;

					}), DispatcherPriority.Normal);

			}
			catch (FaultException<ArgumentFault>)
			{

				this.Dispatcher.BeginInvoke(new Action(delegate()
					{

						MessageBox.Show(this, Properties.Resources.RenameFailedInvalidName, this.Title);
						this.CanApply = true;

					}), DispatcherPriority.Normal);

			}
			catch (FaultException<FieldRequiredFault>)
			{

				this.Dispatcher.BeginInvoke(new Action(delegate()
				{

					MessageBox.Show(this, Properties.Resources.CommitFailedNoDebtRule, this.Title);
					this.CanApply = true;

				}), DispatcherPriority.Normal);

			}
			catch (FaultException<RecordExistsFault>)
			{


				this.Dispatcher.BeginInvoke(new Action(delegate()
				{

					MessageBox.Show(this, Properties.Resources.RenameFailedRecordExists, this.Title);
					this.CanApply = true;

				}), DispatcherPriority.Normal);

			}
			catch (Exception exception)
			{

				this.Dispatcher.BeginInvoke(new Action(delegate()
					{

						MessageBox.Show(this, Properties.Resources.OperationFailed, this.Title);
						this.CanApply = true;

					}), DispatcherPriority.Normal);
				EventLog.Warning("Unknown error in entity commit: {0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

        /// <summary>
        /// Apply changes made to the entity under the Security tab.
        /// </summary>
        private void ApplySecurityTab(List<AccessControl> roles)
        {

			try
			{

				foreach (AccessControl user in roles)
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
		protected void BackgroundFail(Exception exception)
		{

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
			{

				MessageBox.Show(this, Properties.Resources.OperationFailed, this.Title);
				this.CanApply = true;

			}));

		}

		/// <summary>
		/// When the entity is set, create a clone of it for our use.
		/// </summary>
		/// <param name="sender">The properties window.</param>
		/// <param name="baseObject">The object Entity has been set to.</param>
		/// <returns></returns>
		private static object CoerceEntity(DependencyObject sender, object baseObject)
		{

			return (baseObject as Entity).Clone();

		}

        /// <summary>
        /// Count all of the entities beneath an entity in the hierarchy and return the total. The caller should lock the DataModel.
        /// </summary>
        /// <param name="parent">The entity to start from.</param>
        /// <returns>The total number of entities beneath 'entity' in the hierarchy.</returns>
        private int CountChildren(EntityRow parent)
        {

            int count = 0;

            foreach (EntityTreeRow row in parent.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
            {

                count += 1;
                count += this.CountChildren(row.EntityRowByFK_Entity_EntityTree_ChildId);

			}

            return count;

        }

		/// <summary>
		/// Determine whether a row is one of the rows being displayed by the window.
		/// </summary>
		/// <param name="sender">The table that sent the event.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void FilterRow(object sender, EventArgs eventArgs)
		{

			DataRowAction action = eventArgs is DataRowChangeEventArgs ? (eventArgs as DataRowChangeEventArgs).Action : DataRowAction.Add;
			DataRow row = eventArgs is DataRowChangeEventArgs ? (eventArgs as DataRowChangeEventArgs).Row : (eventArgs as DataTableNewRowEventArgs).Row;

			if (!(row.RowState == DataRowState.Detached || row.RowState == DataRowState.Deleted))
				if (row is EntityRow && (row as EntityRow).EntityId == this.entityId)
					this.MustRedisplay = true;
				else if (row is AccessControlRow && row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached && (row as AccessControlRow).EntityId == this.entityId)
					this.MustRedisplay = true;

		}

        /// <summary>
        /// Get a path string representing the location of this object.
        /// </summary>
        /// <returns>The path string.</returns>
        private string GetLocation(Guid entityId)
        {

			List<string> location = this.GetLocation(DataModel.Entity.EntityKey.Find(this.treeRootEntityId), entityId);
            // The first element of the path is actually the User entity, which doesn't need to show up in the path.

            if (location != null)
                return String.Join(@"\", location.ToArray(), 1, location.Count - 1);
            else
                return null;

        }

        /// <summary>
        /// Get a list of the entity names that describes the path to this object.
        /// </summary>
        /// <param name="current">The current entity in the path to the object.</param>
		/// <param name="target">The entityId of the entity we're building a path for.</param>
        /// <returns>The list of entity names that represent the path to the object.</returns>
        private List<string> GetLocation(EntityRow current, Guid target)
        {

            List<string> list = null;

			if (current == null)
				return null;

            // If current is the entity that we're looking for, we'll create our own list.
            if (current.EntityId == target)
            {

                list = new List<string>();

            }
            // .... Otherwise, see if we can generate a path list from one of our children.
            else
            {

                foreach (EntityTreeRow node in current.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
                {

                    list = this.GetLocation(node.EntityRowByFK_Entity_EntityTree_ChildId, target);
                    if (list != null)
                        break;

                }

            }

            if (list != null)
                list.Insert(0, current.Name);

            return list;

        }

        /// <summary>
        /// Add permission to the current entity to a new user/group.
        /// </summary>
        /// <param name="sender">The security tab's Add... button</param>
        /// <param name="eventArgs">The event arguments (unused).</param>
        private void OnAddPermissionGroup(object sender, RoutedEventArgs eventArgs)
        {

            WindowFindUser windowAddUserAccess = new WindowFindUser();

			windowAddUserAccess.Owner = this;
			windowAddUserAccess.Closed += this.OnFindUserClose;
			windowAddUserAccess.Show();
			Win32Interop.DisableWindow(this);

        }

        /// <summary>
        /// Commit the changes to the entity, but leave the window open.
        /// </summary>
        /// <param name="sender">The object the originated the event - ie. the Apply or Okay button.</param>
        /// <param name="e">The event arguments.</param>
        private void OnApply(Object sender, RoutedEventArgs e)
        {

			if (!this.IsEntityDeleted && this.CanApply)
			{

				this.Cursor = Cursors.Wait;
				Entity entity = this.Entity.Clone() as Entity;
				List<AccessControl> roles = this.roles.ToList();

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Apply(entity, roles));
				this.CanApply = false;
				this.Cursor = Cursors.Arrow;

			}

        }

		/// <summary>
		/// Set the browse permission of the current user/role to that of the update check box.
		/// </summary>
		/// <param name="sender">The update permission check box.</param>
		/// <param name="eventArgs">The event arguments (unused).</param>
		private void OnBrowsePermissionClicked(object sender, RoutedEventArgs eventArgs)
		{

			AccessControl selected = this.RolesAndUsers.SelectedItem as AccessControl;

			if (selected != null && !this.Populating)
			{

				selected.Browse = (Boolean)this.BrowsePermission.IsChecked;
				this.CanApply = true;

			}

		}

        /// <summary>
        /// Close the window and forget any changes.
        /// </summary>
        /// <param name="sender">The object that originated the event - ie. the Cancel button.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCancel(Object sender, RoutedEventArgs e)
        {

            Close();

        }

        /// <summary>
        /// Launch an icon/image selector dialog box and set the entity's icon to the result.
        /// </summary>
        /// <param name="sender">The changeIconButton.</param>
        /// <param name="eventArgs">The event arguments</param>
        private void OnChangeIconButtonClick(object sender, RoutedEventArgs eventArgs)
        {

            WindowIconChooser iconChooser = new WindowIconChooser();
			iconChooser.Closed += this.OnIconChooserClosed;
			iconChooser.Show();
			Win32Interop.DisableWindow(this);


        }

		/// <summary>
		/// When the icon chooser window closes, get its selected icon (if any) and sets the entity's icon.
		/// </summary>
		/// <param name="sender">The icon chooser.</param>
		/// <param name="eventArgs">The event args.</param>
		private void OnIconChooserClosed(object sender, EventArgs eventArgs)
		{

			WindowIconChooser iconChooser = sender as WindowIconChooser;

			if (iconChooser.SelectedIconId != null)
			{

				this.Entity.ImageId = iconChooser.SelectedIconId.Value;
				this.Entity.ImageData = iconChooser.SelectedIcon.ImageData;
				if (!this.Populating)
					this.isEntityDirty = true;

			}

			Win32Interop.EnableWindow(this);
			this.Activate();

		}

        /// <summary>
        /// Handle an update to the data model. If no changes have been made to the entity, re-populate the dialog box.
        /// </summary>
        private void OnEndMerge(object sender, EventArgs eventArgs)
        {

			if (this.MustRedisplay)
			{

				EntityRow entityRow = DataModel.Entity.EntityKey.Find(this.entityId);

				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Populate(data as Entity), entityRow == null ? null : Entity.New(entityRow));

				this.MustRedisplay = false;

			}

        }

		/// <summary>
		/// Handle the Entity changing.
		/// </summary>
		/// <param name="sender">The WindowEntityProperties.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private static void OnEntityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
		{

			(sender as WindowEntityProperties).OnEntityChanged();

		}

		/// <summary>
		/// Called when the Entity is changed.
		/// </summary>
		protected virtual void OnEntityChanged()
		{

			this.DataContext = this.Entity;
			this.Entity.PropertyChanged += this.OnEntityChanged;
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
				{
					lock (DataModel.SyncRoot)
						this.entityId = (Guid)data;
				}, this.Entity.EntityId);

		}

		/// <summary>
		/// Handle changes to properties in the current Entity.
		/// </summary>
		/// <param name="sender">The Entity.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnEntityChanged(object sender, PropertyChangedEventArgs eventArgs)
		{

			if (!this.Populating)
			{

				this.CanApply = true;

			}

		}

        /// <summary>
        /// Set the execute permission of the current user/role to that of the execute check box.
        /// </summary>
        /// <param name="sender">The execute permission check box.</param>
        /// <param name="eventArgs">The event arguments (unused).</param>
        private void OnExecutePermissionClicked(object sender, RoutedEventArgs eventArgs)
        {

            AccessControl selected = this.RolesAndUsers.SelectedItem as AccessControl;

			if (selected != null && !this.Populating)
            {

                selected.Execute = (Boolean)this.ExecutePermission.IsChecked;
                this.CanApply = true;

            }

        }

		/// <summary>
		/// When the find-user window closes, grab the rights holder that was selected and add it.
		/// </summary>
		/// <param name="sender">The find window.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnFindUserClose(object sender, EventArgs eventArgs)
		{

			WindowFindUser windowAddUserAccess = sender as WindowFindUser;

			if (windowAddUserAccess.SelectedUser != null)
			{

				DependencyObject container = null;

				if (!this.roles.Any(a => a.User == windowAddUserAccess.SelectedUser))
				{

					this.roles.Add(new AccessControl(windowAddUserAccess.SelectedUser, this.Entity, AccessRight.None, this.Entity.TenantId));
					this.CanApply = true;

				}

				container = this.RolesAndUsers.ItemContainerGenerator.ContainerFromItem(windowAddUserAccess.SelectedUser);
				
				if (container != null)
					container.SetValue(FrameworkElement.VisibilityProperty, Visibility.Visible);
				this.RolesAndUsers.SelectedItem = this.roles.FirstOrDefault(a => a.User == windowAddUserAccess.SelectedUser);
				this.RolesAndUsers.ScrollIntoView(this.RolesAndUsers.SelectedItem);

			}

			Win32Interop.EnableWindow(this);
			this.Activate();

		}

        /// <summary>
        /// Toggle all of the other check boxes when Full Control is toggled.
        /// </summary>
        /// <param name="sender">The full control check box.</param>
        /// <param name="eventArgs">The event arguments (unused).</param>
        private void OnFullPermissionClicked(object sender, RoutedEventArgs eventArgs)
        {

			this.BrowsePermission.IsChecked = this.FullPermission.IsChecked;
			this.ReadPermission.IsChecked = this.FullPermission.IsChecked;
            this.WritePermission.IsChecked = this.FullPermission.IsChecked;
            this.ExecutePermission.IsChecked = this.FullPermission.IsChecked;

        }

		/// <summary>
		/// Handle the entity hidden state changing.
		/// </summary>
		/// <param name="sender">The properties dialog box.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnHiddenChanged(object sender, RoutedEventArgs eventArgs)
		{

			CheckBox hidden = sender as CheckBox;

			if (!this.Populating && hidden != null)
			{

				this.isEntityDirty = true;
				this.Entity.IsHidden = (Boolean)hidden.IsChecked;
				this.CanApply = true;

			}

		}

		/// <summary>
		/// Handle the Loaded event.
		/// </summary>
		/// <param name="sender">The properties window.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs eventArgs)
		{

			DataModel.EndMerge += this.OnEndMerge;
			DataModel.Entity.RowChanging += this.FilterRow;
			DataModel.Entity.RowDeleting += this.FilterRow;
			DataModel.AccessControl.RowChanging += this.FilterRow;
			DataModel.AccessControl.RowDeleting += this.FilterRow;
			DataModel.AccessControl.TableNewRow += this.FilterRow;
			this.SetSize();

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(delegate(object data)
				{

					Entity entity = null;

					lock (DataModel.SyncRoot)
					{

						EntityRow entityRow = DataModel.Entity.EntityKey.Find(this.entityId);
						entity = entityRow == null ? null : Entity.New(entityRow);

					}

					this.Populate(entity);
				});

		}

        /// <summary>
        /// Commit the changes to the entity and close the window.
        /// </summary>
        /// <param name="sender">The object that originated the event - ie. the Okay button.</param>
        /// <param name="e">The event arguments.</param>
        private void OnOkay(Object sender, RoutedEventArgs e)
        {

			if (!this.IsEntityDeleted && this.CanApply)
			{

				this.Cursor = Cursors.Wait;
				Entity entity = this.Entity.Clone() as Entity;
				List<AccessControl> roles = this.roles.ToList();
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Apply(entity, roles));
				this.Cursor = Cursors.Arrow;

			}

			this.Close();

        }

        /// <summary>
        /// Fill in access rights that selected role/user has to the entity.
        /// </summary>
        /// <param name="sender">The object that originated the event - ie. the "Group and users" list box.</param>
        /// <param name="selectionEventArgs">The event arguments.</param>
        private void OnPermissionsRoleSelected(Object sender, SelectionChangedEventArgs selectionEventArgs)
        {

            lock (DataModel.SyncRoot)
            {

                AccessControl selected = this.RolesAndUsers.SelectedValue as AccessControl;
                Boolean canApply = this.CanApply;

                if (selected != null)
                {

					this.Populating = true;
					this.FullPermission.IsChecked = selected.HasFullControl;
					this.BrowsePermission.IsChecked = selected.Browse;
					this.ReadPermission.IsChecked = selected.Read;
                    this.WritePermission.IsChecked = selected.Write;
                    this.ExecutePermission.IsChecked = selected.Execute;
					this.Populating = false;

                }
                else
                {

                    // If there is no longer anything selected, clear the permissions information.
                    //this.PermissionsForName.Text = "";
					this.FullPermission.IsChecked = false;
					this.BrowsePermission.IsChecked = false;
					this.ReadPermission.IsChecked = false;
                    this.WritePermission.IsChecked = false;
                    this.ExecutePermission.IsChecked = false;

                }

                this.CanApply = canApply;
                
            }

        }

		/// <summary>
		/// Handle the entity read-only state changing.
		/// </summary>
		/// <param name="sender">The properties dialog box.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnReadOnlyChanged(object sender, RoutedEventArgs eventArgs)
		{

			CheckBox readOnly = sender as CheckBox;

			if (!this.Populating && readOnly != null)
			{

				this.isEntityDirty = true;
				this.Entity.IsReadOnly = (Boolean)readOnly.IsChecked;
				this.CanApply = true;

			}

		}

        /// <summary>
        /// Set the read permission of the current user/role to that of the read check box.
        /// </summary>
        /// <param name="sender">The read permission check box.</param>
        /// <param name="eventArgs">The event arguments (unused).</param>
        private void OnReadPermissionClicked(object sender, RoutedEventArgs eventArgs)
        {

            AccessControl selected = this.RolesAndUsers.SelectedItem as AccessControl;

            if (selected != null && !this.Populating)
            {

                selected.Read = (Boolean)this.ReadPermission.IsChecked;
                this.CanApply = true;

            }

        }

        /// <summary>
        /// Remove the currently selected user/group's permissions on the current entity.
        /// </summary>
        /// <param name="sender">The security tab's Remove button</param>
        /// <param name="eventArgs">The event arguments (unused).</param>
        private void OnRemovePermissionGroup(object sender, RoutedEventArgs eventArgs)
        {

            ListBoxItem item = RolesAndUsers.ItemContainerGenerator.ContainerFromIndex(RolesAndUsers.SelectedIndex) as ListBoxItem;

            if (item != null)
            {

                // Collapse the list item to "remove" it from the list and mark it for deletion.
                item.Visibility = System.Windows.Visibility.Collapsed;
                (item.Content as AccessControl).Deleted = true;
                RolesAndUsers.SelectedItem = null;
                this.CanApply = true;

            }

        }

        /// <summary>
        /// Set the write permission of the current user/role to that of the update check box.
        /// </summary>
        /// <param name="sender">The update permission check box.</param>
        /// <param name="eventArgs">The event arguments (unused).</param>
        private void OnWritePermissionClicked(object sender, RoutedEventArgs eventArgs)
        {

            AccessControl selected = this.RolesAndUsers.SelectedItem as AccessControl;

			if (selected != null && !this.Populating)
            {

                selected.Write = (Boolean)this.WritePermission.IsChecked;
                this.CanApply = true;

            }

        }

        /// <summary>
        /// Fill in the controls in with the entity's data.
        /// </summary>
        protected virtual void Populate(Entity entity)
        {

			Boolean enabled = true;
            string type = "";
            string location = "";
            string contains = "";
            List<AccessControl> roles = new List<AccessControl>();

            try
            {

                lock (DataModel.SyncRoot)
                {

                    if (entity == null)
                    {

                        enabled = false;

                    }
                    else
                    {

						EntityRow entityRow = DataModel.Entity.EntityKey.Find(entity.EntityId);

                        // General tab:
						type = DataModel.Type.TypeKey.Find(entity.TypeId).Description;
                        location = this.GetLocation(entity.EntityId);
						contains = String.Format("{0} items", this.CountChildren(entityRow));

                        // Security tab:
						foreach (AccessControlRow accessor in entityRow.GetAccessControlRows())
                            roles.Add(new AccessControl(accessor));

                    }

                }

                // Push the information we just collected to the foreground to update the dialog box.
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new populate(this.Populate), enabled, entity, type, location, contains, roles);

            }
            catch (Exception exception)
            {

				EventLog.Warning("Error populating properties window. {0}:\n{1}", exception.Message, exception.StackTrace);
               
            }

        }
        
        /// <summary>
        /// Fill in the properties window with the information gathered in the background.
        /// </summary>
		/// <param name="enabled">Whether the window is enabled.</param>
		/// <param name="entity">An entity with current information.</param>
		/// <param name="type">The type description.</param>
        /// <param name="location">The path to the entity.</param>
        /// <param name="contains">The number of contained items.</param>
        /// <param name="roles">The list of access controls.</param>
		private void Populate(Boolean enabled, Entity entity, string type, string location, string contains, List<AccessControl> roles)
        {

			Boolean canApply = this.CanApply;
            this.populating = true;

            if (enabled)
            {

                // General tab:
				if (!this.isEntityDirty && this.entityRowVersion != this.Entity.RowVersion)
                {

					this.Entity.Copy(entity);
					this.Icon = BitmapFrame.Create(new MemoryStream(Convert.FromBase64String(this.Entity.ImageData)));
                    this.labelType.Text = type;
                    this.labelLocation.Text = location;
                    this.labelContains.Text = contains;

                }

                // Security tab:
				if (!this.isEntityDirty && this.entityRowVersion != this.Entity.RowVersion)
                    this.ObjectName.Text = location;
                this.PopulateRolesAndUsers(roles);

            }
            else
            {
				
                this.isEntityDeleted = true;

            }

            this.populating = false;
			this.CanApply = canApply;

        }

        /// <summary>
        /// Update the roles and users list.
        /// </summary>
        /// <param name="roles">The current access control list.</param>
        private void PopulateRolesAndUsers(List<AccessControl> roles)
        {

            AccessControl selected = this.RolesAndUsers.SelectedItem as AccessControl;

            for (int index = 0; index < this.roles.Count; index += 1)
            {

                AccessControl role = this.roles[index] as AccessControl;

                if (roles.Contains(role))
                {

                    role.Update(roles.Find((r) => r.Equals(role)));
                    roles.Remove(role);

                }
                else if (!role.New || role.Deleted)
                {

                    this.roles.Remove(role);
                    index -= 1;

                }

            }
            foreach (AccessControl role in roles)
                this.roles.Add(role);

            if (selected == null || !this.roles.Contains(selected))
            {

                this.RolesAndUsers.SelectedItem = null;
                this.RolesAndUsers.SelectedIndex = 0;

            }
            else
            {

                this.RolesAndUsers.SelectedItem = null;
                this.RolesAndUsers.SelectedItem = selected;

            }

        }

        /// <summary>
        /// Whether the window is currenlty being populated with data. If true, any changes to UI elements should not be considered for database
        /// updates. In particular, CanApply should not be changed while Populate is true.
        /// </summary>
        protected Boolean Populating
        {

            get { return this.populating; }
            set { this.populating = value; }

        }

        /// <summary>
        /// Set an appropriate size for the window.
        /// </summary>
        private void SetSize()
        {

			FrameworkElement content = this.tabControl.Template.FindName("PART_SelectedContentHost", this.tabControl) as ContentPresenter;
			ColumnDefinition tabColumn = this.tabControl.Template.FindName("ColumnDefinition0", this.tabControl) as ColumnDefinition;
			double windowHeight = 0;
            double windowWidth = 0;

			this.tabControl.Measure(new Size(double.MaxValue, double.MaxValue));
			Size tabSize = this.tabControl.DesiredSize;

            foreach (TabItem tabItem in this.tabControl.Items)
            {

                tabItem.Measure(new Size(double.MaxValue, double.MaxValue));

                if (tabItem.Content != null)
                {

                    UIElement tab = tabItem.Content as UIElement;

                    tab.Measure(new Size(double.MaxValue, double.MaxValue));

                    if (tab.DesiredSize.Height > windowHeight)
                        windowHeight = tab.DesiredSize.Height;

                    if (tab.DesiredSize.Width > windowWidth)
                        windowWidth = tab.DesiredSize.Width;

                }

            }

			if (content != null)
			{

				// The tab control bases its size of the size of the currently selected content, so we can lock the size of the tab control by
				// locking the size of the control that contains the selected content.
				content.Height = windowHeight;
				// Since the tab panel in the tab control tries to take up as much space as necessary to display all the tabs on out line, we've got
				// to muck around with the width of the tab panel's container.
#if false // We'll enable this later when we've tweaked the xaml such that the window isn't too small.
				if (tabColumn != null)
					tabColumn.Width = new GridLength(windowWidth + this.tabControl.BorderThickness.Left + this.tabControl.BorderThickness.Right +
						this.tabControl.Padding.Left + this.tabControl.Padding.Right);
				else
					content.Width = windowWidth
#endif

			}
			else
			{

				// In the unlikely event that we're unable to find the tab control's content presenter, we'll have to go with whatever WPF was able
				// to figure out.
				this.Height = this.ActualHeight;
				this.Width = this.ActualWidth;

			}

        }

		/// <summary>
		/// Set the entityId of the entity at the root of the entity tree.
		/// </summary>
		/// <param name="entityId"></param>
		public void SetTreeRoot(Guid entityId)
		{

			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(delegate(Object data)
				{
					lock (DataModel.SyncRoot)
						this.treeRootEntityId = entityId;
				});

		}

		/// <summary>
		/// Update our entity from an entity row (if necessary).
		/// HACK: Honestly, this is a hack. Entity (and its derivitives) should have an Update similar to DebtRule or AccessControl (and a Commit
		/// as well).
		/// </summary>
		/// <param name="entityRow">The row to update from.</param>
		private void UpdateEntity(EntityRow entityRow)
		{

			if (entityRow.RowVersion != this.Entity.RowVersion)
			{

				Assembly assembly = null;
				string className = null;

				Entity.LoadAssembly(entityRow.TypeRow.Type, out assembly, out className);

				if (assembly != null && className != null)
					this.Entity.Copy(
						assembly.CreateInstance(
							className,
							false,
							BindingFlags.CreateInstance,
							null,
							new object[] { entityRow },
							System.Globalization.CultureInfo.CurrentCulture,
							null) as Entity);

			}

		}

    }

}

namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.ServiceModel;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Core.Windows;
	using FluidTrade.Guardian.TradingSupportReference;

    /// <summary>
	/// A TreeView used to view, navigate and maintain entities in the data model.
	/// </summary>
	public partial class FolderTreeView : TreeView
	{

		// Private Fields
		private Boolean hasNewData;
		private ContextMenu lastContextMenu;
		private Guid? newEntity;
		System.Diagnostics.Stopwatch newWatch;

		// Private Delegates
		private delegate void TreeViewItemDelegate(FolderTreeNode folderTreeNode, FolderTreeNode selectedValue, RoutedCommand command);

		/// <summary>
		/// Identifies the FluidTrade.Guardian.FolderTreeView.SelectedValue property.
		/// </summary>
		public static new readonly DependencyProperty SelectedValueProperty;

        /// <summary>
        /// Identifies the FluidTrade.GuardianFolderTreeView.TreeRefreshedProperty property.
        /// </summary>
        public static  readonly DependencyProperty TreeRefreshedProperty;

        /// <summary>
        /// Routed event triggered when the there is a change in the nav
        /// </summary>
        public static readonly RoutedEvent TreeUpdatedEvent; 

		/// <summary>
		/// Create the static resources required by this class.
		/// </summary>
		static FolderTreeView()
		{

			// SelectedValue
			FolderTreeView.SelectedValueProperty = DependencyProperty.Register(
				"SelectedValue",
				typeof(Entity),
				typeof(FolderTreeView),
				new FrameworkPropertyMetadata(OnSelectedValueChanged));

            FolderTreeView.TreeRefreshedProperty = DependencyProperty.Register(
                "TreeRefreshed",
                typeof(Entity),
                typeof(FolderTreeView));


            FolderTreeView.TreeUpdatedEvent = EventManager.RegisterRoutedEvent(
                "TreeUpdated",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(FolderTreeView));
		}

		/// <summary>
		/// Create a control that displays the hierarchical organization of objects.
		/// </summary>
		public FolderTreeView()
		{

			// The AXML generated resources are initialized here.
			InitializeComponent();

			// The data used to populate this control comes from the shared data model.  These event handlers will attach this TreeView to the data model when
			// the control is loaded into the visual tree hierarchy and remove it from the data model events when the control is removed from the visual
			// hierarchy.
			this.Loaded += new RoutedEventHandler(OnLoaded);
			this.Unloaded += new RoutedEventHandler(OnUnloaded);

			// The native 'SelectedValue' property that comes with a TreeView is read only, so one is created for this class that can have values written to
			// it.  This event handler will keep the new 'SelectedValue' property synchronized with the value with the same name in the base class.  The
			// 'SelectedValue' can also be set programatically and is used by the navigator to keep the TreeView synchronized with the currently navigated 
			// page.
			this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(OnSelectedItemChanged);

			// This will be used to force an evaluation of the structure of the TreeView against the current data model.
			this.hasNewData = true;

			// This will be used to track which context menu was last opened so we can figure out which item if it was opened for (and whether it's still
			// open).
			this.lastContextMenu = null;

		}

		/// <summary>
		/// Gets an indication of whether the application is in the designer or not.
		/// </summary>
		private Boolean IsDesignMode
		{

			get
			{
				object isDesignModeProperty = AppDomain.CurrentDomain.GetData("IsDesignModeProperty");
				return isDesignModeProperty == null ? true : Convert.ToBoolean(isDesignModeProperty);
			}

		}

		/// <summary>
		/// Gets or sets the selected value shown in the TreeView.
		/// </summary>
		public new Object SelectedValue
		{
			get { return this.GetValue(FolderTreeView.SelectedValueProperty); }
			set { this.SetValue(FolderTreeView.SelectedValueProperty, value); }
		}

        /// <summary>
        /// Gets or sets Tree refresh property
        /// </summary>
        public Object TreeRefreshed
        {
            get { return this.GetValue(FolderTreeView.TreeRefreshedProperty); }
            set { this.SetValue(FolderTreeView.TreeRefreshedProperty, value); }
        }

		/// <summary>
		/// Raised after the contents of the tree have been updated from the data model.
		/// </summary>
        public event RoutedEventHandler TreeUpdated
        {
            add { AddHandler(FolderTreeView.TreeUpdatedEvent, value); }
            remove { RemoveHandler(FolderTreeView.TreeUpdatedEvent, value); }
        }

		/// <summary>
		/// Handles a change to the DataModel.Entity table.
		/// </summary>
		/// <param name="sender">Object that generated the event.</param>
        /// <param name="entityRowChangeEvent">The event arguments.</param>
		private void ChangeEntityRow(object sender, EntityRowChangeEventArgs entityRowChangeEvent)
		{

			// Setting this flag will cause the next refresh to incrementally update the data in the tree view.
			this.hasNewData = true;

		}

		/// <summary>
		/// Handles a change to the DataModel.EntityTree table.
		/// </summary>
		/// <param name="sender">Object that generated the event.</param>
        /// <param name="entityTreeRowChangeEvent">The event arguments.</param>
		private void ChangeEntityTreeRow(object sender, EntityTreeRowChangeEventArgs entityTreeRowChangeEvent)
		{

			// Setting this flag will cause the next refresh to incrementally update the data in the tree view.
			this.hasNewData = true;

		}

		/// <summary>
		/// Changes the relationship in the shared data model of the given object in the TreeView.
		/// </summary>
		/// <param name="state">The thread initialization parameter.</param>
		private void ChangeRelation(object state)
		{

			// Extract the thread arguments from the generic parameter.
			ChangeRelationArgs relationInformation = state as ChangeRelationArgs;

			try
			{

				// This channel is used to pass the instruction along to the middle tier.
				TradingSupportClient client = new TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);

				switch (relationInformation.Action)
				{

					case ChangeRelationAction.Update:

						if (!this.IsSubfolder(relationInformation.ChildId, relationInformation.ParentId))
							// Call the shared data model to change the relationship between the parent and the child.
							client.UpdateEntityTree(
								new EntityTree[] { new EntityTree()
									{
										RowId = relationInformation.RelationId,
										ChildId = relationInformation.ChildId,
										ParentId = relationInformation.ParentId,
										RowVersion = relationInformation.RowVersion
									} });
						else
						{

							Entity destination = null;
							Entity source = null;

							lock (DataModel.SyncRoot)
							{
								EntityRow destinationRow = DataModel.Entity.EntityKey.Find(relationInformation.ParentId);
								EntityRow sourceRow = DataModel.Entity.EntityKey.Find(relationInformation.ChildId);

								if (destinationRow != null)
									destination = new Entity(destinationRow);
								if (sourceRow != null)
									source = new Entity(sourceRow);

							}

							if (destination != null && source != null)
								this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
									WindowMoveError.Show(destination, source, relationInformation.ChildId == relationInformation.ParentId)));

						}

					break;

				}

				// The operation was a success. Close down the channel gracefully.
				client.Close();

			}
			catch (FaultException<RecordExistsFault>)
			{

				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					MessageBox.Show(
						Application.Current.MainWindow,
						Properties.Resources.RenameFailedRecordExists,
						Application.Current.MainWindow.Title)));

			}
			catch (Exception)
			{

				// There's no point in getting overly detailed with the error message, either it worked or it didn't.
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					MessageBox.Show(Properties.Resources.OperationFailed, Application.Current.MainWindow.Name)));

			}

		}

		/// <summary>
		/// Handles a change to the DataModel.User table.
		/// </summary>
		/// <param name="sender">User that generated the event.</param>
		/// <param name="userRowChangeEvent">The event arguments.</param>
		private void ChangeUserRow(object sender, UserRowChangeEventArgs userRowChangeEvent)
		{

			// Setting this flag will cause the next refresh to incrementally update the data in the tree view.
			this.hasNewData = true;

		}

		/// <summary>
		/// Initializes the interaction with the data model.
		/// </summary>
		/// <param name="parameter">The unused thread initialization parameter.</param>
		private void CreateData(object parameter)
		{

			try
			{

				// Lock the data model while the tables are read.
				Monitor.Enter(DataModel.SyncRoot);

				// This will provide the initial view of the hierachy tree.  After the initial view, the rest of the changes are 
				// driven from events.
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(UpdateFolderTree), this.SelectedValue)));

				// Install the event handlers for refreshing the FolderTreeView when the data model changes.
				DataModel.User.UserRowChanged += new UserRowChangeEventHandler(ChangeUserRow);
				DataModel.Entity.EntityRowChanged += new EntityRowChangeEventHandler(ChangeEntityRow);
				DataModel.EntityTree.EntityTreeRowChanged += new EntityTreeRowChangeEventHandler(ChangeEntityTreeRow);
				DataModel.EndMerge += new EventHandler(EndMerge);

			}
			finally
			{

				// Allow other threads to access the data model.
				Monitor.Exit(DataModel.SyncRoot);

			}

		}

		/// <summary>
		/// Creates a TreeViewItem with the attributes of an object found in the database.
		/// </summary>
        /// <param name="parentRelationRow">A row that describes how to construct the attributes for this node.</param>
		/// <returns>A node that contains the attributes of the object found in the database, and its descendants.</returns>
		private FolderTreeNode CreateObjectList(EntityTreeRow parentRelationRow)
		{

			// This node will be attached to the tree view in a hierarchical order according to the data found in the 'EntityTree' data structure.  The node
			// itself will be given the properties of the object in the database used to create this node.  The childre are recursively added to the node once
			// it is created.
			FolderTreeNode folderTreeNode = new FolderTreeNode(parentRelationRow);
			EntityRow childRow = parentRelationRow.EntityRowByFK_Entity_EntityTree_ChildId;
            var children = childRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId().OrderBy(child => child.EntityRowByFK_Entity_EntityTree_ChildId.Name);

            foreach (EntityTreeRow childRelationRow in children)
				folderTreeNode.Add(this.CreateObjectList(childRelationRow));
			return folderTreeNode;

		}

		/// <summary>
		/// Removes this control from the data model event handlers.
		/// </summary>
		/// <param name="parameter">Not used.</param>
		private void DestroyData(object parameter)
		{

			try
			{

				// Lock the data model while the tables are read.
				Monitor.Enter(DataModel.SyncRoot);

				// Remove the event handlers.
				DataModel.User.UserRowChanged -= new UserRowChangeEventHandler(ChangeUserRow);
				DataModel.Entity.EntityRowChanged -= new EntityRowChangeEventHandler(ChangeEntityRow);
				DataModel.EntityTree.EntityTreeRowChanged -= new EntityTreeRowChangeEventHandler(ChangeEntityTreeRow);
				DataModel.EndMerge -= new EventHandler(EndMerge);

			}
			finally
			{

				// Allow other threads to access the data model.
				Monitor.Exit(DataModel.SyncRoot);

			}

		}

		/// <summary>
		/// Handles changes to the data model after a reconcilliation.
		/// </summary>
		/// <param name="sender">Object that generated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void EndMerge(object sender, EventArgs e)
		{

			// If any elements of the data model that are displayed in this control have been modified during the reconcillation, then a new version of the
			// tree will be generated.  This method will spawn a worker thread to regenerate the data that appears in the view and will send it to the
			// foreground where it will be used to udpate the current view. Once the event has been handled, it is reset until the data used to generate this
			// view is changed.
			if (this.hasNewData)
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(UpdateFolderTree), this.SelectedValue)));
				this.hasNewData = false;
			}

		}

		/// <summary>
		/// Expand a path in the tree.
		/// </summary>
		/// <param name="path">The list nodes, from shallow to deep, to expand.</param>
		/// <param name="select">If true, select the final node in the path.</param>
		public void ExpandPath(List<FolderTreeNode> path, Boolean select)
		{

			if (path[0].RelationId == Guid.Empty)
				path.RemoveAt(0);

			this.ExpandPath(this, path, select);

		}

		/// <summary>
		/// Do the actual recursive expansion of a path.
		/// </summary>
		/// <param name="container">The container to start expanding in.</param>
		/// <param name="path">The path to expand.</param>
		/// <param name="select">Whether to select the last item in the path.</param>
		private void ExpandPath(ItemsControl container, List<FolderTreeNode> path, Boolean select)
		{

			foreach (object item in container.Items)
			{

				if (path[0].Equals(item))
				{

					path.RemoveAt(0);

					if (container.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
					{

						this.ExpandItem(item, container, path, select);

					}
					else
					{

						EventHandler eh = null;
						
						eh = delegate(object s, EventArgs eventArgs)
						{
							if (container.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
								this.ExpandItem(item, container, path, select);
							container.ItemContainerGenerator.StatusChanged -= eh;
						};

						container.ItemContainerGenerator.StatusChanged += eh;

					}

					break;

				}

			}

		}

		/// <summary>
		/// Expand an individual item (of a path) in the tree.
		/// </summary>
		/// <param name="item">The item to expand.</param>
		/// <param name="container">The container that the item is in.</param>
		/// <param name="path">The rest of the path to expand.</param>
		/// <param name="select">Whether to select the last item in the path.</param>
		private void ExpandItem(object item, ItemsControl container, List<FolderTreeNode> path, Boolean select)
		{

			TreeViewItem nextContainer = container.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

			nextContainer.IsExpanded = true;

			if (path.Count > 0)
				ExpandPath(nextContainer, path, select);
			else if (select)
				nextContainer.IsSelected = true;

		}

		/// <summary>
		/// Recusively search the control for the container control that hosts the given Entity.
		/// </summary>
		/// <param name="parentNode">The current node to be searched.</param>
		/// <param name="treeViewItem">The container control for the parentNode.</param>
		/// <param name="entity">The entity that is to be selected.</param>
		private TreeViewItem FindContainer(FolderTreeNode parentNode, TreeViewItem treeViewItem, Entity entity)
		{

			// The idea here is to recursively search each of the nodes in a TreeView for a container that hosts the given entity. If the container isn't found
			// at the current level of the tree, then each of the child nodes is recursively searched.  When a node is found that containes the given Entity,
			// it is selected.
			if (parentNode.Entity == entity)
			{

				return treeViewItem;

			}
			else
			{

				DependencyObject dependencyObject = null;

				// If the child TreeViewItems haven't been generated for treeViewItem, we'll need to force them to be created.
				// Note, however, that Microsoft recomends doing this.
				if (treeViewItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
					treeViewItem.UpdateLayout();

				foreach (FolderTreeNode childNode in parentNode.Children)
				{

					dependencyObject = treeViewItem.ItemContainerGenerator.ContainerFromItem(childNode);
					if (dependencyObject is TreeViewItem)
					{
						TreeViewItem selectedItem = FindContainer(childNode, dependencyObject as TreeViewItem, entity);
						if (selectedItem != null)
							return selectedItem;
					}

				}

			}

			// There were no containers found on this branch of the tree that owned the Entity.
			return null;

		}



        /// <summary>
        /// Return the item in the tree that is the would-be target of commands.
        /// </summary>
        /// <returns>The TreeViewItem is selected or right-clicked on</returns>
        private TreeViewItem GetCommandTarget()
        {

			FolderTreeNode targetNode = null;
			TreeViewItem target = null;

			try
			{

				if (this.lastContextMenu != null && this.lastContextMenu.IsVisible)
					targetNode = this.lastContextMenu.PlacementTarget.GetValue(FrameworkElement.DataContextProperty) as FolderTreeNode;

				if (targetNode != null)
					target = this.FindContainer(this.Items[0] as FolderTreeNode,
							this.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem, targetNode.Entity);

				if (target == null)
					target = this.FindContainer(this.Items[0] as FolderTreeNode,
							this.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem, this.SelectedValue as Entity);

			}
			catch
			{

			}
				

            return target;

        }

		/// <summary>
		/// Gets the FolderTreeNode of the FolderTreeView from the specified coordinates.
		/// </summary>
		/// <param name="location">The location of the element.</param>
		/// <returns>The FolderTreeNode at the given location.</returns>
		private FolderTreeNode GetItemAtLocation(Point location)
		{

			// This returns the FolderTreeNode at the given coordinates.
			HitTestResult hitTestResults = VisualTreeHelper.HitTest(this, location);
			if (hitTestResults != null && hitTestResults.VisualHit is FrameworkElement)
			{
				FrameworkElement frameworkElement = hitTestResults.VisualHit as FrameworkElement;
				if (frameworkElement.DataContext is FolderTreeNode)
					return frameworkElement.DataContext as FolderTreeNode;
			}

			// This indicates there is no FolderTreeNode at the given coordinates.
			return null;

		}

        /// <summary>
        /// Get the item at a particular point in the tree view control.
        /// </summary>
        /// <returns>The TreeViewItem at that point.</returns>
        private TreeViewItem GetTreeViewItemAtLocation(Point point)
        {

            FolderTreeNode node = this.GetItemAtLocation(point);

            if (node != null)
                foreach (FolderTreeNode rootNode in this.Items)
                {

                    TreeViewItem treeViewItem = this.ItemContainerGenerator.ContainerFromItem(rootNode) as TreeViewItem;
                    TreeViewItem selectedItem = this.FindContainer(rootNode, treeViewItem, node.Entity);

                    if (selectedItem != null)
                        return selectedItem;

                }

            return null;
        }

		/// <summary>
		/// Determine whether a folder is a sub-folder of another one.
		/// </summary>
		/// <param name="source">The ultimate parent folder.</param>
		/// <param name="destination">The folder that may be a sub-folder.</param>
		/// <returns>True if destination is a subfolder of source.</returns>
		private Boolean IsSubfolder(FolderTreeNode source, FolderTreeNode destination)
		{

			Boolean isSubfolder = false;

			if (source.Entity.EntityId == destination.Entity.EntityId)
			{

				isSubfolder = true;

			}
			else
			{

				foreach (FolderTreeNode subfolder in source.Children)
					if (this.IsSubfolder(subfolder, destination))
					{

						isSubfolder = true;
						break;

					}

			}

			return isSubfolder;

		}

		/// <summary>
		/// Determine whether a folder is a sub-folder of another one.
		/// </summary>
		/// <param name="source">The ultimate parent folder.</param>
		/// <param name="destination">The folder that may be a sub-folder.</param>
		/// <returns>True if destination is a subfolder of source.</returns>
		private Boolean IsSubfolder(Guid source, Guid destination)
		{

			lock (DataModel.SyncRoot)
			{

				EntityRow sourceRow = DataModel.Entity.EntityKey.Find(source);
				EntityRow destinationRow = DataModel.Entity.EntityKey.Find(destination);

				return this.IsSubfolder(sourceRow, destinationRow);				

			}

		}

		/// <summary>
		/// Determine whether a folder is a sub-folder of another one.
		/// </summary>
		/// <param name="source">The ultimate parent folder.</param>
		/// <param name="destination">The folder that may be a sub-folder.</param>
		/// <returns>True if destination is a subfolder of source.</returns>
		private Boolean IsSubfolder(EntityRow source, EntityRow destination)
		{

			Boolean isSubfolder = false;

			if (source.EntityId == destination.EntityId)
			{

				isSubfolder = true;

			}
			else
			{

				foreach (EntityTreeRow subfolderRelation in DataModel.EntityTree.Where(row => row.ParentId == source.EntityId))
					if (this.IsSubfolder(subfolderRelation.EntityRowByFK_Entity_EntityTree_ChildId, destination))
					{

						isSubfolder = true;
						break;

					}

			}

			return isSubfolder;

		}

        /// <summary>
        /// Create a new entity with the a particular type, under a parent entity in the hierarchy.
        /// </summary>
        /// <param name="typeId">The TypeId of the type to create the entity as.</param>
        /// <param name="parent">The EntityId of the parent entity.</param>
		/// <param name="tenantId"></param>
        private void New(Guid typeId, Guid parent, Guid tenantId)
        {

            try
            {

				newWatch = System.Diagnostics.Stopwatch.StartNew();

				lock (DataModel.SyncRoot)
				{

					this.newEntity = Entity.Create(typeId, parent, tenantId);
					System.Diagnostics.Debug.WriteLine(String.Format("Blotter create web service call elapsed time: {0}", newWatch.Elapsed));

				}

            }
            catch
            {

				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
	                MessageBox.Show(Application.Current.MainWindow, Properties.Resources.OperationFailed, Application.Current.MainWindow.Name)));

            }

        }

		/// <summary>
		/// Handles a command to collapse the tree structure.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The event data.</param>
		private void OnCollapse(object sender, RoutedEventArgs routedEventArgs)
		{

			TreeViewItem target = GetCommandTarget();

			if (target != null)
				target.IsExpanded = false;

        }

        /// <summary>
        /// Handle the opening of the right-click menu. Set the Collapse/Expand menu item's command correctly, and fill in the available New submenus.
        /// </summary>
        /// <param name="sender">The context menu.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
        private void OnContextMenuOpen(object sender, RoutedEventArgs routedEventArgs)
        {

			TreeViewItem target;

			this.lastContextMenu = sender as ContextMenu;
			target = this.GetCommandTarget();

			if (target != null)
			{

				FolderTreeNode folderTreeNode = (target.DataContext as FolderTreeNode);
				Entity entity = folderTreeNode.Entity;
				ObservableCollection<MenuItem> newItems = new ObservableCollection<MenuItem>();

				if (target.IsExpanded)
					this.lastContextMenu.Resources["expandCollapseCommand"] = GuardianCommands.Collapse;
				else
					this.lastContextMenu.Resources["expandCollapseCommand"] = GuardianCommands.Expand;

				// This disable the the Expand/Collapse menu item when the entity has no children.
				this.lastContextMenu.Resources["hasItems"] = target.HasItems;

				// ... and when the Expand/Collapse menu is disabled, we need to bold the Explore menu item.
				if (!target.HasItems)
					this.lastContextMenu.Resources["exploreFontWeight"] = System.Windows.FontWeights.Bold;
				else
					this.lastContextMenu.Resources["exploreFontWeight"] = System.Windows.FontWeights.Regular;

				if (!folderTreeNode.ContextMenuPopulated)
				{

					List<Control> customMenuItems = entity.GetCustomMenuItems();
					Int32 customMenuStart = 0;


					newItems.Clear();

					lock (DataModel.SyncRoot)
					{

						// Find all the entity types that can be created as children of this entity.
						foreach (TypeTreeRow typeTree in DataModel.TypeTree.Where(row => row.ParentId == entity.TypeId))
						{

							MenuItem typeItem = new MenuItem();

							typeItem.CommandTarget = this;
							typeItem.Command = GuardianCommands.New;
							typeItem.CommandParameter = typeTree.ChildId;
							typeItem.Header = typeTree.TypeRowByFK_Type_TypeTreeChildId.Description;
							typeItem.IsEnabled = true;

							newItems.Add(typeItem);

						}

					}

					// Update the New menu with the new-entity menu items we just created.
					this.lastContextMenu.Resources["newItems"] = newItems;

					// Figure out where we should start inserting the custom menu items.
					for (Int32 index = 0; index < this.lastContextMenu.Items.Count; ++index)
						if ((this.lastContextMenu.Items[index] as Control).Name == "menuCustomization")
							customMenuStart = index;

					// Insert said custom menu items.
					foreach (Control menuItem in customMenuItems)
						this.lastContextMenu.Items.Insert(customMenuStart, menuItem);

					if (customMenuItems.Count > 0)
						this.lastContextMenu.Items.Insert(customMenuStart, new Separator());

					folderTreeNode.ContextMenuPopulated = true;

				}

			}

        }

		/// <summary>
		/// Invoked when an unhandled DragEnter routed event reaches an element in its route that is derived from this class.
		/// </summary>
		/// <param name="eventArgs">The event argments.</param>
		protected override void OnDragEnter(DragEventArgs eventArgs)
		{

			// This will set the effects to indicate the cursor can be moved to the given location
			eventArgs.Effects = DragDropEffects.None;
			Object sourceObject = eventArgs.Data.GetData(DragDropHelper.DataFormat.Name);

			if (sourceObject is FolderTreeNode)
			{

				FolderTreeNode sourceNode = sourceObject as FolderTreeNode;
				FolderTreeNode targetNode = GetItemAtLocation(eventArgs.GetPosition(this));

				// We only want to allow the move if it wouldn't put the entity inside itself or inside one of its descendents.
				if (targetNode != null && !this.IsSubfolder(sourceNode, targetNode))
					eventArgs.Effects = DragDropEffects.Move;

			}

			// This prevents any other element from trying to change the effects.
			eventArgs.Handled = true;

		}

		/// <summary>
		/// Invoked when an unhandled DragLeave routed event reaches an element in its route that is derived from this class.
		/// </summary>
		/// <param name="e">The event argments.</param>
		protected override void OnDragLeave(DragEventArgs e)
		{

			// There is nothing that can be done with any object in this tree outside of the TreeView.
			e.Handled = true;

		}

		/// <summary>
		/// Invoked when a unhahdled DragOver routed event reaches and element in its route that is derived from this class.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDragOver(DragEventArgs e)
		{

			// This makes sure that the source object is a FolderTreeNode.
			Object sourceObject = e.Data.GetData(DragDropHelper.DataFormat.Name);
			if (sourceObject is FolderTreeNode)
			{

				// Extract the source and target nodes from the generic arguments.
				FolderTreeNode sourceNode = sourceObject as FolderTreeNode;
				FolderTreeNode targetNode = GetItemAtLocation(e.GetPosition(this));

				// If a target is found then check to see if dropping it is an option.
				if (targetNode != null)
				{

					// Adding an element to itself creates a circular reference which can kill the application.  This will test to see if the source node is
					// also a child.
					Boolean isTarget = !this.IsSubfolder(sourceNode, targetNode);

					// This indicates whether the node under the cursor is a valid target for a movement operation.
					e.Effects = isTarget ? DragDropEffects.Move : DragDropEffects.None;

				}

			}

			// This prevents any other element from trying to change the effects.
			e.Handled = true;

		}

		/// <summary>
		/// Invoked when an unhandled Drop routed event reaches an element in its route that is derived from this class.
		/// </summary>
		/// <param name="e">The event argments.</param>
		protected override void OnDrop(DragEventArgs e)
		{

			// This makes sure that the source object is a FolderTreeNode.
			Object sourceObject = e.Data.GetData(DragDropHelper.DataFormat.Name);
			if (sourceObject is FolderTreeNode)
			{

				// Extract the source and target nodes from the generic arguments.
				FolderTreeNode sourceNode = sourceObject as FolderTreeNode;
				FolderTreeNode targetNode = GetItemAtLocation(e.GetPosition(this));

				// If a valid target has been identified, then change the relationship between the nodes.
				if (targetNode != null)
				{

					switch (e.Effects)
					{

					case DragDropEffects.Move:

						// A background thread is called to handle the movement of the child to a new parent.
						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(
							ChangeRelation,
							new ChangeRelationArgs(
								ChangeRelationAction.Update,
								sourceNode.Entity.EntityId,
								targetNode.Entity.EntityId,
								sourceNode.RelationId,
								sourceNode.RowVersion));

						break;

					}

				}

			}

		}

        /// <summary>
        /// Handles a command to expand the tree structure.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="routedEventArgs">The event data.</param>
        private void OnExpand(object sender, RoutedEventArgs routedEventArgs)
        {

			TreeViewItem target = this.GetCommandTarget();

			if (target != null)
				target.IsExpanded = true;

        }

		/// <summary>
		/// Handles a command to open the currently selected item in the tree view.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The event data.</param>
		private void OnExplore(object sender, RoutedEventArgs routedEventArgs)
		{

			TreeViewItem target = this.GetCommandTarget();

			if (target != null)
				target.IsSelected = true;

        }

		/// <summary>
		/// Invoked when an unhandled DragDrop.PreviewDragLeave attached event reaches this element. 
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="e">The DragEventArgs that contains the event data.</param>
		private void OnFolderTreeItemPreviewDragLeave(object sender, DragEventArgs e)
		{

			// This property indicates that the drag cursor is no longer over this object.
			if (sender is DependencyObject)
			{
				DependencyObject dropTarget = sender as DependencyObject;
				dropTarget.SetValue(DragDropHelper.IsDragOverProperty, false);
			}
		}

		/// <summary>
		/// Invoked when an unhandled DragDrop.PreviewDragEnter attached event reaches this element. 
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="e">The DragEventArgs that contains the event data.</param>
		private void OnFolderTreeItemPreviewDragEnter(object sender, DragEventArgs e)
		{

			// This property indicates that the drag cursor is no over this object.
			if (sender is DependencyObject)
			{
				DependencyObject dropTarget = sender as DependencyObject;
				dropTarget.SetValue(DragDropHelper.IsDragOverProperty, true);
			}

		}

		/// <summary>
		/// Invoked when an unhandled DragDrop.PreviewDrop attached event reaches this element. 
		/// </summary>
		/// <param name="sender">The object where the event handler is attached.</param>
		/// <param name="e">The DragEventArgs that contains the event data.</param>
		private void OnFolderTreeItemPreviewDrop(object sender, DragEventArgs e)
		{

			// This property indicates that the drag cursor is no longer over this object.
			if (sender is DependencyObject)
			{
				DependencyObject dropTarget = sender as DependencyObject;
				dropTarget.SetValue(DragDropHelper.IsDragOverProperty, false);
			}

		}

		/// <summary>
		/// Handle a command to go to the bottom of the expanded tree.
		/// </summary>
		/// <param name="sender">The tree view.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnGoBottom(object sender, RoutedEventArgs eventArgs)
		{

			FolderTreeNode entity = this.ItemsSource as FolderTreeNode;

			if (entity != null && entity.Children.Count > 0)
			{

				TreeViewItem container = this.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
				FolderTreeNode select = null;

				entity = entity.Children[0];

				while (select == null && container != null)
				{

					if (container.IsExpanded)
					{

						FolderTreeNode child = entity.Children[entity.Children.Count - 1];
						container = FindContainer(entity, container, child.Entity);
						entity = child;

					}
					else
						select = entity;

				}

				if (select != null)
				{

					this.SelectedValue = select.Entity;

				}

			}

		}

		/// <summary>
		/// Handle a command to go to the top of the tree.
		/// </summary>
		/// <param name="sender">The tree view.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnGoTop(object sender, RoutedEventArgs eventArgs)
		{

			FolderTreeNode user = this.ItemsSource as FolderTreeNode;

			if (user != null && user.Children.Count > 0)
				this.SelectItem(user.Children[0].Entity);

		}

		/// <summary>
		/// Handle a command to go up a level in the tree.
		/// </summary>
		/// <param name="sender">The tree view.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnGoUp(object sender, RoutedEventArgs eventArgs)
		{
			TreeViewItem source = this.GetCommandTarget();

			if (source != null && source.Header is FolderTreeNode)
			{
				FolderTreeNode folderTreeNode = source.Header as FolderTreeNode;

				// This is initiated via a command and since we are in the foreground we need to call into a background thread to access the datamodel to handle the go up command.
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(GoUp, folderTreeNode);
			}

		}

		private void GoUp(object state)
		{

			FolderTreeNode folderTreeNode = (FolderTreeNode)state;

			lock (DataModel.SyncRoot)
			{

				EntityTreeRow relationship = DataModel.EntityTree.EntityTreeKey.Find(folderTreeNode.RelationId);
				
				//Call back to the foreground to update the UI.
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
				{
					TreeViewItem parent = FindContainer(
						this.Items[0] as FolderTreeNode,
						this.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem,
						new Entity(relationship.EntityRowByFK_Entity_EntityTree_ParentId));

					if (parent != null)
						parent.IsSelected = true;
				}));

			}

		}	
		/// <summary>
		/// Starts a thread that attaches the control into the data model.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs e)
		{

			// This control window uses the events of the data model to drive changes out to the user interface.  Those event handlers, since they are part of
			// the client data model, can only be accessed from a background thread capable of locking the data model from other threads.  Note that trying to
			// load assemblies during design time will crash the designer because it doesn't use the same probing logic as the runtime (go figure), so the
			// background initialization is skipped at design time.
			if (!this.IsDesignMode)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(CreateData));

		}
	
		/// <summary>
		/// Opens a dialog box to link in a new entity to one already in the tree.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnLinkEntity(object sender, RoutedEventArgs e)
		{

			WindowLinkFolder chooser = new WindowLinkFolder();
			TreeViewItem target = this.GetCommandTarget();

			if (target != null
					&& target.DataContext is FolderTreeNode)
			{

				chooser.Owner = Application.Current.MainWindow;
				chooser.ParentFolder = (target.DataContext as FolderTreeNode).Entity;
				chooser.ShowDialog();

			}

		}

        /// <summary>
        /// Handles a command to add a new entity to the tree view.
        /// </summary>
        /// <param name="sender">The object that originated the event (ie. one of the New submenu items).</param>
        /// <param name="routedEventArgs">The event data.</param>
        private void OnNew(object sender, RoutedEventArgs routedEventArgs)
        {

            ExecutedRoutedEventArgs executedRoutedEventArgs = routedEventArgs as ExecutedRoutedEventArgs;
			TreeViewItem target = this.GetCommandTarget();

			if (target != null
					&& target.DataContext is FolderTreeNode
					&& executedRoutedEventArgs != null
					&& executedRoutedEventArgs.Parameter is Guid)
            {

				FolderTreeNode parent = target.DataContext as FolderTreeNode;
				Guid typeId = (Guid)executedRoutedEventArgs.Parameter;
				Guid tenantId = parent.Entity.TenantId;

				// Creating the entity takes an unknown amount of time, so we spin it off into the background.
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.New(typeId, parent.Entity.EntityId, tenantId));

            }

        }

		/// <summary>
		/// Handles a command to change the properties of the currently selected item in the tree view.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The event data.</param>
		private void OnProperties(object sender, RoutedEventArgs routedEventArgs)
		{

			try
			{

				TreeViewItem target = this.GetCommandTarget();

				if (target != null && target.DataContext is FolderTreeNode)
				{

					Entity entity = (target.DataContext as FolderTreeNode).Entity;
					entity.ShowProperties(this.ItemsSource as FolderTreeNode);
				}

			}
			catch (Exception exception)
			{

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Handles a command to rename the currently selected item in the tree view.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The event data.</param>
		private void OnRename(object sender, RoutedEventArgs routedEventArgs)
		{

			TreeViewItem target = this.GetCommandTarget();
            TreeViewItem returnSelection = FindContainer(this.Items[0] as FolderTreeNode,
                    this.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem, this.SelectedValue as Entity);

			if (target != null)
				ShowRenameControl(target, returnSelection);

		}

        /// <summary>
        /// Handle Escape and Enter keys on the edit-item-name control. If the user hits Enter, the new name is saved to the entity. If the user hits
        /// Escape, the rename is canceled.
        /// </summary>
        /// <param name="entity">The entity to rename.</param>
        /// <param name="parent">The panel that contains the edit-name control (so we may remove it).</param>
		/// <param name="returnSelection">The selected tree view item.</param>
		/// <param name="sender">The edit-name control.</param>
        /// <param name="eventArgs">The key arguments.</param>
        private void OnEditNameKeyDown(Entity entity, Panel parent, TreeViewItem returnSelection, object sender, KeyEventArgs eventArgs)
        {

            TextBox editName = sender as TextBox;

			(VisualTreeHelper.GetParent(parent) as TreeViewItem).IsHitTestVisible = true;

            if (editName != null)
            {

				try
				{

					if (eventArgs.Key == Key.Enter)
					{

						Entity updatedEntity = entity.Clone() as Entity;

						updatedEntity.Name = editName.Text;
						FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Rename(updatedEntity));
						parent.Children.Remove(editName);
						editName.Visibility = Visibility.Hidden;
						returnSelection.Focus();
						this.SetValue(DragDropHelper.IsDragSourceProperty, true);

					}
					else if (eventArgs.Key == Key.Escape)
					{

						parent.Children.Remove(editName);
						editName.Visibility = Visibility.Hidden;
						returnSelection.Focus();
						this.SetValue(DragDropHelper.IsDragSourceProperty, true);

					}

				}
				catch (Exception exception)
				{

					EventLog.Error("Error closing rename control: {0}\n{1}", exception.Message, exception.StackTrace);

				}

            }

        }

		/// <summary>
		/// Handle the edit-item-name control losing focus by saving the new name.
		/// </summary>
		/// <param name="entity">The entity to rename.</param>
		/// <param name="parent">The panel that contains the edit-name control (so we may remove it).</param>
		/// <param name="sender">The edit-name control.</param>
		/// <param name="eventArgs">The key arguments.</param>
		private void OnEditNameLostFocus(Entity entity, Panel parent, object sender, EventArgs eventArgs)
		{

			TextBox editName = sender as TextBox;

			(VisualTreeHelper.GetParent(parent) as TreeViewItem).IsHitTestVisible = true;

			if (editName != null && editName.Visibility == Visibility.Visible)
			{

				try
				{

					Entity updatedEntity = entity.Clone() as Entity;

					updatedEntity.Name = editName.Text;
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(data => this.Rename(updatedEntity));
					parent.Children.Remove(editName);
					this.SetValue(DragDropHelper.IsDragSourceProperty, true);

				}
				catch (Exception exception)
				{

					EventLog.Error("Error closing rename control: {0}\n{1}", exception.Message, exception.StackTrace);

				}

			}

		}

        /// <summary>
        /// Handles a command to delete the currently selected item in the tree view.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="routedEventArgs">The event data.</param>
        private void OnDelete(object sender, ExecutedRoutedEventArgs routedEventArgs)
        {

			TreeViewItem target = this.GetCommandTarget();

			if (target != null && target.DataContext is FolderTreeNode)
            {

                WindowDeleteSingle windowDeleteFolder = new WindowDeleteSingle();
                windowDeleteFolder.Owner = Application.Current.MainWindow;
				windowDeleteFolder.Entity = (target.DataContext as FolderTreeNode).Entity;
                if (!windowDeleteFolder.IsVisible) windowDeleteFolder.ShowDialog();

            }

        }
        
		/// <summary>
		/// Handles a changed to the selected item in the MarkThree.Windows.FolderTreeView.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event data.</param>
		private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{

			// The 'SelectedValue' property of the base class is a read only value.  The 'SelectedValue' property of the FolderTreeView reflects the currently
			// selected viewer and can be set programatically by the navigator in addition to the user actions.  This code keeps the subclassed property
			// synchronized with the user's selection.
			if (this.SelectedItem != null)
				this.SetValue(FolderTreeView.SelectedValueProperty, base.SelectedValue);
			else
				this.SetValue(FolderTreeView.SelectedValueProperty, null);
  		}

		/// <summary>
		/// Handles a change to the Menu property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnSelectedValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the variables from the generic arguments.
			FolderTreeView folderTreeView = dependencyObject as FolderTreeView;
			Entity newValue = dependencyPropertyChangedEventArgs.NewValue as Entity;

			//Use the extension method to expand tree and select node.
			folderTreeView.SelectItem(newValue);
		
		}

		/// <summary>
		/// Handle the the left mouse button being pressed. Mark the event is handled so the TreeViewItem isn't selected (yet).
		/// </summary>
		/// <param name="sender">The TreeViewItem the mouse button was pressed on.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnTreeViewItemMouseDown(object sender, MouseButtonEventArgs eventArgs)
		{

			if (eventArgs.ChangedButton == MouseButton.Left)
			{

				eventArgs.Handled = true;

			}

		}

		/// <summary>
		/// Handle the left mouse button being released. Select the TreeViewItem the mouse is currently over.
		/// </summary>
		/// <param name="sender">The display element the mouse is over.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnTreeViewItemMouseUp(object sender, MouseButtonEventArgs eventArgs)
		{

			if (eventArgs.ChangedButton == MouseButton.Left)
			{

				FrameworkElement displayWidget = sender as FrameworkElement;

				if (displayWidget != null)
				{

					FolderTreeNode node = displayWidget.DataContext as FolderTreeNode;

					if (node != null)
					{

						TreeViewItem item = this.FindContainer(this.Items[0] as FolderTreeNode,
									this.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem, node.Entity);

						if (item != null)
						{

							item.IsSelected = true;
							eventArgs.Handled = true;

						}

					}

				}

			}

		}

		/// <summary>
		/// Opens a dialog box to unlink an entity from another.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnUnlinkEntity(object sender, ExecutedRoutedEventArgs e)
		{

			WindowUnlinkFolder chooser = new WindowUnlinkFolder();

			chooser.Owner = Application.Current.MainWindow;
			chooser.ParentFolder = e.Parameter as Entity;
			chooser.ShowDialog();

		}

		/// <summary>
		/// Shuts down the application when the main window is unloaded.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
		{

			// When the window handle is destroyed, this control will remove itself from the data model event handlers.  Note again that attempting to access
			// the data model from the Visual Studio designer will crash the designer.
			if (!this.IsDesignMode)
				FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new WaitCallback(DestroyData));

		}

		/// <summary>
		/// Rename and entity.
		/// </summary>
		/// <param name="entity">The entity to commit.</param>
		private void Rename(Entity entity)
		{

			try
			{

				entity.Commit();

			}
			catch (FaultException<ArgumentFault>)
			{

				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					MessageBox.Show(
						Application.Current.MainWindow,
						Properties.Resources.RenameFailedInvalidName,
						Application.Current.MainWindow.Title)));

			}
			catch (FaultException<RecordExistsFault>)
			{

				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					MessageBox.Show(
						Application.Current.MainWindow,
						Properties.Resources.RenameFailedRecordExists,
						Application.Current.MainWindow.Title)));

			}
			catch
			{

				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, Properties.Resources.OperationFailed, Application.Current.MainWindow.Title)));

			}

		}

        /// <summary>
        /// Display rename edit box for a particular node in the tree.
        /// </summary>
        /// <param name="selection">The item to display the edit box for.</param>
        /// <param name="returnSelection">The selected tree view item.</param>
        private void ShowRenameControl(TreeViewItem selection, TreeViewItem returnSelection)
        {

			FolderTreeNode selectedNode = selection.DataContext as FolderTreeNode;
			Entity entity = selectedNode.Entity;
			Grid grid = VisualTreeHelper.GetChild(selection, 0) as Grid;
			TextBox editName = new TextBox();

			this.SetValue(DragDropHelper.IsDragSourceProperty, false);

			// If the user hits enter, save the new name; if they hit escape, cancel the edit.
			editName.KeyUp += (object s, KeyEventArgs a) =>
					this.OnEditNameKeyDown(entity, grid, returnSelection, s, a);
			// If the user clicks somewhere else, save the new name.
			editName.LostFocus += (s, a) =>
					this.OnEditNameLostFocus(entity, grid, s, a);
			editName.Text = entity.Name;

			editName.SetValue(Grid.ColumnProperty, 1);
			editName.SetValue(Panel.ZIndexProperty, 1);
			editName.HorizontalAlignment = HorizontalAlignment.Left;
			// This margin makes the edit box line-up with the name-label beneath it.
			editName.Margin = new Thickness(18.0, -1, 0, -1);
			// Base the edit box's size on the containing grid to make sure we cover up the old name.
			editName.Width = grid.ColumnDefinitions[1].ActualWidth - editName.Margin.Left;
			grid.Children.Add(editName);
			editName.Visibility = Visibility.Visible;
			editName.Focus();
			editName.SelectAll();

        }

		/// <summary>
		/// Updates the TreeView data structures from a background thread.
		/// </summary>
		/// <param name="state">The unused thread initialization parameter.</param>
		private void UpdateFolderTree(object state)
		{

			try
			{

				//This will lock until we are all logged in.  We should not lock the Datamodel until we are logged in.
				ChannelStatus.LogggedInEvent.WaitOne();
				Guid userId = UserContext.Instance.UserId;

				lock (DataModel.SyncRoot)
				{

					EntityRow userEntityRow;

					// The user identifier drives the selection of items for the navigation tree.  A hierarchical organization of objects (blotters, appraisals,
					// etc.) is constructed using the object tree to associate the user to those objects to which they've been granted access.
					userEntityRow = DataModel.Entity.EntityKey.Find(userId);

					// Make sure the user themself has been loaded but also the entity row which we need.
					if (DataModel.User.UserKey.Find(userId) != null && userEntityRow != null)
					{

						// This creates a hierarchical tree of objects that the current user can view.  The top level node is the user and the next level is all
						// objects in that user's system folder.
						FolderTreeNode rootNode = new FolderTreeNode(userEntityRow);
						// See if there is a newly created node that the user expects to be selected.
						EntityRow newEntityRow = this.newEntity == null ? null : DataModel.Entity.EntityKey.Find(this.newEntity.Value);
						FolderTreeNode selectedValue = null;

						if (newEntityRow != null)
						{

							System.Diagnostics.Debug.WriteLine(String.Format("Blotter create reconciled entity row elapsed time: {0}", newWatch.Elapsed));

							selectedValue = new FolderTreeNode() { Entity = Entity.New(DataModel.Entity.EntityKey.Find(this.newEntity.Value)) };
							this.newEntity = null;

						}

						// At this point, the tree is complete.  Pass the tree to the foreground for where it will be copied into any existing hierarchy.
						this.Dispatcher.BeginInvoke(
								DispatcherPriority.Normal,
								new TreeViewItemDelegate(UpdateFolderTree),
								rootNode,
								selectedValue,
								newEntityRow == null ? null : GuardianCommands.Rename);

					}

				}

			}
			catch (Exception exception)
			{

				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, Properties.Resources.FolderUpdateFailed, Application.Current.MainWindow.Name)));

				// Catch the most general error and send it to the debug console.
				EventLog.Error("Tree Update error: {0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Populates the Tree Control with data collected in the initialization thread.
		/// </summary>
        /// <param name="sourceNode">Contains a hierarchical organization of objects.</param>
		/// <param name="selectedValue">What the new SelectedValue should be.</param>
		/// <param name="command">Which command, if any, to execute after the update.</param>
		private void UpdateFolderTree(FolderTreeNode sourceNode, FolderTreeNode selectedValue, RoutedCommand command)
		{

			// This will catch a corrupted folder hierarchy and clear out the tree view.
			if (sourceNode.Entity == null)
			{

				// Clear out the tree view.  The application will be useless until the user logs in with another identity, but this action will prevent the
				// application from crashing and provides feedback that something needs to be fixed.
				this.ItemsSource = null;
				this.SelectedValue = null;

			}
			else
			{

				// The logic below will refresh the folder tree.  This variable is used to preserve the current selection.  If the current selection is not
				// part of the folder tree view after the update, then the root of the tree is selected to be the current item.
				FolderTreeNode originalNode = this.SelectedItem as FolderTreeNode;

				// The source tree contains the current state of the hierarchy of objects used to navigate the application.  The 'ItemsSource' of the TreeView
				// control contains the current hierarchy.  The current tree is reconciled with the new tree and the changes are automatically propogated to
				// the control through binding to the VisualCollection events.
				FolderTreeNode targetNode = this.ItemsSource as FolderTreeNode;

				// This is will keep track of the IsExpanded state of every node in the current tree. After updating the tree, we'll make sure the tree is
				// still similarly expanded.
				TreeViewHelper.ExpandState expandState = this.RecordExpandState();

				targetNode.Copy(sourceNode);

                if (targetNode.Children.Count != 0)
                {

					//Raise the event to let the main window know that we have new data.
					RoutedEventArgs newEventArgs = new RoutedEventArgs(FolderTreeView.TreeUpdatedEvent);
					Entity selection = null;

					this.ReplayExpandState(expandState);

					newEventArgs.Source = targetNode.Children[0];
					this.RaiseEvent(newEventArgs);

					if (selectedValue != null && targetNode.Contains(selectedValue))
						selection = selectedValue.Entity;
					else if (originalNode != null && targetNode.Contains(originalNode))
						selection = originalNode.Entity;

					if (selection != null)
					{

						List<FolderTreeNode> pathToSelection = (this.ItemsSource as FolderTreeNode).FindPath(selection);
						this.ExpandPath(pathToSelection, true);

					}

					if (command != null)
					{

						System.Diagnostics.Debug.WriteLine(String.Format("Blotter create tree update with entity elapsed time: {0}", newWatch.Elapsed));

						command.Execute(null, this);

					}

                }

			}

		}

	}

}

namespace FluidTrade.Guardian.Windows
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
	using System.Collections.Generic;

    /// <summary>
    /// Extension class to traverse through treeview control.
    /// </summary>
    public static class TreeViewHelper
    {

		/// <summary>
		/// A helper class for tracking the expanded/collapsed state of a tree node.
		/// </summary>
		public class ExpandState
		{

			/// <summary>
			/// The node at the current level.
			/// </summary>
			public FolderTreeNode Node;
			/// <summary>
			/// If expanded, the children underneath this level. Otherwise, null.
			/// </summary>
			public List<ExpandState> Children;

		}

        /// <summary>
        /// Searches through the tree expanding along the way. http://blog.quantumbitdesigns.com/tag/treeview/
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TreeViewItem SelectItem(this TreeView treeView, object item)
        {
            return ExpandAndSelectItem(treeView, item);
        }


        /// <summary>
        /// Find the item by expanding the nodes in tree
        /// </summary>
        /// <param name="parentContainer"></param>
        /// <param name="itemToSelect"></param>
        /// <returns></returns>
        private static TreeViewItem ExpandAndSelectItem(ItemsControl parentContainer, object itemToSelect)
        {
            //check all items at the current level
            foreach (FolderTreeNode item in parentContainer.Items)
            {
                //This will return null if the node is expanded yet
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (item.Entity == itemToSelect && currentContainer != null)
                {
                    currentContainer.IsSelected = true;
                    currentContainer.BringIntoView();
                    currentContainer.Focus();
                    return currentContainer;
                }
            }

            //If we get to this point, the selected item was not found at the current level, so we must check the children                    
            foreach (Object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //If children exists.
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    //Keep track of if the TreeViewItem was expanded or not
                    Boolean wasExpanded = currentContainer.IsExpanded;

                    //Expand the current TreeViewItem so we can check its child TreeViewItems
                    currentContainer.IsExpanded = true;

                    //If the TreeViewItem child containers have not been generated, we must listen to
                    //the StatusChanged event until they are
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        //Store the event handler in a variable so we can remove it (in the handler itself)
                        EventHandler eh = null;
                        eh = new EventHandler(delegate
                        {
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                TreeViewItem itemInCurrentHeirachy = ExpandAndSelectItem(currentContainer, itemToSelect);
                                if (itemInCurrentHeirachy == null)
                                {
                                    //The assumption is that code executing in this EventHandler is the result of the parent not
                                    //being expanded since the containers were not generated.
                                    //since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
									currentContainer.IsExpanded = wasExpanded;
                                }

                                //Remove the StatusChanged event handler since we just handled it (we only needed it once)
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                            }
                        });
                        currentContainer.ItemContainerGenerator.StatusChanged += eh;                        
                    }                    
                    else 
                    {
                        //Otherwise the containers have been generated, so look for item to select in the children.
                        TreeViewItem itemInCurrentHeirachy = ExpandAndSelectItem(currentContainer, itemToSelect);
                        if (itemInCurrentHeirachy == null)
                        {
                            //Restore the current TreeViewItem's expanded state
                            currentContainer.IsExpanded = wasExpanded;
                        }                        
                        else 
                        {
                            //Otherwise the node was found and selected, so return true
                            return itemInCurrentHeirachy;
                        }
                    }
                }
            }

            //No item was found
            return null;       
        }

		/// <summary>
		/// Create a record of the expanded/collapsed state of the tree.
		/// </summary>
		/// <param name="view">The tree view.</param>
		/// <returns>The record of the expanded/collapsed state.</returns>
		public static ExpandState RecordExpandState(this FolderTreeView view)
		{

			ExpandState expand = new ExpandState() { Node = view.ItemsSource as FolderTreeNode, Children = new List<ExpandState>() };

			foreach (FolderTreeNode node in expand.Node)
			{

				TreeViewItem treeViewItem = view.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;

				if (treeViewItem != null)
					expand.Children.Add(TreeViewHelper.RecordExpandState(treeViewItem));
				// We can't wait for the item to be generated, so we'll to ignore it.
				else
					expand.Children.Add(new ExpandState() { Node = node, Children = null });

			}

			return expand;

		}

		/// <summary>
		/// Create a record of the expanded/collapsed state of a sub-tree.
		/// </summary>
		/// <param name="item">The tree view item.</param>
		/// <returns>The record of the expanded/collapsed state.</returns>
		private static ExpandState RecordExpandState(TreeViewItem item)
		{

			ExpandState expand = new ExpandState() { Node = item.DataContext as FolderTreeNode };

			if (item.IsExpanded)
			{

				expand.Children = new List<ExpandState>();

				foreach (FolderTreeNode node in expand.Node)
				{

					TreeViewItem treeViewItem = item.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;

					if (treeViewItem != null)
						expand.Children.Add(TreeViewHelper.RecordExpandState(treeViewItem));
					// We can't wait for the item to be generated, so we'll to ignore it.
					else
						expand.Children.Add(new ExpandState() { Node = node, Children = null });

				}

			}

			return expand;

		}

		/// <summary>
		/// Replay an expanded state record to return a tree to its previously expanded/collapsed state.
		/// </summary>
		/// <param name="view">The tree view.</param>
		/// <param name="expand">The expand record.</param>
		public static void ReplayExpandState(this FolderTreeView view, ExpandState expand)
		{

			if (view.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{

				TreeViewHelper.ReplayExpandState(view, expand.Children);

			}
			else
			{

				EventHandler replay = null;

				replay = delegate(object sender, EventArgs eventArgs)
				{
					if (view.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
					{

						view.ItemContainerGenerator.StatusChanged -= replay;
						TreeViewHelper.ReplayExpandState(view, expand.Children);

					}
				};

				view.ItemContainerGenerator.StatusChanged += replay;

			}

		}

		/// <summary>
		/// Replay an expanded state record to return a sub-tree to its previously expanded/collapsed state.
		/// </summary>
		/// <param name="item">The tree view item.</param>
		/// <param name="expand">The expand record.</param>
		private static void ReplayExpandState(TreeViewItem item, ExpandState expand)
		{

			if (expand.Children == null)
			{

				item.IsExpanded = false;

			}
			else
			{

				item.IsExpanded = true;

				if (item.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
				{

					TreeViewHelper.ReplayExpandState(item, expand.Children);
					
				}
				else
				{

					EventHandler replay = null;

					replay = delegate(object sender, EventArgs eventArgs)
					{
						if (item.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
						{

							item.ItemContainerGenerator.StatusChanged -= replay;
							TreeViewHelper.ReplayExpandState(item, expand);

						}
					};

					item.ItemContainerGenerator.StatusChanged += replay;

				}

			}

		}

		/// <summary>
		/// Replay an expanded state record to return a sub-tree to its previously expanded/collapsed state.
		/// </summary>
		/// <param name="item">The tree view control or item.</param>
		/// <param name="expand">The list of child expand records.</param>
		private static void ReplayExpandState(ItemsControl item, List<ExpandState> expand)
		{

			foreach (ExpandState expandState in expand)
			{

				TreeViewItem treeViewItem = item.ItemContainerGenerator.ContainerFromItem(expandState.Node) as TreeViewItem;

				// If the treeViewItem is null, then the original node was deleted and we don't need to worry about it.
				if (treeViewItem != null)
					TreeViewHelper.ReplayExpandState(treeViewItem, expandState);

			}

		}

    }
}

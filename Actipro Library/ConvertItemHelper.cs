namespace FluidTrade.Actipro
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using ActiproSoftware.Windows.Controls.Navigation;
    using System.Linq;
    using System.Text;

	/// <summary>
	/// This class includes helper methods for working with the Breadcrumb ConvertItem event.
	/// </summary>
	public static class ConvertItemHelper 
    {

        /// <summary>
        ///  GetPath of the item
        /// </summary>
        /// <param name="rootItem"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetPath(object rootItem, object item)
        {
            FolderNavNode rootNode = rootItem as FolderNavNode;
            FolderNavNode targetNode = item as FolderNavNode;

            if (null == rootNode || null == targetNode)
            {
                return string.Empty;
            }

            if (rootNode.Entity.EntityId == targetNode.Entity.EntityId)
                return rootNode.Entity.Name;

            TreeTraverseHelper.Node<FolderNavNode> leafNode = rootNode.FindLeafNode(
                t => t.Entity.EntityId == targetNode.Entity.EntityId,
                (parent) => parent.Children
                );                        


             //Unwind the leaf node.
            if (leafNode != null)
            {
                List<FolderNavNode> listPath = new List<FolderNavNode>();
                TreeTraverseHelper.Node<FolderNavNode> currentNode = leafNode;
                do
                {
                    listPath.Add(currentNode.Item);
                    currentNode = currentNode.Parent;
                }
                while (currentNode != null);

                listPath.Reverse();

                StringBuilder path = new StringBuilder(rootNode.Entity.Name);
                foreach(var node in listPath)
                {
                    path.Append(Path.DirectorySeparatorChar + node.Entity.Name);
                }

                return path.ToString();
            }            
            
            return String.Empty;
        }

        /// <summary>
		/// Gets the id for the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
        public static string GetObjectName(object item) 
        {
			if (item is FolderNavNode) 
            {
				FolderNavNode FolderNavNode = item as FolderNavNode;
                return FolderNavNode.Entity.Name;  
			}
			else 
            {
				EntityNode entity = item as EntityNode;
				if (null != entity) {
					return entity.Name;
				}
			}

			return String.Empty;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootItem"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IList GetTrail(object rootItem, object item)
        {            
            string path = GetPath(rootItem, item);
            return GetTrail(rootItem, path);
        }

		/// <summary>
		/// Gets the trail for the specified item.
		/// </summary>
		/// <param name="rootItem">The root item.</param>
		/// <param name="path">The item.</param>
		/// <returns></returns>        
		public static IList GetTrail(object rootItem, string path) 
        {        
			// Make sure the specified path is valid
            if (string.IsNullOrEmpty(path))
				return null;

			// If the root item was not passed, then we cannot build a trail
            FolderNavNode rootNode = rootItem as FolderNavNode;
            if (null == rootNode)
				return null;

            return GetPathList(path, rootNode);

        }

        private static IList GetPathList(string path, FolderNavNode rootNode)
        {
            // Break the path up based on the available path separators
            string[] pathEntries = path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);
            if (null == pathEntries || 0 == pathEntries.Length)
                return null;

            List<object> trail = new List<object>();
            trail.Add(rootNode);
           
            IEnumerable<FolderNavNode> currentDepthCollection = rootNode;            
            //Start from 1 since the rootnode is already added.
            for(int index = 1; index < pathEntries.Length; index++)
            {
                bool bEntryFound = false;
                foreach (var treeNode in currentDepthCollection)
                {
                    if (string.CompareOrdinal(treeNode.Entity.Name, pathEntries[index]) == 0)
                    {
                        trail.Add(treeNode);
                        bEntryFound = true;
                        currentDepthCollection = treeNode.Children;
                        break;
                    }
                }

                if(false == bEntryFound)
                {
                    ReportPathError(path);
                    return null;
                }            
            }    
            return trail;   
        }

        private static void ReportPathError(string path)
        {
            string errorMessage = string.Format("'{0}' does not exist. Please check the spelling and try again.", path);            
            MessageBox.Show(errorMessage, "Address Bar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
            
		/// <summary>
		/// Handles the ConvertItem event of a Breadcrumb control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="BreadcrumbConvertItemEventArgs"/> instance containing the event data.</param>
		public static void HandleConvertItem(object sender, BreadcrumbConvertItemEventArgs e) 
        {
			if (BreadcrumbConvertItemTargetType.Path == e.TargetType) 
            {
                Breadcrumb breadCrumb = sender as Breadcrumb;
                if (null != breadCrumb)
                {
                    // Convert either the item or the trail to a path
                    object item = e.Item;
                    if (null == item && null != e.Trail && 0 != e.Trail.Count)
                        item = e.Trail[e.Trail.Count - 1];

                    e.Path = GetPath(breadCrumb.RootItem, item);
                }
			}
			else if (BreadcrumbConvertItemTargetType.Trail == e.TargetType) {
				IList trail = null;
				if (null != e.Path)
					trail = GetTrail(e.RootItem, e.Path);
				else if (null != e.Item)
					trail = GetTrail(e.RootItem, e.Item);

				if (null == trail) {
					return;
				}

				e.Trail = trail;
			}
			else {
				throw new NotImplementedException("Unsupported Breadcrumb target type");
			}
		}
	}
}


//public static IList GetTrail(object rootItem, Guid path) 
//        {        
//            // Make sure the specified path is valid
//            if (path == Guid.Empty)
//                return null;

//            // If the root item was not passed, then we cannot build a trail
//            FolderNavNode rootNode = rootItem as FolderNavNode;
//            if (null == rootNode)
//                return null;

//            MarkThree.Guardian.Utilities.TreeTraverseHelper.Node<FolderNavNode> leafNode = rootNode.FindLeafNode(
//                t => t.Entity.EntityId == path,
//                (parent) => parent.Children
//                );                        

//            List<object> trail = new List<object>();
//            trail.Add(rootNode);

//            //Unwind the leaf node.
//            if (leafNode != null)
//            {
//                List<Object> listPath = new List<Object>();
//                MarkThree.Guardian.Utilities.TreeTraverseHelper.Node<FolderNavNode> currentNode = leafNode;
//                do
//                {
//                    listPath.Add(currentNode.Item);
//                    currentNode = currentNode.Parent;
//                }
//                while (currentNode != null);

//                listPath.Reverse();
//                trail.AddRange(listPath);
//            }
            
        
//            return trail;
//        }

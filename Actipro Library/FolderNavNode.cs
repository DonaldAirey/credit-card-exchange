using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace FluidTrade.Actipro
{
    /// <summary>
    /// 
    /// </summary>
    public class FolderNavNode : ObservableCollection<FolderNavNode>
    {

        private EntityNode entity;
        private Int64 rowVersion;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FolderNavNode()
		{
            this.entity = new EntityNode();
        }


        /// <summary>
        /// The children of this node (required for Hierarchical Data Templates in WCF).
        /// </summary>
        public ObservableCollection<FolderNavNode> Children { get { return this; } }

        /// <summary>
        /// 
        /// </summary>
        public EntityNode Entity
        {
            get
            {
                return this.entity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Int64 RowVersion
        {
            get
            {
                return this.rowVersion;
            }

            set
            {
                this.rowVersion = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentSourceNode"></param>
        public void Copy(FolderNavNode parentSourceNode)
        {

            // Copy the source object to to the destiation object.  This includes most of the properties seen in the tree, the 
            // name, the image, the description, etc.
            this.entity.Copy(parentSourceNode.entity);

            // The next step is to reconcile the child nodes between the source and the destination nodes.  This list keeps track
            // of what needs to be copied from the source tree to the destination tree and what needs to be added as new.
            List<FolderNavNode> childSourceNodes = new List<FolderNavNode>();
            foreach (FolderNavNode folderTreeNode in parentSourceNode.Children)
                childSourceNodes.Add(folderTreeNode);

            // The main idea here is to run through all the nodes looking for Adds, Deletes and Updates.  When we find a Node that
            // shouldn't be in the structure, it's deleted from the tree.  Since the tree is made up of linked lists, we can't
            // rightly delete the current link in the list.  For this reason we need to use the index into the list for spanning
            // the structure instead of the collection operations.  When we find an element that needs to be removed, we can delete
            // it and the index will get us safely to the next element in the list. The first loop scans the list already in the
            // TreeView structure.
            for (int targetIndex = 0; targetIndex < this.Children.Count; targetIndex++)
            {

                // Get a reference to the next child in the target list.
                FolderNavNode childTargetNode = this.Children[targetIndex];

                // If we don't find the target (older) element in the source (newer) list, it will be deleted.
                bool found = false;

                // Cycle through all of the source (newer) elements looking for changes and removing any elements that
                // exist in both lists.
                for (int sourceIndex = 0; sourceIndex < childSourceNodes.Count; sourceIndex++)
                {

                    // This will compare the child FolderTreeNodes from the source tree to the target.  If the target node
                    // exists in the source list, it is updated with the information found in the source.
                    FolderNavNode childSourceNode = childSourceNodes[sourceIndex];

                    // If the elements are equal (as defined by the equality operator of the object), then recurse into
                    // the structure looking for changes to the children.  After that, check the Node for any changes
                    // since it was added to the tree.
                    if (childTargetNode.Entity.EntityId == childSourceNode.Entity.EntityId)
                    {

                        // Recurse down into the tree structures bringing all the children in sync with the new
                        // structure.
                        childTargetNode.Copy(childSourceNode);

                        // At this point, we've checked all the children and applied any changes to the node.  Remove
                        // it from the list.  Any elements left in the source list are assumed to be new members and
                        // will be added to the tree structure.  That's why it's important to remove the ones already
                        // in the tree.
                        childSourceNodes.Remove(childSourceNode);
                        sourceIndex--;

                        // This will signal the target loop that this element still exists in the structure.  If it
                        // isn't found, it'll be deleted.
                        found = true;
                        break;

                    }

                }

                // The target node is deleted when it can't be found among the source nodes.
                if (!found)
                {
                    this.Children.Remove(childTargetNode);
                    targetIndex--;
                }

            }

            // The remaining items in the source tree are new to the hierarchy; insert them sorted by name.
            foreach (FolderNavNode childSourceNode in childSourceNodes)
            {
                bool inserted = false;

                for (int targetIndex = 0; !inserted && targetIndex < this.Children.Count; targetIndex++)
                {

                    if (childSourceNode.Entity.Name.CompareTo(this.Children[targetIndex].Entity.Name) < 0)
                    {

                        this.Children.Insert(targetIndex, childSourceNode);
                        inserted = true;

                    }

                }

                if (!inserted)
                    this.Children.Add(childSourceNode);

            }

        }

        /// <summary>
        /// Gets a value that can be used in Hash Table lookups.
        /// </summary>
        /// <returns>A value that can be used in Hash Table lookups.</returns>
        public override int GetHashCode()
        {
            return this.entity.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current FolderTreeNode.
        /// </summary>
        /// <returns>A string representation of the current node.</returns>
        public override string ToString()
        {
            return string.Format("{0}, Count={1}", this.Entity.Name, this.Count);
        }


    }
}

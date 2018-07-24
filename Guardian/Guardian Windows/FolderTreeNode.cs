namespace FluidTrade.Guardian.Windows
{

    using System;
	using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
	using System.ComponentModel;

    /// <summary>
	/// Provides a strongly typed node used for navigating the objects in the Guardian application.
	/// </summary>
	public class FolderTreeNode : ObservableCollection<FolderTreeNode>, INotifyPropertyChanged
	{

		// Private Instance Fields
		private Guid relationId;
		private Int64 rowVersion;
		private Entity entity;

		/// <summary>
		/// The event raised when a property of note changes.
		/// </summary>
		public new event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Creates an empty node.
		/// </summary>
		public FolderTreeNode()
		{

			this.entity = new Entity();
		
		}

		/// <summary>
		/// Creates a node from an object in the data model.
		/// </summary>
		/// <param name="entityRow">The data model object used to create a node in the TreeView.</param>
		public FolderTreeNode(EntityRow entityRow)
		{

			// Initialize the object from the data in the record.
			this.relationId = Guid.Empty;
			this.rowVersion = Int64.MinValue;
			this.entity = Entity.New(entityRow);

			this.BuildTree(entityRow);

		}

		/// <summary>
		/// Creates a node from an object in the data model.
		/// </summary>
		/// <param name="entityTreeRow">The data model object used to create a node in the TreeView.</param>
		public FolderTreeNode(EntityTreeRow entityTreeRow)
		{

			EntityRow entityRow = entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId;

			// Initialize the object from the data in the record.
			this.relationId = entityTreeRow.EntityTreeId;
			this.rowVersion = entityTreeRow.RowVersion;
			this.entity = Entity.New(entityRow);

			this.BuildTree(entityRow);

		}

		/// <summary>
		/// The children of this node (required for Hierarchical Data Templates in WCF).
		/// </summary>
		public ObservableCollection<FolderTreeNode> Children { get { return this; } }

		/// <summary>
		/// Gets or sets whether the context menu has been shown already.
		/// </summary>
		public Boolean ContextMenuPopulated
		{

			get;
			set;

		}

		/// <summary>
		/// Gets or sets the entity at this node.
		/// </summary>
		public Entity Entity
		{
			get
			{
				return this.entity;
			}
			set
			{
				this.entity = value;
			}
		}

		/// <summary>
		/// True if this folder tree node has children.
		/// </summary>
		public Boolean HasItems
		{
			get { return this.Count > 0; }
		}

		/// <summary>
		/// Gets the identifier for the relation of the item in the EntityTree.
		/// </summary>
		public Guid RelationId
		{
			get
			{
				return this.relationId;
			}
		}

		/// <summary>
		/// Gets the row version in the EntityTree this relationship corresponds to.
		/// </summary>
		public Int64 RowVersion
		{
			get
			{
				return this.rowVersion;
			}
		}

		/// <summary>
		/// Build the folder tree below this node.
		/// </summary>
		/// <param name="entityRow"></param>
		private void BuildTree(EntityRow entityRow)
		{

			EntityTreeRow[] entityTreeRows = entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId();

			foreach (EntityTreeRow entityTreeRow in entityTreeRows)
			{

                Boolean inserted = false;

				// Add the new tree relations in alphabetical order.
                for (int targetIndex = 0; !inserted && targetIndex < this.Children.Count; targetIndex++)
                {

					if (entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId.Name.CompareTo(this.Children[targetIndex].Entity.Name) < 0)
                    {

						this.Children.Insert(targetIndex, new FolderTreeNode(entityTreeRow));
                        inserted = true;

                    }

                }

				// Any we missed go at the end.
                if (!inserted)
					this.Children.Add(new FolderTreeNode(entityTreeRow));

			}

		}

		/// <summary>
		/// Compare two FolderTreeNodes for equality.
		/// </summary>
		/// <param name="firstNode">The left node.</param>
		/// <param name="secondNode">The right node.</param>
		/// <returns>True if both nodes are null or the two nodes entities are equal; false otherwise.</returns>
		public static Boolean operator==(FolderTreeNode firstNode, FolderTreeNode secondNode)
		{
			if (Object.ReferenceEquals(firstNode, null) && Object.ReferenceEquals(secondNode, null))
				return true;

			if (Object.ReferenceEquals(firstNode, null) || Object.ReferenceEquals(secondNode, null))
				return false;

			//return firstNode.entity == secondNode.entity;
			return firstNode.relationId == secondNode.relationId;

		}

		/// <summary>
		/// Compare two FolderTreeNodes for inequality.
		/// </summary>
		/// <param name="firstNode">The left node.</param>
		/// <param name="secondNode">The right node.</param>
		/// <returns>False if both nodes are null or the two nodes entities are equal; true otherwise.</returns>
		public static Boolean operator !=(FolderTreeNode firstNode, FolderTreeNode secondNode)
		{

			if (Object.ReferenceEquals(firstNode, null) && Object.ReferenceEquals(secondNode, null))
				return false;

			if (Object.ReferenceEquals(firstNode, null) || Object.ReferenceEquals(secondNode, null))
				return true;

			return firstNode.entity != secondNode.entity;

		}

        /// <summary>
        /// Copies sourceNode into this object.
        /// </summary>
        /// <param name="sourceNode"></param>
        public void Assign(FolderTreeNode sourceNode)
        {
            this.relationId = sourceNode.RelationId;
            this.rowVersion = sourceNode.rowVersion;

            EntityRow entityRow = null;            
            lock (DataModel.SyncRoot)
            {
                entityRow = DataModel.Entity.EntityKey.Find(sourceNode.Entity.EntityId);
            }

            String[] typeParts = entityRow.TypeRow.Type.Split(',');
            Assembly assembly = Assembly.Load(typeParts[1].Trim());
            this.entity = assembly.CreateInstance(typeParts[0].Trim(), false, BindingFlags.CreateInstance, null,
                new object[] { entityRow }, CultureInfo.InvariantCulture, null) as Entity;   
             
            //Recursively copy the children
            this.Copy(sourceNode);
        }

		/// <summary>
		/// Recursively copies the source FolderTreeNode to the current FolderTreeNode.
		/// </summary>
		/// <param name="parentSourceNode">The source tree containing the new hierarchical structure and data.</param>
		public void Copy(FolderTreeNode parentSourceNode)
		{

			// The next step is to reconcile the child nodes between the source and the destination nodes.  This list keeps track
			// of what needs to be copied from the source tree to the destination tree and what needs to be added as new.
			List<FolderTreeNode> childSourceNodes = parentSourceNode.ToList();
			List<FolderTreeNode> childTargetNodes = this.ToList();

			// Copy the source object to to the destiation object.  This includes most of the properties seen in the tree: the 
			// name, the image, the description, etc.
			this.entity.Copy(parentSourceNode.entity);

			// The main idea here is to run through all the nodes looking for Adds, Deletes and Updates.  When we find a Node that
			// shouldn't be in the structure, it's deleted from the tree.  Since the tree is made up of linked lists, we can't
			// rightly delete the current link in the list.  For this reason we need to use the index into the list for spanning
			// the structure instead of the collection operations.  When we find an element that needs to be removed, we can delete
			// it and the index will get us safely to the next element in the list. The first loop scans the list already in the
			// TreeView structure.
			for (int targetIndex = 0; targetIndex < childTargetNodes.Count; targetIndex++)
			{

				// Get a reference to the next child in the target list.
				FolderTreeNode childTargetNode = childTargetNodes[targetIndex];

				// If we don't find the target (older) element in the source (newer) list, it will be deleted.
				Boolean found = false;

				// Cycle through all of the source (newer) elements looking for changes and removing any elements that
				// exist in both lists.
				for (int sourceIndex = 0; sourceIndex < childSourceNodes.Count; sourceIndex++)
				{

					// This will compare the child FolderTreeNodes from the source tree to the target.  If the target node
					// exists in the source list, it is updated with the information found in the source.
					FolderTreeNode childSourceNode = childSourceNodes[sourceIndex];

					// If the elements are equal (as defined by the equality operator of the object), then recurse into
					// the structure looking for changes to the children.  After that, check the Node for any changes
					// since it was added to the tree.
					if (childTargetNode.Entity.EntityId == childSourceNode.Entity.EntityId)
					{

						Int32 moveTo = this.FindChildInsertPoint(childSourceNode, childTargetNode);
						Int64 oldVersion = childTargetNode.Entity.RowVersion;
						String oldName = childTargetNode.Entity.Name;

						// Recurse down into the tree structures bringing all the children in sync with the new
						// structure.
						childTargetNode.Copy(childSourceNode);

						// If the source child node actually contains an updated entity, we'll move it to its new right place.
						if (oldVersion != childSourceNode.Entity.RowVersion && !oldName.Equals(childSourceNode.Entity.Name))
							this.Move(this.IndexOf(childTargetNode), moveTo);

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
					this.Remove(childTargetNode);

			}

			// The remaining items in the source tree are new to the hierarchy; insert them sorted by name.
			foreach (FolderTreeNode childSourceNode in childSourceNodes)
				this.Insert(this.FindChildInsertPoint(childSourceNode, null), childSourceNode);

			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs("HasItems"));

		}

		/// <summary>
		/// Determine whether another node is within the folder tree that this node is the root of.
		/// </summary>
		/// <param name="folderTreeNode">The node to look for.</param>
		/// <returns>True if the node is within the folder tree, false otherwise.</returns>
		public new Boolean Contains(FolderTreeNode folderTreeNode)
		{

			return this.Contains(this, folderTreeNode);

		}

		/// <summary>
		/// Determine whether a node is within the folder tree that a parent node is the root of.
		/// </summary>
		/// <param name="parentNode">The root of the tree.</param>
		/// <param name="childNode">The node to look for.</param>
		/// <returns>True if the child node is within the parent node's folder tree, false otherwise.</returns>
		private Boolean Contains(FolderTreeNode parentNode, FolderTreeNode childNode)
		{

			if (parentNode == childNode)
				return true;

			foreach (FolderTreeNode folderTreeNode in parentNode)
				if (this.Contains(folderTreeNode, childNode))
					return true;

			return false;

		}

		/// <summary>
		/// Determines whether one node is equal to another.
		/// </summary>
		/// <param name="obj">The node to be compared.</param>
		/// <returns>true if the two nodes are equal.</returns>
		public override Boolean Equals(Object obj)
		{

			// Only other FolderTreeNodes can be compared.
			if (obj is FolderTreeNode)
			{
				FolderTreeNode folderTreeNode = obj as FolderTreeNode;
				return this.entity == folderTreeNode.entity;
			}

			// All other types of objects are not equal.
			return false;
    
		}

		/// <summary>
		/// Find where a new node should be inserted, or an updated node should be moved.
		/// </summary>
		/// <param name="insert">The node to insert.</param>
		/// <param name="ignore">The original node that insert would update.</param>
		/// <returns>The index where new node should inserted.</returns>
		private Int32 FindChildInsertPoint(FolderTreeNode insert, FolderTreeNode ignore)
		{

			Int32 at = 0;
			Int32 index = 0;

			// This looks a little weird because the Move happens in two steps (a Remove and an Insert), and the index to Move to is the final index
			// of the item we're moving (ie. the index after the Remove step).
			for (; index < this.Count; ++index)
				if (this[index] == ignore)
					continue;//at -= 1;
				else if (insert.Entity.Name.CompareTo(this[index].Entity.Name) < 0)
					break;
				else
					at += 1;

			return at;

		}

		/// <summary>
		/// Find a path to an entity.
		/// </summary>
		/// <param name="entity">The entity to find.</param>
		/// <returns>A list, from shallow to deep, representing a path to the entity.</returns>
		public List<FolderTreeNode> FindPath(Entity entity)
		{

			List<FolderTreeNode> list = new List<FolderTreeNode>();

			this.FindPath(list, entity);

			return list;

		}

		/// <summary>
		/// Find a path to an entity.
		/// </summary>
		/// <param name="list">The list to add path parts to.</param>
		/// <param name="entity">The entity to find.</param>
		/// <returns>A list, from shallow to deep, representing a path to the entity.</returns>
		private Boolean FindPath(List<FolderTreeNode> list, Entity entity)
		{

			Boolean includesThis = false;

			if (this.Entity.Equals(entity))
			{

				list.Insert(0, this);
				includesThis = true;

			}
			else
			{

				foreach (FolderTreeNode node in this)
					if (node.FindPath(list, entity))
					{

						list.Insert(0, this);
						includesThis = true;
						break;

					}

			}

			return includesThis;

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

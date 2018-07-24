namespace FluidTrade.Guardian
{
	using FluidTrade.Guardian.Windows;
	using FluidTrade.Actipro;

	/// <summary>
	/// Wrapper class from FolderTreeNode.  This transates FolderTreeNode to FolderNavNode.
	/// </summary>
    public sealed class FolderTreeNodeAdapter
    {   
		/// <summary>
		/// Thirdparty FolderNavNode
		/// </summary>
        private FolderNavNode folderNavNode = new FolderNavNode();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceNode">FolderTreeNode to convert.</param>
        public FolderTreeNodeAdapter(FolderTreeNode sourceNode)
        {            
            folderNavNode.RowVersion = sourceNode.RowVersion;
            //Recursively copy the children
            Copy(sourceNode, folderNavNode);
        }

		/// <summary>
		/// Copy FolderTreenode to FolderNavNode
		/// </summary>
		/// <param name="sourceNode"></param>
		/// <param name="targetNode"></param>
        private void Copy(FolderTreeNode sourceNode, FolderNavNode targetNode)
        {
            CopyEntity(sourceNode.Entity, targetNode.Entity);
            var childSourceNodes = sourceNode.Children; 

            // The remaining items in the source tree are new to the hierarchy
            foreach (FolderTreeNode childSourceNode in childSourceNodes)
            {
                FolderNavNode navNode = new FolderNavNode();
                CopyEntity(childSourceNode.Entity, navNode.Entity);
                targetNode.Children.Add(navNode);
                Copy(childSourceNode, navNode);
            }
        }

		/// <summary>
		/// Copy object's entity field.
		/// </summary>
		/// <param name="sourceNode"></param>
		/// <param name="targetNode"></param>
        private void CopyEntity(Entity sourceNode, EntityNode targetNode)
        {
            targetNode.Name = sourceNode.Name;
            targetNode.EntityId = sourceNode.EntityId;
            targetNode.ImageData = sourceNode.ImageData;
        }


        /// <summary>
        /// Accessor
        /// </summary>       
        public FolderNavNode FolderNavNode
        {
            get { return this.folderNavNode; }
        }


    }
}

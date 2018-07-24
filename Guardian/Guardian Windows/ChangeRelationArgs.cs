namespace FluidTrade.Guardian.Windows
{

	using System;

	/// <summary>
	/// Used to communicate a change in the relationship between entities in the data model.
	/// </summary>
	internal class ChangeRelationArgs
	{

		// Private Instance Fields
		private ChangeRelationAction changeRelationAction;
		private Guid childId;
		private Guid parentId;
		private Guid relationId;
		private Int64 rowVersion;

		/// <summary>
		/// Creates arguments for changing the relationship between entities.
		/// </summary>
		/// <param name="changeRelationAction">The action to be taken.</param>
		/// <param name="childId">The identity of the child element.</param>
		/// <param name="parentId">The identity of the parent element.</param>
		/// <param name="relationId">The identity of the relation between the parent and child.</param>
		public ChangeRelationArgs(ChangeRelationAction changeRelationAction, Guid childId, Guid parentId, Guid relationId)
		{

			// Initialize the object
			this.changeRelationAction = changeRelationAction;
			this.childId = childId;
			this.parentId = parentId;
			this.relationId = relationId;

		}

		/// <summary>
		/// Creates arguments for changing the relationship between entities.
		/// </summary>
		/// <param name="changeRelationAction">The action to be taken.</param>
		/// <param name="childId">The identity of the child element.</param>
		/// <param name="parentId">The identity of the parent element.</param>
		/// <param name="relationId">The identity of the relation between the parent and child.</param>
		/// <param name="rowVersion">The row-version the information corresponds to.</param>
		public ChangeRelationArgs(ChangeRelationAction changeRelationAction, Guid childId, Guid parentId, Guid relationId, long rowVersion)
		{

			// Initialize the object
			this.changeRelationAction = changeRelationAction;
			this.childId = childId;
			this.parentId = parentId;
			this.relationId = relationId;
			this.rowVersion = rowVersion;

		}

		/// <summary>
		/// Gets the unique identifier of the child.
		/// </summary>
		public Guid ChildId
		{
			get { return this.childId; }
		}

		/// <summary>
		/// Gets the unique identifier of the parent.
		/// </summary>
		public Guid ParentId
		{
			get { return this.parentId; }
		}

		/// <summary>
		/// Gets the unique identifier of the relationship between the parent and child.
		/// </summary>
		public Guid RelationId
		{
			get { return this.relationId; }
		}

		/// <summary>
		/// Gets the row version of the unique relationship between the parent and child.
		/// </summary>
		public Int64 RowVersion
		{
			get { return this.rowVersion; }
		}

		/// <summary>
		/// Gets an action that describes the change between the parent and child.
		/// </summary>
		public ChangeRelationAction Action
		{
			get { return this.changeRelationAction; }
		}

	}

}

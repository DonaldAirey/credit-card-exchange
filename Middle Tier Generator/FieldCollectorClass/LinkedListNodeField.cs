namespace FluidTrade.MiddleTierGenerator.FieldCollectorClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a field that holds a node that aggregates fields from various transaction log records into a single record.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class LinkedListNodeField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds a node that aggregates fields from various transaction log records into a single record.
		/// </summary>
		public LinkedListNodeField()
		{

			//		internal global::System.Collections.Generic.LinkedListNode<TransactionLogItem> linkedListNode;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeTypeReference("global::System.Collections.Generic.LinkedListNode<TransactionLogItem>");
			this.Name = "linkedListNode";

		}

	}

}

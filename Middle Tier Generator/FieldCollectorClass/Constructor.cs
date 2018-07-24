namespace FluidTrade.MiddleTierGenerator.FieldCollectorClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a static constructor for the transaction.
	/// </summary>
	class Constructor : CodeConstructor
	{

		/// <summary>
		/// Creates a static constructor for the transaction.
		/// </summary>
		/// <param name="dataModelSchema">A description of the data model.</param>
		public Constructor(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Creates a collector of fields for the transaction log compression.
			//		/// </summary>
			//		/// <param name="linkedListNode">The node that is to be compressed.</param>
			//		/// <param name="fieldTable">A collection of transaction log fields.</param>
			//		private FieldCollector(global::System.Collections.Generic.LinkedListNode<TransactionLogItem> linkedListNode, global::System.Collections.Generic.Dictionary<int, object> fieldTable)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Creates a collector of fields for the transaction log compression.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"linkedListNode\">The node that is to be compressed.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"fieldTable\">A collection of transaction log fields.</param>", true));
			this.Attributes = MemberAttributes.Assembly;
			this.Parameters.Add(
				new CodeParameterDeclarationExpression(
					new CodeTypeReference("global::System.Collections.Generic.LinkedListNode<TransactionLogItem>"), "linkedListNode"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Dictionary<Int32, Object>)), "fieldTable"));

			//			this.linkedListNode = linkedListNode;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "linkedListNode"), new CodeArgumentReferenceExpression("linkedListNode")));

			//			this.fieldTable = fieldTable;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "fieldTable"), new CodeArgumentReferenceExpression("fieldTable")));

			//		}

		}

	}

}

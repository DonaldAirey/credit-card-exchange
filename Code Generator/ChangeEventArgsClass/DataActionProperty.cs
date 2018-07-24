namespace FluidTrade.Core.ChangeEventArgsClass
{

    using System.CodeDom;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class DataActionProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public DataActionProperty(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Gets the action that caused the change to the row.
			//            /// </summary>
			//            public global::System.Data.DataRowAction Action {
			//                get {
			//                    return this.dataRowAction;
			//                }
			//            }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets the action that caused the change to the row.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(System.Data.DataRowAction));
			this.Name = "Action";
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "dataRowAction")));
		
		}

	}
}

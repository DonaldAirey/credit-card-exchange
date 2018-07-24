namespace FluidTrade.ClientGenerator.RowClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Represents a declaration of a method that gets a list of the child rows.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class GetChildRowsMethod : CodeMemberMethod
	{

		/// <summary>
		/// Generates a method to get a list of child rows.
		/// </summary>
		/// <param name="relationSchema">A description of the relation between two tables.</param>
		public GetChildRowsMethod(RelationSchema relationSchema)
		{

			// These variables are used to construct the method.
			TableSchema childTable = relationSchema.ChildTable;
			TableSchema parentTable = relationSchema.ParentTable;
			string rowTypeName = string.Format("{0}Row", childTable.Name);
			string tableFieldName = string.Format("table{0}", parentTable.Name);
			string childRowTypeName = string.Format("{0}Row", relationSchema.ChildTable.Name);

			//        /// <summary>
			//        /// Gets the children rows in the Engineer table.
			//        /// </summary>
			//        public EngineerRow[] GetEngineerRows() {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the children rows in the {0} table.", relationSchema.ChildTable.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeTypeReference(childRowTypeName, 1);
			this.Name = relationSchema.IsDistinctPathToChild ?
				string.Format("Get{0}s", childRowTypeName) :
				string.Format("Get{0}sBy{1}", childRowTypeName, relationSchema.Name);
			string relationName = relationSchema.IsDistinctPathToChild ?
				string.Format("{0}{1}Relation", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name) :
				string.Format("{0}{1}By{2}Relation", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name);

			//            try {
			//                // The child table must be locked to insure it doesn't change while the relation is used to create a list of
			//                // all the child rows of this row.
			//                return ((EngineerRow[])(this.GetChildRows(this.tableEmployee.EmployeeEngineerRelation)));
			//            }
			//            finally {
			//                // The child table can be released once the list is built.
			//            }
			CodeTryCatchFinallyStatement getTryStatement = new CodeTryCatchFinallyStatement();
			getTryStatement.TryStatements.Add(new CodeCommentStatement("The child table must be locked to insure it doesn't change while the relation is used to create a list of"));
			getTryStatement.TryStatements.Add(new CodeCommentStatement("all the child rows of this row."));
			getTryStatement.TryStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(new CodeTypeReference(childRowTypeName, 1), new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetChildRows", new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), tableFieldName), relationName)))));
			getTryStatement.FinallyStatements.Add(new CodeCommentStatement("The child table can be released once the list is built."));
			this.Statements.Add(getTryStatement);

			//        }

		}

	}

}

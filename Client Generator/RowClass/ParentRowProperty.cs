namespace FluidTrade.ClientGenerator.RowClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a property that gets the parent row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class ParentRowProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property that gets the parent row.
		/// </summary>
		/// <param name="relationSchema">The foreign key that references the parent table.</param>
		public ParentRowProperty(RelationSchema relationSchema)
		{

			// These constructs are used several times to generate the property.
			TableSchema childTable = relationSchema.ChildTable;
			TableSchema parentTable = relationSchema.ParentTable;
			string rowTypeName = string.Format("{0}Row", parentTable.Name);
			string tableFieldName = string.Format("table{0}", childTable.Name);
			string relationName = relationSchema.IsDistinctPathToParent ?
				string.Format("{0}{1}Relation", parentTable.Name, childTable.Name) :
				string.Format("{0}{1}By{2}Relation", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name);
			string relationFieldName = relationSchema.IsDistinctPathToParent ?
				string.Format("{0}{1}Relation", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name) :
				string.Format("{0}{1}By{2}Relation", relationSchema.ParentTable.Name, relationSchema.ChildTable.Name, relationSchema.Name);

			//        /// <summary>
			//        /// Gets the parent row in the Department table.
			//        /// </summary>
			//        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the parent row in the {0} table.", relationSchema.ParentTable.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
            // HACK - Put this line back in for official releases
            //			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.Diagnostics.DebuggerNonUserCodeAttribute))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference(rowTypeName);
			this.Name = relationSchema.IsDistinctPathToParent ? rowTypeName : string.Format("{0}By{1}", rowTypeName, relationSchema.Name);

			//        public DepartmentRow DepartmentRow {
			//            get {
			//                try {
			//                    // The parent table must be locked to insure it doesn't change before attempting to access the parent row.
			//                    return ((DepartmentRow)(this.GetParentRow(this.tableEmployee.DepartmentEmployeeRelation)));
			//                }
			//                finally {
			//                    // The parent table can be released once the parent row is found.
			//                }
			//            }
			CodeTryCatchFinallyStatement getTryStatement = new CodeTryCatchFinallyStatement();
			getTryStatement.TryStatements.Add(new CodeCommentStatement("The parent table must be locked to insure it doesn't change before attempting to access the parent row."));
			CodeExpression parentRelationExpression = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), tableFieldName), relationName);
			getTryStatement.TryStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(rowTypeName, new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetParentRow", new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), tableFieldName), relationFieldName)))));
			getTryStatement.FinallyStatements.Add(new CodeCommentStatement("The parent table can be released once the parent row is found."));
			this.GetStatements.Add(getTryStatement);

			//        }

		}

	}

}

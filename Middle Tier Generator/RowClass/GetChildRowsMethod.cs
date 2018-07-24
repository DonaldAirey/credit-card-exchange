namespace FluidTrade.MiddleTierGenerator.RowClass
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

			//		/// <summary>
			//		/// Gets the children rows in the Allocation table.
			//		/// </summary>
			//		public AllocationRow[] GetAllocationRows()
			//		{
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

			//			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			CodeVariableReferenceExpression transactionExpression = new CodeVariableReferenceExpression(
				string.Format("{0}Transaction", CommonConversion.ToCamelCase(parentTable.DataModel.Name)));
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference(string.Format("{0}Transaction", parentTable.DataModel.Name)),
					transactionExpression.VariableName,
					new CodePropertyReferenceExpression(
						new CodeTypeReferenceExpression(string.Format("{0}Transaction", parentTable.DataModel.Name)),
						"Current")));

			//			if ((this.IsLockHeld(dataModelTransaction.transactionId) == false))
			//			{
			//				throw new global::System.ServiceModel.FaultException<SynchronizationLockFault>(new global::FluidTrade.Core.SynchronizationLockFault("Account"));
			//			}
			this.Statements.AddRange(new CodeCheckReaderLockHeldStatements(new CodeThisReferenceExpression(), relationSchema.ParentTable, transactionExpression));

			//			try
			//			{
			CodeTryCatchFinallyStatement getTryStatement = new CodeTryCatchFinallyStatement();

			//				DataModel.dataLock.EnterReadLock();
			getTryStatement.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(relationSchema.ParentTable.DataModel.Name), "DataLock"),
					"EnterReadLock"));

			//				return ((AllocationRow[])(this.GetChildRows(this.tableAccount.AccountAllocationRelation)));
			//			}
			getTryStatement.TryStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(new CodeTypeReference(childRowTypeName, 1), new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetChildRows", new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), tableFieldName), relationName)))));

			//			finally
			//			{
			//				DataModel.dataLock.ExitReadLock();
			//			}
			getTryStatement.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(relationSchema.ParentTable.DataModel.Name), "DataLock"),
					"ExitReadLock"));
			this.Statements.Add(getTryStatement);

			//		}

		}

	}

}

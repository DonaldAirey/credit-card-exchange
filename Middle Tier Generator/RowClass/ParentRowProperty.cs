namespace FluidTrade.MiddleTierGenerator.RowClass
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

			//		/// <summary>
			//		/// Gets the parent row in the AccountBase table.
			//		/// </summary>
			//		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//		public AccountBaseRow AccountBaseRow
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the parent row in the {0} table.", relationSchema.ParentTable.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
            // HACK - Put this line back in for official releases
            //this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.Diagnostics.DebuggerNonUserCodeAttribute))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference(rowTypeName);
			this.Name = relationSchema.IsDistinctPathToParent ? rowTypeName : string.Format("{0}By{1}", rowTypeName, relationSchema.Name);

			//			get
			//			{
			//				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			CodeVariableReferenceExpression transactionExpression = new CodeVariableReferenceExpression(
				string.Format("{0}Transaction", CommonConversion.ToCamelCase(parentTable.DataModel.Name)));
			this.GetStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference(string.Format("{0}Transaction", parentTable.DataModel.Name)),
					transactionExpression.VariableName,
					new CodePropertyReferenceExpression(
						new CodeTypeReferenceExpression(string.Format("{0}Transaction", parentTable.DataModel.Name)),
						"Current")));

			//				if ((this.IsLockHeld(dataModelTransaction.TransactionId) == false))
			//				{
			//					throw new global::System.ServiceModel.FaultException<SynchronizationLockFault>(new global::FluidTrade.Core.SynchronizationLockFault("Account"));
			//				}
			this.GetStatements.AddRange(
				new CodeCheckReaderLockHeldStatements(new CodeThisReferenceExpression(), relationSchema.ChildTable, transactionExpression));

			//				try
			//				{
			CodeTryCatchFinallyStatement getTryStatement = new CodeTryCatchFinallyStatement();

			//					DataModel.dataLock.EnterReadLock();
			getTryStatement.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(parentTable.DataModel.Name), "DataLock"),
					"EnterReadLock"));

			//					return ((AccountBaseRow)(this.GetParentRow(this.tableAccount.AccountBaseAccountRelation)));
			//				}
			getTryStatement.TryStatements.Add(
				new CodeMethodReturnStatement(
					new CodeCastExpression(
						rowTypeName,
						new CodeMethodInvokeExpression(
							new CodeThisReferenceExpression(),
							"GetParentRow",
							new CodePropertyReferenceExpression(
								new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), tableFieldName),
								relationFieldName)))));
	
			//				finally
			//				{
			//					DataModel.dataLock.ExitReadLock();
			//				}
			getTryStatement.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(parentTable.DataModel.Name), "DataLock"),
					"ExitReadLock"));

			//			}
			this.GetStatements.Add(getTryStatement);

			//		}

		}

	}

}

namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using System.Data;
    using System.Transactions;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class RollbackMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public RollbackMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Rollback (undo) any of the changes made during the transaction.
			//		/// </summary>
			//		/// <param name="enlistment">Facilitates communication between an enlisted transaction and the transaction manager during the
			//		/// final phase of the transaction.</param>
			//		public void Rollback(global::System.Transactions.Enlistment enlistment)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Adds a row lock to the list of locks that must be released at the end of a transaction.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"enlistment\">Facilitates communication bewtween an enlisted transaction participant and the transaction", true));
			this.Comments.Add(new CodeCommentStatement("manager during the final phase of the transaction.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "Rollback";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Enlistment)), "enlistment"));

			//			this.recordList.Reverse();
			this.Statements.Add(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "recordList"), "Reverse"));

			//			try
			//			{
			CodeTryCatchFinallyStatement tryEnterLock = new CodeTryCatchFinallyStatement();

			//				DataModel.DataLock.EnterWriteLock();
			tryEnterLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "DataLock"),
					"EnterWriteLock"));

			//				for (int recordIndex = 0; (recordIndex < this.recordList.Count); recordIndex = (recordIndex + 1))
			//				{
			CodeIterationStatement forRecordList = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"recordIndex",
					new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("recordIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "recordList"), "Count")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("recordIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("recordIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//					IRow iRow = this.recordList[recordIndex];
			forRecordList.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)),
					"iRow",
					new CodeIndexerExpression(
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "recordList"),
						new CodeVariableReferenceExpression("recordIndex"))));

			//					if ((iRow.RowState == global::System.Data.DataRowState.Added) ||
			//								((iRow.RowState == global::System.Data.DataRowState.Deleted) ||
			//												(iRow.RowState == global::System.Data.DataRowState.Modified)))
			//					{
			CodeConditionStatement ifRejectable = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeBinaryOperatorExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iRow"), "RowState"),
						CodeBinaryOperatorType.IdentityEquality,
						new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowState)), "Added")),
					CodeBinaryOperatorType.BooleanOr,
					new CodeBinaryOperatorExpression(
						new CodeBinaryOperatorExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iRow"), "RowState"),
							CodeBinaryOperatorType.IdentityEquality,
							new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowState)), "Deleted")),
						CodeBinaryOperatorType.BooleanOr,
						new CodeBinaryOperatorExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iRow"), "RowState"),
							CodeBinaryOperatorType.IdentityEquality,
							new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowState)), "Modified")))));

			//						iRow.RejectChanges();
			ifRejectable.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("iRow"), "RejectChanges"));

			//					}
			forRecordList.Statements.Add(ifRejectable);

			//				}
			tryEnterLock.TryStatements.Add(forRecordList);

			//				for (int recordIndex = 0; recordIndex < this.lockList.Count; recordIndex = (recordIndex + 1))
			//				{
			CodeIterationStatement forLockList = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"lockIndex",
					new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("lockIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "lockList"), "Count")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("lockIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("lockIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//					this.lockList[recordIndex].ReleaseLock(this.transactionId);
			forLockList.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeIndexerExpression(
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "lockList"),
						new CodeVariableReferenceExpression("lockIndex")),
					"ReleaseLock",
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionId")));

			//				}
			tryEnterLock.TryStatements.Add(forLockList);			

			//			}
			//			finally
			//			{
			//				DataModel.DataLock.ExitWriteLock();
			tryEnterLock.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "DataLock"),
					"ExitWriteLock"));

			//			}
			this.Statements.Add(tryEnterLock);

			//			enlistment.Done();
			this.Statements.Add(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("enlistment"), "Done"));

			//		}

		}

	}
}

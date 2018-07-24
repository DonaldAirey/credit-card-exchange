namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Data;
    using System.Transactions;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class CommitMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public CommitMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Adds a row lock to the list of locks that must be released at the end of a transaction.
			//		/// </summary>
			//		/// <param name="enlistment">Facilitates communication bewtween an enlisted transaction participant and the transaction
			//		/// manager during the final phase of the transaction.</param>
			//		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//		public void Commit(global::System.Transactions.Enlistment enlistment)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Adds a row lock to the list of locks that must be released at the end of a transaction.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"enlistment\">Facilitates communication bewtween an enlisted transaction participant and the transaction", true));
			this.Comments.Add(new CodeCommentStatement("manager during the final phase of the transaction.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "Commit";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Enlistment)), "enlistment"));

			//			try
			//			{
			CodeTryCatchFinallyStatement tryEnterLock = new CodeTryCatchFinallyStatement();

			//				global::System.Collections.Generic.List<object> transactionLogItem = new global::System.Collections.Generic.List<object>();
			tryEnterLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(List<Object>)),
					"transactionLogItem",
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(List<Object>)))));

			//				DataModel.TransactionLogLock.EnterWriteLock();
			tryEnterLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "TransactionLogLock"),
					"EnterWriteLock"));

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

			//					ITable iTable = ((ITable)(iRow.Table));
			forRecordList.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(FluidTrade.Core.ITable)),
					"iTable",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(FluidTrade.Core.ITable)),
						new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("iRow"), "Table"))));

			//					if ((iRow.RowState == global::System.Data.DataRowState.Modified))
			//					{
			CodeConditionStatement ifModified = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iRow"), "RowState"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowState)), "Modified")));

			//						transactionLogItem.Clear();
			ifModified.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("transactionLogItem"), "Clear"));

			//						transactionLogItem.Add(global::FluidTrade.Core.RecordState.Modified);
			ifModified.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Modified")));

			//						transactionLogItem.Add(iTable.Ordinal);
			ifModified.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "Ordinal")));

			//						for (int keyIndex = 0; (keyIndex < iTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
			//						{
			CodeIterationStatement keyLoop0 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "keyIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("keyIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iTable"), "PrimaryKey"),
							"Length")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("keyIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("keyIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//							transactionLogItem.Add(iRow[iTable.PrimaryKey[keyIndex]]);
			keyLoop0.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("iRow"),
							new CodeIndexerExpression(
								new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "PrimaryKey"),
								new CodeVariableReferenceExpression("keyIndex")))));

			//						}
			ifModified.TrueStatements.Add(keyLoop0);

			//						for (int columnIndex = 0; (columnIndex < iTable.Columns.Count); columnIndex = (columnIndex + 1))
			//						{
			CodeIterationStatement columnLoop1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "columnIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("columnIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iTable"), "Columns"),
							"Count")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("columnIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("columnIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//							if ((iRow[columnIndex].Equals(iRow[columnIndex, global::System.Data.DataRowVersion.Original]) == false))
			//							{
			CodeConditionStatement ifChanged = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodeIndexerExpression(new CodeVariableReferenceExpression("iRow"), new CodeVariableReferenceExpression("columnIndex")),
						"Equals",
						new CodeIndexerExpression(
							new CodeVariableReferenceExpression("iRow"),
							new CodeVariableReferenceExpression("columnIndex"),
							new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowVersion)), "Original"))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(false)));

			//								transactionLogItem.Add(columnIndex);
			ifChanged.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeVariableReferenceExpression("columnIndex")));

			//								transactionLogItem.Add(iRow[columnIndex]);
			ifChanged.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeIndexerExpression(
						new CodeArgumentReferenceExpression("iRow"),
						new CodeVariableReferenceExpression("columnIndex"))));

			//							}
			columnLoop1.Statements.Add(ifChanged);

			//						}
			ifModified.TrueStatements.Add(columnLoop1);

			//						DataModel.AddTransaction(iRow, transactionLogItem.ToArray());
			ifModified.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"AddTransaction",
					new CodeVariableReferenceExpression("iRow"),
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						"ToArray")));

			//						iRow.AcceptChanges();
			ifModified.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("iRow"), "AcceptChanges"));

			//					}
			//					else
			//					{
			//						if ((iRow.RowState == global::System.Data.DataRowState.Added))
			//						{
			CodeConditionStatement ifAdded = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iRow"), "RowState"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowState)), "Added")));

			//							transactionLogItem.Clear();
			ifAdded.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("transactionLogItem"), "Clear"));

			//							transactionLogItem.Add(global::FluidTrade.Core.RecordState.Added);
			ifAdded.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Added")));

			//							transactionLogItem.Add(iTable.Ordinal);
			ifAdded.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "Ordinal")));

			//							for (int keyIndex = 0; (keyIndex < iTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
			//							{
			CodeIterationStatement keyLoop1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "keyIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("keyIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iTable"), "PrimaryKey"),
							"Length")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("keyIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("keyIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//								transactionLogItem.Add(iRow[iTable.PrimaryKey[keyIndex]]);
			keyLoop1.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("iRow"),
						new CodeIndexerExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "PrimaryKey"),
							new CodeVariableReferenceExpression("keyIndex")))));

			//							}
			ifAdded.TrueStatements.Add(keyLoop1);

			//							for (int columnIndex = 0; (columnIndex < iTable.Columns.Count); columnIndex = (columnIndex + 1))
			//							{
			CodeIterationStatement columnLoop0 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "columnIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("columnIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iTable"), "Columns"),
							"Count")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("columnIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("columnIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//								if ((iRow[columnIndex].Equals(iTable.Columns[columnIndex].DefaultValue) == false))
			//								{
			CodeConditionStatement ifDefault = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodeIndexerExpression(new CodeVariableReferenceExpression("iRow"), new CodeVariableReferenceExpression("columnIndex")),
						"Equals",
						new CodePropertyReferenceExpression(
							new CodeIndexerExpression(
								new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "Columns"),
								new CodeVariableReferenceExpression("columnIndex")),
							"DefaultValue")),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(false)));


			//									transactionLogItem.Add(columnIndex);
			ifDefault.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeVariableReferenceExpression("columnIndex")));

			//									transactionLogItem.Add(iRow[columnIndex]);
			ifDefault.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeIndexerExpression(
						new CodeArgumentReferenceExpression("iRow"),
						new CodeVariableReferenceExpression("columnIndex"))));

			//								}
			columnLoop0.Statements.Add(ifDefault);

			//							}
			ifAdded.TrueStatements.Add(columnLoop0);

			//							DataModel.AddTransaction(iRow, transactionLogItem.ToArray());
			ifAdded.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"AddTransaction",
					new CodeVariableReferenceExpression("iRow"),
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						"ToArray")));

			//							iRow.AcceptChanges();
			ifAdded.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("iRow"), "AcceptChanges"));

			//						}
			//						else
			//						{
			//							transactionLogItem.Clear();
			ifAdded.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("transactionLogItem"), "Clear"));

			//							transactionLogItem.Add(global::FluidTrade.Core.RecordState.Deleted);
			ifAdded.FalseStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Deleted")));

			//							transactionLogItem.Add(iTable.Ordinal);
			ifAdded.FalseStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "Ordinal")));

			//							for (int keyIndex = 0; (keyIndex < iTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
			//							{
			CodeIterationStatement keyLoop2 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "keyIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("keyIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iTable"), "PrimaryKey"),
							"Length")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("keyIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("keyIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//								transactionLogItem.Add(iRow[iTable.PrimaryKey[keyIndex], global::System.Data.DataRowVersion.Original]);
			keyLoop2.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("iRow"),
							new CodeIndexerExpression(
								new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "PrimaryKey"),
								new CodeVariableReferenceExpression("keyIndex")),
							new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowVersion)), "Original"))));

			//							}
			ifAdded.FalseStatements.Add(keyLoop2);

			//							DataModel.AddTransaction(iRow, transactionLogItem.ToArray());
			ifAdded.FalseStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"AddTransaction",
					new CodeVariableReferenceExpression("iRow"),
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						"ToArray")));

			//							iRow.AcceptChanges();
			ifAdded.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("iRow"), "AcceptChanges"));

			//						}
			ifModified.FalseStatements.Add(ifAdded);

			//					}
			forRecordList.Statements.Add(ifModified);

			//				}
			tryEnterLock.TryStatements.Add(forRecordList);

			//				for (int lockIndex = 0; (lockIndex < this.lockList.Count); lockIndex = (lockIndex + 1))
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

			//					this.lockList[lockIndex].ReleaseLock(this.transactionId);
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
			//				DataModel.TransactionLogLock.ExitWriteLock();
			tryEnterLock.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "TransactionLogLock"),
					"ExitWriteLock"));

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

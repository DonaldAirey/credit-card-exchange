namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class CompressLogMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public CompressLogMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Compresses multiple updates to a single record and removes deleted records.
			//		/// </summary>
			//		private void CompressLog()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Purges the deleted data model of obsolete rows.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Final;
			this.Name = "CompressLog";

			//			global::System.Collections.Generic.LinkedListNode<TransactionLogItem> currentLink = null;
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("global::System.Collections.Generic.LinkedListNode<TransactionLogItem>"),
					"currentLink",
					new CodePrimitiveExpression(null)));

			//			global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.LinkedListNode<TransactionLogItem>> deletedDictionary = new global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.LinkedListNode<TransactionLogItem>>();
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.LinkedListNode<TransactionLogItem>>"),
					"deletedDictionary",
					new CodeObjectCreateExpression(
						new CodeTypeReference("global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.LinkedListNode<TransactionLogItem>>"))));

			//			global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.Dictionary<int, object>> updatedDictionary = new global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.Dictionary<int, object>>();
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.Dictionary<int, object>>"),
					"updatedDictionary",
					new CodeObjectCreateExpression(
						new CodeTypeReference("global::System.Collections.Generic.Dictionary<TransactionLogItem, global::System.Collections.Generic.Dictionary<int, object>>"))));

			//			global::System.Collections.Generic.List<global::System.Collections.Generic.LinkedListNode<TransactionLogItem>> deletedItemList = new global::System.Collections.Generic.List<global::System.Collections.Generic.LinkedListNode<TransactionLogItem>>();
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("global::System.Collections.Generic.List<global::System.Collections.Generic.LinkedListNode<TransactionLogItem>>"),
					"deletedItemList",
					new CodeObjectCreateExpression(
						new CodeTypeReference("global::System.Collections.Generic.List<global::System.Collections.Generic.LinkedListNode<TransactionLogItem>>"))));

			//			global::System.Collections.Generic.List<FieldCollector> updatedItemList = new global::System.Collections.Generic.List<FieldCollector>();
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("global::System.Collections.Generic.List<FieldCollector>"),
					"updatedItemList",
					new CodeObjectCreateExpression(new CodeTypeReference("global::System.Collections.Generic.List<FieldCollector>"))));

			//			for (
			//			; true; 
			//			)
			//			{
			CodeIterationStatement whileCollecting = new CodeIterationStatement(new CodeSnippetStatement(), new CodePrimitiveExpression(true), new CodeSnippetStatement());

			//				global::System.DateTime currentTime = global::System.DateTime.Now;
			whileCollecting.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DateTime)),
					"currentTime",
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DateTime)), "Now")));

			//				try
			//				{
			CodeTryCatchFinallyStatement tryReadLock = new CodeTryCatchFinallyStatement();

			//					this.transactionLogLock.EnterReadLock();
			tryReadLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"EnterReadLock"));

			//					for (int count = 0; (count < this.transactionLogCompressionSize); count = (count + 1))
			//					{
			CodeIterationStatement forCount = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"count",
					new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("count"),
						CodeBinaryOperatorType.LessThan,
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogCompressionSize")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("count"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("count"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//						if ((currentLink == null))
			//						{
			CodeConditionStatement ifListStart = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("currentLink"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(null)));

			//							currentLink = this.transactionLog.Last;
			ifListStart.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("currentLink"),
					new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLog"), "Last")));

			//						}
			forCount.Statements.Add(ifListStart);

			//						if ((currentLink != null))
			//						{
			CodeConditionStatement ifLink = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("currentLink"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//							if ((currentTime.Subtract(currentLink.Value.timeStamp) > this.transactionLogAge))
			//							{
			CodeConditionStatement ifRecordOld = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("currentTime"),
						"Subtract",
						new CodeFieldReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
							"timeStamp")),
					CodeBinaryOperatorType.GreaterThan,
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogAge")));

			//								object[] transactionLogData = currentLink.Value.data;
			ifRecordOld.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"transactionLogData",
					new CodeFieldReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
						"data")));

			//								if ((((int)(transactionLogData[0])) == global::FluidTrade.Core.RecordState.Deleted))
			//								{
			CodeConditionStatement ifDeleted = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("transactionLogData"), new CodePrimitiveExpression(0))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Deleted")));

			//									deletedDictionary.Add(currentLink.Value, currentLink);
			ifDeleted.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedDictionary"),
					"Add",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
					new CodeVariableReferenceExpression("currentLink")));

			//									deletedItemList.Add(currentLink);
			ifDeleted.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedItemList"),
					"Add",
					new CodeVariableReferenceExpression("currentLink")));

			//								}
			ifRecordOld.TrueStatements.Add(ifDeleted);

			//								if ((((int)(transactionLogData[0])) == global::FluidTrade.Core.RecordState.Modified))
			//								{
			CodeConditionStatement ifModified = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("transactionLogData"), new CodePrimitiveExpression(0))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Modified")));

			//									if (deletedDictionary.ContainsKey(currentLink.Value))
			//									{
			CodeConditionStatement ifInDeletedDictionary0 = new CodeConditionStatement(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedDictionary"),
					"ContainsKey",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value")));

			//										deletedItemList.Add(currentLink);
			ifInDeletedDictionary0.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedItemList"),
					"Add",
					new CodeVariableReferenceExpression("currentLink")));

			//									}
			//									else
			//									{
			//										int offset = (2 + currentLink.Value.keyLength);
			ifInDeletedDictionary0.FalseStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"offset",
					new CodeBinaryOperatorExpression(
						new CodePrimitiveExpression(2),
						CodeBinaryOperatorType.Add,
						new CodeFieldReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
							"keyLength"))));

			//										global::System.Collections.Generic.Dictionary<int, object> fieldTable;
			ifInDeletedDictionary0.FalseStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Dictionary<Int32, Object>)),
					"fieldTable"));

			//										if (updatedDictionary.TryGetValue(currentLink.Value, out fieldTable))
			//										{
			CodeConditionStatement ifInUpdatedDictionary0 = new CodeConditionStatement(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("updatedDictionary"),
					"TryGetValue",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
					new CodeDirectionExpression(FieldDirection.Out, new CodeVariableReferenceExpression("fieldTable"))));

			//											for (int fieldIndex = offset; (fieldIndex < transactionLogData.Length); fieldIndex = (fieldIndex + 2))
			//											{
			CodeIterationStatement forFieldIndex0 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fieldIndex",
					new CodeVariableReferenceExpression("offset")),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("fieldIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLogData"), "Length")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("fieldIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(2))));

			//												if ((fieldTable.ContainsKey(((int)(transactionLogData[fieldIndex]))) == false))
			//												{
			CodeConditionStatement ifLacksField0 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("fieldTable"),
						"ContainsKey",
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("transactionLogData"),
								new CodeVariableReferenceExpression("fieldIndex")))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(false)));

			//													fieldTable.Add(((int)(transactionLogData[fieldIndex])), transactionLogData[(fieldIndex + 1)]);
			ifLacksField0.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("fieldTable"),
					"Add",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(
							new CodeVariableReferenceExpression("transactionLogData"),
							new CodeVariableReferenceExpression("fieldIndex"))),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogData"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1)))));

			//												}
			forFieldIndex0.Statements.Add(ifLacksField0);

			//											}
			ifInUpdatedDictionary0.TrueStatements.Add(forFieldIndex0);

			//											deletedItemList.Add(currentLink);
			ifInUpdatedDictionary0.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedItemList"),
					"Add",
					new CodeVariableReferenceExpression("currentLink")));

			//										}
			//										else
			//										{
			//											fieldTable = new global::System.Collections.Generic.Dictionary<int, object>();
			ifInUpdatedDictionary0.FalseStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("fieldTable"),
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(Dictionary<Int32, Object>)))));

			//											for (int fieldIndex = offset; (fieldIndex < transactionLogData.Length); fieldIndex = (fieldIndex + 2))
			//											{
			CodeIterationStatement forFieldIndex1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fieldIndex",
					new CodeVariableReferenceExpression("offset")),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("fieldIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLogData"), "Length")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("fieldIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(2))));

			//												fieldTable.Add(((int)(transactionLogData[fieldIndex])), transactionLogData[(fieldIndex + 1)]);
			forFieldIndex1.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("fieldTable"),
					"Add",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(
							new CodeVariableReferenceExpression("transactionLogData"),
							new CodeVariableReferenceExpression("fieldIndex"))),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogData"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1)))));

			//											}
			ifInUpdatedDictionary0.FalseStatements.Add(forFieldIndex1);

			//											updatedDictionary.Add(currentLink.Value, fieldTable);
			ifInUpdatedDictionary0.FalseStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("updatedDictionary"),
					"Add",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
					new CodeVariableReferenceExpression("fieldTable")));

			//											updatedItemList.Add(new FieldCollector(currentLink, fieldTable));
			ifInUpdatedDictionary0.FalseStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("updatedItemList"),
					"Add",
					new CodeObjectCreateExpression(
						new CodeTypeReference("FieldCollector"),
						new CodeVariableReferenceExpression("currentLink"),
						new CodeVariableReferenceExpression("fieldTable"))));

			//										}
			ifInDeletedDictionary0.FalseStatements.Add(ifInUpdatedDictionary0);

			//									}
			ifModified.TrueStatements.Add(ifInDeletedDictionary0);

			//								}
			ifRecordOld.TrueStatements.Add(ifModified);

			//								if ((((int)(transactionLogData[0])) == global::FluidTrade.Core.RecordState.Added))
			//								{
			CodeConditionStatement ifAdded = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("transactionLogData"), new CodePrimitiveExpression(0))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Added")));

			//									if (deletedDictionary.ContainsKey(currentLink.Value))
			//									{
			CodeConditionStatement ifInDeletedDictionary1 = new CodeConditionStatement(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedDictionary"),
					"ContainsKey",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value")));

			//										deletedItemList.Add(currentLink);
			ifInDeletedDictionary1.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedItemList"),
					"Add",
					new CodeVariableReferenceExpression("currentLink")));

			//									}
			//									else
			//									{
			//										global::System.Collections.Generic.Dictionary<int, object> fieldTable;
			ifInDeletedDictionary1.FalseStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Dictionary<Int32, Object>)),
					"fieldTable"));

			//										if (updatedDictionary.TryGetValue(currentLink.Value, out fieldTable))
			//										{
			CodeConditionStatement ifInUpdatedDictionary1 = new CodeConditionStatement(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("updatedDictionary"),
					"TryGetValue",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
					new CodeDirectionExpression(FieldDirection.Out, new CodeVariableReferenceExpression("fieldTable"))));

			//										int offset = (2 + currentLink.Value.keyLength);
			ifInUpdatedDictionary1.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"offset",
					new CodeBinaryOperatorExpression(
						new CodePrimitiveExpression(2),
						CodeBinaryOperatorType.Add,
						new CodeFieldReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Value"),
							"keyLength"))));

			//											for (int fieldIndex = offset; (fieldIndex < transactionLogData.Length); fieldIndex = (fieldIndex + 2))
			//											{
			CodeIterationStatement forFieldIndex2 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fieldIndex",
					new CodeVariableReferenceExpression("offset")),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("fieldIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLogData"), "Length")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("fieldIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(2))));

			//												if ((fieldTable.ContainsKey(((int)(transactionLogData[fieldIndex]))) == false))
			//												{
			CodeConditionStatement ifLacksField1 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("fieldTable"),
						"ContainsKey",
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("transactionLogData"),
								new CodeVariableReferenceExpression("fieldIndex")))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(false)));

			//													fieldTable.Add(((int)(transactionLogData[fieldIndex])), transactionLogData[(fieldIndex + 1)]);
			ifLacksField1.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("fieldTable"),
					"Add",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(
							new CodeVariableReferenceExpression("transactionLogData"),
							new CodeVariableReferenceExpression("fieldIndex"))),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogData"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1)))));

			//												}
			forFieldIndex2.Statements.Add(ifLacksField1);

			//											}
			ifInUpdatedDictionary1.TrueStatements.Add(forFieldIndex2);

			//											deletedItemList.Add(currentLink);
			ifInUpdatedDictionary1.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("deletedItemList"),
					"Add",
					new CodeVariableReferenceExpression("currentLink")));

			//										}
			ifInDeletedDictionary1.FalseStatements.Add(ifInUpdatedDictionary1);

			//									}
			ifAdded.TrueStatements.Add(ifInDeletedDictionary1);

			//								}
			ifRecordOld.TrueStatements.Add(ifAdded);

			//							}
			ifLink.TrueStatements.Add(ifRecordOld);

			//							currentLink = currentLink.Previous;
			ifLink.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("currentLink"),
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("currentLink"), "Previous")));

			//						}
			forCount.Statements.Add(ifLink);

			//						if ((currentLink == null))
			//						{
			CodeConditionStatement ifStartOfList = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("currentLink"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(null)));

			//							count = this.transactionLogCompressionSize;
			ifStartOfList.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("count"),
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogCompressionSize")));

			//							deletedDictionary.Clear();
			ifStartOfList.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("deletedDictionary"), "Clear"));

			//							updatedDictionary.Clear();
			ifStartOfList.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("updatedDictionary"), "Clear"));

			//							this.transactionLogLock.ExitReadLock();
			ifStartOfList.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"ExitReadLock"));

			//							global::System.Threading.Thread.Sleep(0);
			ifStartOfList.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Thread)),
					"Sleep",
					new CodePrimitiveExpression(0)));

			//							try
			//							{
			CodeTryCatchFinallyStatement tryWriteLock = new CodeTryCatchFinallyStatement();

			//								this.transactionLogLock.EnterWriteLock();
			tryWriteLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"EnterWriteLock"));

			//								for (int deletedIndex = 0; (deletedIndex < deletedItemList.Count); deletedIndex = (deletedIndex + 1))
			//								{
			CodeIterationStatement forDeletedIndex = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"deletedIndex",
					new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("deletedIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("deletedItemList"), "Count")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("deletedIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("deletedIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//									this.transactionLog.Remove(deletedItemList[deletedIndex]);
			forDeletedIndex.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLog"),
					"Remove",
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("deletedItemList"),
						new CodeVariableReferenceExpression("deletedIndex"))));

			//								}
			tryWriteLock.TryStatements.Add(forDeletedIndex);

			//								deletedItemList.Clear();
			tryWriteLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("deletedItemList"), "Clear"));

			//								for (int updateIndex = 0; (updateIndex < updatedItemList.Count); updateIndex = (updateIndex + 1))
			//								{
			CodeIterationStatement forUpdateIndex = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"updateIndex",
					new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("updateIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("updatedItemList"), "Count")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("updateIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("updateIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//									FieldCollector fieldCollector = updatedItemList[updateIndex];
			forUpdateIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("FieldCollector"),
					"fieldCollector",
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("updatedItemList"),
						new CodeVariableReferenceExpression("updateIndex"))));

			//									global::System.Collections.Generic.Dictionary<int, object> fieldTable = fieldCollector.fieldTable;
			forUpdateIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Dictionary<Int32, Object>)),
					"fieldTable",
					new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("fieldCollector"), "fieldTable")));

			//									int keyLength = fieldCollector.linkedListNode.Value.keyLength;
			forUpdateIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"keyLength",
					new CodeFieldReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("fieldCollector"), "linkedListNode"),
							"Value"),
						"keyLength")));

			//									int offset = (2 + keyLength);
			forUpdateIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"offset",
					new CodeBinaryOperatorExpression(
						new CodePrimitiveExpression(2),
						CodeBinaryOperatorType.Add,
						new CodeVariableReferenceExpression("keyLength"))));

			//									object[] data = new object[(offset
			//															+ (fieldTable.Count * 2))];
			forUpdateIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"data",
					new CodeArrayCreateExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("offset"),
							CodeBinaryOperatorType.Add,
							new CodeBinaryOperatorExpression(
								new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("fieldTable"), "Count"),
								CodeBinaryOperatorType.Multiply,
								new CodePrimitiveExpression(2))))));

			//									data[0] = global::FluidTrade.Core.RecordState.Added;
			forUpdateIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("data"), new CodePrimitiveExpression(0)),
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Added")));

			//									data[1] = fieldCollector.linkedListNode.Value.data[1];
			forUpdateIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("data"), new CodePrimitiveExpression(1)),
					new CodeIndexerExpression(
						new CodeFieldReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("fieldCollector"), "linkedListNode"),
								"Value"),
							"data"),
							new CodePrimitiveExpression(1))));

			//									for (int keyIndex = 2; (keyIndex
			//																< (2 + keyLength)); keyIndex = (keyIndex + 1))
			//									{
			CodeIterationStatement forKeyIndex = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"keyIndex",
					new CodePrimitiveExpression(2)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("keyIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodeBinaryOperatorExpression(
							new CodePrimitiveExpression(2),
							CodeBinaryOperatorType.Add,
							new CodeVariableReferenceExpression("keyLength"))),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("keyIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("keyIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//										data[keyIndex] = fieldCollector.linkedListNode.Value.data[keyIndex];
			forKeyIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("data"), new CodeVariableReferenceExpression("keyIndex")),
					new CodeIndexerExpression(
						new CodeFieldReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("fieldCollector"), "linkedListNode"),
								"Value"),
							"data"),
						new CodeVariableReferenceExpression("keyIndex"))));

			//									}
			forUpdateIndex.Statements.Add(forKeyIndex);

			//									int index = offset;
			forUpdateIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"index",
					new CodeVariableReferenceExpression("offset")));

			//									global::System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int, object>> fieldEnumerator = fieldTable.GetEnumerator();
			forUpdateIndex.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(IEnumerator<KeyValuePair<Int32, Object>>)),
					"fieldEnumerator",
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("fieldTable"),
						"GetEnumerator")));

			//								fieldLoop:
			forUpdateIndex.Statements.Add(new CodeLabeledStatement("fieldLoop"));

			//									if ((fieldEnumerator.MoveNext() == false))
			//									{
			CodeConditionStatement ifListEnd = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("fieldEnumerator"), "MoveNext"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(false)));

			//										goto fieldEnd;
			ifListEnd.TrueStatements.Add(new CodeGotoStatement("fieldEnd"));

			//									}
			forUpdateIndex.Statements.Add(ifListEnd);

			//									data[index] = fieldEnumerator.Current.Key;
			forUpdateIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("data"), new CodeVariableReferenceExpression("index")),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("fieldEnumerator"), "Current"),
						"Key")));

			//									index = (index + 1);
			forUpdateIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("index"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("index"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//									data[index] = fieldEnumerator.Current.Value;
			forUpdateIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("data"), new CodeVariableReferenceExpression("index")),
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("fieldEnumerator"), "Current"),
						"Value")));

			//									index = (index + 1);
			forUpdateIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("index"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("index"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//									goto fieldLoop;
			forUpdateIndex.Statements.Add(new CodeGotoStatement("fieldLoop"));

			//								fieldEnd:
			forUpdateIndex.Statements.Add(new CodeLabeledStatement("fieldEnd"));

			//									fieldCollector.linkedListNode.Value.data = data;
			forUpdateIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("fieldCollector"), "linkedListNode"),
							"Value"),
						"data"),
					new CodeVariableReferenceExpression("data")));

			//								}
			tryWriteLock.TryStatements.Add(forUpdateIndex);

			//								updatedItemList.Clear();
			tryWriteLock.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("updatedItemList"), "Clear"));

			//							}
			//							finally
			//							{
			//								this.transactionLogLock.ExitWriteLock();
			tryWriteLock.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"ExitWriteLock"));

			//							}
			ifStartOfList.TrueStatements.Add(tryWriteLock);

			//							global::System.Threading.Thread.Sleep(this.transactionLogCompressionTime);
			ifStartOfList.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Thread)),
					"Sleep",
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogCompressionTime")));

			//							this.transactionLogLock.EnterReadLock();
			ifStartOfList.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"EnterReadLock"));

			//						}
			forCount.Statements.Add(ifStartOfList);

			//					}
			tryReadLock.TryStatements.Add(forCount);

			//				}
			//				finally
			//				{
			//					this.transactionLogLock.ExitReadLock();
			tryReadLock.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"ExitReadLock"));

			//				}
			whileCollecting.Statements.Add(tryReadLock);

			//				global::System.Threading.Thread.Sleep(0);
			whileCollecting.Statements.Add(
				new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(Thread)), "Sleep", new CodePrimitiveExpression(0)));

			//			}
			this.Statements.Add(whileCollecting);

			//		}

		}

	}

}

namespace FluidTrade.ClientGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Data;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to read the modified records from the server data model into the client data model.
	/// </summary>
	class ReadDataModelMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public ReadDataModelMethod(DataModelSchema dataModelSchema)
		{

			// These variable are used to create a connection to the server.
			string clientTypeName = string.Format("{0}Client", dataModelSchema.Name);
			string endpointName = string.Format("{0}Endpoint", dataModelSchema.Name);
			string clientVariableName = CommonConversion.ToCamelCase(clientTypeName);

			//		/// <summary>
			//		/// This thread will periodically reconcile the client data model with the server's.
			//		/// </summary>
			//		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//		private static void ReadDataModel()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("This thread will periodically reconcile the client data model with the server's.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Name = "ReadDataModel";

			//			DataModel.ReconcileDataModel();
			this.Statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "PurgeDataModel"));

			//			object[] transactionLog = new global::System.Object[0];
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"transactionLog",
					new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(Object[])), 0)));

			//			for (
			//			; (DataModel.IsReading == true); 
			//			)
			//			{
			CodeIterationStatement whileReconciling = new CodeIterationStatement(
				new CodeSnippetStatement(),
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "IsReading"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(true)),
				new CodeSnippetStatement());

			//					DataModelClient dataModelClient = null;
			whileReconciling.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference(clientTypeName),
					clientVariableName,
					new CodePrimitiveExpression(null)));

			//				global::System.Int64 maxSequence = 0;
			whileReconciling.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int64)),
					"maxSequence",
					new CodePrimitiveExpression(0)));

			//				try
			//				{
			CodeTryCatchFinallyStatement tryReading = new CodeTryCatchFinallyStatement();

			//					dataModelClient = new DataModelClient(FluidTrade.Core.Guardian.Properties.Settings.Default.DataModelEndpoint);
			tryReading.TryStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression(clientVariableName),
					new CodeObjectCreateExpression(
						new CodeTypeReference(clientTypeName),
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodePropertyReferenceExpression(
									new CodeTypeReferenceExpression(
										string.Format("{0}.{1}", dataModelSchema.TargetNamespace, "Properties")),
									"Settings"),
								"Default"),
							string.Format("{0}Endpoint", dataModelSchema.Name)))));

			//					object[] dataHeader = dataModelClient.Read(DataModel.dataSetId, DataModel.sequence);
			tryReading.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"dataHeader",
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression(clientVariableName),
						"Read",
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSetId"),
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "sequence"))));

			//					try
			//					{
			CodeTryCatchFinallyStatement tryLockingModel = new CodeTryCatchFinallyStatement();

			//						global::System.Threading.Monitor.Enter(DataModel.syncRoot);
			tryLockingModel.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)),
					"Enter",
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//						global::System.Guid dataSetId = ((global::System.Guid)(dataHeader[0]));
			tryLockingModel.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Guid)),
					"dataSetId",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Guid)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(0)))));

			//						DataModel.sequence = ((long)(dataHeader[1]));
			tryLockingModel.TryStatements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "sequence"),
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int64)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(1)))));

			//						maxSequence = ((long)(dataHeader[2]));
			tryLockingModel.TryStatements.Add(
				new CodeAssignStatement(new CodeVariableReferenceExpression("maxSequence"),
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int64)),
						new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(2)))));

			//						if (transactionLog.Length == 0)
			//						{
			CodeConditionStatement ifBufferEmpty = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(
						new CodeVariableReferenceExpression("transactionLog"), "Length"),
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression(0)));

			//							transactionLog = ((object[])(dataHeader[3]));
			ifBufferEmpty.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("transactionLog"),
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(3)))));

			//						}
			//						else
			//						{
			//							object[] pagedLog = ((object[])(dataHeader[3]));
			ifBufferEmpty.FalseStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"pagedLog",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(3)))));

			//							global::System.Object[] mergedLog = new global::System.Object[(transactionLog.Length + pagedLog.Length)];
			ifBufferEmpty.FalseStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"mergedLog",
					new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(Object[])),
						new CodeBinaryOperatorExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLog"), "Length"),
							CodeBinaryOperatorType.Add,
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("pagedLog"), "Length")))));

			//							global::System.Array.Copy(transactionLog, mergedLog, transactionLog.Length);
			ifBufferEmpty.FalseStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Array)),
					"Copy",
					new CodeVariableReferenceExpression("transactionLog"),
					new CodeVariableReferenceExpression("mergedLog"),
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLog"), "Length")));

			//							global::System.Array.Copy(pagedLog, 0, mergedLog, transactionLog.Length, pagedLog.Length);
			ifBufferEmpty.FalseStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Array)),
					"Copy",
					new CodeVariableReferenceExpression("pagedLog"),
					new CodePrimitiveExpression(0),
					new CodeVariableReferenceExpression("mergedLog"),
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLog"), "Length"),
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("pagedLog"), "Length")));

			//							transactionLog = mergedLog;
			ifBufferEmpty.FalseStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("transactionLog"), new CodeVariableReferenceExpression("mergedLog")));

			//						}
			tryLockingModel.TryStatements.Add(ifBufferEmpty);		
			
			//						if ((dataSetId != DataModel.dataSetId))
			//						{
			CodeConditionStatement ifInvalidDataSet = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("dataSetId"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSetId")));

			//							DataModel.dataSetId = dataSetId;
			ifInvalidDataSet.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSetId"),
					new CodeVariableReferenceExpression("dataSetId")));

			//							DataModel.dataSet.EnforceConstraints = false;
			ifInvalidDataSet.TrueStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"),
						"EnforceConstraints"),
					new CodePrimitiveExpression(false)));

			//								DataModel.dataSet.Clear();
			ifInvalidDataSet.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"),
					"Clear"));

			//							DataModel.dataSet.EnforceConstraints = true;
			ifInvalidDataSet.TrueStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "dataSet"),
						"EnforceConstraints"),
					new CodePrimitiveExpression(true)));

			//						}
			tryLockingModel.TryStatements.Add(ifInvalidDataSet);

			//						if ((transactionLog.Length != 0))
			//						{
			CodeConditionStatement ifResults = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLog"), "Length"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(0)));

			//							DataModel.OnBeginMerge(typeof(DataModel));
			ifResults.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"OnBeginMerge",
					new CodeTypeOfExpression(dataModelSchema.Name)));

			//							if (DataModel.Progress != null)
			//							{
			//								DataModel.Progress(typeof(DataModel), new global::FluidTrade.Core.ProgressEventArgs(double.MinValue, maxSequence, DataModel.sequence));
			//							}
			CodeConditionStatement ifProgressEvent = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Progress"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));
			ifProgressEvent.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"Progress",
					new CodeTypeOfExpression(new CodeTypeReference(dataModelSchema.Name)),
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(ProgressEventArgs)),
						new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Double)), "MinValue"),
						new CodeVariableReferenceExpression("maxSequence"),
						new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "sequence"))));
			ifResults.TrueStatements.Add(ifProgressEvent);

			//						StartRowsLoop:
			ifResults.TrueStatements.Add(new CodeLabeledStatement("StartRowsLoop"));

			//							if ((transactionLog.Length == 0))
			//							{
			CodeConditionStatement ifLogEmpty = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLog"), "Length"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(0)));

			//								goto EndRowsLoop;
			ifLogEmpty.TrueStatements.Add(new CodeGotoStatement("EndRowsLoop"));

			//							}
			ifResults.TrueStatements.Add(ifLogEmpty);

			//							bool isAnythingMerged = false;
			ifResults.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Boolean)),
					"isAnythingMerged",
					new CodePrimitiveExpression(false)));

			//							global::System.Data.DataRow destinationRow = null;
			ifResults.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataRow)),
					"destinationRow",
					new CodePrimitiveExpression(null)));

			//							global::System.Collections.ArrayList unhandledRows = new global::System.Collections.ArrayList();
			ifResults.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(ArrayList)),
					"unhandledRows",
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(ArrayList)))));

			//							int rowIndex = 0;
			ifResults.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"rowIndex",
					new CodePrimitiveExpression(0)));

			//							try
			//							{
			CodeTryCatchFinallyStatement tryItem = new CodeTryCatchFinallyStatement();

			//							StartRowLoop:
			tryItem.TryStatements.Add(new CodeLabeledStatement("StartRowLoop"));

			//								if ((rowIndex == transactionLogLength))
			//								{
			CodeConditionStatement ifRowsAllRead = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("rowIndex"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLog"), "Length")));

			//									goto EndRowLoop;
			ifRowsAllRead.TrueStatements.Add(new CodeGotoStatement("EndRowLoop"));

			//								}
			tryItem.TryStatements.Add(ifRowsAllRead);

			//								object[] transactionLogItem = ((object[])(transactionLog[rowIndex]));
			tryItem.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"transactionLogItem",
					new CodeCastExpression(
						new CodeTypeReference(typeof(Object[])),
						new CodeIndexerExpression(
							new CodeVariableReferenceExpression("transactionLog"),
							new CodeVariableReferenceExpression("rowIndex")))));

			//								global::System.Data.DataTable destinationTable = DataModel.Tables[((int)(transactionLogItem[1]))];
			tryItem.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataTable)),
					"destinationTable",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"),
						new CodeCastExpression(
							new CodeTypeReference(typeof(Int32)),
							new CodeIndexerExpression(new CodeVariableReferenceExpression("transactionLogItem"), new CodePrimitiveExpression(1))))));

			//								object[] key = new object[destinationTable.PrimaryKey.Length];
			tryItem.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"key",
					new CodeArrayCreateExpression(
						new CodeGlobalTypeReference(typeof(Object)),
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "PrimaryKey"),
							"Length"))));

			//								for (int keyIndex = 0; (keyIndex < destinationTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
			//								{
			CodeIterationStatement forKeyIndex0 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)),
					"keyIndex",
					new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("keyIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodeVariableReferenceExpression("destinationTable"), "PrimaryKey"),
						"Length")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("keyIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("keyIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//									key[keyIndex] = transactionLogItem[(2 + keyIndex)];
			forKeyIndex0.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("key"), new CodeVariableReferenceExpression("keyIndex")),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						new CodeBinaryOperatorExpression(
							new CodePrimitiveExpression(2),
							CodeBinaryOperatorType.Add,
							new CodeVariableReferenceExpression("keyIndex")))));

			//								}
			tryItem.TryStatements.Add(forKeyIndex0);

			//								destinationRow = destinationTable.Rows.Find(key);
			tryItem.TryStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("destinationRow"),
					new CodeMethodInvokeExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "Rows"),
						"Find",
						new CodeVariableReferenceExpression("key"))));

			//								int dataRowState = ((int)(transactionLogItem[0]));
			tryItem.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"dataRowState",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("transactionLogItem"), new CodePrimitiveExpression(0)))));

			//								if ((dataRowState == global::FluidTrade.Core.RecordState.Modified))
			//								{
			CodeConditionStatement ifModified = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("dataRowState"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Modified")));

			//									if ((destinationRow == null))
			//									{
			CodeConditionStatement ifNotInModel0 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("destinationRow"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(null)));

			//										goto KeepRow;
			ifNotInModel0.TrueStatements.Add(new CodeGotoStatement("KeepRow"));

			//									}
			ifModified.TrueStatements.Add(ifNotInModel0);

			//									int offset = (2 + destinationTable.PrimaryKey.Length);
			ifModified.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"offset",
					new CodeBinaryOperatorExpression(
						new CodePrimitiveExpression(2),
						CodeBinaryOperatorType.Add,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "PrimaryKey"),
							"Length"))));

			//									int fields = ((transactionLogItem.Length - offset) 
			//												/ 2);
			ifModified.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fields",
					new CodeBinaryOperatorExpression(
						new CodeBinaryOperatorExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLogItem"), "Length"),
							CodeBinaryOperatorType.Subtract,
							new CodeVariableReferenceExpression("offset")),
							CodeBinaryOperatorType.Divide,
							new CodePrimitiveExpression(2))));

            //										for (int parentIndex = 0; (parentIndex < destinationTable.ParentRelations.Count); parentIndex = (parentIndex + 1))
            //										{
            CodeIterationStatement parentLoop0 = new CodeIterationStatement(
                new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "parentIndex", new CodePrimitiveExpression(0)),
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("parentIndex"),
                    CodeBinaryOperatorType.LessThan,
                    new CodePropertyReferenceExpression(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "ParentRelations"),
                        "Count")),
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("parentIndex"),
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression("parentIndex"),
                        CodeBinaryOperatorType.Add,
                        new CodePrimitiveExpression(1))));

            //											global::System.Data.DataRelation parentRelation = destinationTable.ParentRelations[parentIndex];
            parentLoop0.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeGlobalTypeReference(typeof(System.Data.DataRelation)),
                    "parentRelation",
                    new CodeIndexerExpression(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "ParentRelations"),
                        new CodeVariableReferenceExpression("parentIndex"))));

            //											bool isNullKey = true;
            parentLoop0.Statements.Add(
                new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Boolean)), "isNullKey", new CodePrimitiveExpression(true)));

            //											object[] parentKey = new object[parentRelation.ChildColumns.Length];
            parentLoop0.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeGlobalTypeReference(typeof(System.Object[])),
                    "parentKey",
                    new CodeArrayCreateExpression(
                        typeof(System.Object[]),
                        new CodePropertyReferenceExpression(
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ChildColumns"),
                            "Length"))));

            //											for (int keyIndex = 0; (keyIndex < parentRelation.ChildColumns.Length); keyIndex = (keyIndex + 1))
            //											{
            CodeIterationStatement keyLoop0 = new CodeIterationStatement(
                new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "keyIndex", new CodePrimitiveExpression(0)),
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("keyIndex"),
                    CodeBinaryOperatorType.LessThan,
                    new CodePropertyReferenceExpression(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ChildColumns"),
                        "Length")),
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("keyIndex"),
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression("keyIndex"),
                        CodeBinaryOperatorType.Add,
                        new CodePrimitiveExpression(1))));

            //												global::System.Data.DataColumn dataColumn = parentRelation.ChildColumns[keyIndex];
            keyLoop0.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeGlobalTypeReference(typeof(DataColumn)),
                    "dataColumn",
                    new CodeIndexerExpression(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ChildColumns"),
                        new CodeVariableReferenceExpression("keyIndex"))));

            //												parentKey[keyIndex] = dataColumn.DefaultValue;
            keyLoop0.Statements.Add(
                new CodeAssignStatement(
                    new CodeIndexerExpression(new CodeVariableReferenceExpression("parentKey"), new CodeVariableReferenceExpression("keyIndex")),
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataColumn"), "DefaultValue")));

            //												for (int field = 0; (field < fields); field = (field + 1))
            //												{
            CodeIterationStatement fieldLoop0 = new CodeIterationStatement(
                new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "field", new CodePrimitiveExpression(0)),
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("field"),
                    CodeBinaryOperatorType.LessThan,
                    new CodeVariableReferenceExpression("fields")),
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("field"),
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression("field"),
                        CodeBinaryOperatorType.Add,
                        new CodePrimitiveExpression(1))));

            //													int fieldIndex = (offset 
            //																+ (field * 2));
            fieldLoop0.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeGlobalTypeReference(typeof(Int32)),
                    "fieldIndex",
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression("offset"),
                        CodeBinaryOperatorType.Add,
                        new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression("field"),
                            CodeBinaryOperatorType.Multiply,
                            new CodePrimitiveExpression(2)))));

            //													if ((((int)(transactionLogItem[fieldIndex])) == dataColumn.Ordinal))
            //													{
            CodeConditionStatement ifIsPartOfIndex0 = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeCastExpression(
                        new CodeGlobalTypeReference(typeof(Int32)),
                        new CodeIndexerExpression(
                            new CodeVariableReferenceExpression("transactionLogItem"),
                            new CodeVariableReferenceExpression("fieldIndex"))),
                    CodeBinaryOperatorType.IdentityEquality,
                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataColumn"), "Ordinal")));

            //														parentKey[keyIndex] = transactionLogItem[(fieldIndex + 1)];
            ifIsPartOfIndex0.TrueStatements.Add(
                new CodeAssignStatement(
                    new CodeIndexerExpression(new CodeVariableReferenceExpression("parentKey"), new CodeVariableReferenceExpression("keyIndex")),
                    new CodeIndexerExpression(
                        new CodeVariableReferenceExpression("transactionLogItem"),
                        new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression("fieldIndex"),
                            CodeBinaryOperatorType.Add,
                            new CodePrimitiveExpression(1)))));

            //													}
            fieldLoop0.Statements.Add(ifIsPartOfIndex0);

            //												}
            keyLoop0.Statements.Add(fieldLoop0);

            //												if ((parentKey[keyIndex] != global::System.DBNull.Value))
            //												{
            CodeConditionStatement isNullKey0 = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeIndexerExpression(new CodeVariableReferenceExpression("parentKey"), new CodeVariableReferenceExpression("keyIndex")),
                    CodeBinaryOperatorType.IdentityInequality,
                    new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.DBNull)), "Value")));

            //													isNullKey = false;
            isNullKey0.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("isNullKey"), new CodePrimitiveExpression(false)));

            //												}
            keyLoop0.Statements.Add(isNullKey0);

            //											}
            parentLoop0.Statements.Add(keyLoop0);

            //											if (((isNullKey == false) 
            //														&& (parentRelation.ParentTable.Rows.Find(parentKey) == null)))
            //											{
            CodeConditionStatement keepRow0 = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression("isNullKey"),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(false)),
                    CodeBinaryOperatorType.BooleanAnd,
                    new CodeBinaryOperatorExpression(
                        new CodeMethodInvokeExpression(
                            new CodePropertyReferenceExpression(
                                new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ParentTable"),
                                "Rows"),
                            "Find",
                            new CodeVariableReferenceExpression("parentKey")),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null))));

            //												goto KeepRow;
            keepRow0.TrueStatements.Add(new CodeGotoStatement("KeepRow"));

            //											}
            parentLoop0.Statements.Add(keepRow0);

            //										}
            ifModified.TrueStatements.Add(parentLoop0);

            //									destinationRow.BeginEdit();
			ifModified.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "BeginEdit"));

			//									for (int field = 0; (field < fields); field = (field + 1))
			//									{
			CodeIterationStatement forFields2 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"field",
					new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("field"),
					CodeBinaryOperatorType.LessThan,
					new CodeVariableReferenceExpression("fields")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("field"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("field"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//										int fieldIndex = (offset 
			//													+ (field * 2));
			forFields2.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fieldIndex",
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("offset"),
						CodeBinaryOperatorType.Add,
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("field"),
							CodeBinaryOperatorType.Multiply,
							new CodePrimitiveExpression(2)))));

			//										destinationRow[((int)(transactionLogItem[fieldIndex]))] = transactionLogItem[(fieldIndex + 1)];
			forFields2.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("destinationRow"),
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("transactionLogItem"),
								new CodeVariableReferenceExpression("fieldIndex")))),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1)))));

			//									}
			ifModified.TrueStatements.Add(forFields2);

			//									destinationRow.EndEdit();
			ifModified.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "EndEdit"));

			//									destinationRow.AcceptChanges();
			ifModified.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "AcceptChanges"));

			//									isAnythingMerged = true;
			ifModified.TrueStatements.Add(
				new CodeAssignStatement(new CodeVariableReferenceExpression("isAnythingMerged"), new CodePrimitiveExpression(true)));

			//								}
			//								else
			//								{
			//									if ((dataRowState == global::FluidTrade.Core.RecordState.Added))
			//									{
			CodeConditionStatement ifAdded = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("dataRowState"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Added")));

			//										int offset = (2 + destinationTable.PrimaryKey.Length);
			ifAdded.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"offset",
					new CodeBinaryOperatorExpression(
						new CodePrimitiveExpression(2),
						CodeBinaryOperatorType.Add,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "PrimaryKey"),
							"Length"))));

			//										int fields = ((transactionLogItem.Length - offset) 
			//													/ 2);
			ifAdded.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fields",
					new CodeBinaryOperatorExpression(
						new CodeBinaryOperatorExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionLogItem"), "Length"),
							CodeBinaryOperatorType.Subtract,
							new CodeVariableReferenceExpression("offset")),
							CodeBinaryOperatorType.Divide,
							new CodePrimitiveExpression(2))));

			//										for (int parentIndex = 0; (parentIndex < destinationTable.ParentRelations.Count); parentIndex = (parentIndex + 1))
			//										{
			CodeIterationStatement parentLoop1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "parentIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("parentIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "ParentRelations"),
						"Count")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("parentIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("parentIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//											global::System.Data.DataRelation parentRelation = destinationTable.ParentRelations[parentIndex];
			parentLoop1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Data.DataRelation)),
					"parentRelation",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "ParentRelations"),
						new CodeVariableReferenceExpression("parentIndex"))));

			//											bool isNullKey = true;
			parentLoop1.Statements.Add(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Boolean)), "isNullKey", new CodePrimitiveExpression(true)));

			//											object[] parentKey = new object[parentRelation.ChildColumns.Length];
			parentLoop1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Object[])),
					"parentKey",
					new CodeArrayCreateExpression(
						typeof(System.Object[]),
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ChildColumns"),
							"Length"))));

			//											for (int keyIndex = 0; (keyIndex < parentRelation.ChildColumns.Length); keyIndex = (keyIndex + 1))
			//											{
			CodeIterationStatement keyLoop1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "keyIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("keyIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ChildColumns"),
						"Length")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("keyIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("keyIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//												global::System.Data.DataColumn dataColumn = parentRelation.ChildColumns[keyIndex];
			keyLoop1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataColumn)),
					"dataColumn",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ChildColumns"),
						new CodeVariableReferenceExpression("keyIndex"))));

			//												parentKey[keyIndex] = dataColumn.DefaultValue;
			keyLoop1.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("parentKey"), new CodeVariableReferenceExpression("keyIndex")),
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataColumn"), "DefaultValue")));

			//												for (int field = 0; (field < fields); field = (field + 1))
			//												{
			CodeIterationStatement fieldLoop1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "field", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("field"),
					CodeBinaryOperatorType.LessThan,
					new CodeVariableReferenceExpression("fields")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("field"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("field"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//													int fieldIndex = (offset 
			//																+ (field * 2));
			fieldLoop1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fieldIndex",
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("offset"),
						CodeBinaryOperatorType.Add,
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("field"),
							CodeBinaryOperatorType.Multiply,
							new CodePrimitiveExpression(2)))));

			//													if ((((int)(transactionLogItem[fieldIndex])) == dataColumn.Ordinal))
			//													{
			CodeConditionStatement ifisPartOfIndex1 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(
							new CodeVariableReferenceExpression("transactionLogItem"),
							new CodeVariableReferenceExpression("fieldIndex"))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataColumn"), "Ordinal")));

			//														parentKey[keyIndex] = transactionLogItem[(fieldIndex + 1)];
			ifisPartOfIndex1.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("parentKey"), new CodeVariableReferenceExpression("keyIndex")),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1)))));

			//													}
			fieldLoop1.Statements.Add(ifisPartOfIndex1);

			//												}
			keyLoop1.Statements.Add(fieldLoop1);

			//												if ((parentKey[keyIndex] != global::System.DBNull.Value))
			//												{
			CodeConditionStatement isNullKey1 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("parentKey"), new CodeVariableReferenceExpression("keyIndex")),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.DBNull)), "Value")));

			//													isNullKey = false;
			isNullKey1.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("isNullKey"), new CodePrimitiveExpression(false)));

			//												}
			keyLoop1.Statements.Add(isNullKey1);

			//											}
			parentLoop1.Statements.Add(keyLoop1);

			//											if (((isNullKey == false) 
			//														&& (parentRelation.ParentTable.Rows.Find(parentKey) == null)))
			//											{
			CodeConditionStatement keepRow1 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("isNullKey"),
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression(false)),
					CodeBinaryOperatorType.BooleanAnd,
					new CodeBinaryOperatorExpression(
						new CodeMethodInvokeExpression(
							new CodePropertyReferenceExpression(
								new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("parentRelation"), "ParentTable"),
								"Rows"),
							"Find",
							new CodeVariableReferenceExpression("parentKey")),
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression(null))));

			//												goto KeepRow;
			keepRow1.TrueStatements.Add(new CodeGotoStatement("KeepRow"));

			//											}
			parentLoop1.Statements.Add(keepRow1);

			//										}
			ifAdded.TrueStatements.Add(parentLoop1);

			//										if ((destinationRow == null))
			//										{
			CodeConditionStatement ifNotInModel1 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("destinationRow"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(null)));

			//											destinationRow = destinationTable.NewRow();
			ifNotInModel1.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("destinationRow"),
					new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationTable"), "NewRow")));

			//											for (int field = 0; (field < fields); field = (field + 1))
			//											{
			CodeIterationStatement forFields0 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"field",
					new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("field"),
					CodeBinaryOperatorType.LessThan,
					new CodeVariableReferenceExpression("fields")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("field"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("field"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//												int fieldIndex = (offset 
			//															+ (field * 2));
			forFields0.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fieldIndex",
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("offset"),
						CodeBinaryOperatorType.Add,
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("field"),
							CodeBinaryOperatorType.Multiply,
							new CodePrimitiveExpression(2)))));

			//												destinationRow[((int)(transactionLogItem[fieldIndex]))] = transactionLogItem[(fieldIndex + 1)];
			forFields0.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("destinationRow"),
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("transactionLogItem"),
								new CodeVariableReferenceExpression("fieldIndex")))),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1)))));

			//											}
			ifNotInModel1.TrueStatements.Add(forFields0);

			//											destinationTable.Rows.Add(destinationRow);
			ifNotInModel1.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("destinationTable"), "Rows"),
					"Add",
					new CodeVariableReferenceExpression("destinationRow")));

			//										}
			//										else
			//										{
			//											destinationRow.BeginEdit();
			ifNotInModel1.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "BeginEdit"));

			//											for (int field = 0; (field < fields); field = (field + 1))
			//											{
			CodeIterationStatement forFields1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"field",
					new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("field"),
					CodeBinaryOperatorType.LessThan,
					new CodeVariableReferenceExpression("fields")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("field"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("field"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//												int fieldIndex = (offset 
			//															+ (field * 2));
			forFields1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"fieldIndex",
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("offset"),
						CodeBinaryOperatorType.Add,
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("field"),
							CodeBinaryOperatorType.Multiply,
							new CodePrimitiveExpression(2)))));

			//												destinationRow[((int)(transactionLogItem[fieldIndex]))] = transactionLogItem[(fieldIndex + 1)];
			forFields1.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("destinationRow"),
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("transactionLogItem"),
								new CodeVariableReferenceExpression("fieldIndex")))),
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("fieldIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1)))));

			//											}
			ifNotInModel1.FalseStatements.Add(forFields1);

			//											destinationRow.EndEdit();
			ifNotInModel1.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "EndEdit"));

			//										}
			ifAdded.TrueStatements.Add(ifNotInModel1);

			//										destinationRow.AcceptChanges();
			ifAdded.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "AcceptChanges"));

			//										isAnythingMerged = true;
			ifAdded.TrueStatements.Add(
				new CodeAssignStatement(new CodeVariableReferenceExpression("isAnythingMerged"), new CodePrimitiveExpression(true)));

			//									}
			//									else
			//									{
			//										if ((destinationRow == null))
			//										{
			CodeConditionStatement ifNotInModel2 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("destinationRow"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(null)));

			//											goto KeepRow;
			ifNotInModel2.TrueStatements.Add(new CodeGotoStatement("KeepRow"));

			//										}
			ifAdded.FalseStatements.Add(ifNotInModel0);

			//										destinationRow.Delete();
			ifAdded.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "Delete"));

			//										destinationRow.AcceptChanges();
			ifAdded.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "AcceptChanges"));

			//										isAnythingMerged = true;
			ifAdded.FalseStatements.Add(
				new CodeAssignStatement(new CodeVariableReferenceExpression("isAnythingMerged"), new CodePrimitiveExpression(true)));

			//									}
			ifModified.FalseStatements.Add(ifAdded);

			//								}
			tryItem.TryStatements.Add(ifModified);

			//								goto NextRow;
			tryItem.TryStatements.Add(new CodeGotoStatement("NextRow"));

			//							KeepRow:
			tryItem.TryStatements.Add(new CodeLabeledStatement("KeepRow"));

			//								unhandledRows.Add(transactionLogItem);
			tryItem.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("unhandledRows"),
					"Add",
					new CodeVariableReferenceExpression("transactionLogItem")));

			//							NextRow:
			tryItem.TryStatements.Add(new CodeLabeledStatement("NextRow"));

			//								rowIndex = (rowIndex - 1);
			tryItem.TryStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("rowIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("rowIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//								goto StartRowLoop;
			tryItem.TryStatements.Add(new CodeGotoStatement("StartRowLoop"));

			//							}
			//							catch (global::System.Exception exception)
			//							{
			CodeCatchClause tryItemCatch = new CodeCatchClause("exception0", new CodeGlobalTypeReference(typeof(Exception)));

			//								global::FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			tryItemCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(EventLog)),
					"Error",
					new CodePrimitiveExpression("{0}, {1}"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception0"), "Message"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception0"), "StackTrace")));

			//								try
			//								{
			CodeTryCatchFinallyStatement tryRejectChanges = new CodeTryCatchFinallyStatement();

			//									if ((destinationRow != null))
			//									{
			CodeConditionStatement ifDestinationRowNull = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("destinationRow"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//										destinationRow.RejectChanges();
			ifDestinationRowNull.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("destinationRow"), "RejectChanges"));

			//									}
			tryRejectChanges.TryStatements.Add(ifDestinationRowNull);

			//								}
			//								catch (global::System.Exception exception)
			//								{
			CodeCatchClause tryRejectChangesCatch = new CodeCatchClause("exception1", new CodeGlobalTypeReference(typeof(Exception)));

			//									global::FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			tryRejectChangesCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(EventLog)),
					"Error",
					new CodePrimitiveExpression("{0}, {1}"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception1"), "Message"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception1"), "StackTrace")));

			//								}
			tryRejectChanges.CatchClauses.Add(tryRejectChangesCatch);
			tryItemCatch.Statements.Add(tryRejectChanges);

			//							}
			tryItem.CatchClauses.Add(tryItemCatch);
			ifResults.TrueStatements.Add(tryItem);

			//						EndRowLoop:
			ifResults.TrueStatements.Add(new CodeLabeledStatement("EndRowLoop"));

			//							transactionLog = unhandledRows.ToArray();
			ifResults.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("transactionLog"),
					new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("unhandledRows"), "ToArray")));

			//							if (((unhandledRows.Count != 0) 
			//										&& (isAnythingMerged == false)))
			//							{
			CodeConditionStatement ifNothingMerged = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeBinaryOperatorExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("unhandledRows"), "Count"),
						CodeBinaryOperatorType.IdentityInequality,
						new CodePrimitiveExpression(0)),
					CodeBinaryOperatorType.BooleanAnd,
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("isAnythingMerged"),
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression(false))));

			//								throw new global::System.Data.ConstraintException("Results from the server couldn\'t be merged into the client data model.");
			//							goto StartRowsLoop;
			ifNothingMerged.TrueStatements.Add(new CodeGotoStatement("EndRowsLoop"));

			//							}
			ifResults.TrueStatements.Add(ifNothingMerged);

			//							goto StartRowsLoop;
			ifResults.TrueStatements.Add(new CodeGotoStatement("StartRowsLoop"));

			//						EndRowsLoop:
			ifResults.TrueStatements.Add(new CodeLabeledStatement("EndRowsLoop"));

			//							DataModel.OnEndMerge(typeof(DataModel));
			ifResults.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"OnEndMerge",
					new CodeTypeOfExpression(new CodeTypeReference(dataModelSchema.Name))));

			//						}
			tryLockingModel.TryStatements.Add(ifResults);

			//					}
			//					finally
			//					{
			//						global::System.Threading.Monitor.Exit(DataModel.syncRoot);
			//					}
			tryLockingModel.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Monitor)),
					"Exit",
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));
			tryReading.TryStatements.Add(tryLockingModel);

			//					try
			//					{
			CodeTryCatchFinallyStatement trySignalEndOfMerge = new CodeTryCatchFinallyStatement();

			//						global::System.Threading.Monitor.Enter(DataModel.syncUpdate);
			trySignalEndOfMerge.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)),
					"Enter",
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncUpdate")));

			//						global::System.Threading.Monitor.PulseAll(DataModel.syncUpdate);
			trySignalEndOfMerge.TryStatements.Add(
				new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)),
					"PulseAll",
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncUpdate")));

			//					}
			//					finally
			//					{
			//						global::System.Threading.Monitor.Exit(DataModel.syncUpdate);
			//					}
			trySignalEndOfMerge.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)),
					"Exit",
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncUpdate")));

			//				}
			tryReading.TryStatements.Add(trySignalEndOfMerge);

			//				catch (global::System.ServiceModel.CommunicationObjectAbortedException )
			//				{
			//					global::FluidTrade.Core.ChannelStatus.LoginEvent.Reset();
			//				}
			CodeCatchClause communicationObjectAbortCatch = new CodeCatchClause(
				null,
				new CodeGlobalTypeReference(typeof(System.ServiceModel.CommunicationObjectAbortedException)));
			communicationObjectAbortCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ChannelStatus)), "LoginEvent"),
					"Reset"));
			tryReading.CatchClauses.Add(communicationObjectAbortCatch);

			//				catch (global::System.ServiceModel.EndpointNotFoundException )
			//				{
			CodeCatchClause endpointNotFoundCatch = new CodeCatchClause(
				null,
				new CodeGlobalTypeReference(typeof(System.ServiceModel.EndpointNotFoundException)));

			//					global::System.Threading.Thread.Sleep(1000);
			endpointNotFoundCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Thread)),
					"Sleep",
					new CodePrimitiveExpression(1000)));

			//				}
			tryReading.CatchClauses.Add(endpointNotFoundCatch);

			//				catch (global::System.ServiceModel.Security.SecurityAccessDeniedException securityAccessDeniedException)
			//				{
			CodeCatchClause securityAccessDeniedCatch = new CodeCatchClause(
				"securityAccessDeniedException",
				new CodeGlobalTypeReference(typeof(System.ServiceModel.Security.SecurityAccessDeniedException)));

			//					if ((DataModel.CommunicationException != null))
			//					{
			CodeConditionStatement ifCommunicationException0 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "CommunicationException"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//						DataModel.CommunicationException(typeof(DataModel), new global::FluidTrade.Core.ExceptionEventArgs(securityAccessDeniedException));
			ifCommunicationException0.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"CommunicationException",
					new CodeTypeOfExpression(new CodeTypeReference(dataModelSchema.Name)),
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(ExceptionEventArgs)),
						new CodeVariableReferenceExpression("securityAccessDeniedException"))));

			//					}
			securityAccessDeniedCatch.Statements.Add(ifCommunicationException0);

			//					global::FluidTrade.Core.ChannelStatus.IsPrompted = true;
			securityAccessDeniedCatch.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ChannelStatus)), "IsPrompted"),
					new CodePrimitiveExpression(true)));

			//				}
			tryReading.CatchClauses.Add(securityAccessDeniedCatch);

			//				catch (global::System.ServiceModel.Security.MessageSecurityException messageSecurityException)
			//				{
			CodeCatchClause messageSecurityCatch = new CodeCatchClause(
				"messageSecurityException",
				new CodeGlobalTypeReference(typeof(System.ServiceModel.Security.MessageSecurityException)));

			//					if ((DataModel.CommunicationException != null))
			//					{
			CodeConditionStatement ifCommunicationException1 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "CommunicationException"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//						DataModel.CommunicationException(typeof(DataModel), new global::FluidTrade.Core.ExceptionEventArgs(messageSecurityException));
			ifCommunicationException1.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"CommunicationException",
					new CodeTypeOfExpression(
						new CodeTypeReference(dataModelSchema.Name)),
						new CodeObjectCreateExpression(
							new CodeGlobalTypeReference(typeof(ExceptionEventArgs)),
							new CodeVariableReferenceExpression("messageSecurityException"))));

			//					}
			messageSecurityCatch.Statements.Add(ifCommunicationException1);

			//					global::FluidTrade.Core.ChannelStatus.IsPrompted = true;
			messageSecurityCatch.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ChannelStatus)), "IsPrompted"),
					new CodePrimitiveExpression(true)));

			//				}
			tryReading.CatchClauses.Add(messageSecurityCatch);

			//				catch (global::System.ServiceModel.CommunicationException communicationException)
			//				{
			CodeCatchClause communicationCatch = new CodeCatchClause(
				"communicationException",
				new CodeGlobalTypeReference(typeof(System.ServiceModel.CommunicationException)));

			//					global::FluidTrade.Core.EventLog.Error("{0}, {1}", communicationException.Message, communicationException.StackTrace);
			communicationCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(EventLog)),
					"Error",
					new CodePrimitiveExpression("{0}, {1}"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("communicationException"), "Message"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("communicationException"), "StackTrace")));

			//				}
			tryReading.CatchClauses.Add(communicationCatch);

			//				catch (global::System.Exception exception)
			//				{
			CodeCatchClause generalCatch = new CodeCatchClause("exception", new CodeGlobalTypeReference(typeof(System.Exception)));

			//					global::FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			generalCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(EventLog)),
					"Error",
					new CodePrimitiveExpression("{0}, {1}"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception"), "Message"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception"), "StackTrace")));

			//				}
			tryReading.CatchClauses.Add(generalCatch);
			whileReconciling.Statements.Add(tryReading);

			//				finally
			//				{
			//					try
			//					{
			CodeTryCatchFinallyStatement tryClose = new CodeTryCatchFinallyStatement();

			//						if ((dataModelClient != null) && (dataModelClient.State == System.ServiceModel.CommunicationState.Opened))
			//							dataModelClient.Close();
			CodeConditionStatement ifChannelOpen = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("dataModelClient"),
						CodeBinaryOperatorType.IdentityInequality,
						new CodePrimitiveExpression(null)),
					CodeBinaryOperatorType.BooleanAnd,
					new CodeBinaryOperatorExpression(
						new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("dataModelClient"), "State"),
						CodeBinaryOperatorType.IdentityEquality,
						new CodeFieldReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.ServiceModel.CommunicationState)), "Opened"))));
			ifChannelOpen.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("dataModelClient"), "Close"));
			tryClose.TryStatements.Add(ifChannelOpen);

			//					}
			//					catch (Exception exception)
			//					{
			CodeCatchClause tryCloseCatch = new CodeCatchClause("exception", new CodeGlobalTypeReference(typeof(Exception)));

			//						global::FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			tryCloseCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(EventLog)),
					"Error",
					new CodePrimitiveExpression("{0}, {1}"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception"), "Message"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception"), "StackTrace")));

			//					}
			tryClose.CatchClauses.Add(tryCloseCatch);

			//				}
			tryReading.FinallyStatements.Add(tryClose);

			//				global::System.Threading.Thread.Sleep(DataModel.refreshInterval);
			whileReconciling.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Thread)),
					"Sleep",
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "refreshInterval")));

			//			}
			this.Statements.Add(whileReconciling);

			//		}

		}

	}

}

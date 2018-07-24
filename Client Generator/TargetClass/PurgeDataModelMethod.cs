namespace FluidTrade.ClientGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using System.Collections;
	using System.Collections.Generic;
	using System.Data;
    using System.Threading;
    using FluidTrade.Core;
	using FluidTrade.Core.Properties;

	/// <summary>
	/// Creates a method to handle purging the data model of records that have been deleted.
	/// </summary>
	class PurgeDataModelMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle purging the data model of records that have been deleted.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public PurgeDataModelMethod(DataModelSchema dataModelSchema)
		{

			// These variable are used to create a connection to the server.
			string clientTypeName = string.Format("{0}Client", dataModelSchema.Name);
			string endpointName = string.Format("{0}Endpoint", dataModelSchema.Name);
			string clientVariableName = CommonConversion.ToCamelCase(clientTypeName);

			//		/// <summary>
			//		/// This method will purge the data model of deleted records.
			//		/// </summary>
			//		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//		private static void PurgeDataModel()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("This method will purge the data model of deleted records.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Name = "PurgeDataModel";

			//			int recordCount = 0;
			this.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "recordCount", new CodePrimitiveExpression(0)));

			//			System.Collections.Generic.List<object[]> keys = new System.Collections.Generic.List<object[]>();
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(List<Object[]>)),
					"keys",
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(List<Object[]>)))));

			//			DataModelClient dataModelClient = null;
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference(String.Format("{0}Client", dataModelSchema.Name)),
					"dataModelClient",
					new CodePrimitiveExpression(null)));

			//			global::System.Threading.Monitor.Enter(DataModel.syncRoot);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Monitor)),
					"Enter",
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));
	
			//		Start:
			this.Statements.Add(new CodeLabeledStatement("Start"));

			//			try
			//			{
			CodeTryCatchFinallyStatement tryPurging = new CodeTryCatchFinallyStatement();

			//				dataModelClient = new DataModelClient(FluidTrade.Guardian.Properties.Settings.Default.DataModelEndpoint);
			tryPurging.TryStatements.Add(
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

			//				for (int sourceTableIndex = 0; sourceTableIndex < DataModel.Tables.Count; sourceTableIndex = (sourceTableIndex + 1))
			//				{
			CodeIterationStatement forSourceTable = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "sourceTableIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("sourceTableIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"),
						"Count")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("sourceTableIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("sourceTableIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//					System.Data.DataTable sourceTable = DataModel.Tables[sourceTableIndex];
			forSourceTable.Statements.Add(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(DataTable)), "sourceTable",
					new CodeArrayIndexerExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"),
						new CodeVariableReferenceExpression("sourceTableIndex"))));

			//					for (int sourceRowIndex = 0; sourceRowIndex < sourceTable.Rows.Count; sourceRowIndex = (sourceRowIndex + 1))
			//					{
			CodeIterationStatement forSourceRow = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "sourceRowIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("sourceRowIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sourceTable"), "Rows"),
						"Count")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("sourceRowIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("sourceRowIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//						global::System.Data.DataRow sourceRow = sourceTable.Rows[sourceRowIndex];
			forSourceRow.Statements.Add(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(DataRow)),
					"sourceRow",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sourceTable"), "Rows"),
						new CodeVariableReferenceExpression("sourceRowIndex"))));

			//						object[] sourceKeyItem = new object[2];
			forSourceRow.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"sourceKeyItem",
					new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(Object[])), new CodePrimitiveExpression(2))));

			//						object[] sourceKey = new object[sourceTable.PrimaryKey.Length];
			forSourceRow.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"sourceKey",
					new CodeArrayCreateExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sourceTable"), "PrimaryKey"),
							"Length"))));

			//						sourceKeyItem[0] = sourceTableIndex;
			forSourceRow.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("sourceKeyItem"), new CodePrimitiveExpression(0)),
					new CodeVariableReferenceExpression("sourceTableIndex")));

			//						sourceKeyItem[1] = sourceKey;
			forSourceRow.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("sourceKeyItem"), new CodePrimitiveExpression(1)),
					new CodeVariableReferenceExpression("sourceKey")));

			//						for (int keyIndex = 0; (keyIndex < sourceTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
			//						{
			CodeIterationStatement forKey = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "keyIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("keyIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sourceTable"), "PrimaryKey"),
						"Length")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("keyIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("keyIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//							sourceKey[keyIndex] = sourceRow[sourceTable.PrimaryKey[keyIndex]];
			forKey.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("sourceKey"), new CodeVariableReferenceExpression("keyIndex")),
					new CodeIndexerExpression(new CodeVariableReferenceExpression("sourceRow"),
						new CodeIndexerExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sourceTable"), "PrimaryKey"),
							new CodeVariableReferenceExpression("keyIndex")))));

			//						}
			forSourceRow.Statements.Add(forKey);

			//						keys.Add(sourceKeyItem);
			forSourceRow.Statements.Add(
				new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("keys"), "Add", new CodeVariableReferenceExpression("sourceKeyItem")));

			//						recordCount = (recordCount + 1);
			forSourceRow.Statements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("recordCount"),
					new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("recordCount"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));

			//						if ((recordCount > DataModel.purgeBufferSize))
			//						{
			CodeConditionStatement ifPageTooBig = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("recordCount"),
					CodeBinaryOperatorType.GreaterThan,
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "purgeBufferSize")));

			//							object[] deleteArray = dataModelClient.Reconcile(keys.ToArray());
			ifPageTooBig.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"deleteArray",
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("dataModelClient"),
						"Reconcile",
						new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("keys"), "ToArray"))));

			//							for (int recordIndex = 0; recordIndex < deleteArray.Length; recordIndex = (recordIndex + 1))
			//							{
			CodeIterationStatement forRecords0 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "recordIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("recordIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("deleteArray"), "Length")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("recordIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("recordIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//								object[] deletedItem = (object[])deleteArray[recordIndex];
			forRecords0.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"deletedItem",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("deleteArray"), new CodeVariableReferenceExpression("recordIndex")))));

			//								System.Data.DataTable targetTable = DataModel.Tables[(int)deletedItem[0]];
			forRecords0.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataTable)),
					"targetTable",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"),
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(new CodeVariableReferenceExpression("deletedItem"), new CodePrimitiveExpression(0))))));

			//								System.Data.DataRow targetRow = targetTable.Rows.Find((object[])deletedItem[1]);
			forRecords0.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataRow)),
					"targetRow",
					new CodeMethodInvokeExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("targetTable"), "Rows"),
						"Find",
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Object[])),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("deletedItem"),
								new CodePrimitiveExpression(1))))));

			//								if ((targetRow != null))
			//								{
			CodeConditionStatement ifTargetRow0 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("targetRow"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//									targetRow.Delete();
			ifTargetRow0.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("targetRow"), "Delete"));

			//									targetRow.AcceptChanges();
			ifTargetRow0.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("targetRow"), "AcceptChanges"));

			//								}
			forRecords0.Statements.Add(ifTargetRow0);

			//							}
			ifPageTooBig.TrueStatements.Add(forRecords0);

			//							recordCount = 0;
			ifPageTooBig.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("recordCount"), new CodePrimitiveExpression(0)));

			//							keys = new System.Collections.Generic.List<object[]>();
			ifPageTooBig.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("keys"),
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(List<Object[]>)))));

			//						}
			forSourceRow.Statements.Add(ifPageTooBig);

			//					}
			forSourceTable.Statements.Add(forSourceRow);

			//				}
			tryPurging.TryStatements.Add(forSourceTable);

			//				if ((keys.Count != 0))
			//				{
			CodeConditionStatement ifKeys = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("keys"), "Count"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(0)));

			//						object[] deleteArray = dataModelClient.Reconcile(keys.ToArray());
			ifKeys.TrueStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"deleteArray",
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("dataModelClient"),
						"Reconcile",
						new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("keys"), "ToArray"))));

			//						for (int recordIndex = 0; recordIndex < deleteArray.Length; recordIndex = (recordIndex + 1))
			//						{
			CodeIterationStatement forRecords1 = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(Int32)), "recordIndex", new CodePrimitiveExpression(0)),
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("recordIndex"),
					CodeBinaryOperatorType.LessThan,
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("deleteArray"), "Length")),
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("recordIndex"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("recordIndex"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//							object[] deletedItem = (object[])deleteArray[recordIndex];
			forRecords1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"deletedItem",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("deleteArray"), new CodeVariableReferenceExpression("recordIndex")))));

			//							System.Data.DataTable targetTable = DataModel.Tables[(int)deletedItem[0]];
			forRecords1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataTable)),
					"targetTable",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"),
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(new CodeVariableReferenceExpression("deletedItem"), new CodePrimitiveExpression(0))))));

			//							System.Data.DataRow targetRow = targetTable.Rows.Find((object[])deletedItem[1]);
			forRecords1.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataRow)),
					"targetRow",
					new CodeMethodInvokeExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("targetTable"), "Rows"),
						"Find",
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Object[])),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("deletedItem"),
								new CodePrimitiveExpression(1))))));

			//							if ((targetRow != null))
			//							{
			CodeConditionStatement ifTargetRow1 = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("targetRow"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//								targetRow.Delete();
			ifTargetRow1.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("targetRow"), "Delete"));

			//								targetRow.AcceptChanges();
			ifTargetRow1.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("targetRow"), "AcceptChanges"));

			//							}
			forRecords1.Statements.Add(ifTargetRow1);

			//						}
			ifKeys.TrueStatements.Add(forRecords0);

			//				}
			tryPurging.TryStatements.Add(ifKeys);

			//			catch (global::System.Exception exception)
			//			{
			CodeCatchClause tryCatch = new CodeCatchClause("exception", new CodeGlobalTypeReference(typeof(Exception)));

			//				global::FluidTrade.Core.EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			tryCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(EventLog)),
					"Error",
					new CodePrimitiveExpression("{0}, {1}"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception"), "Message"),
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("exception"), "StackTrace")));

			//					global::System.Threading.Thread.Sleep(1000);
			tryCatch.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(System.Threading.Thread)),
					"Sleep",
					new CodePrimitiveExpression(1000)));

			//				goto Start;
			tryCatch.Statements.Add(new CodeGotoStatement("Start"));

			//			}
			tryPurging.CatchClauses.Add(tryCatch);

			//			finally
			//			{
			//				if ((dataModelClient != null))
			//				{
			CodeConditionStatement ifDataModelClient = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("dataModelClient"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//					dataModelClient.Close();
			ifDataModelClient.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("dataModelClient"), "Close"));

			//				}
			tryPurging.FinallyStatements.Add(ifDataModelClient);

			//			}
			this.Statements.Add(tryPurging);

			//			global::System.Threading.Monitor.Exit(DataModel.syncRoot);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Monitor)),
					"Exit",
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "syncRoot")));

			//		}

		}

	}

}

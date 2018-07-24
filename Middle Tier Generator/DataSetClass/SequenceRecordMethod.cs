namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Data;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method for recursively sequencing the records in a relational data model.
	/// </summary>
	class SequenceRecordMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method for recursively sequencing the records in a relational data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public SequenceRecordMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Recursively orders the records in a relational data model.
			//		/// </summary>
			//		private static void Initialize(global::FluidTrade.Core.IRow iRow, global::System.Collections.Generic.List<global::System.Object> transactionLogItem)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Recursively orders the records in a relational data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"iRow\">The row to be recursively sequenced.</param>", true));
			this.Comments.Add(
				new CodeCommentStatement(
					"<param name=\"transactionLogItem\">A common buffer in which to create the raw transaction log items.</param>",
					true));
			this.Attributes = MemberAttributes.Private;
			this.Name = "SequenceRecord";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)), "iRow"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(List<Object>)), "transactionLogItem"));

			//			ITable iTable = (ITable)iRow.Table;
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(FluidTrade.Core.ITable)),
					"iTable",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(FluidTrade.Core.ITable)),
						new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iRow"), "Table"))));

			//			if (iRow.RowState == global::System.Data.DataRowState.Added)
			//			{
			CodeConditionStatement ifAdded = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iRow"), "RowState"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowState)), "Added")));

			//				for (int relationIndex = 0; (relationIndex < iRow.Table.ParentRelations.Count); (relationIndex = relationIndex + 1))
			//				{
			CodeIterationStatement relationLoop = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "relationIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("relationIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iRow"), "Table"),
								"ParentRelations"),
							"Count")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("relationIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("relationIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//					global::System.Data.DataRow parentRow = iRow.GetParentRow(iRow.Table.ParentRelations[relationIndex]);
			relationLoop.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataRow)),
					"parentRow",
					new CodeMethodInvokeExpression(
						new CodeArgumentReferenceExpression("iRow"),
						"GetParentRow",
						new CodeIndexerExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iRow"), "Table"),
							"ParentRelations"),
						new CodeVariableReferenceExpression("relationIndex")))));

			//					if (parentRow != null)
			//					{
			CodeConditionStatement ifParent = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("parentRow"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null)));

			//						SequenceRecord(((IRow)(parentRow)), transactionLogItem);
			ifParent.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeThisReferenceExpression(),
					"SequenceRecord",
					new CodeCastExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)), new CodeVariableReferenceExpression("parentRow")),
					new CodeVariableReferenceExpression("transactionLogItem")));

			//					}
			relationLoop.Statements.Add(ifParent);

			//				}
			ifAdded.TrueStatements.Add(relationLoop);

			//				transactionLogItem.Clear();
			ifAdded.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("transactionLogItem"), "Clear"));

			//				transactionLogItem.Add(FluidTrade.Core.RecordState.Added);
			ifAdded.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(RecordState)), "Added")));

			//				transactionLogItem.Add(iTable.Ordinal);
			ifAdded.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "Ordinal")));

			//				for (int keyIndex = 0; (keyIndex < iTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
			//				{
			CodeIterationStatement keyLoop = new CodeIterationStatement(
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

			//					transactionLogItem.Add(iRow[iTable.PrimaryKey[index].Ordinal]);
			keyLoop.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeIndexerExpression(
						new CodeVariableReferenceExpression("iRow"),
						new CodePropertyReferenceExpression(
							new CodeIndexerExpression(
								new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "PrimaryKey"),
								new CodeVariableReferenceExpression("keyIndex")),
							"Ordinal"))));

			//				}
			ifAdded.TrueStatements.Add(keyLoop);

			//				for (int index = 0; index < iTable.Columns.Count; index++)
			//				{
			CodeIterationStatement columnLoop = new CodeIterationStatement(
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

			//					if (iRow[index].Equals(iTable.Columns[index].DefaultValue) == false)
			//					{
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

			//						transactionLogItem.Add(index);
			ifDefault.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeVariableReferenceExpression("columnIndex")));

			//						transactionLogItem.Add(iRow[index]);
			ifDefault.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionLogItem"),
					"Add",
					new CodeIndexerExpression(
						new CodeArgumentReferenceExpression("iRow"),
						new CodeVariableReferenceExpression("columnIndex"))));

			//					}
			columnLoop.Statements.Add(ifDefault);

			//				}
			ifAdded.TrueStatements.Add(columnLoop);

			//				DataModel.AddTransaction(iRow, transactionLogItem.ToArray());
			ifAdded.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(dataModelSchema.Name),
					"AddTransaction",
					new CodeVariableReferenceExpression("iRow"),
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("transactionLogItem"),
						"ToArray")));

			//				iRow.AcceptChanges();
			ifAdded.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("iRow"),"AcceptChanges"));

			//			}
			this.Statements.Add(ifAdded);

			//		}

		}

	}

}

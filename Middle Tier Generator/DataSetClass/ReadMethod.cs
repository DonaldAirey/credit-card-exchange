namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Data;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class ReadMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public ReadMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Collects the set of modified records that will reconcile the client data model to the master data model.
			//		/// </summary>
			//		/// <param name="identifier">A unique identifier of an instance of the data.</param>
			//		/// <param name="sequence">The sequence of the client data model.</param>
			//		/// <returns>An array of records that will reconcile the client data model to the server.</returns>
			//		[global::FluidTrade.Core.ClaimsPrincipalPermission(global::System.Security.Permissions.SecurityAction.Demand, ClaimType=global::FluidTrade.Core.ClaimTypes.Read, Resource=global::FluidTrade.Core.Resources.Application)]
			//		public object[] Read(global::System.Guid identifier, long sequence)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Collects the set of modified records that will reconcile the client data model to the master data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"identifier\">A unique identifier of an instance of the data.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"sequence\">The sequence of the client data model.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>An array of records that will reconcile the client data model to the server.</returns>", true));
			this.Name = "Read";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object[]));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Guid)), "identifier"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Int64)), "sequence"));

			//            global::FluidTrade.Core.MiddleTierContext middleTierTransaction = global::FluidTrade.Core.MiddleTierContext.Current;
			CodeVariableReferenceExpression transactionExpression = new CodeVariableReferenceExpression(
				String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name)));
			this.Statements.Add(new CodeCreateMiddleTierContextStatement(dataModelSchema, transactionExpression));
			
			//			object filterContext = DataModel.getFilterContextHandler();
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object)),
					"filterContext",
					new CodeMethodInvokeExpression(
						new CodeThisReferenceExpression(),
						"getFilterContextHandler",
						transactionExpression)));

			//			long transactionCount = 0L;
			this.Statements.Add(
				new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(long)), "transactionCount", new CodePrimitiveExpression(0L)));

			//			long currentSequence = sequence;
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(long)),
					"currentSequence",
					new CodeArgumentReferenceExpression("sequence")));

			//			try
			//			{
			CodeTryCatchFinallyStatement tryTransactionLogLock = new CodeTryCatchFinallyStatement();

			//				DataModel.transactionLogLock.EnterReadLock();
			tryTransactionLogLock.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"EnterReadLock"));

			//				object[] dataHeader = new object[4];
			tryTransactionLogLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"dataHeader", new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(Object[])), 4)));

			//				dataHeader[0] = DataModel.identifier;
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(0)),
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "identifier")));

			//				if ((identifier != DataModel.identifier))
			//				{
			CodeConditionStatement ifNewDataModel = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeArgumentReferenceExpression("identifier"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "identifier")));

			//					sequence = long.MinValue;
			ifNewDataModel.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeArgumentReferenceExpression("sequence"),
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(Int64)), "MinValue")));
			tryTransactionLogLock.TryStatements.Add(ifNewDataModel);

			//				}

			//				global::System.Collections.ArrayList data = new global::System.Collections.ArrayList();
			tryTransactionLogLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(ArrayList)), "data", new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(ArrayList)))));

			//				global::System.Collections.Generic.LinkedListNode<TransactionLogItem> transactionNode = this.transactionLog.Last;
			tryTransactionLogLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("global::System.Collections.Generic.LinkedListNode<TransactionLogItem>"),
					"transactionNode",
					new CodePropertyReferenceExpression(
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLog"),
						"Last")));

			//			findLoop:
			tryTransactionLogLock.TryStatements.Add(new CodeLabeledStatement("findLoop"));

			//				if ((transactionNode == transactionLog.First))
			//				{
			CodeConditionStatement ifFoundFist = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("transactionNode"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("transactionLog"), "First")));

			//					goto logLoop;
			ifFoundFist.TrueStatements.Add(new CodeGotoStatement("logLoop"));

			//				}
			tryTransactionLogLock.TryStatements.Add(ifFoundFist);

			//				if ((transactionNode.Value.sequence <= sequence))
			//				{
			CodeConditionStatement ifTransactionFound = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeFieldReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionNode"), "Value"),
						"sequence"),
						CodeBinaryOperatorType.LessThanOrEqual,
						new CodeVariableReferenceExpression("sequence")));

			//					if ((transactionNode == this.transactionLog.Last))
			//					{
			CodeConditionStatement ifTransactionLast = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("transactionNode"),
					CodeBinaryOperatorType.IdentityEquality,
					new CodeFieldReferenceExpression(
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLog"),
						"Last")));

			//						transactionNode = null;
			ifTransactionLast.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("transactionNode"), new CodePrimitiveExpression(null)));

			//					}
			//					else
			//					{
			//						transactionNode = transactionNode.Next;
			ifTransactionLast.FalseStatements.Add(
				new CodeAssignStatement(new CodeVariableReferenceExpression("transactionNode"),
					new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("transactionNode"), "Next")));

			//					}
			ifTransactionFound.TrueStatements.Add(ifTransactionLast);

			//					goto logLoop;
			ifTransactionFound.TrueStatements.Add(new CodeGotoStatement("logLoop"));

			//				}
			tryTransactionLogLock.TryStatements.Add(ifTransactionFound);

			//				transactionNode = transactionNode.Previous;
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("transactionNode"),
					new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("transactionNode"), "Previous")));

			//				goto findLoop;
			tryTransactionLogLock.TryStatements.Add(new CodeGotoStatement("findLoop"));

			//			logLoop:
			tryTransactionLogLock.TryStatements.Add(new CodeLabeledStatement("logLoop"));

			//				if ((transactionNode == null) || (transactionCount == this.readBufferSize))
			//				{
			CodeConditionStatement ifFull = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("transactionNode"),
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression(null)),
					CodeBinaryOperatorType.BooleanOr,
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("transactionCount"),
						CodeBinaryOperatorType.IdentityEquality,
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "readBufferSize"))));

			//					goto endLoop;
			ifFull.TrueStatements.Add(new CodeGotoStatement("endLoop"));

			//				}
			tryTransactionLogLock.TryStatements.Add(ifFull);

			//				object[] transactionLogItem = ((object[])(transactionNode.Value.data));
			tryTransactionLogLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object[])),
					"transactionLogItem",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Object[])),
						new CodeFieldReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionNode"), "Value"),
							"data"))));

			//						global::System.Data.DataTable dataTable = DataModel.dataSet.Tables[((int)(transactionLogItem[1]))];
			tryTransactionLogLock.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataTable)),
					"dataTable",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Tables"),
						new CodeCastExpression(
							new CodeGlobalTypeReference(typeof(Int32)),
							new CodeIndexerExpression(
								new CodeVariableReferenceExpression("transactionLogItem"),
								new CodePrimitiveExpression(1))))));

			//				if ((((ITable)(dataTable)).FilterRowHandler(dataModelTransaction, filterContext, transactionNode.Value.containerContext)))
			//						{
			CodeConditionStatement ifFiltered =
				new CodeConditionStatement(
					new CodeMethodInvokeExpression(
						new CodeCastExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.ITable)), new CodeVariableReferenceExpression("dataTable")),
						"FilterRowHandler",
						new CodeVariableReferenceExpression(String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						new CodeVariableReferenceExpression("filterContext"),
						new CodeFieldReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transactionNode"), "Value"),
							"containerContext")));

			//							data.Add(transactionLogItem);
			ifFiltered.TrueStatements.Add(
				new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("data"), "Add", new CodeVariableReferenceExpression("transactionLogItem")));

			//						}
			tryTransactionLogLock.TryStatements.Add(ifFiltered);

			//				currentSequence = transactionNode.Value.sequence;
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("currentSequence"),
					new CodeFieldReferenceExpression(
						new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("transactionNode"), "Value"), "sequence")));

			//				transactionNode = transactionNode.Next;
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("transactionNode"),
					new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("transactionNode"), "Next")));

			//				transactionCount = (transactionCount + 1);
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("transactionCount"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("transactionCount"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//				goto logLoop;
			tryTransactionLogLock.TryStatements.Add(new CodeGotoStatement("logLoop"));

			//			endLoop:
			tryTransactionLogLock.TryStatements.Add(new CodeLabeledStatement("endLoop"));

			//				dataHeader[1] = currentSequence;
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(1)),
					new CodeVariableReferenceExpression("currentSequence")));

			//				dataHeader[2] = this.transactionLog.Last.Value.sequence;
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(2)),
					new CodeFieldReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLog"),
								"Last"),
							"Value"),
						"sequence")));

			//				dataHeader[3] = data.ToArray();
			tryTransactionLogLock.TryStatements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(new CodeVariableReferenceExpression("dataHeader"), new CodePrimitiveExpression(3)),
					new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("data"), "ToArray")));

			//				return dataHeader;
			tryTransactionLogLock.TryStatements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("dataHeader")));

			//			finally
			//			{
			//				DataModel.transactionLogLock.ExitReadLock();
			tryTransactionLogLock.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock"),
					"ExitReadLock"));

			//			}
			this.Statements.Add(tryTransactionLogLock);

			//		}

		}

	}

}

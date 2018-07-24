namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using System.Threading;
    using System.Transactions;
    using FluidTrade.Core;

	/// <summary>
	/// Generates a property that gets the lock for the data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class CurrentProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets the lock for the data model.
		/// </summary>
		public CurrentProperty(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Gets the current DataModelTransaction.
			//		/// </summary>
			//		public static DataModelTransaction Current
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets the lock for the data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Type = new CodeTypeReference(String.Format("{0}Transaction", dataModelSchema.Name));
			this.Name = "Current";

			//			get
			//			{
			//				try
			//				{
			CodeTryCatchFinallyStatement tryLockStatement = new CodeTryCatchFinallyStatement();

			//					global::System.Threading.Monitor.Enter(DataModelTransaction.syncRoot);
			tryLockStatement.TryStatements.Add(
					new CodeMethodInvokeExpression(
						new CodeGlobalTypeReferenceExpression(typeof(Monitor)),
						"Enter",
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
							"syncRoot")));

			//					global::System.Transactions.Transaction transaction = global::System.Transactions.Transaction.Current;
			tryLockStatement.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Transaction)),
					"transaction",
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Transaction)), "Current")));

			//					string localIdentifier = transaction.TransactionInformation.LocalIdentifier;
			tryLockStatement.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(String)),
					"localIdentifier",
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transaction"), "TransactionInformation"),
						"LocalIdentifier")));

			//					DataModelTransaction dataModelTransaction;
			tryLockStatement.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference(String.Format("{0}Transaction", dataModelSchema.Name)),
					String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))));

			//					if (DataModelTransaction.transactionTable.TryGetValue(localIdentifier, out dataModelTransaction) == false)
			//					{
			CodeConditionStatement ifTransactionFound = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
							"transactionTable"),
							"TryGetValue",
							new CodeVariableReferenceExpression("localIdentifier"),
							new CodeDirectionExpression(
								FieldDirection.Out,
								new CodeVariableReferenceExpression(String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(false)));

			//						dataModelTransaction = new DataModelTransaction();
			ifTransactionFound.TrueStatements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression(String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))),
					new CodeObjectCreateExpression(
						new CodeTypeReference(String.Format("{0}Transaction", dataModelSchema.Name)),
						new CodeVariableReferenceExpression("transaction"))));

			//						transactionTable.Add(localIdentifier, dataModelTransaction);
			ifTransactionFound.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeVariableReferenceExpression("transactionTable"),
					"Add",
					new CodeVariableReferenceExpression("localIdentifier"),
					new CodeVariableReferenceExpression(String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name)))));

			//						transaction.TransactionCompleted += new global::System.Transactions.TransactionCompletedEventHandler(OnTransactionCompleted);
			ifTransactionFound.TrueStatements.Add(
				new CodeAttachEventStatement(
					new CodeEventReferenceExpression(new CodeVariableReferenceExpression("transaction"), "TransactionCompleted"),
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(TransactionCompletedEventHandler)),
						new CodeMethodReferenceExpression(
							new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
							"OnTransactionCompleted"))));

			//					}
			tryLockStatement.TryStatements.Add(ifTransactionFound);

			//					return dataModelTransaction;
			tryLockStatement.TryStatements.Add(
				new CodeMethodReturnStatement(
					new CodeVariableReferenceExpression(String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name)))));

			//				}

			//				finally
			//				{
			//					global::System.Threading.Monitor.Exit(DataModelTransaction.syncRoot);
			tryLockStatement.FinallyStatements.Add(
					new CodeMethodInvokeExpression(
						new CodeGlobalTypeReferenceExpression(typeof(Monitor)),
						"Exit",
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
							"syncRoot")));

			//				}
			//			}
			//		}
			this.GetStatements.Add(tryLockStatement);

			//		}

		}

	}

}

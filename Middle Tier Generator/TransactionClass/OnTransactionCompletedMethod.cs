namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using System.Threading;
    using System.Transactions;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class OnTransactionCompletedMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public OnTransactionCompletedMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Processes the completion of a transaction.
			//		/// </summary>
			//		/// <param name="sender">The object that originated the event.</param>
			//		/// <param name="transactionEventArgs">The event arguments.</param>
			//		private static void OnTransactionCompleted(object sender, global::System.Transactions.TransactionEventArgs transactionEventArgs)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Processes the completion of a transaction.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"sender\">The object that originated the event.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"transactionEventArgs\">The event arguments.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Name = "OnTransactionCompleted";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object)), "sender"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(TransactionEventArgs)), "transactionEventArgs"));

			//			lock (DataModelTransaction.syncRoot)
			//			{
			CodeTryCatchFinallyStatement tryLockRoot = new CodeTryCatchFinallyStatement();

			//				global::System.Threading.Monitor.Enter(DataModelTransaction.syncRoot);
			tryLockRoot.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Monitor)),
					"Enter",
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)), "syncRoot")));

			//				global::System.Transactions.Transaction transaction = e.Transaction;
			tryLockRoot.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Transaction)),
					"transaction",
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("transactionEventArgs"), "Transaction")));

			//				string localIdentifier = transaction.TransactionInformation.LocalIdentifier;
			tryLockRoot.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(String)),
					"localIdentifier",
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("transaction"), "TransactionInformation"),
						"LocalIdentifier")));

			//				DataModelTransaction dataModelTransaction;
			tryLockRoot.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference(String.Format("{0}Transaction", dataModelSchema.Name)),
					String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))));

			//				if (DataModelTransaction.transactionTable.TryGetValue(localIdentifier, out dataModelTransaction))
			//				{
			CodeConditionStatement ifNewTransaction = new CodeConditionStatement(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)), "transactionTable"),
					"TryGetValue",
					new CodeVariableReferenceExpression("localIdentifier"),
					new CodeDirectionExpression(FieldDirection.Out, new CodeVariableReferenceExpression("dataModelTransaction"))));

			//					dataModelTransaction.SqlConnection.Close();
			ifNewTransaction.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(
						new CodeVariableReferenceExpression(String.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						"SqlConnection"),
					"Close"));

			//					DataModelTransaction.transactionTable.Remove(localIdentifier);
			ifNewTransaction.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(
						new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)), "transactionTable"),
					"Remove",
					new CodeVariableReferenceExpression("localIdentifier")));

			//				}
			//			}
			tryLockRoot.TryStatements.Add(ifNewTransaction);

			//			}
			//			finally
			//			{
			//				global::System.Threading.Monitor.Exit(DataModelTransaction.syncRoot);
			//			}
			tryLockRoot.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeGlobalTypeReferenceExpression(typeof(Monitor)),
					"Exit",
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)), "syncRoot")));

			//		}
			this.Statements.Add(tryLockRoot);

		}

	}
}

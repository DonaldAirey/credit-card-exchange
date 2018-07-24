namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using System.Data.SqlClient;
    using System.Transactions;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a static constructor for the transaction.
	/// </summary>
	class VoidConstructor : CodeConstructor
	{

		/// <summary>
		/// Creates a static constructor for the transaction.
		/// </summary>
		/// <param name="dataModelSchema">A description of the data model.</param>
		public VoidConstructor(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Creates a middle tier context for a transaction.
			//		/// </summary>
			//		/// <param name="transaction">The transaction to which this context is bound.</param>
			//		private DataModelTransaction()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates a {0}Transaction.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"transaction\">The transaction to which this context is bound.</param>", true));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Transaction)), "transaction"));

			//			this.lockList = new global::System.Collections.Generic.List<global::FluidTrade.Core.IRow>();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "lockList"),
					new CodeObjectCreateExpression(new CodeTypeReference("global::System.Collections.Generic.List<global::FluidTrade.Core.IRow>"))));

			//			this.recordList = new global::System.Collections.Generic.List<global::FluidTrade.Core.IRow>();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "recordList"),
					new CodeObjectCreateExpression(new CodeTypeReference("global::System.Collections.Generic.List<global::FluidTrade.Core.IRow>"))));

			//			this.transactionId = global::System.Guid.NewGuid();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionId"),
					new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(Guid)), "NewGuid")));

			//			this.sqlConnection = new global::System.Data.SqlClient.SqlConnection(DataModelTransaction.connectionString);
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sqlConnection"),
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(SqlConnection)),
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
							"connectionString"))));

			//			this.sqlConnection.Open();
			this.Statements.Add(
				new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sqlConnection"), "Open"));

			//			this.sqlConnection.EnlistTransaction(transaction);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sqlConnection"),
					"EnlistTransaction",
					new CodeArgumentReferenceExpression("transaction")));

			//			transaction.EnlistVolatile(this, global::System.Transactions.EnlistmentOptions.None);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeArgumentReferenceExpression("transaction"),
					"EnlistVolatile",
					new CodeThisReferenceExpression(),
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(EnlistmentOptions)), "None")));

			//		}

		}

	}

}

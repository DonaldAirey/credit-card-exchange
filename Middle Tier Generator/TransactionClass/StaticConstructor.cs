namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using System.Configuration;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a static constructor for the transaction.
	/// </summary>
	class StaticConstructor : CodeTypeConstructor
	{

		/// <summary>
		/// Creates a static constructor for the transaction.
		/// </summary>
		/// <param name="dataModelSchema">A description of the data model.</param>
		public StaticConstructor(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Creates the static resources required by the DataModelTransaction.
			//		/// </summary>
			//		static DataModelTransaction()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates the static resources required by the {0}Transaction.", dataModelSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));

			//			DataModelTransaction.connectionString = global::System.Configuration.ConfigurationManager.ConnectionStrings["DataModel"].ConnectionString;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
						"connectionString"),
					new CodePropertyReferenceExpression(
						new CodeIndexerExpression(
							new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ConfigurationManager)), "ConnectionStrings"),
							new CodePrimitiveExpression(dataModelSchema.Name)),
					"ConnectionString")));

			//			DataModelTransaction.transactionTable = new global::System.Collections.Generic.Dictionary<string, DataModelTransaction>();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(
						new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
						"transactionTable"),
					new CodeObjectCreateExpression(new CodeTypeReference(
						string.Format("global::System.Collections.Generic.Dictionary<string, {0}Transaction>", dataModelSchema.Name)))));

			//			DataModelTransaction.syncRoot = new object();
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(
						new CodeTypeReferenceExpression(String.Format("{0}Transaction", dataModelSchema.Name)),
						"syncRoot"),
					new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(Object)))));

			//		}

		}

	}

}

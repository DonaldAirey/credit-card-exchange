namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class AddTransactionMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public AddTransactionMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Adds a transaction item to the log.
			//		/// </summary>
			//		/// <param name="iRow">The record to be added to the transaction log.</param>
			//		/// <param name="data">An array of updated fields.</param>
			//		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//		[global::System.ComponentModel.BrowsableAttribute(false)]
			//		internal void AddTransaction(IRow iRow, object[] data)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Adds a transaction item to the log.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"iRow\">The record to be added to the transaction log.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"data\">An array of updated fields.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Name = "AddTransaction";
			this.Attributes = MemberAttributes.Assembly | MemberAttributes.Final;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)), "iRow"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object[])), "data"));

			//			ITable iTable = ((ITable)(iRow.Table));
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(FluidTrade.Core.ITable)),
					"iTable",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(FluidTrade.Core.ITable)),
						new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iRow"), "Table"))));

			//			object containerContext = iTable.GetContainerHandler(iRow);
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Object)),
					"containerContext",
					new CodeMethodInvokeExpression(
						new CodeVariableReferenceExpression("iTable"),
						"GetContainerHandler",
						new CodeArgumentReferenceExpression("iRow"))));

			//			this.transactionLog.AddLast(new TransactionLogItem(containerContext, data, iTable.PrimaryKey.Length, this.sequence, global::System.DateTime.Now));
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLog"),
					"AddLast",
					new CodeObjectCreateExpression(
						new CodeTypeReference("TransactionLogItem"),
						new CodeVariableReferenceExpression("containerContext"),
						new CodeArgumentReferenceExpression("data"),
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("iTable"), "PrimaryKey"),
							"Length"),
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sequence"),
						new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DateTime)), "Now"))));

			//			this.sequence = (this.sequence + 1);
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sequence"),
					new CodeBinaryOperatorExpression(
						new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sequence"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression(1))));

			//		}

		}

	}
}

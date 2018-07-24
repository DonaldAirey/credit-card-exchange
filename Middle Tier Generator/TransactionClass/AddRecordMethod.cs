namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;
    using System.Data;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to add a record to the transaction.
	/// </summary>
	class AddRecordMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to add a lock to the transaction.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public AddRecordMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Adds a row to the transaction.
			//		/// </summary>
			//		/// <param name="row">This row will be accepted or rolled back with the other rows in the transaction.</param>
			//		public void AddRecord(IRow iRow)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Adds a row lock to the list of locks that must be released at the end of a transaction.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"iRow\">The record to be added to the transaction.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "AddRecord";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)), "iRow"));

			//			if ((iRow.RowState != System.Data.DataRowState.Unchanged))
			//			{
			CodeConditionStatement ifChanged = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("iRow"), "RowState"),
					CodeBinaryOperatorType.IdentityInequality,
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DataRowState)), "Unchanged")));

			//				this.recordList.Remove(iRow);
			ifChanged.TrueStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "recordList"),
					"Remove",
					new CodeArgumentReferenceExpression("iRow")));

			//			}
			this.Statements.Add(ifChanged);
			
			//			this.recordList.Add(iRow);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "recordList"),
					"Add",
					new CodeArgumentReferenceExpression("iRow")));

			//		}

		}

	}
}

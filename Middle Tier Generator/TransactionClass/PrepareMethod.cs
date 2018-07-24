namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;
    using System.Transactions;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class PrepareMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public PrepareMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Indicates that the transaction can be committed.
			//		/// </summary>
			//		/// <param name="preparingEnlistment">Facilitates communication bewtween an enlisted transaction participant and the
			//		/// transaction manager during the final phase of the transaction.</param>
			//		public void Prepare(global::System.Transactions.PreparingEnlistment preparingEnlistment)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Indicates that the transaction can be committed.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"preparingEnlistment\">Facilitates communication bewtween an enlisted transaction participant and the", true));
			this.Comments.Add(new CodeCommentStatement("transaction manager during the final phase of the transaction.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "Prepare";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(PreparingEnlistment)), "preparingEnlistment"));

			//			preparingEnlistment.Prepared();
			this.Statements.Add(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("preparingEnlistment"), "Prepared"));

			//		}

		}

	}
}

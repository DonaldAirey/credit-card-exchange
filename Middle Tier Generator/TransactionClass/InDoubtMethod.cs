namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;
    using System.Transactions;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class InDoubtMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public InDoubtMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Called when the state of a transaction is in doubt because the second phase of a two phase commit is in progress.
			//		/// </summary>
			//		/// <param name="enlistment">Facilitates communication bewtween an enlisted transaction participant and the transaction
			//		/// manager during the final phase of the transaction.</param>
			//		public void InDoubt(global::System.Transactions.Enlistment enlistment) { }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Adds a row lock to the list of locks that must be released at the end of a transaction.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"enlistment\">Facilitates communication bewtween an enlisted transaction participant and the transaction", true));
			this.Comments.Add(new CodeCommentStatement("manager during the final phase of the transaction.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "InDoubt";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Enlistment)), "enlistment"));

		}

	}
}

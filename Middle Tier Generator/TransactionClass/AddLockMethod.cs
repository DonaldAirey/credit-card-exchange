namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a method to add a lock to the transaction.
	/// </summary>
	class AddLockMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to add a lock to the transaction.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public AddLockMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Adds a row lock to the list of locks that must be released at the end of a transaction.
			//		/// </summary>
			//		/// <param name="row">The row containing the lock.</param>
			//		public void AddLock(IRow iRow)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Adds a row lock to the list of locks that must be released at the end of a transaction.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"iRow\">The lock to be added to the transaction.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "AddLock";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)), "iRow"));

			//			this.lockList.Add(iRow);
			this.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "lockList"),
					"Add",
					new CodeArgumentReferenceExpression("iRow")));

			//		}

		}

	}
}

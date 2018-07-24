namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class GetHashCodeMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public GetHashCodeMethod(DataModelSchema dataModelSchema)
		{

			//		public override int GetHashCode(object obj)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Serves as the hash function for a particular type.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>A hash code for the current Object.</returns>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Int32));
			this.Name = "GetHashCode";

			//			int hashCode = ((int)(this.data[1]));
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"hashCode",
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(
							new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "data"),
							new CodePrimitiveExpression(1)))));

			//			for (int index = 2; (index < 2 + this.keyLength); index = (index + 1))
			//			{
			CodeIterationStatement forIndex = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(Int32)),
					"index",
					new CodePrimitiveExpression(2)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("index"),
						CodeBinaryOperatorType.LessThan,
						new CodeBinaryOperatorExpression(
							new CodePrimitiveExpression(2),
							CodeBinaryOperatorType.Add,
							new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "keyLength"))),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("index"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("index"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//				hashCode = (hashCode + this.data[index].GetHashCode());
			forIndex.Statements.Add(
				new CodeAssignStatement(
					new CodeVariableReferenceExpression("hashCode"),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("hashCode"),
						CodeBinaryOperatorType.Add,
						new CodeMethodInvokeExpression(
							new CodeIndexerExpression(
								new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "data"),
								new CodeVariableReferenceExpression("index")),
							"GetHashCode"))));

			//			}
			this.Statements.Add(forIndex);

			//			return hashCode;
			this.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("hashCode")));

			//		}

		}

	}
}

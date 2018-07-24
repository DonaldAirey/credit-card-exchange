namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to read an XML file.
	/// </summary>
	class EqualsMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to read an XML file.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public EqualsMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Determines whether the specified System.Object is equal to the current TransactionLogItem.
			//		/// </summary>
			//		/// <param name="obj">The System.Object to be compared to the current TransactionLogItem.</param>
			//		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//		public override bool Equals(object obj)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Determines whether the specified System.Object is equal to the current TransactionLogItem.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"obj\">The System.Object to be compared to the current TransactionLogItem.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Boolean));
			this.Name = "Equals";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object)), "obj"));

			//			TransactionLogItem transactionLogItem = ((TransactionLogItem)(obj));
			this.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeTypeReference("TransactionLogItem"),
					"transactionLogItem",
					new CodeCastExpression(new CodeTypeReference("TransactionLogItem"), new CodeArgumentReferenceExpression("obj"))));

			//			if (((int)(this.data[1])) == ((int)(transactionLogItem.data[1])))
			//			{
			CodeConditionStatement ifTables = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(
							new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "data"),
							new CodePrimitiveExpression(1))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int32)),
						new CodeIndexerExpression(
							new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("transactionLogItem"), "data"),
							new CodePrimitiveExpression(1)))));

			//				for (int index = 2; (index < 2 + this.keyLength); index = (index + 1))
			//				{
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

			//					if (this.data[index].Equals(transactionLogItem.data[index]) == false)
			//					{
			CodeConditionStatement ifEquals = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeMethodInvokeExpression(
						new CodeIndexerExpression(
							new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "data"),
							new CodeVariableReferenceExpression("index")),
						"Equals",
						new CodeIndexerExpression(
							new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("transactionLogItem"), "data"),
							new CodeVariableReferenceExpression("index"))),
					CodeBinaryOperatorType.IdentityEquality,
					new CodePrimitiveExpression(false)));

			//						return false;
			ifEquals.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

			//					}
			forIndex.Statements.Add(ifEquals);

			//				}
			ifTables.TrueStatements.Add(forIndex);

			//				return true;
			ifTables.TrueStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));

			//			}
			this.Statements.Add(ifTables);

			//			return false;
			this.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));

			//		}

		}

	}
}

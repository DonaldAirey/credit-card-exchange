namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a statement to check that a record has been found.
	/// </summary>
	class CodeCheckIndexExistsStatement : CodeConditionStatement
	{

		/// <summary>
		/// Creates a statement to check that a record has been found.
		/// </summary>
		/// <param name="tableSchema"></param>
		public CodeCheckIndexExistsStatement(CodeExpression indexExpression, TableSchema tableSchema, CodeExpression indexNameExpression)
		{

			//			if ((objectRow0 == null)) {
			//				string departmentIdKeyText = string.Empty;
			//				for (int departmentIdKeyIndex = 0; departmentIdKeyIndex < departmentId.Length; departmentIdKeyIndex++)
			//				{
			//					departmentIdKeyText = departmentIdKeyText + global::String.Format("{0}", departmentId[departmentIdKeyIndex]);
			//					if (departmentIdKeyIndex < departmentId.Length - 1)
			//						departmentIdKeyText = departmentIdKeyText + ",";
			//				}
			//				throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Object record ({0}) that doesn\'t exist", departmentIdKeyText));
			//			}
			this.Condition = new CodeBinaryOperatorExpression(indexExpression, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
			this.TrueStatements.Add(new CodeThrowIndexNotFoundExceptionStatement(tableSchema, indexNameExpression));

		}

	}

}

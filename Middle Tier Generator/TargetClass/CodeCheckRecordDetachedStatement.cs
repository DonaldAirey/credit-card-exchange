namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a statement to check that a record hasn't been deleted.
	/// </summary>
	class CodeCheckRecordDetachedStatement : CodeConditionStatement
	{

		/// <summary>
		/// Creates a statement to check that a record hasn't been deleted.
		/// </summary>
		/// <param name="tableSchema"></param>
		public CodeCheckRecordDetachedStatement(TableSchema tableSchema, CodeExpression rowExpression, CodeExpression keyExpression)
		{

			//            // This makes sure the record wasn't deleted between the time it was found and the time it was locked.
			//            if ((employeeRow.RowState == System.Data.DataRowState.Detached)) {
			//                throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new FluidTrade.Core.RecordNotFoundFault("Attempt to access a Employee record ({0}) that doesn\'t exist", employeeId));
			//            }
			this.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(rowExpression, "RowState"), CodeBinaryOperatorType.IdentityEquality, new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Data.DataRowState)), "Detached"));
			this.TrueStatements.Add(new CodeThrowRecordNotFoundExceptionStatement(tableSchema, keyExpression));

		}

	}

}

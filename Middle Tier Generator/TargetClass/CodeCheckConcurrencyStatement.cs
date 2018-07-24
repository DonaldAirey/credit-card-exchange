namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a statement for optimistic concurrency checking.
	/// </summary>
	class CodeCheckConcurrencyStatement : CodeConditionStatement
	{

		/// <summary>
		/// Creates a statement for optimistic concurrency checking.
		/// </summary>
		/// <param name="tableSchema"></param>
		public CodeCheckConcurrencyStatement(TableSchema tableSchema, CodeVariableReferenceExpression rowExpression, CodeExpression keyExpression)
		{

			//            // The Optimistic Concurrency check allows only one client to update a record at a time.
			//            if ((employeeRow.RowVersion != rowVersion)) {
			//                throw new global::System.ServiceModel.FaultException<OptimisticConcurrencyFault>(new FluidTrade.Core.OptimisticConcurrencyFault("The Employee record ({0}) is busy.  Please try again later.", employeeId));
			//            }
			this.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(rowExpression, "RowVersion"), CodeBinaryOperatorType.IdentityInequality, new CodeArgumentReferenceExpression("rowVersion"));
			this.TrueStatements.Add(new CodeThrowConcurrencyExceptionStatement(tableSchema, keyExpression));

		}

	}

}

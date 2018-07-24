namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a statement method invocation that adds a record to an ADO transaction.
	/// </summary>
	class CodeAddLockToTransactionExpression : CodeMethodInvokeExpression
	{

		/// <summary>
		/// Creates a statement method invocation that adds a record to an ADO transaction.
		/// </summary>
		/// <param name="transactionExpression">The MiddleTierContext used for the transaction.</param>
		/// <param name="columnSchema">The record that is held for the duration of the transaction.</param>
		public CodeAddLockToTransactionExpression(CodeVariableReferenceExpression transactionExpression, CodeExpression rowExpression)
		{

			//            middleTierTransaction.AdoResourceManager.AddLock(departmentRow.ObjectRow);
			this.Method = new CodeMethodReferenceExpression(transactionExpression, "AddLock");
			this.Parameters.Add(rowExpression);

		}

	}

}

namespace FluidTrade.MiddleTierGenerator.RowClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a 'Record Not Locked' Exception for a table.
	/// </summary>
	class CodeThrowSynchronizationExceptionStatement : CodeThrowExceptionStatement
	{

		/// <summary>
		/// Creates a 'Record Not Locked' Exception for a table.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public CodeThrowSynchronizationExceptionStatement(TableSchema tableSchema)
		{

			//                        throw new global::System.ServiceModel.FaultException<SynchronizationLockFault>(new global::FluidTrade.Core.SynchronizationLockFault("Attempt to access a Department record without a lock."));
			this.ToThrow = new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultException<SynchronizationLockFault>)),
				new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(SynchronizationLockFault)), new CodePrimitiveExpression(tableSchema.Name)));

		}

	}
}

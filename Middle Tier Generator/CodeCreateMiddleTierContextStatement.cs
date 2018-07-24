namespace FluidTrade.MiddleTierGenerator
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a middle tier context for an operation.
	/// </summary>
	class CodeCreateMiddleTierContextStatement : CodeVariableDeclarationStatement
	{

		/// <summary>
		/// Creates a middle tier context for an operation.
		/// </summary>
		/// <param name="tableSchema"></param>
		public CodeCreateMiddleTierContextStatement(DataModelSchema dataModelSchema, CodeVariableReferenceExpression transactionExpression)
		{

			//                    global::FluidTrade.Core.MiddleTierContext middleTierTransaction = global::FluidTrade.Core.MiddleTierContext.Current;
			this.Type = new CodeTypeReference(string.Format("{0}Transaction", dataModelSchema.Name));
			this.Name = transactionExpression.VariableName;
			this.InitExpression = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(this.Type), "Current");

		}

	}

}

namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a 'Record Not Found' Fault for the primary key of a table.
	/// </summary>
	class CodeThrowIndexNotFoundExceptionStatement : CodeThrowExceptionStatement
	{

		/// <summary>
		/// Creates a 'Record Not Found' Exception for the primary key of a table.
		/// </summary>
		/// <param name="tableSchema">The table where the error occured.</param>
		public CodeThrowIndexNotFoundExceptionStatement(TableSchema tableSchema, CodeExpression indexNameExpression)
		{

			//                throw new global::System.ServiceModel.FaultException<IndexNotFoundFault>(new global::FluidTrade.Core.IndexNotFoundFault("Attempt to access a Employee record ({0}) that doesn\'t exist", employeeIdKeyText));
			this.ToThrow = new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultException<IndexNotFoundFault>)),
				new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(IndexNotFoundFault)), new CodePrimitiveExpression(tableSchema.Name), indexNameExpression));

		}

	}
}

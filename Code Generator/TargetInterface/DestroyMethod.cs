namespace FluidTrade.Core.TargetInterface
{

    using System.CodeDom;
    using System.Collections.Generic;

	/// <summary>
	/// Creates the CodeDOM of a method to destroy a record using transacted logic.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DestroyMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates the CodeDOM for a method to delete a record from a table using transacted logic.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public DestroyMethod(TableSchema tableSchema)
		{

			// Create a matrix of parameters for this operation.
			DestroyParameterMatrix destroyParameterMatrix = new DestroyParameterMatrix(tableSchema);

			//        /// <summary>
			//        /// Deletes a Employee record.
			//        /// </summary>
			//        /// <param name="employeeId">The value for the EmployeeId column.</param>
			//        /// <param name="rowVersion">The value for the RowVersion column.</param>
			//        /// <param name="rowVersion">Used for Optimistic Concurrency Checking.</param>			
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Deletes a {0} record.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"{0}\">{1}</param>", parameterPair.Value.Name, parameterPair.Value.Description), true));

			//        [global::System.ServiceModel.OperationContractAttribute()]
			//        [global::System.ServiceModel.TransactionFlowAttribute(global::System.ServiceModel.TransactionFlowOption.Allowed)]
			//        [global::System.ServiceModel.ServiceKnownTypeAttribute(typeof(global::System.DBNull))]
			//        [global::System.ServiceModel.FaultContractAttribute(typeof(global::FluidTrade.Core.RecordNotFoundFault))]
			//        [global::System.ServiceModel.FaultContractAttribute(typeof(global::FluidTrade.Core.OptimisticConcurrencyFault))]
			//        void DestroyEmployee(int employeeId, long rowVersion);
			string actionUri = string.Format("http://tempuri.org/I{0}/Destroy{1}", tableSchema.DataModel.Name, tableSchema.Name);
			string actionReplyUri = string.Format("http://tempuri.org/I{0}/Destroy{1}Response", tableSchema.DataModel.Name, tableSchema.Name);
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.OperationContractAttribute)), new CodeAttributeArgument("Action", new CodePrimitiveExpression(actionUri)), new CodeAttributeArgument("ReplyAction", new CodePrimitiveExpression(actionReplyUri))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.TransactionFlowAttribute)), new CodeAttributeArgument(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.ServiceModel.TransactionFlowOption)), "Allowed"))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.ServiceKnownTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(System.DBNull))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(RecordNotFoundFault))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(OptimisticConcurrencyFault))))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Abstract;
			this.Name = string.Format("Destroy{0}", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

		}

	}

}

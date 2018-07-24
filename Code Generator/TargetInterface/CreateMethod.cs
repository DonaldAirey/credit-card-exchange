namespace FluidTrade.Core.TargetInterface
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

    /// <summary>
	/// Creates the CodeDOM of an interface to insert a record using transacted logic.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class CreateMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates the CodeDOM of an interface to insert a record using transacted logic.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public CreateMethod(TableSchema tableSchema)
		{

			// Create a matrix of parameters for this operation.
			CreateParameterMatrix createParameterMatrix = new CreateParameterMatrix(tableSchema);

			//        /// <summary>
			//        /// Creates a Employee record.
			//        /// </summary>
			//        /// <param name="age">The required value for the Age column.</param>
			//        /// <param name="departmentId">The required value for the DepartmentId column.</param>
			//        /// <param name="employeeId">The required value for the EmployeeId column.</param>
			//        /// <param name="raceCode">The optional value for the RaceCode column.</param>
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates a {0} record.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createParameterMatrix.ExternalParameterItems)
				this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"{0}\">{1}</param>", parameterPair.Value.Name, parameterPair.Value.Description), true));
			//        [global::System.ServiceModel.OperationContractAttribute()]
			//        [global::System.ServiceModel.TransactionFlowAttribute(global::System.ServiceModel.TransactionFlowOption.Allowed)]
			//        [global::System.ServiceModel.ServiceKnownTypeAttribute(typeof(global::System.DBNull))]
			//        [global::System.ServiceModel.FaultContractAttribute(typeof(global::FluidTrade.Core.RecordNotFoundFault))]
			//        void CreateEmployee(int age, int departmentId, int employeeId, object raceCode, out long rowVersion);
			string actionUri = string.Format("http://tempuri.org/I{0}/Create{1}", tableSchema.DataModel.Name, tableSchema.Name);
			string actionReplyUri = string.Format("http://tempuri.org/I{0}/Create{1}Response", tableSchema.DataModel.Name, tableSchema.Name);
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.OperationContractAttribute)), new CodeAttributeArgument("Action", new CodePrimitiveExpression(actionUri)), new CodeAttributeArgument("ReplyAction", new CodePrimitiveExpression(actionReplyUri))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.TransactionFlowAttribute)), new CodeAttributeArgument(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.ServiceModel.TransactionFlowOption)), "Allowed"))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.ServiceKnownTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(System.DBNull))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(RecordNotFoundFault))))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Abstract;
			this.Name = string.Format("Create{0}", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

		}

	}

}

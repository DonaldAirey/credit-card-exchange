namespace FluidTrade.Core.TargetInterface
{

    using System.CodeDom;
	using System.Collections.Generic;

    /// <summary>
	/// Creates a method that loads records into the database from an external source.
	/// </summary>
	class CreateExMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that loads records into the database from an external source.
		/// </summary>
		/// <param name="tableSchema">The schema used to describe the table.</param>
		public CreateExMethod(TableSchema tableSchema)
		{

			// This shreds the list of parameters up into a metadata stucture that is helpful in extracting ordinary parameters 
			// from those that need to be found in other tables using external identifiers.
			CreateExParameterMatrix createExParameterMatrix = new CreateExParameterMatrix(tableSchema);

			//        /// <summary>
			//        /// Loads a record into the Department table from an external source.
			//        /// </summary>
			//        /// <param name="age">The required value for the Age column.</param>
			//        /// <param name="configurationId">Selects a configuration of unique indices used to resolve external identifiers.</param>
			//        /// <param name="genderKey">A required unique key for the parent Gender record.</param>
			//        /// <param name="objectKey">A required unique key for the parent Object record.</param>
			//        /// <param name="unionKey">A required unique key for the parent Union record.</param>			
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Loads a record into the {0} table from an external source.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createExParameterMatrix.ExternalParameterItems)
				this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"{0}\">{1}</param>", parameterPair.Value.Name, parameterPair.Value.Description), true));

			//        public void CreateEmployee(int age, string configurationId, string departmentId, string employeeId, string marriageCode) {
			//			{
			string actionUri = string.Format("http://tempuri.org/I{0}/Create{1}Ex", tableSchema.DataModel.Name, tableSchema.Name);
			string actionReplyUri = string.Format("http://tempuri.org/I{0}/Create{1}ExResponse", tableSchema.DataModel.Name, tableSchema.Name);
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.OperationContractAttribute)), new CodeAttributeArgument("Action", new CodePrimitiveExpression(actionUri)), new CodeAttributeArgument("ReplyAction", new CodePrimitiveExpression(actionReplyUri))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.TransactionFlowAttribute)), new CodeAttributeArgument(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.ServiceModel.TransactionFlowOption)), "Allowed"))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.ServiceKnownTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(System.DBNull))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(RecordNotFoundFault))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(IndexNotFoundFault))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(ArgumentFault))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(FormatFault))))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Abstract;
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = string.Format("Create{0}Ex", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createExParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

		}

	}

}

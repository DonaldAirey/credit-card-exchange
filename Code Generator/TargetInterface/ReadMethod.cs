namespace FluidTrade.Core.TargetInterface
{

	using System;
	using System.CodeDom;
    using System.ServiceModel;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class ReadMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">A description of the data model.</param>
		public ReadMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Collects the set of modified records that will reconcile the client data model to the master data model.
			//		/// </summary>
			//		/// <param name="identifier">A unique identifier of an instance of the data.</param>
			//		/// <param name="sequence">The sequence of the client data model.</param>
			//		/// <returns>An array of records that will reconcile the client data model to the server.</returns>			
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Collects the set of modified records that will reconcile the client data model to the master data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"dataSetId\">A unique identifier of an instance of the data.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"sequence\">The sequence of the client data model.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>An array of records that will reconcile the client data model to the server.</returns>", true));

			//	[global::System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModel/Read", ReplyAction="http://tempuri.org/IDataModel/ReadResponse")]
			//	[global::System.ServiceModel.TransactionFlowAttribute(global::System.ServiceModel.TransactionFlowOption.NotAllowed)]
			//	[global::System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.DBNull))]
			//	[global::System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
			//	object[] Read(global::System.Guid dataSetId, long sequence);
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(OperationContractAttribute)),
					new CodeAttributeArgument("Action", new CodePrimitiveExpression(string.Format("http://tempuri.org/I{0}/Read", dataModelSchema.Name))),
					new CodeAttributeArgument(
						"ReplyAction",
						new CodePrimitiveExpression(string.Format("http://tempuri.org/I{0}/ReadResponse", dataModelSchema.Name)))));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(TransactionFlowAttribute)),
					new CodeAttributeArgument(
						new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(TransactionFlowOption)), "NotAllowed"))));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(ServiceKnownTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(typeof(DBNull)))));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(ServiceKnownTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(typeof(Guid)))));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(ServiceKnownTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(typeof(Object[])))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Abstract;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object[]));
			this.Name = "Read";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Guid)), "dataSetId"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Int64)), "sequence"));

		}

	}

}

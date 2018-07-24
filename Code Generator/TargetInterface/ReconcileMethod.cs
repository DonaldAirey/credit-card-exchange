namespace FluidTrade.Core.TargetInterface
{

	using System;
	using System.CodeDom;
    using System.ServiceModel;

    /// <summary>
	/// Creates a method to handle reconciling the deleted records on the client with the deleted records on the server.
	/// </summary>
	class ReconcileMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle reconciling the deleted records on the client with the deleted records on the server.
		/// </summary>
		/// <param name="schema">A description of the data model.</param>
		public ReconcileMethod(DataModelSchema dataModelSchema)
		{

			//	/// <summary>
			//	/// Determines whether the given records exist in the current data model.
			//	/// </summary>
			//	/// <param name="keys">An array of record keys and the index of the table to which they belong.</param>
			//	/// <returns>An array of records keys that have been deleted.</returns>
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Determines whether the given records exist in the current data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"keys\">An array of record keys and the index of the table to which they belong.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>An array of records keys that have been deleted.</returns>", true));

			//	[global::System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataModel/Reconcile", ReplyAction="http://tempuri.org/IDataModel/ReconcileResponse")]
			//	[global::System.ServiceModel.TransactionFlowAttribute(global::System.ServiceModel.TransactionFlowOption.NotAllowed)]
			//	[global::System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.DBNull))]
			//	[global::System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
			//	object[] Reconcile(global::System.Guid dataSetId, long sequence);
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(OperationContractAttribute)),
					new CodeAttributeArgument("Action", new CodePrimitiveExpression(string.Format("http://tempuri.org/I{0}/Reconcile", dataModelSchema.Name))),
					new CodeAttributeArgument(
						"ReplyAction",
						new CodePrimitiveExpression(string.Format("http://tempuri.org/I{0}/ReconcileResponse", dataModelSchema.Name)))));
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
			this.Name = "Reconcile";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object[])), "keys"));

		}

	}

}

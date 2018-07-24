namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using System.Security.Permissions;
    using System.ServiceModel;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class ReadMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public ReadMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Collects the set of modified records that will reconcile the client data model to the master data model.
			//		/// </summary>
			//		/// <param name="identifier">A unique identifier of an instance of the data.</param>
			//		/// <param name="sequence">The sequence of the client data model.</param>
			//		/// <returns>An array of records that will reconcile the client data model to the server.</returns>
			//		[global::FluidTrade.Core.ClaimsPrincipalPermission(global::System.Security.Permissions.SecurityAction.Demand, ClaimType=global::FluidTrade.Core.ClaimTypes.Read, Resource=global::FluidTrade.Core.Resources.Application)]
			//		public object[] Read(global::System.Guid identifier, long sequence)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Collects the set of modified records that will reconcile the client data model to the master data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"identifier\">A unique identifier of an instance of the data.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"sequence\">The sequence of the client data model.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>An array of records that will reconcile the client data model to the server.</returns>", true));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(OperationBehaviorAttribute)),
					new CodeAttributeArgument("TransactionScopeRequired", new CodePrimitiveExpression(true))));
			//AR FB 408 - Remove Claims requirement
			//this.CustomAttributes.Add(
			//    new CodeAttributeDeclaration(
			//        new CodeGlobalTypeReference(typeof(ClaimsPrincipalPermission)),
			//        new CodeAttributeArgument(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(SecurityAction)), "Demand")),
			//        new CodeAttributeArgument("ClaimType", new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ClaimTypes)), "Read")),
			//        new CodeAttributeArgument("Resource", new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Resources)), "Application"))));
			this.Name = "Read";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object[]));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Guid)), "identifier"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Int64)), "sequence"));

			this.Statements.Add(
				new CodeMethodReturnStatement(
					new CodeMethodInvokeExpression(
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name),
							String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						"Read",
						new CodeArgumentReferenceExpression("identifier"),
						new CodeArgumentReferenceExpression("sequence"))));

		}

	}

}

namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method that loads records into the database from an external source.
	/// </summary>
	class DestroyExMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that loads records into the database from an external source.
		/// </summary>
		/// <param name="tableSchema">The schema used to describe the table.</param>
		public DestroyExMethod(TableSchema tableSchema)
		{


			// This shreds the list of parameters up into a metadata stucture that is helpful in extracting ordinary parameters 
			// from those that need to be found in other tables using external identifiers.
			DestroyExParameterMatrix destroyParameterMatrix = new DestroyExParameterMatrix(tableSchema);

			//        /// <summary>
			//        /// Loads a record into the Department table from an external source.
			//        /// </summary>
			//        /// <param name="configurationId">Selects a configuration of unique indices used to resolve external identifiers.</param>
			//        /// <param name="employeeKey">An optional unique key for the parent Employee record.</param>
			//        /// <param name="managerKey">An optional unique key for the parent Manager record.</param>
			//        [global::System.ServiceModel.OperationBehaviorAttribute(TransactionScopeRequired=true)]
			//        [global::FluidTrade.Core.ClaimsPrincipalPermission(global::System.Security.Permissions.SecurityAction.Demand, ClaimType=global::FluidTrade.Core.ClaimTypes.Create, Resource=global::FluidTrade.Core.Resources.Application)]
			//        public void DestroyEngineerEx(string configurationId, object[] employeeKey, object[] managerKey) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Loads a record into the Department table from an external source.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.OperationBehaviorAttribute)), new CodeAttributeArgument("TransactionScopeRequired", new CodePrimitiveExpression(true))));
			//AR FB 408 - Remove Claims requirement
			//this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(ClaimsPrincipalPermission)), new CodeAttributeArgument(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Security.Permissions.SecurityAction)), "Demand")),
			//    new CodeAttributeArgument("ClaimType", new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ClaimTypes)), "Create")),
			//    new CodeAttributeArgument("Resource", new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Resources)), "Application"))));
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"{0}\">{1}</param>", parameterPair.Value.Name, parameterPair.Value.Description), true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = string.Format("Destroy{0}Ex", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

			//            // This provides a context for the middle tier transactions.
			//            global::FluidTrade.Core.MiddleTierContext middleTierTransaction = global::FluidTrade.Core.MiddleTierContext.Current;
			CodeVariableReferenceExpression transactionExpression = new CodeRandomVariableReferenceExpression();
			this.Statements.Add(new CodeCreateMiddleTierContextStatement(tableSchema.DataModel, transactionExpression));

			// This will resolve the external identifiers and the build the primary key for the target record.  The main idea is to
			// map elements from foreign rows into parameters that can be used to call the internal methods.
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
			{

				// Every internal update method requires a primary key.  The external methods do not have this requirement and can
				// use any unique key.  The translation between the external unique key and the internal primary key is created
				// here.
				if (parameterPair.Value is UniqueConstraintParameterItem)
					this.Statements.AddRange(new CodeResolvePrimaryKeyStatements(tableSchema, transactionExpression, parameterPair.Value as UniqueConstraintParameterItem));

			}

			// At this point, all the external variables have been resolved and the primary index of the target row has been
			// calculated in the parameter matrix.  This will perform the destroy with the internal method.
			this.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), string.Format("Destroy{0}", tableSchema.Name), destroyParameterMatrix.DestroyParameters));

		}

	}

}

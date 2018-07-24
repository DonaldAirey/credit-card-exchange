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
	class ReconcileMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public ReconcileMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Determines whether the given records exist in the current data model.
			//		/// </summary>
			//		/// <param name="keys">An array of record keys and the index of the table to which they belong.</param>
			//		/// <returns>An array of records keys that have been deleted.</returns>
			//		[global::System.ServiceModel.OperationBehaviorAttribute(TransactionScopeRequired=true)]
			//		public object[] Reconcile(object[] keys)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Determines whether the given records exist in the current data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"keys\">An array of record keys and the index of the table to which they belong.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>An array of records keys that have been deleted.</returns>", true));
			this.CustomAttributes.Add(
				new CodeAttributeDeclaration(
					new CodeGlobalTypeReference(typeof(OperationBehaviorAttribute)),
					new CodeAttributeArgument("TransactionScopeRequired", new CodePrimitiveExpression(true))));
			this.Name = "Reconcile";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object[]));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object[])), "keys"));

			//			return DataModel.dataModelDataSet.Reconcile(keys);
			this.Statements.Add(
				new CodeMethodReturnStatement(
					new CodeMethodInvokeExpression(
						new CodeFieldReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name),
							String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						"Reconcile",
						new CodeArgumentReferenceExpression("keys"))));

			//		}

		}

	}

}

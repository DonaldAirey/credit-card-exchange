namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System;
    using System.CodeDom;
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

			//		public object[] Reconcile(object[] keys)
			//		{
			//			return base.Channel.Reconcile(keys);
			//		}
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.ReturnType = new CodeGlobalTypeReference(typeof(System.Object[]));
			this.Name = "Reconcile";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object[])), "keys"));
			this.Statements.Add(
				new CodeMethodReturnStatement(
					new CodeMethodInvokeExpression(
						new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Channel"),
						this.Name,
						new CodeArgumentReferenceExpression("keys"))));

		}

	}
}

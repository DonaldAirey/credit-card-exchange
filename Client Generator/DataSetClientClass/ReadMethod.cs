namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System;
    using System.CodeDom;
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

			//		public object[] Read(System.DateTime clientStartTime, long[][] clientGap)
			//		{
			//			return base.Channel.Read(clientStartTime, clientGap);
			//		}
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.ReturnType = new CodeGlobalTypeReference(typeof(System.Object[]));
			this.Name = "Read";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Guid)), "dataSetId"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Int64)), "sequence"));
			this.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Channel"), this.Name,
				new CodeArgumentReferenceExpression("dataSetId"), new CodeArgumentReferenceExpression("sequence"))));

		}

	}
}

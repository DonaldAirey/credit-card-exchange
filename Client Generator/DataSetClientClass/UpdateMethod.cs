namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

	/// <summary>
	/// Creates the CodeDOM of a method to update a record using transacted logic.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class UpdateMethod	: CodeMemberMethod
	{

		/// <summary>
		/// Creates the CodeDOM for a method to update a record in a table using transacted logic.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public UpdateMethod(TableSchema tableSchema)
		{

			// Create a matrix of parameters for this operation.
			UpdateParameterMatrix updateParameterMatrix = new UpdateParameterMatrix(tableSchema);

			//	[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//	public void UpdateObject(object description, object externalId, object name, System.Guid objectId, long rowVersion, object typeCode)
			//	{
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = string.Format("Update{0}", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in updateParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

			//		base.Channel.UpdateObject(description, externalId, name, objectId, rowVersion, typeCode);
			List<CodeExpression> arguments = new List<CodeExpression>();
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in updateParameterMatrix.ExternalParameterItems)
				arguments.Add(new CodeArgumentReferenceExpression(parameterPair.Value.Name));
			this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Channel"), this.Name,
				arguments.ToArray()));

			//	}

		}

	}

}

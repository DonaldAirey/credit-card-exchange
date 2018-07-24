namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

    /// <summary>
	/// Creates the CodeDOM of a method to insert a record using transacted logic.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class CreateMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates the CodeDOM for a method to insert a record into a table using transacted logic.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public CreateMethod(TableSchema tableSchema)
		{

			// Create a matrix of parameters for this operation.
			CreateParameterMatrix createParameterMatrix = new CreateParameterMatrix(tableSchema);

			//	[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//	public void CreateObject(object description, string externalId, string name, System.Guid objectId, object typeCode)
			//	{
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = string.Format("Create{0}", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

			//		base.Channel.UpdateObject(description, externalId, name, objectId, rowVersion, typeCode);
			List<CodeExpression> arguments = new List<CodeExpression>();
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createParameterMatrix.ExternalParameterItems)
			{
				ExternalParameterItem externalParameterItem = parameterPair.Value;
				CodeDirectionExpression codeDirectionExpression = new CodeDirectionExpression(
					externalParameterItem.FieldDirection,
					new CodeArgumentReferenceExpression(parameterPair.Value.Name));
				arguments.Add(codeDirectionExpression);
			}
			this.Statements.Add(
				new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Channel"), this.Name, arguments.ToArray()));

			//	}

		}

	}

}

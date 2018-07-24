namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

	/// <summary>
	/// Creates the CodeDOM of a method to destroy a record using transacted logic.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DestroyMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates the CodeDOM for a method to delete a record from a table using transacted logic.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public DestroyMethod(TableSchema tableSchema)
		{

			// Create a matrix of parameters for this operation.
			DestroyParameterMatrix destroyParameterMatrix = new DestroyParameterMatrix(tableSchema);

			//	[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//	public void DestroyObject(System.Guid objectId, long rowVersion)
			//	{
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Name = string.Format("Destroy{0}", tableSchema.Name);
			List<CodeParameterDeclarationExpression> parameters = new List<CodeParameterDeclarationExpression>();
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

			//		base.Channel.DestroyObject(description, externalId, name, objectId, rowVersion, typeCode);
			List<CodeExpression> arguments = new List<CodeExpression>();
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				arguments.Add(new CodeArgumentReferenceExpression(parameterPair.Value.Name));
			this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Channel"), this.Name,
				arguments.ToArray()));

			//	}

		}

	}

}

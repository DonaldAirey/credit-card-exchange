namespace FluidTrade.ClientGenerator.DataSetClientClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

    /// <summary>
	/// Creates the CodeDOM of a method to insert a record using transacted logic.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DestroyExMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates the CodeDOM for a method to insert a record into a table using transacted logic.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public DestroyExMethod(TableSchema tableSchema)
		{

			// This shreds the list of parameters up into a metadata stucture that is helpful in extracting ordinary parameters 
			// from those that need to be found in other tables using external identifiers.
			DestroyExParameterMatrix destroyParameterMatrix = new DestroyExParameterMatrix(tableSchema);

			//	[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//	public void DestroyObjectEx(string configurationId, object externalId, global::System.Guid objectId)
			//	{
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = string.Format("Destroy{0}Ex", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

			//		base.Channel.DestroyObjectEx(configurationId, externalId, objectId);
			List<CodeExpression> arguments = new List<CodeExpression>();
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in destroyParameterMatrix.ExternalParameterItems)
				arguments.Add(new CodeArgumentReferenceExpression(parameterPair.Value.Name));
			this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Channel"), this.Name,
				arguments.ToArray()));

			//	}

		}

	}

}

namespace FluidTrade.Core.TargetInterface
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;

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


			// This collects a distinct list of data types that are added to the contract.
			SortedList<string, Type> knownTypes = new SortedList<string, Type>();
			foreach (KeyValuePair<string, ExternalParameterItem> externalParameterPair in updateParameterMatrix.ExternalParameterItems)
			{
				if (externalParameterPair.Value.DeclaredDataType == typeof(System.Object))
					if (!knownTypes.ContainsKey(typeof(System.DBNull).Name))
						knownTypes.Add(typeof(System.DBNull).Name, typeof(System.DBNull));
				if (externalParameterPair.Value.ActualDataType != externalParameterPair.Value.DeclaredDataType)
					if (!externalParameterPair.Value.ActualDataType.IsPrimitive)
						if (!knownTypes.ContainsKey(externalParameterPair.Value.ActualDataType.Name))
							knownTypes.Add(externalParameterPair.Value.ActualDataType.Name, externalParameterPair.Value.ActualDataType);
			}

			//        /// <summary>
			//        /// Updates a Employee record.
			//        /// </summary>
			//        /// <param name="age">The optional value for the Age column.</param>
			//        /// <param name="departmentId">The optional value for the DepartmentId column.</param>
			//        /// <param name="employeeId">The required value for the EmployeeId column.</param>
			//        /// <param name="raceCode">The optional value for the RaceCode column.</param>
			//        /// <param name="rowVersion">Used for Optimistic Concurrency Checking.</param>
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Updates a {0} record.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in updateParameterMatrix.ExternalParameterItems)
				this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"{0}\">{1}</param>", parameterPair.Value.Name, parameterPair.Value.Description), true));

			//        [global::System.ServiceModel.OperationContractAttribute()]
			//        [global::System.ServiceModel.TransactionFlowAttribute(global::System.ServiceModel.TransactionFlowOption.Allowed)]
			//        [global::System.ServiceModel.ServiceKnownTypeAttribute(typeof(global::System.DBNull))]
			//        [global::System.ServiceModel.FaultContractAttribute(typeof(global::FluidTrade.Core.RecordNotFoundFault))]
			//        [global::System.ServiceModel.FaultContractAttribute(typeof(global::FluidTrade.Core.OptimisticConcurrencyFault))]
			//        void UpdateEmployee(object age, object departmentId, int employeeId, object raceCode, ref long rowVersion);
			string actionUri = string.Format("http://tempuri.org/I{0}/Update{1}", tableSchema.DataModel.Name, tableSchema.Name);
			string actionReplyUri = string.Format("http://tempuri.org/I{0}/Update{1}Response", tableSchema.DataModel.Name, tableSchema.Name);
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.OperationContractAttribute)), new CodeAttributeArgument("Action", new CodePrimitiveExpression(actionUri)), new CodeAttributeArgument("ReplyAction", new CodePrimitiveExpression(actionReplyUri))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.TransactionFlowAttribute)), new CodeAttributeArgument(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.ServiceModel.TransactionFlowOption)), "Allowed"))));
			foreach (KeyValuePair<string, Type> knownTypePair in knownTypes)
				this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.ServiceKnownTypeAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(knownTypePair.Value)))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(RecordNotFoundFault))))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.FaultContractAttribute)), new CodeAttributeArgument(new CodeTypeOfExpression(new CodeGlobalTypeReference(typeof(OptimisticConcurrencyFault))))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Abstract;
			this.Name = string.Format("Update{0}", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in updateParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

		}

	}

}

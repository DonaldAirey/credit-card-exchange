namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method that loads records into the database from an external source.
	/// </summary>
	class CreateExMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that loads records into the database from an external source.
		/// </summary>
		/// <param name="tableSchema">The schema used to describe the table.</param>
		public CreateExMethod(TableSchema tableSchema)
		{

			// This shreds the list of parameters up into a metadata stucture that is helpful in extracting ordinary parameters 
			// from those that need to be found in other tables using external identifiers.
			CreateExParameterMatrix createParameterMatrix = new CreateExParameterMatrix(tableSchema);

			//        /// <summary>
			//        /// Loads a record into the Department table from an external source.
			//        /// </summary>
			//        /// <param name="age">The required value for the Age column.</param>
			//        /// <param name="configurationId">Selects a configuration of unique indices used to resolve external identifiers.</param>
			//        /// <param name="genderKey">A required unique key for the parent Gender record.</param>
			//        /// <param name="objectKey">A required unique key for the parent Object record.</param>
			//        /// <param name="unionKey">A required unique key for the parent Union record.</param>
			//        [global::System.ServiceModel.OperationBehaviorAttribute(TransactionScopeRequired=true)]
			//        [global::FluidTrade.Core.ClaimsPrincipalPermission(global::System.Security.Permissions.SecurityAction.Demand, ClaimType=global::FluidTrade.Core.ClaimTypes.Create, Resource=global::FluidTrade.Core.Resources.Application)]
			//        public void CreateEmployeeEx(int age, string configurationId, object[] genderKey, object[] objectKey, object[] unionKey) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Loads a record into the {0} table from an external source.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ServiceModel.OperationBehaviorAttribute)), new CodeAttributeArgument("TransactionScopeRequired", new CodePrimitiveExpression(true))));
			//AR FB 408 - Remove Claims requirement
			//this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(ClaimsPrincipalPermission)), new CodeAttributeArgument(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Security.Permissions.SecurityAction)), "Demand")),
			//    new CodeAttributeArgument("ClaimType", new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(ClaimTypes)), "Create")),
			//    new CodeAttributeArgument("Resource", new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(Resources)), "Application"))));
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createParameterMatrix.ExternalParameterItems)
				this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"{0}\">{1}</param>", parameterPair.Value.Name, parameterPair.Value.Description), true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = string.Format("Create{0}Ex", tableSchema.Name);
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createParameterMatrix.ExternalParameterItems)
				this.Parameters.Add(parameterPair.Value.CodeParameterDeclarationExpression);

			//            // This provides a context for the middle tier transactions.
			//            global::FluidTrade.Core.MiddleTierContext middleTierTransaction = global::FluidTrade.Core.MiddleTierContext.Current;
			CodeVariableReferenceExpression transactionExpression = new CodeRandomVariableReferenceExpression();
			this.Statements.Add(new CodeCreateMiddleTierContextStatement(tableSchema.DataModel, transactionExpression));

			// This will resolve the external identifiers that relate to foreign tables.  The main idea is to map elements from 
			// foreign rows into parameters that can be used to call the internal methods.
			foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in createParameterMatrix.ExternalParameterItems)
			{

				// This will recurse into the foreign key relations that use external identifiers and create code to resolve the
				// variables using the external record.
				if (parameterPair.Value is ForeignKeyConstraintParameterItem)
					this.Statements.AddRange(new CodeResolveExternalVariableStatements(tableSchema, transactionExpression, parameterPair.Value as ForeignKeyConstraintParameterItem));

			}

			// All the external identifiers have been resolved.  Now it is time to see if the record exists or if it has to be
			// created.  Finding the record requires a unique index.  If there are more than one unique index, a decision needs to
			// be made as to which one should be used.  The configuration will drive that decision.  If there is only one unique
			// constraint, then the decision doesn't need to be made.
			UniqueConstraintSchema[] uniqueConstraints = tableSchema.UniqueConstraintSchemas;

			// Optimized code is provided when there is only one unique constraint on a table.  This saves the database
			// administrator from having to configure every single table in the data model with a description of the unique index
			// that is to be used when finding a row in that table.  If there is more than one unique constraint on a table, a
			// value will need to be provided in the configuration to tell the loader which one to use.
			if (uniqueConstraints.Length == 1)
			{

				//            // Find the record using the only index available.
				//            object[] configurationKey0 = new object[] {
				//                    configurationId,
				//                    relationName};
				//            ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configurationKey0);
				this.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Object[])), createParameterMatrix.UniqueKeyExpression.VariableName,
					new CodeKeyCreateExpression(createParameterMatrix.CreateKey(uniqueConstraints[0]))));
				this.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(string.Format("{0}Row", tableSchema.Name)), createParameterMatrix.RowExpression.VariableName,
					new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), tableSchema.Name), uniqueConstraints[0].Name), "Find", createParameterMatrix.UniqueKeyExpression)));

			}
			else
			{

				//            // This will find and lock the configuration row that selects the unique constraint for this table.
				//            object[] configurationKey0 = new object[] {
				//                    configurationId,
				//                    "Gender"};
				//            ConfigurationRow configurationRow0 = DataModel.Configuration.ConfigurationKey.Find(configurationKey0);
				//            if ((configurationRow0 == null)) {
				//                throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Configuration record ({0}) that doesn\'t exist", configurationKey0));
				//            }
				//            configurationRow0.AcquireReaderLock(middleTierTransaction.AdoResourceManager.Guid, DataModel.lockTimeout);
				//            middleTierTransaction.AdoResourceManager.AddLock(configurationRow0);
				//            if ((configurationRow0.RowState == global::System.Data.DataRowState.Detached)) {
				//                throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Configuration record ({0}) that doesn\'t exist", configurationKey0));
				//            }
				CodeVariableReferenceExpression configurationKeyExpression = new CodeRandomVariableReferenceExpression();
				this.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Object[])), configurationKeyExpression.VariableName,
					new CodeKeyCreateExpression(new CodeArgumentReferenceExpression("configurationId"), new CodePrimitiveExpression(tableSchema.Name))));
				CodeVariableReferenceExpression configurationRowExpression = new CodeRandomVariableReferenceExpression();
				this.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("ConfigurationRow"), configurationRowExpression.VariableName,
					new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), "Configuration"), "ConfigurationKey"), "Find", configurationKeyExpression)));
				TableSchema configurationTableSchema = tableSchema.DataModel.Tables["Configuration"];
				this.Statements.Add(new CodeCheckRecordExistsStatement(configurationTableSchema, configurationRowExpression, configurationKeyExpression));
				this.Statements.Add(new CodeAcquireRecordReaderLockExpression(transactionExpression, configurationRowExpression, tableSchema.DataModel));
				this.Statements.Add(new CodeAddLockToTransactionExpression(transactionExpression, configurationRowExpression));
				this.Statements.Add(new CodeCheckRecordDetachedStatement(configurationTableSchema, configurationRowExpression, configurationKeyExpression));

				//            object[] genderKey1 = null;
				//            if ((configurationRow0.IndexName == "GenderKey")) {
				//                genderKey1 = new object[] {
				//                        genderCode};
				//            }
				//            if ((configurationRow0.IndexName == "GenderKeyExternalId0")) {
				//                genderKey1 = new object[] {
				//                        externalId0};
				//            }
				this.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Object[])), createParameterMatrix.UniqueKeyExpression.VariableName, new CodePrimitiveExpression(null)));
				CodePropertyReferenceExpression indexNameExpression = new CodePropertyReferenceExpression(configurationRowExpression, "IndexName");
				foreach (UniqueConstraintSchema uniqueConstraintSchema in uniqueConstraints)
				{
					CodeConditionStatement ifConfiguration = new CodeConditionStatement(new CodeBinaryOperatorExpression(indexNameExpression, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(uniqueConstraintSchema.Name)));
					ifConfiguration.TrueStatements.Add(new CodeAssignStatement(createParameterMatrix.UniqueKeyExpression, new CodeKeyCreateExpression(createParameterMatrix.CreateKey(uniqueConstraintSchema))));
					this.Statements.Add(ifConfiguration);
				}

				//            // Use the index and the key specified by the configuration to find the record.
				//            IGenderIndex dataIndex0 = ((IGenderIndex)(DataModel.Gender.Indices[configurationRow0.IndexName]));
				//            GenderRow genderRow = dataIndex0.Find(genderKey1);
				CodeTypeReference dataIndexType = new CodeTypeReference(string.Format("I{0}Index", tableSchema.Name));
				CodeVariableReferenceExpression dataIndexExpression = new CodeRandomVariableReferenceExpression();
				this.Statements.Add(new CodeVariableDeclarationStatement(dataIndexType, dataIndexExpression.VariableName,
					new CodeCastExpression(dataIndexType, new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), tableSchema.Name), "Indices"), indexNameExpression))));
				this.Statements.Add(new CodeCheckIndexExistsStatement(dataIndexExpression, tableSchema, indexNameExpression));
				this.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(string.Format("{0}Row", tableSchema.Name)), createParameterMatrix.RowExpression.VariableName, new CodeMethodInvokeExpression(dataIndexExpression, "Find", createParameterMatrix.UniqueKeyExpression)));

			}

			//			// Create the record if it doesn't exist, update if it does.
			//			if ((objectTreeRow == null))
			//			{
			//				if ((objectTreeId == global::System.Guid.Empty))
			//				{
			//					objectTreeId = global::System.Guid.NewGuid();
			//				}
			//				this.CreateObjectTree(childId, objectTreeId, parentId, out rowVersion);
			//			}
			CodeConditionStatement ifRowIsNew = new CodeConditionStatement(new CodeBinaryOperatorExpression(createParameterMatrix.RowExpression, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)));
			foreach (KeyValuePair<string, InternalParameterItem> internalParameterPair in createParameterMatrix.CreateParameterItems)
			{
				InternalParameterItem internalParameterItem = internalParameterPair.Value;
				if (internalParameterItem.ColumnSchema.DataType == typeof(System.Guid) && internalParameterItem.ColumnSchema.IsOrphan)
				{
					CodeConditionStatement ifGuidNull = internalParameterItem.ColumnSchema.IsNullable || internalParameterItem.ColumnSchema.DefaultValue != DBNull.Value ?
						new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(internalParameterItem.Name), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)), CodeBinaryOperatorType.BooleanAnd,
						new CodeBinaryOperatorExpression(new CodeCastExpression(new CodeGlobalTypeReference(internalParameterItem.ColumnSchema.DataType), new CodeVariableReferenceExpression(internalParameterItem.Name)), CodeBinaryOperatorType.IdentityEquality, new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Guid)), "Empty")))) :
						new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(internalParameterItem.Name), CodeBinaryOperatorType.IdentityEquality, new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Guid)), "Empty")));
					ifGuidNull.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(internalParameterItem.Name), new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Guid)), "NewGuid")));
					ifRowIsNew.TrueStatements.Add(ifGuidNull);
				}
			}
			ifRowIsNew.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), string.Format("Create{0}", tableSchema.Name), createParameterMatrix.CreateParameterExpressions));

			//			else
			//			{
			//				objectTreeRow.AcquireReaderLock(middleTierTransaction.AdoResourceManager.Guid, DataModel.lockTimeout);
			//				middleTierTransaction.AdoResourceManager.AddLock(objectTreeRow);
			//				if ((objectTreeRow.RowState == global::System.Data.DataRowState.Detached))
			//				{
			//					throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a ObjectTree record ({0}) that doesn\'t exist", objectTreeKey2));
			//				}
			//				rowVersion = objectTreeRow.RowVersion;
			//				this.UpdateObjectTree(childId, objectTreeId, parentId, ref rowVersion);
			//			}
			ifRowIsNew.FalseStatements.Add(new CodeAcquireRecordReaderLockExpression(transactionExpression, createParameterMatrix.RowExpression, tableSchema.DataModel));
			ifRowIsNew.FalseStatements.Add(new CodeAddLockToTransactionExpression(transactionExpression, createParameterMatrix.RowExpression));
			ifRowIsNew.FalseStatements.Add(new CodeCheckRecordDetachedStatement(tableSchema, createParameterMatrix.RowExpression, createParameterMatrix.UniqueKeyExpression));
			foreach (KeyValuePair<string, InternalParameterItem> internalParameterPair in createParameterMatrix.CreateParameterItems)
			{
				InternalParameterItem internalParameterItem = internalParameterPair.Value;
				if (internalParameterItem.ColumnSchema.DataType == typeof(System.Guid) && internalParameterItem.ColumnSchema.IsOrphan)
				{
					CodeConditionStatement ifGuidNull = internalParameterItem.ColumnSchema.IsNullable || internalParameterItem.ColumnSchema.DefaultValue != DBNull.Value ?
						new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(internalParameterItem.Name), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)), CodeBinaryOperatorType.BooleanAnd,
						new CodeBinaryOperatorExpression(new CodeCastExpression(new CodeGlobalTypeReference(internalParameterItem.ColumnSchema.DataType), new CodeVariableReferenceExpression(internalParameterItem.Name)), CodeBinaryOperatorType.IdentityEquality, new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Guid)), "Empty")))) :
						new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(internalParameterItem.Name), CodeBinaryOperatorType.IdentityEquality, new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Guid)), "Empty")));
					ifGuidNull.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(internalParameterItem.Name), new CodePropertyReferenceExpression(createParameterMatrix.RowExpression, internalParameterItem.ColumnSchema.Name)));
					ifRowIsNew.FalseStatements.Add(ifGuidNull);
				}
			}
			ifRowIsNew.FalseStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), string.Format("Update{0}", tableSchema.Name), createParameterMatrix.UpdateParameterExpressions));

			//            }
			this.Statements.Add(ifRowIsNew);

		}

	}

}

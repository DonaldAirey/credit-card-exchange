namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a series of statements that resolve variables found in parent tables using external identifiers.
	/// </summary>
	public class CodeResolveExternalVariableStatements : CodeStatementCollection
	{

		/// <summary>
		/// Resolves the variables related to foreign tables.
		/// </summary>
		/// <param name="tableSchema">A description of a table.</param>
		/// <param name="transactionExpression">Used to support locking and provide database resources.</param>
		/// <param name="foreignKeyConstraintParameterItem">A description of a variable related to a foreign table.</param>
		public CodeResolveExternalVariableStatements(TableSchema tableSchema, CodeVariableReferenceExpression transactionExpression, ForeignKeyConstraintParameterItem foreignKeyConstraintParameterItem)
		{

			// This is the foreign key that will be resolved here.
			ForeignKeyConstraintSchema foreignKeyConstraintSchema = foreignKeyConstraintParameterItem.ForeignKeyConstraintSchema;

			//            // This will resolve the optional managerKey foreign key.
			//            object managerId;
			foreach (ForeignKeyVariableItem foreignKeyVariableItem in foreignKeyConstraintParameterItem.ForeignKeyVariables)
				this.Add(new CodeVariableDeclarationStatement(foreignKeyVariableItem.DataType, foreignKeyVariableItem.Expression.VariableName));

			// Optional parameters tied to foreign constraints can be explicitly set to DBNull.Value by passing an empty key. They
			// can implicitly be set to the default by passing null.
			CodeStatementCollection codeStatementCollection = this;
			if (foreignKeyConstraintParameterItem.IsNullable)
			{

				//            if ((managerKey == null)) {
				//                managerId = null;
				//            }

				CodeConditionStatement ifIsNull = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeArgumentReferenceExpression(foreignKeyConstraintParameterItem.Name), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)));
				codeStatementCollection = ifIsNull.FalseStatements;
				foreach (ForeignKeyVariableItem foreignKeyVariableItem in foreignKeyConstraintParameterItem.ForeignKeyVariables)
					ifIsNull.TrueStatements.Add(new CodeAssignStatement(foreignKeyVariableItem.Expression, new CodePrimitiveExpression(null)));

				//            else {
				//                if ((managerKey.Length == 0)) {
				//                    managerId = global::System.DBNull.Value;
				//                }
				CodeConditionStatement ifArrayEmpty = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression(foreignKeyConstraintParameterItem.Name), "Length"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(0)));
				foreach (ForeignKeyVariableItem foreignKeyVariableItem in foreignKeyConstraintParameterItem.ForeignKeyVariables)
					ifArrayEmpty.TrueStatements.Add(new CodeAssignStatement(foreignKeyVariableItem.Expression, new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.DBNull)), "Value")));
				codeStatementCollection = ifArrayEmpty.FalseStatements;
				ifIsNull.FalseStatements.Add(ifArrayEmpty);

				//                else {
				this.Add(ifIsNull);

			}

			//                    object[] configurationKey2 = new object[] {
			//                            configurationId,
			//                            "FK_Manager_Engineer"};
			//                    ConfigurationRow configurationRow3 = DataModel.Configuration.ConfigurationKey.Find(configurationKey2);
			//                    if ((configurationRow3 == null)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Configuration record ({0}) that doesn\'t exist", configurationKey2));
			//                    }
			//                    configurationRow3.AcquireReaderLock(middleTierTransaction.AdoResourceManager.Guid, DataModel.lockTimeout);
			//                    middleTierTransaction.AdoResourceManager.AddLock(configurationRow3);
			//                    if ((configurationRow3.RowState == global::System.Data.DataRowState.Detached)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Configuration record ({0}) that doesn\'t exist", configurationKey2));
			//                    }
			//                    IObjectIndex dataIndex1 = ((IObjectIndex)(DataModel.Object.Indices[configurationRow3.IndexName]));
			//                    ObjectRow objectRow4 = dataIndex1.Find(managerKey);
			//                    if ((objectRow4 == null)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Object record ({0}) that doesn\'t exist", managerKey));
			//                    }
			//                    objectRow4.AcquireReaderLock(middleTierTransaction.AdoResourceManager.Guid, DataModel.lockTimeout);
			//                    middleTierTransaction.AdoResourceManager.AddLock(objectRow4);
			//                    if ((objectRow4.RowState == global::System.Data.DataRowState.Detached)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Object record ({0}) that doesn\'t exist", managerKey));
			//                    }
			//                    // Employee level of the managerKey foreign key search.
			//                    object[] employeeKey3 = new object[] {
			//                            objectRow4.ObjectId};
			//                    EmployeeRow employeeRow5 = DataModel.Employee.EmployeeKey.Find(employeeKey3);
			//                    if ((employeeRow5 == null)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Employee record ({0}) that doesn\'t exist", employeeKey3));
			//                    }
			//                    employeeRow5.AcquireReaderLock(middleTierTransaction.AdoResourceManager.Guid, DataModel.lockTimeout);
			//                    middleTierTransaction.AdoResourceManager.AddLock(employeeRow5);
			//                    if ((employeeRow5.RowState == global::System.Data.DataRowState.Detached)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Employee record ({0}) that doesn\'t exist", employeeKey3));
			//                    }
			//                    // Manager level of the managerKey foreign key search.
			//                    object[] managerKey4 = new object[] {
			//                            employeeRow5.EmployeeId};
			//                    ManagerRow managerRow6 = DataModel.Manager.ManagerKey.Find(managerKey4);
			//                    if ((managerRow6 == null)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Manager record ({0}) that doesn\'t exist", managerKey4));
			//                    }
			//                    managerRow6.AcquireReaderLock(middleTierTransaction.AdoResourceManager.Guid, DataModel.lockTimeout);
			//                    middleTierTransaction.AdoResourceManager.AddLock(managerRow6);
			//                    if ((managerRow6.RowState == global::System.Data.DataRowState.Detached)) {
			//                        throw new global::System.ServiceModel.FaultException<RecordNotFoundFault>(new global::FluidTrade.Core.RecordNotFoundFault("Attempt to access a Manager record ({0}) that doesn\'t exist", managerKey4));
			//                    }
			CodeVariableReferenceExpression rootRowExpression = new CodeRandomVariableReferenceExpression();
			CodeArgumentReferenceExpression rootKeyExpression = new CodeArgumentReferenceExpression(foreignKeyConstraintParameterItem.Name);
			codeStatementCollection.AddRange(new CodeResolveForeignKeyExpression(rootRowExpression, transactionExpression, rootKeyExpression, foreignKeyConstraintSchema));

			//                    managerId = managerRow6.ManagerId;
			//                }
			for (int columnIndex = 0; columnIndex < foreignKeyConstraintSchema.Columns.Length; columnIndex++)
				if (foreignKeyConstraintParameterItem.ForeignKeyVariables[columnIndex] != null)
					codeStatementCollection.Add(new CodeAssignStatement(foreignKeyConstraintParameterItem.ForeignKeyVariables[columnIndex].Expression,
						new CodePropertyReferenceExpression(rootRowExpression, foreignKeyConstraintSchema.RelatedColumns[columnIndex].Name)));

			//            }

		}

	}

}

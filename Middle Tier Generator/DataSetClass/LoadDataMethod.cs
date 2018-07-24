namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Data;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a static constructor for the data model.
	/// </summary>
	class LoadDataMethod : CodeMemberMethod
	{

		/// <summary>
		/// Create a static constructor for the data model.
		/// </summary>
		/// <param name="dataModelSchema">A description of the data model.</param>
		public LoadDataMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Loads data from the persistent store into the data model.
			//		/// </summary>
			//		/// <param name="state">The thread start parameter.</param>
			//		private static void LoadData(object state)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Loads data from the persistent store into the data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"state\">The thread start parameter.</param>", true));
			this.Name = "LoadData";
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object)), "state"));

			//            try {
			CodeTryCatchFinallyStatement tryLoadData = new CodeTryCatchFinallyStatement();

			//				DataModel.dataLock.EnterWriteLock();
			tryLoadData.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "DataLock"),
					"EnterWriteLock"));
	
			//                global::System.Configuration.ConnectionStringSettings connectionStringSettings = global::System.Configuration.ConfigurationManager.ConnectionStrings["DataModel"];
			//                global::System.Data.SqlClient.SqlConnection sqlConnection = new global::System.Data.SqlClient.SqlConnection(connectionStringSettings.ConnectionString);
			//                sqlConnection.Open();
			tryLoadData.TryStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Configuration.ConnectionStringSettings)), "connectionStringSettings", new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Configuration.ConfigurationManager)), "ConnectionStrings"), new CodePrimitiveExpression(dataModelSchema.Name))));
			tryLoadData.TryStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.SqlClient.SqlConnection)), "sqlConnection", new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.SqlClient.SqlConnection)), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("connectionStringSettings"), "ConnectionString"))));
			tryLoadData.TryStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("sqlConnection"), "Open"));

			//                FluidTrade.UnitTest.Server.DataModel.dataSet.EnforceConstraints = false;
			tryLoadData.TryStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name),
							String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
					"EnforceConstraints"), new CodePrimitiveExpression(false)));

			//                for (int tableIndex = 0; (tableIndex < FluidTrade.UnitTest.Server.DataModel.Tables.Count); tableIndex = (tableIndex + 1)) {
			CodeIterationStatement tableLoop1 = new CodeIterationStatement(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "tableIndex", new CodePrimitiveExpression(0)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("tableIndex"), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"), "Count")), new CodeAssignStatement(new CodeVariableReferenceExpression("tableIndex"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("tableIndex"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));

			//                    global::System.Data.DataTable dataTable = FluidTrade.UnitTest.Server.DataModel.Tables[tableIndex];
			tableLoop1.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.DataTable)), "dataTable", new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"), new CodeVariableReferenceExpression("tableIndex"))));

			//                    global::System.Data.DataColumn rowVersionColumn = dataTable.Columns["RowVersion"];
			tableLoop1.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), "rowVersionColumn",
				new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Columns"), new CodePrimitiveExpression("RowVersion"))));

			//                    if (((bool)(dataTable.ExtendedProperties["IsPersistent"]))) {
			CodeConditionStatement ifPersistent = new CodeConditionStatement();
			ifPersistent.Condition = new CodeCastExpression(new CodeGlobalTypeReference(typeof(System.Boolean)), new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "ExtendedProperties"), new CodePrimitiveExpression("IsPersistent")));

			//                        string columnList = "\"IsArchived\", \"IsDeleted\"";
			//                        for (int columnIndex = 0; (columnIndex < dataTable.Columns.Count); columnIndex = (columnIndex + 1)) {
			//                            global::System.Data.DataColumn dataColumn = dataTable.Columns[columnIndex];
			//                            if (((bool)(dataColumn.ExtendedProperties["IsPersistent"]))) {
			//                                columnList = (columnList 
			//                                            + (",\"" 
			//                                            + (dataColumn.ColumnName + "\"")));
			//                            }
			//                        }
			ifPersistent.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(String)), "columnList", new CodePrimitiveExpression("\"IsArchived\", \"IsDeleted\"")));
			CodeIterationStatement columnLoop1 = new CodeIterationStatement(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "columnIndex", new CodePrimitiveExpression(0)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("columnIndex"), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Columns"), "Count")), new CodeAssignStatement(new CodeVariableReferenceExpression("columnIndex"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("columnIndex"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
			columnLoop1.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), "dataColumn", new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Columns"), new CodeVariableReferenceExpression("columnIndex"))));
			columnLoop1.Statements.Add(new CodeConditionStatement(new CodeCastExpression(new CodeGlobalTypeReference(typeof(System.Boolean)), new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataColumn"), "ExtendedProperties"), new CodePrimitiveExpression("IsPersistent"))),
				new CodeAssignStatement(new CodeVariableReferenceExpression("columnList"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("columnList"), CodeBinaryOperatorType.Add, new CodeBinaryOperatorExpression(new CodePrimitiveExpression(",\""), CodeBinaryOperatorType.Add, new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataColumn"), "ColumnName"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression("\"")))))));
			ifPersistent.TrueStatements.Add(columnLoop1);

			//                        string selectCommand = string.Format("select {0} from \"{1}\"", columnList, dataTable.TableName);
			//                        System.Data.SqlClient.SqlCommand sqlCommand = new System.Data.SqlClient.SqlCommand(selectCommand);
			//                        sqlCommand.Connection = sqlConnection;
			//                        System.Data.SqlClient.SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			ifPersistent.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(String)), "selectCommand", new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(String)), "Format", new CodePrimitiveExpression("select {0} from \"{1}\""), new CodeVariableReferenceExpression("columnList"), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "TableName"))));
			ifPersistent.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.SqlClient.SqlCommand)), "sqlCommand", new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.SqlClient.SqlCommand)), new CodeVariableReferenceExpression("selectCommand"))));
			ifPersistent.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sqlCommand"), "Connection"), new CodeVariableReferenceExpression("sqlConnection")));
			ifPersistent.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.SqlClient.SqlDataReader)), "sqlDataReader", new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("sqlCommand"), "ExecuteReader")));

			//                        for (
			//                        ; (sqlDataReader.Read() == true); 
			//                        ) {
			CodeIterationStatement readLoop = new CodeIterationStatement(new CodeSnippetStatement(""), new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("sqlDataReader"), "Read"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(true)), new CodeSnippetStatement(""));

			//                            if (((((bool)(sqlDataReader["IsArchived"])) == true) 
			//                                        || (((bool)(sqlDataReader["IsDeleted"])) == true))) {
			//                                long rowVersion = ((long)(sqlDataReader["RowVersion"]));
			//                                if ((rowVersion > FluidTrade.UnitTest.Server.DataModel.masterRowVersion)) {
			//                                    FluidTrade.UnitTest.Server.DataModel.masterRowVersion = rowVersion;
			//                                }
			//                            }
			CodeConditionStatement activeRecords = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeBinaryOperatorExpression(new CodeCastExpression(typeof(System.Boolean), new CodeIndexerExpression(new CodeVariableReferenceExpression("sqlDataReader"), new CodePrimitiveExpression("IsArchived"))), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(true)), CodeBinaryOperatorType.BooleanOr, new CodeBinaryOperatorExpression(new CodeCastExpression(typeof(System.Boolean), new CodeIndexerExpression(new CodeVariableReferenceExpression("sqlDataReader"), new CodePrimitiveExpression("IsDeleted"))), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(true))));
			activeRecords.TrueStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int64)), "rowVersion", new CodeCastExpression(typeof(System.Int64), new CodeIndexerExpression(new CodeVariableReferenceExpression("sqlDataReader"), new CodePrimitiveExpression("RowVersion")))));
			CodeConditionStatement ifRowVersionUpdate = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeVariableReferenceExpression("rowVersion"),
					CodeBinaryOperatorType.GreaterThan,
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "rowVersion")));
			ifRowVersionUpdate.TrueStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "rowVersion"),
					new CodeVariableReferenceExpression("rowVersion")));
			activeRecords.TrueStatements.Add(ifRowVersionUpdate);

			//                            else {
			//                                global::System.Data.DataRow dataRow = dataTable.NewRow();
			activeRecords.FalseStatements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.DataRow)), "dataRow", new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("dataTable"), "NewRow")));

			//                                for (int ordinal = 0; (ordinal < sqlDataReader.FieldCount); ordinal = (ordinal + 1)) {
			//                                    global::System.Data.DataColumn destinationColumn = dataTable.Columns[sqlDataReader.GetName(ordinal)];
			//                                    if ((destinationColumn != null)) {
			//                                        dataRow[destinationColumn] = sqlDataReader.GetValue(ordinal);
			//                                    }
			//                                }
			CodeIterationStatement ordinalLoop = new CodeIterationStatement(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "ordinal", new CodePrimitiveExpression(0)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("ordinal"), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sqlDataReader"), "FieldCount")), new CodeAssignStatement(new CodeVariableReferenceExpression("ordinal"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("ordinal"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
			ordinalLoop.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), "destinationColumn", new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Columns"), new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("sqlDataReader"), "GetName", new CodeVariableReferenceExpression("ordinal")))));
			ordinalLoop.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("destinationColumn"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
				new CodeAssignStatement(new CodeIndexerExpression(new CodeVariableReferenceExpression("dataRow"), new CodeVariableReferenceExpression("destinationColumn")), new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("sqlDataReader"), "GetValue", new CodeVariableReferenceExpression("ordinal")))));
			activeRecords.FalseStatements.Add(ordinalLoop);

			//                                dataTable.Rows.Add(dataRow);
			//                                if ((((long)(dataRow["RowVersion"])) > FluidTrade.UnitTest.Server.DataModel.masterRowVersion)) {
			//                                    FluidTrade.UnitTest.Server.DataModel.masterRowVersion = ((long)(dataRow["RowVersion"]));
			//                                }
			activeRecords.FalseStatements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Rows"), "Add", new CodeVariableReferenceExpression("dataRow")));
			CodeConditionStatement ifBiggerRowVersion = new CodeConditionStatement(
				new CodeBinaryOperatorExpression(
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int64)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("dataRow"), new CodePrimitiveExpression("RowVersion"))),
					CodeBinaryOperatorType.GreaterThan,
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "rowVersion")));
			ifBiggerRowVersion.TrueStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "rowVersion"),
					new CodeCastExpression(
						new CodeGlobalTypeReference(typeof(Int64)),
						new CodeIndexerExpression(new CodeVariableReferenceExpression("dataRow"), new CodePrimitiveExpression("RowVersion")))));
			activeRecords.FalseStatements.Add(ifBiggerRowVersion);

			//                            }
			readLoop.Statements.Add(activeRecords);

			//                        }
			//                        sqlDataReader.Close();
			ifPersistent.TrueStatements.Add(readLoop);
			ifPersistent.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("sqlDataReader"), "Close"));

			//                    }
			tableLoop1.Statements.Add(ifPersistent);

			//                }
			tryLoadData.TryStatements.Add(tableLoop1);

			//                FluidTrade.UnitTest.Server.DataModel.dataSet.EnforceConstraints = true;
			tryLoadData.TryStatements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(
						new CodePropertyReferenceExpression(
							new CodeTypeReferenceExpression(dataModelSchema.Name),
							String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
						"EnforceConstraints"),
					new CodePrimitiveExpression(true)));

			//				global::System.Collections.Generic.List<object> transactionLogItem = new global::System.Collections.Generic.List<object>();
			tryLoadData.TryStatements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(List<Object>)),
					"transactionLogItem",
					new CodeObjectCreateExpression(
						new CodeGlobalTypeReference(typeof(List<Object>)))));

			//				for (int tableIndex = 0; (tableIndex < DataModel.Tables.Count); tableIndex = (tableIndex + 1))
			//				{
			CodeIterationStatement tableLoop = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "tableIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("tableIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"), "Count")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("tableIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("tableIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//					global::System.Data.DataTable dataTable = DataModel.dataSet.Tables[tableIndex];
			tableLoop.Statements.Add(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(DataTable)),
					"dataTable",
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(
								new CodeTypeReferenceExpression(dataModelSchema.Name),
								String.Format("{0}DataSet", CommonConversion.ToCamelCase(dataModelSchema.Name))),
							"Tables"),
						new CodeVariableReferenceExpression("tableIndex"))));

			//					for (int rowIndex = 0; (rowIndex < dataTable.Rows.Count); rowIndex = (rowIndex + 1))
			//					{
			CodeIterationStatement rowLoop = new CodeIterationStatement(
				new CodeVariableDeclarationStatement(
					new CodeGlobalTypeReference(typeof(System.Int32)), "rowIndex", new CodePrimitiveExpression(0)),
					new CodeBinaryOperatorExpression(
						new CodeVariableReferenceExpression("rowIndex"),
						CodeBinaryOperatorType.LessThan,
						new CodePropertyReferenceExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Rows"),
							"Count")),
					new CodeAssignStatement(
						new CodeVariableReferenceExpression("rowIndex"),
						new CodeBinaryOperatorExpression(
							new CodeVariableReferenceExpression("rowIndex"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression(1))));

			//						DataModel.Initialize(((global::FluidTrade.Core.IRow)(dataTable.Rows[rowIndex])), transactionLogItem);
			rowLoop.Statements.Add(
				new CodeMethodInvokeExpression(
					new CodeThisReferenceExpression(),
					"SequenceRecord",
					new CodeCastExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)),
						new CodeIndexerExpression(
							new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Rows"),
							new CodeVariableReferenceExpression("rowIndex"))),
					new CodeVariableReferenceExpression("transactionLogItem")));

			//					}
			tableLoop.Statements.Add(rowLoop);

			//				}
			tryLoadData.TryStatements.Add(tableLoop);

			//            }

			//            catch (global::System.Data.ConstraintException constraintException) {
			//                for (int tableIndex = 0; (tableIndex < FluidTrade.UnitTest.Server.DataModel.Tables.Count); tableIndex = (tableIndex + 1)) {
			//                    global::System.Data.DataTable dataTable = FluidTrade.UnitTest.Server.DataModel.Tables[tableIndex];
			//                    for (int rowIndex = 0; (rowIndex < dataTable.Rows.Count); rowIndex = (rowIndex + 1)) {
			//                        global::System.Data.DataRow dataRow = dataTable.Rows[rowIndex];
			//                        if ((dataRow.HasErrors == true)) {
			//                            EventLog.Error("Error in \'{0}\': {1}", dataRow.Table.TableName, dataRow.RowError);
			//                        }
			//                    }
			//                }
			//                throw constraintException;
			//            }
			CodeCatchClause catchConstraint = new CodeCatchClause("constraintException", new CodeGlobalTypeReference(typeof(System.Data.ConstraintException)));
			CodeIterationStatement constraintTableLoop = new CodeIterationStatement(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "tableIndex", new CodePrimitiveExpression(0)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("tableIndex"), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"), "Count")), new CodeAssignStatement(new CodeVariableReferenceExpression("tableIndex"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("tableIndex"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
			constraintTableLoop.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.DataTable)), "dataTable", new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "Tables"), new CodeVariableReferenceExpression("tableIndex"))));
			CodeIterationStatement constraintRowLoop = new CodeIterationStatement(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "rowIndex", new CodePrimitiveExpression(0)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("rowIndex"), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Rows"), "Count")), new CodeAssignStatement(new CodeVariableReferenceExpression("rowIndex"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("rowIndex"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
			constraintRowLoop.Statements.Add(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Data.DataRow)), "dataRow", new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataTable"), "Rows"), new CodeVariableReferenceExpression("rowIndex"))));
			constraintRowLoop.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataRow"), "HasErrors"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(true)),
				new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(EventLog)), "Error", new CodePrimitiveExpression("Error in '{0}': {1}"), new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataRow"), "Table"), "TableName"), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataRow"), "RowError")))));
			constraintTableLoop.Statements.Add(constraintRowLoop);
			catchConstraint.Statements.Add(constraintTableLoop);
			catchConstraint.Statements.Add(new CodeThrowExceptionStatement(new CodeVariableReferenceExpression("constraintException")));
			tryLoadData.CatchClauses.Add(catchConstraint);

			//            catch (global::System.Data.SqlClient.SqlException sqlException) {
			//                for (int errorIndex = 0; (errorIndex < sqlException.Errors.Count); errorIndex = (errorIndex + 1)) {
			//                    EventLog.Error(sqlException.Errors[errorIndex].Message);
			//                }
			//                throw sqlException;
			//            }
			CodeCatchClause catchSqlException = new CodeCatchClause("sqlException", new CodeGlobalTypeReference(typeof(System.Data.SqlClient.SqlException)));
			CodeIterationStatement sqlErrorLoop = new CodeIterationStatement(new CodeVariableDeclarationStatement(new CodeGlobalTypeReference(typeof(System.Int32)), "errorIndex", new CodePrimitiveExpression(0)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("errorIndex"), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sqlException"), "Errors"), "Count")), new CodeAssignStatement(new CodeVariableReferenceExpression("errorIndex"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("errorIndex"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
			sqlErrorLoop.Statements.Add(new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(EventLog)), "Error", new CodePropertyReferenceExpression(new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("sqlException"), "Errors"), new CodeVariableReferenceExpression("errorIndex")), "Message")));
			catchSqlException.Statements.Add(sqlErrorLoop);
			catchSqlException.Statements.Add(new CodeThrowExceptionStatement(new CodeVariableReferenceExpression("sqlException")));
			tryLoadData.CatchClauses.Add(catchSqlException);
			this.Statements.Add(tryLoadData);

			//			}
			//			finally
			//			{
			//				DataModel.dataLock.ExitWriteLock();
			//			}
			tryLoadData.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "DataLock"),
					"ExitWriteLock"));

			//        }

		}

	}

}

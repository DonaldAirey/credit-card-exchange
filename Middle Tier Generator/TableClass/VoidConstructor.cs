namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

	/// <summary>
	/// Represents a declaration of a property that gets the parent row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class VoidConstructor : CodeConstructor
	{

		/// <summary>
		/// Generates a property to get a parent row.
		/// </summary>
		/// <param name="foreignKeyConstraintSchema">The foreign key that references the parent table.</param>
		public VoidConstructor(TableSchema tableSchema)
		{

			// Construct the type names for the table and rows within the table.
			string rowTypeName = string.Format("{0}Row", tableSchema.Name);

			//            /// <summary>
			//            /// Creates the Engineer table.
			//            /// </summary>
			//            internal EngineerDataTable() {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates the {0} table.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Assembly;

			//			this.TableName = "Account";
			this.Statements.Add(
				new CodeAssignStatement(
					new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "TableName"),
					new CodePrimitiveExpression(tableSchema.Name)));

			//			this.ExtendedProperties["IsPersistent"] = true;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeIndexerExpression(
						new CodePropertyReferenceExpression(new CodeThisReferenceExpression(),
							"ExtendedProperties"),
						new CodePrimitiveExpression("IsPersistent")),
					new CodePrimitiveExpression(tableSchema.IsPersistent)));

			//			this.filterRowHandler = this.FilterRow;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "filterRowHandler"),
					new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "FilterRow")));

			//			this.getContainerHandler = this.GetContainer;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "getContainerHandler"),
					new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetContainer")));

			//                this.columnEngineerId = new global::System.Data.DataColumn("EngineerId", typeof(int), null, global::System.Data.MappingType.Element);
			//                this.columnEngineerId.ExtendedProperties["IsPersistent"] = true;
			//                this.columnEngineerId.AllowDBNull = false;
			//                this.Columns.Add(this.columnEngineerId);
			//                this.columnManagerId = new global::System.Data.DataColumn("ManagerId", typeof(int), null, global::System.Data.MappingType.Element);
			//                this.columnManagerId.ExtendedProperties["IsPersistent"] = true;
			//                this.columnManagerId.DefaultValue = global::System.DBNull.Value;
			//                this.Columns.Add(this.columnManagerId);
			//                this.columnRowVersion = new global::System.Data.DataColumn("RowVersion", typeof(long), null, global::System.Data.MappingType.Element);
			//                this.columnRowVersion.ExtendedProperties["IsPersistent"] = true;
			//                this.columnRowVersion.AllowDBNull = false;
			//                this.Columns.Add(this.columnRowVersion);
			foreach (ColumnSchema columnSchema in tableSchema.Columns.Values)
			{

				// Create the column using the datatype specified in the schema.
				//                this.columnDepartmentId = new global::System.Data.DataColumn("DepartmentId", typeof(int), null, global::System.Data.MappingType.Element);
				CodeExpression right = new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), new CodePrimitiveExpression(columnSchema.Name), new CodeTypeOfExpression(columnSchema.DataType), new CodePrimitiveExpression(null),
					new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Data.MappingType)), "Element"));
				this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)), right));

				//                this.columnColumnIndex.ExtendedProperties["IsPersistent"] = true;
				this.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)), "ExtendedProperties"), new CodePrimitiveExpression("IsPersistent")), new CodePrimitiveExpression(columnSchema.IsPersistent)));

				// AutoIncrement, AutoIncrementSeed and AutoIncrementStep Properties.
				if (columnSchema.IsAutoIncrement)
				{
					this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)), "AutoIncrement"), new CodePrimitiveExpression(true)));
					if (columnSchema.AutoIncrementSeed > 0)
						this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)), "AutoIncrementSeed"), new CodePrimitiveExpression(columnSchema.AutoIncrementSeed)));
					if (columnSchema.AutoIncrementStep > 1)
						this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)), "AutoIncrementStep"), new CodePrimitiveExpression(columnSchema.AutoIncrementStep)));
				}

				// AllowDBNull Column property
				if (!columnSchema.IsNullable)
					this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)), "AllowDBNull"), new CodePrimitiveExpression(false)));

				// The default value exists as a string in the Xml Schema.  It must be stronly typed before being inserted into
				// the target code. Unfortunately, the type information for the destination column is needed to convert the
				// value properly.
				if (columnSchema.DefaultValue != DBNull.Value)
					this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)), "DefaultValue"), CodeConvert.CreateConstantExpression(columnSchema.DefaultValue)));

				// This will add the column created above to the table.
				this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Columns"), "Add", new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name))));

			}

			//                // The EngineerKey Index
			//                this.indexEngineerKey = new EngineerKeyIndex(new global::System.Data.DataColumn[] {
			//                            this.columnEngineerId});
			foreach (KeyValuePair<string, ConstraintSchema> constraintPair in tableSchema.Constraints)
				if (constraintPair.Value is UniqueConstraintSchema)
				{
					UniqueConstraintSchema uniqueConstraintSchema = constraintPair.Value as UniqueConstraintSchema;
					string indexVariableName = string.Format("index{0}", uniqueConstraintSchema.Name);
					List<CodeExpression> keyColumns = new List<CodeExpression>();
					foreach (ColumnSchema columnSchema in uniqueConstraintSchema.Columns)
						keyColumns.Add(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name)));
					this.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), indexVariableName),
						new CodeObjectCreateExpression(new CodeTypeReference(string.Format("{0}Index", uniqueConstraintSchema.Name)), new CodePrimitiveExpression(uniqueConstraintSchema.Name), new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), keyColumns.ToArray()))));
					if (!uniqueConstraintSchema.IsPrimaryKey && !uniqueConstraintSchema.IsNullable)
					{
						this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Constraints"), "Add",
							new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.UniqueConstraint)), new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.DataColumn)), keyColumns.ToArray()))));
					}
					this.Statements.Add(new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Indices"), "Add", new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), indexVariableName)));
				}

			//            }

		}

	}

}

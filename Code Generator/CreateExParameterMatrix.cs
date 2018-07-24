namespace FluidTrade.Core
{

	using System;
	using System.CodeDom;
	using System.Collections.Generic;

    /// <summary>
	/// Translates the parameters used for external methods to parameters for internal methods.
	/// </summary>
	public class CreateExParameterMatrix
	{

		// Public Instance Properties
		public System.CodeDom.CodeVariableReferenceExpression RowExpression;
		public System.CodeDom.CodeVariableReferenceExpression UniqueKeyExpression;
		public System.Collections.Generic.SortedList<string, ExternalParameterItem> ExternalParameterItems;
		public System.Collections.Generic.SortedList<string, InternalParameterItem> CreateParameterItems;
		public System.Collections.Generic.SortedList<string, InternalParameterItem> UpdateParameterItems;

		/// <summary>
		/// Creates an object that translates the parameters used for external methods to parameters for internal methods.
		/// </summary>
		public CreateExParameterMatrix(TableSchema tableSchema)
		{

			// Initialize the object.
			this.ExternalParameterItems = new SortedList<string, ExternalParameterItem>();
			this.CreateParameterItems = new SortedList<string, InternalParameterItem>();
			this.UpdateParameterItems = new SortedList<string, InternalParameterItem>();

			// Place all the columns of the table in a list that will drive the creation of parameters.  Those that columns that
			// are native to this table will be left in the list.  Those columns that are part of a foreign constraint will be
			// replace in the final argument list with internal values that can be used to look up the actual value.
			List<ColumnSchema> columnList = new List<ColumnSchema>();
			foreach (KeyValuePair<string, ColumnSchema> keyValuePair in tableSchema.Columns)
				columnList.Add(keyValuePair.Value);

			// The 'CreateEx' method doesn't have an explicit unique key in the parameter list like the 'Update' and 'Delete', so
			// there's no explicit input parameter to which to associate the unique row that is the target of the operation.  This 
			// creates an implicit unique index which will be resolved using the explicit input parameters to this method.
			this.RowExpression = new CodeRandomVariableReferenceExpression();
			this.UniqueKeyExpression = new CodeRandomVariableReferenceExpression();

			// Every internal update method requires a primary key specification.  This key is created from the primary key columns
			// of the existing row.  While it may seem redundant to find a row before calling the internal method to update that
			// same row, one of the primary purposes of the external method is to short-circuit the optimistic concurrency
			// checking.  To do that, the row version from the existing row is required.  The same row that provides the current
			// row version also provides the unique key to call the internal method.  However, Any one of the unique constraints
			// associated with the table can be used from the external interface to look up the row before the internal method is
			// called.  This makes it possible to change the primary key of a given column which is a requirement of a true
			// relational database model.  Said differently, this parameter item acts like the 'WHERE' clause in an SQL statement.
			InternalParameterItem primaryKeyParameterItem = new InternalParameterItem();
			List<CodeExpression> primaryKeyExpressions = new List<CodeExpression>();
			foreach (ColumnSchema columnSchema in tableSchema.PrimaryKey.Columns)
				primaryKeyExpressions.Add(new CodePropertyReferenceExpression(this.RowExpression, columnSchema.Name));
			primaryKeyParameterItem.Expression = new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(System.Object)), primaryKeyExpressions.ToArray());
			primaryKeyParameterItem.Name = string.Format("{0}Key", CommonConversion.ToCamelCase(tableSchema.Name));
			this.UpdateParameterItems.Add(primaryKeyParameterItem.Name, primaryKeyParameterItem);

			// This will make sure that all the constraints on a table are satisfied by the list of parameters.
			foreach (ForeignKeyConstraintSchema foreignKeyConstraintSchema in tableSchema.ForeignKeyConstraintSchemas)
			{

				// No attempt will be made to resolve complex, redundant constraints as the same data can be found from simpler
				// ones.  The presence of the redundant, complex foreign keys will only confuse the interface.
				if (foreignKeyConstraintSchema.IsRedundant)
					continue;

				// This item describes an input parameter that uses one or more external identifiers as a keys to parent tables.  
				// Those parent tables provide the actual values that are used to update the record.  This allows an external
				// interface to use external identifiers from related tables.
				ForeignKeyConstraintParameterItem foreignKeyConstraintParameterItem = new ForeignKeyConstraintParameterItem();

				// This is the foreign constraint that is to be resolved with this parameter.
				foreignKeyConstraintParameterItem.ForeignKeyConstraintSchema = foreignKeyConstraintSchema;
				foreignKeyConstraintParameterItem.ActualDataType = typeof(System.Object[]);
				foreignKeyConstraintParameterItem.DeclaredDataType = typeof(System.Object[]);
				foreignKeyConstraintParameterItem.Description = foreignKeyConstraintParameterItem.IsNullable ?
					string.Format("An optional unique key for the parent {0} record.", foreignKeyConstraintSchema.RelatedTable) :
					string.Format("A required unique key for the parent {0} record.", foreignKeyConstraintSchema.RelatedTable);
				foreignKeyConstraintParameterItem.FieldDirection = FieldDirection.In;
				foreignKeyConstraintParameterItem.Name = string.Format("{0}Key", CommonConversion.ToCamelCase(foreignKeyConstraintSchema.RelatedTable.Name));
				if (!foreignKeyConstraintSchema.IsDistinctPathToParent)
				{
					foreignKeyConstraintParameterItem.Name += "By";
					foreach (ColumnSchema columnSchema in foreignKeyConstraintSchema.Columns)
						foreignKeyConstraintParameterItem.Name += columnSchema.Name;
				}
				foreignKeyConstraintParameterItem.IsNullable = true;
				foreach (ColumnSchema columnSchema in foreignKeyConstraintSchema.Columns)
					if (!columnSchema.IsNullable)
						foreignKeyConstraintParameterItem.IsNullable = false;
				this.ExternalParameterItems.Add(foreignKeyConstraintParameterItem.Name, foreignKeyConstraintParameterItem);

				// This constructs a mapping between the foreign key parameter that is provided to the external method and the 
				// individual parameters that are needed by the internal method.
				foreach (ColumnSchema columnSchema in foreignKeyConstraintSchema.Columns)
				{

					// This creates an intermediate variable to hold a value collected from the foreign keys.  When creating an
					// object, the required values are strongly typed.  Optional columns and columns with defaults are defined as
					// generic types so they can be nulled if no cooresponding value is provided.
					ForeignKeyVariableItem foreignKeyVariableItem = new ForeignKeyVariableItem();
					foreignKeyVariableItem.ColumnSchema = columnSchema;
					foreignKeyVariableItem.DataType = columnSchema.IsNullable || columnSchema.DefaultValue != DBNull.Value ? typeof(System.Object) : columnSchema.DataType;
					foreignKeyVariableItem.Expression = new CodeRandomVariableReferenceExpression();
					foreignKeyConstraintParameterItem.ForeignKeyVariables.Add(foreignKeyVariableItem);

					// This creates an internal parameter for the 'Create' method from the elements of the foreign key.
					InternalParameterItem createParameterItem = new InternalParameterItem();
					createParameterItem.ColumnSchema = columnSchema;
					createParameterItem.Expression = foreignKeyVariableItem.Expression;
					createParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.CreateParameterItems.Add(createParameterItem.Name, createParameterItem);

					// This creates an internal parameter for the 'Update' method from the elements of the foreign key.
					InternalParameterItem updateParameterItem = new InternalParameterItem();
					updateParameterItem.ColumnSchema = columnSchema;
					updateParameterItem.Expression = foreignKeyVariableItem.Expression;
					updateParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.UpdateParameterItems.Add(updateParameterItem.Name, updateParameterItem);

					// Any items resolved using foreign constraints are removed from the list of input parameters.  The key is the
					// means of evaluating the column's value and it can't be added directly from the outside world.
					columnList.Remove(columnSchema);

				}

			}

			// At this point, all the parameters that need to be resolved with foreign keys have been added to the list of external
			// and internal parameters and their related columns have been removed from the list of input parameters.  This next 
			// block will add the remaining items to the list of external and internal parameters.
			foreach (ColumnSchema columnSchema in columnList)
			{

				// If a column requires special processing, it is not handled with the rest of the parameters.
				bool isOrdinaryColumn = true;

				// Since the external world doesn't have access to the row versions, they must be removed from the input
				// parameters.  A mechanism is constructed internal to the method to defeat the optimistic concurrency checking.
				if (columnSchema.IsRowVersion)
				{

					// This is no ordinary column.
					isOrdinaryColumn = false;

					// This parameters is used for the internal 'Update' methods to defeat the optimistic concurrency checking.  It is not
					// part of the 'Create' interface.
					InternalParameterItem internalParameterItem = new InternalParameterItem();
					internalParameterItem.ColumnSchema = columnSchema;
					internalParameterItem.Expression = new CodePropertyReferenceExpression(this.RowExpression, "RowVersion");
					internalParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.UpdateParameterItems.Add(internalParameterItem.Name, internalParameterItem);

				}

				// AutoIncremented columns can only be specified as output parameters.
				if (columnSchema.IsAutoIncrement)
				{

					// This is no ordinary column.
					isOrdinaryColumn = false;

					// Create an outpt parameter for the AutoIncremented values as the values are always generated by the middle
					// tier.
					SimpleParameterItem simpleParameterItem = new SimpleParameterItem();
					simpleParameterItem.ActualDataType = columnSchema.DataType;
					simpleParameterItem.ColumnSchema = columnSchema;
					simpleParameterItem.DeclaredDataType = columnSchema.DataType;
					simpleParameterItem.Description = string.Format("The output value for the {0} column.", columnSchema.Name);
					simpleParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.ExternalParameterItems.Add(simpleParameterItem.Name, simpleParameterItem);

					// Note that when an object with an AutoIncrement column is added, the column is specified as an output
					// parameter.
					InternalParameterItem createParameterItem = new InternalParameterItem();
					createParameterItem.ColumnSchema = columnSchema;
					createParameterItem.Expression = new CodeDirectionExpression(FieldDirection.Out, new CodeArgumentReferenceExpression(simpleParameterItem.Name));
					createParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.CreateParameterItems.Add(createParameterItem.Name, createParameterItem);

					// The AutoIncrement column is a simple input parameter when the record is updated.
					InternalParameterItem updateParameterItem = new InternalParameterItem();
					updateParameterItem.ColumnSchema = columnSchema;
					updateParameterItem.Expression = new CodeArgumentReferenceExpression(simpleParameterItem.Name);
					updateParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.UpdateParameterItems.Add(updateParameterItem.Name, updateParameterItem);

				}

				// Ordinary parameters are passed from the external caller to the internal methods without modification or
				// interpretation.
				if (isOrdinaryColumn)
				{

					// Create a simple input parameter from the column information.  The only complication is whether the value
					// must be provided by the caller or is optional.
					SimpleParameterItem simpleParameterItem = new SimpleParameterItem();
					bool isOptional = columnSchema.IsNullable || columnSchema.DefaultValue != DBNull.Value;
					simpleParameterItem.ActualDataType = columnSchema.DataType;
					simpleParameterItem.ColumnSchema = columnSchema;
					simpleParameterItem.DeclaredDataType = isOptional ? typeof(System.Object) : columnSchema.DataType;
					simpleParameterItem.Description = string.Format("The {0} value for the {1} column.", isOptional ? "optional" : "required", CommonConversion.ToCamelCase(columnSchema.Name));
					simpleParameterItem.FieldDirection = FieldDirection.In;
					simpleParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.ExternalParameterItems.Add(simpleParameterItem.Name, simpleParameterItem);

					// This is how the parameter will be interpreted by the 'Insert' and 'Update' internal methods.
					InternalParameterItem internalParameterItem = new InternalParameterItem();
					internalParameterItem.ColumnSchema = columnSchema;
					internalParameterItem.Expression = new CodeArgumentReferenceExpression(CommonConversion.ToCamelCase(columnSchema.Name));
					internalParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.CreateParameterItems.Add(internalParameterItem.Name, internalParameterItem);
					this.UpdateParameterItems.Add(internalParameterItem.Name, internalParameterItem);

				}

			}

			// A configuration identifier is is required on all interface methods except the 'Configuration' table.  While not
			// every method requires the resolution of multiple external keys, the decision to add this for column was made for
			// consistency.  The idea that the external interface shouldn't break if a new unique key is added to the hierarchy.
			// This means more work up front to configure the external interfaces, but it means less work later on as the relations
			// and schema change.
			if (tableSchema.Name != "Configuration")
			{
				ExternalParameterItem externalParameterItem = new ExternalParameterItem();
				externalParameterItem.ActualDataType = typeof(String);
				externalParameterItem.DeclaredDataType = typeof(String);
				externalParameterItem.Description = "Selects a configuration of unique indices used to resolve external identifiers.";
				externalParameterItem.FieldDirection = FieldDirection.In;
				externalParameterItem.IsNullable = false;
				externalParameterItem.Name = "configurationId";
				this.ExternalParameterItems.Add(externalParameterItem.Name, externalParameterItem);
			}

		}

		/// <summary>
		/// The list of parameters required to call the internal Create method.
		/// </summary>
		public CodeExpression[] CreateParameterExpressions
		{

			get
			{

				// This list collects the expressions for the parameters as they are evaluated from the matrix.
				List<CodeExpression> createParameterList = new List<CodeExpression>();
				foreach (KeyValuePair<string, InternalParameterItem> internalParameterItemPair in this.CreateParameterItems)
					createParameterList.Add(internalParameterItemPair.Value.Expression);
				return createParameterList.ToArray();

			}

		}

		/// <summary>
		/// The list of parameters required to call the internal Create method.
		/// </summary>
		public CodeExpression[] UpdateParameterExpressions
		{

			get
			{

				// This list collects the expressions for the parameters as they are evaluated from the matrix.
				List<CodeExpression> updateParameterList = new List<CodeExpression>();
				foreach (KeyValuePair<string, InternalParameterItem> internalParameterItemPair in this.UpdateParameterItems)
					updateParameterList.Add(internalParameterItemPair.Value.Expression);
				return updateParameterList.ToArray();

			}

		}

		/// <summary>
		/// Creates an array of values from a unique constraint that can be used for finding records.
		/// </summary>
		/// <param name="uniqueConstraintSchema">A description of a unique constraint.</param>
		/// <returns>An array of expressions that can be used as a key for finding records in a table.</returns>
		public CodeExpression[] CreateKey(UniqueConstraintSchema uniqueConstraintSchema)
		{

			// This will cycle through all the foreign and simple parameters looking for any columns that match up to the child
			// columns of the constraint.  When found, they are placed in the array in the proper order to match up against the
			// given unique constraint.
			List<CodeExpression> keys = new List<CodeExpression>();
			foreach (ColumnSchema uniqueColumn in uniqueConstraintSchema.Columns)
				foreach (KeyValuePair<string, ExternalParameterItem> parameterPair in this.ExternalParameterItems)
				{

					// This correlates the unique constraint columns with the variables that have been created in the method to
					// hold the key values from the foreign tables.  These variables exist outside of the conditional logic that
					// finds the parent row, so if the parent key is null, these values will also be null.  However, since they're
					// still declared, they can be used to construct keys, which is useful when they're optional values.
					if (parameterPair.Value is ForeignKeyConstraintParameterItem)
					{
						ForeignKeyConstraintParameterItem foreignKeyConstraintParameterItem = parameterPair.Value as ForeignKeyConstraintParameterItem;
						ForeignKeyConstraintSchema foreignKeyConstraintSchema = foreignKeyConstraintParameterItem.ForeignKeyConstraintSchema;
						for (int columnIndex = 0; columnIndex < foreignKeyConstraintSchema.Columns.Length; columnIndex++)
							if (uniqueColumn == foreignKeyConstraintSchema.Columns[columnIndex])
								foreach (ForeignKeyVariableItem foreignKeyVariableItem in foreignKeyConstraintParameterItem.ForeignKeyVariables)
									if (foreignKeyConstraintSchema.Columns[columnIndex] == foreignKeyVariableItem.ColumnSchema)
										keys.Add(foreignKeyVariableItem.Expression);
					}

					// This will match the columns described in the simple parameters to the columns in the unique constraint.
					if (parameterPair.Value is SimpleParameterItem)
					{
						SimpleParameterItem simpleParameterItem = parameterPair.Value as SimpleParameterItem;
						if (uniqueColumn == simpleParameterItem.ColumnSchema)
							keys.Add(new CodeVariableReferenceExpression(CommonConversion.ToCamelCase(simpleParameterItem.ColumnSchema.Name)));
					}

				}

			// This array can be used as a key to find the record in a table.
			return keys.ToArray();

		}

	}

}

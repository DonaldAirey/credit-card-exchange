namespace FluidTrade.Core
{

	using System;
	using System.CodeDom;
	using System.Collections.Generic;

    /// <summary>
	/// Translates the parameters used for external methods to parameters for internal methods.
	/// </summary>
	public class UpdateExParameterMatrix
	{

		// Public Instance Properties
		public System.Collections.Generic.SortedList<string, ExternalParameterItem> ExternalParameterItems;
		public System.Collections.Generic.SortedList<string, InternalParameterItem> InternalParameters;

		/// <summary>
		/// Creates an object that translates the parameters used for external methods to parameters for internal methods.
		/// </summary>
		public UpdateExParameterMatrix(TableSchema tableSchema)
		{

			// Initialize the object.
			this.ExternalParameterItems = new SortedList<string, ExternalParameterItem>();
			this.InternalParameters = new SortedList<string, InternalParameterItem>();

			// Place all the columns of the table in a list that will drive the creation of parameters.  Those that columns that
			// are native to this table will be left in the list.  Those columns that are part of a foreign constraint will be
			// replace in the final argument list with internal values that can be used to look up the actual value.
			List<ColumnSchema> columnList = new List<ColumnSchema>();
			foreach (KeyValuePair<string, ColumnSchema> keyValuePair in tableSchema.Columns)
				columnList.Add(keyValuePair.Value);

			// Every set of parameters to the 'Update' method from an external interface requires a unique identifier in order to 
			// find the record and update it.  This unique identifier can use any of the UniqueConstraints on the table in order to
			// find the record.  This creates a description of that parameter for the external method.
			UniqueConstraintParameterItem uniqueConstraintParameterItem = new UniqueConstraintParameterItem();
			uniqueConstraintParameterItem.ActualDataType = typeof(System.Object[]);
			uniqueConstraintParameterItem.CodeVariableReferenceExpression = new CodeRandomVariableReferenceExpression();
			uniqueConstraintParameterItem.DeclaredDataType = typeof(System.Object[]);
			uniqueConstraintParameterItem.Description = string.Format("A required unique key for the {0} record.", tableSchema.Name);
			uniqueConstraintParameterItem.FieldDirection = FieldDirection.In;
			uniqueConstraintParameterItem.Name = string.Format("{0}Key", CommonConversion.ToCamelCase(tableSchema.Name));
			this.ExternalParameterItems.Add(uniqueConstraintParameterItem.Name, uniqueConstraintParameterItem);

			// Every set of parameters to the 'Update' method in the internal library requires a unique identifier in order to find
			// the record and updated it.  This creates a description of a parameter that identifies the primary key for the
			// internal interface.  By the time it is used in the call to the internal 'Update' method, the record on which this
			// parameter is based must be found using the unique key passed to the external interface.  That key to the external 
			// method does not need to be the primary key, while for the internal method, this is a requirement.
			InternalParameterItem primaryKeyParameterItem = new InternalParameterItem();
			List<CodeExpression> primaryKeyExpressions = new List<CodeExpression>();
			foreach (ColumnSchema columnSchema in tableSchema.PrimaryKey.Columns)
				primaryKeyExpressions.Add(new CodePropertyReferenceExpression(uniqueConstraintParameterItem.CodeVariableReferenceExpression, columnSchema.Name));
			primaryKeyParameterItem.Expression = new CodeArrayCreateExpression(new CodeGlobalTypeReference(typeof(System.Object)), primaryKeyExpressions.ToArray());
			primaryKeyParameterItem.Name = string.Format("{0}Key", CommonConversion.ToCamelCase(tableSchema.Name));
			this.InternalParameters.Add(primaryKeyParameterItem.Name, primaryKeyParameterItem);

			// This will make sure that all the constraints on a table are satisfied by the list of parameters.
			foreach (ForeignKeyConstraintSchema foreignKeyConstraintSchema in tableSchema.ForeignKeyConstraintSchemas)
			{

				// No attempt will be made to resolve complex, redundant constraints as the same data can be found from simpler
				// ones.  The presence of the redundant, complex foreign keys will only confuse the interface.
				if (foreignKeyConstraintSchema.IsRedundant)
					continue;

				// This item describes an input parameter that uses one or more external identifiers as a keys to a parent
				// table.
				ForeignKeyConstraintParameterItem foreignKeyConstraintParameterItem = new ForeignKeyConstraintParameterItem();
				foreignKeyConstraintParameterItem.ActualDataType = typeof(System.Object[]);
				foreignKeyConstraintParameterItem.Description = string.Format("An optional unique key for the parent {0} record.", foreignKeyConstraintSchema.RelatedTable);
				foreignKeyConstraintParameterItem.DeclaredDataType = typeof(System.Object[]);
				foreignKeyConstraintParameterItem.FieldDirection = FieldDirection.In;
				foreignKeyConstraintParameterItem.ForeignKeyConstraintSchema = foreignKeyConstraintSchema;
				foreignKeyConstraintParameterItem.IsNullable = true;
				foreignKeyConstraintParameterItem.Name = string.Format("{0}Key", CommonConversion.ToCamelCase(foreignKeyConstraintSchema.RelatedTable.Name));
				if (!foreignKeyConstraintSchema.IsDistinctPathToParent)
				{
					foreignKeyConstraintParameterItem.Name += "By";
					foreach (ColumnSchema columnSchema in foreignKeyConstraintSchema.Columns)
						foreignKeyConstraintParameterItem.Name += columnSchema.Name;
				}
				this.ExternalParameterItems.Add(foreignKeyConstraintParameterItem.Name, foreignKeyConstraintParameterItem);

				// This constructs a mapping between the multi-element key that is provided to the external method and the single
				// parameter that is needed by the internal method.
				foreach (ColumnSchema columnSchema in foreignKeyConstraintSchema.Columns)
				{

					// This creates an intermediate variable to hold a value collected from the foreign keys.
					ForeignKeyVariableItem foreignKeyVariableItem = new ForeignKeyVariableItem();
					foreignKeyVariableItem.ColumnSchema = columnSchema;
					foreignKeyVariableItem.DataType = typeof(System.Object);
					foreignKeyVariableItem.Expression = new CodeRandomVariableReferenceExpression();
					foreignKeyConstraintParameterItem.ForeignKeyVariables.Add(foreignKeyVariableItem);

					// This creates an internal parameter for the update method from the elements of the foreign key.
					InternalParameterItem internalParameterItem = new InternalParameterItem();
					internalParameterItem.ColumnSchema = columnSchema;
					internalParameterItem.Expression = foreignKeyVariableItem.Expression;
					internalParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.InternalParameters.Add(internalParameterItem.Name, internalParameterItem);

					// Any items resolved using foreign constraints are removed from the list of input parameters.  The key is the
					// means of evaluating the column's value and it can't be added directly from the outside world.
					columnList.Remove(columnSchema);

				}

			}

			// At this point, all the parameters that need to be resolved with foreign keys have been added to the list of
			// parameters and their child columns have been removed from the list of input parameters.  This will add the remaining
			// items to the list of external and internal parameters.  Note that the RowVersion is not useful for an external
			// interface and is removed automatically from the external parameter list.
			foreach (ColumnSchema columnSchema in columnList)
			{

				// If a column requires special processing, it is not handled with the rest of the parameters.
				bool isOrdinaryColumn = true;

				// Since the external world doesn't have access to the row versions, they must be removed from the input
				// parameters.  A mechanism is constructed internal to the method to defeat the optimistic concurrency checking.
				if (columnSchema.IsRowVersion)
				{
					isOrdinaryColumn = false;
					InternalParameterItem internalParameterItem = new InternalParameterItem();
					internalParameterItem.Expression = new CodePropertyReferenceExpression(uniqueConstraintParameterItem.CodeVariableReferenceExpression, columnSchema.Name);
					internalParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.InternalParameters.Add(internalParameterItem.Name, internalParameterItem);
				}

				// Ordinary parameters are passed from the external caller to the internal methods without modification or 
				// interpretation.
				if (isOrdinaryColumn)
				{

					// This parameter is exposed in the external method to the outside world.
					SimpleParameterItem simpleParameterItem = new SimpleParameterItem();
					simpleParameterItem.ActualDataType = columnSchema.DataType;
					simpleParameterItem.ColumnSchema = columnSchema;
					simpleParameterItem.DeclaredDataType = typeof(System.Object);
					simpleParameterItem.Description = string.Format("The optional value for the {0} column.", CommonConversion.ToCamelCase(columnSchema.Name));
					simpleParameterItem.FieldDirection = FieldDirection.In;
					simpleParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.ExternalParameterItems.Add(simpleParameterItem.Name, simpleParameterItem);

					// These parameters are used for the internal 'Update' method.
					InternalParameterItem internalParameterItem = new InternalParameterItem();
					internalParameterItem.Expression = new CodeVariableReferenceExpression(CommonConversion.ToCamelCase(columnSchema.Name));
					internalParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.InternalParameters.Add(internalParameterItem.Name, internalParameterItem);

				}

			}

			// A configuration identifier is is required on all interface methods except the 'Configuration' table.  While not
			// every method requires the resolution of multiple external keys, the decision to add this for column was made for
			// consistency.  The idea that the external interface shouldn't break if a new unique key is added to the hierarchy.
			// This means more work up front to configure the external interfaces, but it means less work later on as the relations
			// and schema change.
			if (!this.ExternalParameterItems.ContainsKey("configurationId"))
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
		public CodeExpression[] UpdateParameters
		{

			get
			{

				// This list collects the expressions for the parameters as they are evaluated from the matrix.
				List<CodeExpression> updateParameterList = new List<CodeExpression>();
				foreach (KeyValuePair<string, InternalParameterItem> internalParameterItemPair in this.InternalParameters)
					updateParameterList.Add(internalParameterItemPair.Value.Expression);
				return updateParameterList.ToArray();

			}

		}

	}

}

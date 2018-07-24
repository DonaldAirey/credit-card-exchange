namespace FluidTrade.Core
{

	using System;
	using System.CodeDom;
	using System.Collections.Generic;

    /// <summary>
	/// Translates the parameters used for external methods to parameters for internal methods.
	/// </summary>
	public class CreateParameterMatrix
	{

		// Public Instance Properties
		public System.Collections.Generic.SortedList<string, ExternalParameterItem> ExternalParameterItems;

		/// <summary>
		/// Creates an object that translates the parameters used for external methods to parameters for internal methods.
		/// </summary>
		public CreateParameterMatrix(TableSchema tableSchema)
		{

			// Initialize the object.
			this.ExternalParameterItems = new SortedList<string, ExternalParameterItem>();

			// This will create an interface for the 'Create' method.
			foreach (KeyValuePair<string, ColumnSchema> columnPair in tableSchema.Columns)
			{

				// This column is turned into a simple parameter.
				ColumnSchema columnSchema = columnPair.Value;

				// If a column requires special processing, it is not handled with the rest of the parameters.
				bool isOrdinaryColumn = true;

				// The row version is not used in the set of Create parameters.
				if (columnSchema.IsRowVersion)
					isOrdinaryColumn = false;

				// AutoIncremented columns can only be specified as output parameters.
				if (columnSchema.IsAutoIncrement)
				{
					isOrdinaryColumn = false;
					SimpleParameterItem simpleParameterItem = new SimpleParameterItem();
					simpleParameterItem.ActualDataType = columnSchema.DataType;
					simpleParameterItem.ColumnSchema = columnSchema;
					simpleParameterItem.DeclaredDataType = columnSchema.DataType;
					simpleParameterItem.Description = string.Format("The generated value for the {0} column.", columnSchema.Name);
					simpleParameterItem.FieldDirection = FieldDirection.Out;
					simpleParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.ExternalParameterItems.Add(simpleParameterItem.Name, simpleParameterItem);
				}

				// The only complication for ordinary parameters is whether the data type can accept a default or not.
				if (isOrdinaryColumn)
				{
					SimpleParameterItem simpleParameterItem = new SimpleParameterItem();
					bool isOptional = columnSchema.IsNullable || columnSchema.DefaultValue != DBNull.Value;
					simpleParameterItem.ActualDataType = columnSchema.DataType;
					simpleParameterItem.ColumnSchema = columnSchema;
					simpleParameterItem.DeclaredDataType = isOptional ? typeof(System.Object) : columnSchema.DataType;
					simpleParameterItem.Description = string.Format("The {0} value for the {1} column.", isOptional ? "optional" : "required", columnSchema.Name);
					simpleParameterItem.FieldDirection = FieldDirection.In;
					simpleParameterItem.Name = CommonConversion.ToCamelCase(columnSchema.Name);
					this.ExternalParameterItems.Add(simpleParameterItem.Name, simpleParameterItem);
				}

			}

		}

	}

}

namespace FluidTrade.Core
{

	using System;
    using System.Collections.Generic;
    using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// A Schema of a table.
	/// </summary>
	public class TableSchema : ObjectSchema
	{

		// Private Instance Fields
		private DataModelSchema dataModelSchema;
		private UniqueConstraintSchema primaryKey;
		private System.Boolean isPersistent;
		private System.Collections.Generic.SortedList<string, ConstraintSchema> constraintList;
		private System.Collections.Generic.SortedList<string, RelationSchema> childRelationList;
		private System.Collections.Generic.SortedList<string, RelationSchema> parentRelationList;
		private System.Collections.Generic.SortedList<string, ColumnSchema> columnList;
		private String name;

		/// <summary>
		/// Create a table schema from the XML Schema specification.
		/// </summary>
		/// <param name="dataModelSchema">The data model to which this table belongs.</param>
		/// <param name="xmlSchemaElement">The root of the XmlSchema element that describes the table.</param>
		public TableSchema(DataModelSchema dataModelSchema, XmlSchemaElement xmlSchemaElement)
			: base(dataModelSchema, xmlSchemaElement)
		{

			// Initialize the object.
			this.dataModelSchema = dataModelSchema;
			this.isPersistent = GetPersistentIndicator(xmlSchemaElement);
			this.name = xmlSchemaElement.Name;
			this.columnList = new SortedList<string, ColumnSchema>();
			this.constraintList = new SortedList<string, ConstraintSchema>();
			this.childRelationList = new SortedList<string, RelationSchema>();
			this.parentRelationList = new SortedList<string, RelationSchema>();

			// Every table has a row version column which tracks the history of changes to the row.
			ColumnSchema rowVersionSchema = new ColumnSchema(this, "RowVersion", typeof(Int64), false, true, DBNull.Value, Int32.MaxValue);
			this.columnList.Add(rowVersionSchema.Name, rowVersionSchema);

			// Initialize the columns of the table.
			Initialize(xmlSchemaElement);

		}

		/// <summary>
		/// A collection of child relations for this FluidTrade.Core.TableSchema.
		/// </summary>
		public SortedList<string, RelationSchema> ChildRelations
		{
			get { return this.childRelationList; }
		}

		/// <summary>
		/// Gets the sorted list of columns in a table.
		/// </summary>
		public SortedList<string, ColumnSchema> Columns
		{
			get { return this.columnList; }
		}

		/// <summary>
		/// A collection of constraints associated with this table.
		/// </summary>
		public SortedList<string, ConstraintSchema> Constraints
		{
			get { return this.constraintList; }
		}

		/// <summary>
		/// Gets the parent DataModelSchema for this table.
		/// </summary>
		public DataModelSchema DataModel
		{
			get { return this.dataModelSchema; }
		}

		/// <summary>
		/// Gets the ForeignKey constraints.
		/// </summary>
		public ForeignKeyConstraintSchema[] ForeignKeyConstraintSchemas
		{
			get
			{

				// This creates an array of the foreignKey constraints on this table.
				List<ForeignKeyConstraintSchema> foreignKeyConstraintList = new List<ForeignKeyConstraintSchema>();
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in this.Constraints)
					if (constraintPair.Value is ForeignKeyConstraintSchema)
						foreignKeyConstraintList.Add(constraintPair.Value as ForeignKeyConstraintSchema);
				return foreignKeyConstraintList.ToArray();

			}
		}

		/// <summary>
		/// Gets an indication whether the table is written to a persistent store.
		/// </summary>
		public Boolean IsPersistent
		{
			get { return this.isPersistent; }
		}

		/// <summary>
		/// Gets an indication of whether the table contains any constraints that allow it to be addressed from an external source.
		/// </summary>
		public Boolean IsExternal
		{

			get
			{

				// Search each of the unique constraints on the table.  If any of them contain elements that are not generated
				// internally to the middle tier, then the table can be addressed from an external source.  A table containing only
				// AutoIncrement unique constraints can't be addressed using scripts or other tools that don't have an image of the
				// data model.
				foreach (UniqueConstraintSchema uniqueConstraintSchema in this.UniqueConstraintSchemas)
				{
					bool isExternal = true;
					foreach (ColumnSchema columnSchema in uniqueConstraintSchema.Columns)
						if (columnSchema.IsAutoIncrement)
							isExternal = false;
					if (isExternal)
						return true;
				}

				// This table doesn't contain any constraints that can be addressed from an external source.
				return false;

			}

		}

		/// <summary>
		/// The name of the table.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// The foreign key constraint of the parent table that shares a unique key with this table.
		/// </summary>
		public ForeignKeyConstraintSchema ParentConstraint
		{

			get
			{

				// Search through each of the foreign key constraints looking for a unique path back to this table.
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in this.Constraints)
					if (constraintPair.Value is ForeignKeyConstraintSchema)
					{
						ForeignKeyConstraintSchema foreignKeyConstraintSchema = constraintPair.Value as ForeignKeyConstraintSchema;
						if (this.GetUniqueConstraint(foreignKeyConstraintSchema.Columns) != null)
							return foreignKeyConstraintSchema;
					}

				// At this point, there are no parent tables that share a unique key with this table.
				return null;

			}

		}

		/// <summary>
		/// A collection of parent relations for this FluidTrade.Core.TableSchema.
		/// </summary>
		public SortedList<string, RelationSchema> ParentRelations
		{
			get { return this.parentRelationList; }
		}
	
		/// <summary>
		/// A description of the primary key on this table.
		/// </summary>
		public UniqueConstraintSchema PrimaryKey
		{
			get { return this.primaryKey; }
		}

		/// <summary>
		/// Gets the unique constraints.
		/// </summary>
		public UniqueConstraintSchema[] UniqueConstraintSchemas
		{
			get
			{

				// This creates an array of the unique constraints on this table.
				List<UniqueConstraintSchema> uniqueConstraintList = new List<UniqueConstraintSchema>();
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in this.Constraints)
					if (constraintPair.Value is UniqueConstraintSchema)
					{
						UniqueConstraintSchema uniqueConstraintSchema = constraintPair.Value as UniqueConstraintSchema;
						uniqueConstraintList.Add(uniqueConstraintSchema);
					}
				return uniqueConstraintList.ToArray();

			}
		}

		/// <summary>
		/// The display text of the object.
		/// </summary>
		/// <returns></returns>
		public override string ToString() { return this.Name; }

		/// <summary>
		/// Gets a collection of columns that belong to this table and includes all the inherited columns.
		/// </summary>
		/// <param name="xmlSchemaObject">The XmlSchema that defines the class.</param>
		/// <param name="columnSchemaCollection">A collection of schemas that describe the columns in this table.</param>
		private void Initialize(XmlSchemaElement xmlSchemaElement)
		{

			// This runs through the schema and creates columns from the XML description.
			if (xmlSchemaElement.SchemaType is XmlSchemaComplexType)
			{

				// Extract the complex type from the generic object.
				XmlSchemaComplexType xmlSchemaComplexType = xmlSchemaElement.SchemaType as XmlSchemaComplexType;

				// Columns can be specified as attributes.
				foreach (XmlSchemaAttribute xmlSchemaAttribute in xmlSchemaComplexType.Attributes)
					this.columnList.Add(xmlSchemaAttribute.Name, new ColumnSchema(this, xmlSchemaAttribute));

				// This section will parse the simple particle.
				if (xmlSchemaComplexType.Particle is XmlSchemaSequence)
				{

					// This makes it easier to access the fields of the Particle.
					XmlSchemaSequence xmlSchemaSequence = xmlSchemaComplexType.Particle as XmlSchemaSequence;

					// Each XmlSchemaElement on the Particle describes a column.
					foreach (XmlSchemaObject item in xmlSchemaSequence.Items)
					{
						if (item is XmlSchemaElement)
						{
							XmlSchemaElement columnElement = item as XmlSchemaElement;
							this.columnList.Add(columnElement.Name, new ColumnSchema(this, columnElement));
						}

					}

				}

			}

		}

		/// <summary>
		/// Adds a constraint to the table.
		/// </summary>
		/// <param name="constraintSchema">The constraint to be added.</param>
		public void Add(ConstraintSchema constraintSchema)
		{

			// This member provides quick access to the primary unique constraint.
			if (constraintSchema is UniqueConstraintSchema)
			{
				UniqueConstraintSchema uniqueSchema = constraintSchema as UniqueConstraintSchema;
				if (uniqueSchema.IsPrimaryKey)
					this.primaryKey = uniqueSchema;
			}

			// All constraints on the table are place in this collection.  When the CodeDOM is created, this table will be 
			// examined and an equivalent constraint will be placed on the target table.
			this.constraintList.Add(constraintSchema.Name, constraintSchema);

		}

		/// <summary>
		/// Indicates whether the column is persisted in a data store.
		/// </summary>
		/// <param name="xmlSchemaElement">The XML Element.</param>
		/// <returns>true if the column should be stored in a persistent database.</returns>
		private bool GetPersistentIndicator(XmlSchemaElement xmlSchemaElement)
		{

			// Extract the persistence attribute of a column.
			XmlAttribute xmlAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaElement, QualifiedName.IsPersistent);
			if (xmlAttribute != null)
				return Convert.ToBoolean(xmlAttribute.Value);

			// The default attribute is to generate a persistent table.
			return true;

		}

		/// <summary>
		/// Gets a unique constraint matching the given column set.
		/// </summary>
		/// <param name="columns">A key described as a set of ColumnSchemas.</param>
		/// <returns>The unique constraint matching the given columns or null if no such constraint exists.</returns>
		public UniqueConstraintSchema GetUniqueConstraint(ColumnSchema[] columns)
		{

			// Search for a unique constraint that matches the given column set exactly.
			foreach (KeyValuePair<string, ConstraintSchema> constraintPair in this.Constraints)
				if (constraintPair.Value is UniqueConstraintSchema)
				{

					// This constraint is tested for an exact match to the given columns.
					UniqueConstraintSchema uniqueConstraintSchema = constraintPair.Value as UniqueConstraintSchema;

					// There's no match if the number of columns between the two constraints can't agree.
					if (uniqueConstraintSchema.Columns.Length != columns.Length)
						return null;

					// The order of the columns and the columns must match for this test.
					for (int columnIndex = 0; columnIndex < uniqueConstraintSchema.Columns.Length; columnIndex++)
						if (uniqueConstraintSchema.Columns[columnIndex] != columns[columnIndex])
							return null;

					// At this point, a unique constraint matching the given set of columns has been found.
					return uniqueConstraintSchema;

				}

			// At this point, all of the constraints have been examined and none of them contain the given column set.
			return null;

		}

	}

}

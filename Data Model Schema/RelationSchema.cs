namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
    using System.Xml.Schema;

	/// <summary>
	/// Creates foreign key constraint on two tables.
	/// </summary>
	public class RelationSchema
	{

		// Private Instance Fields
		private ColumnSchema[] childColumns;
		private ColumnSchema[] parentColumns;
		private ForeignKeyConstraintSchema childKeyConstraint;
		private UniqueConstraintSchema parentKeyConstraint;
		private TableSchema childTable;
		private TableSchema parentTable;
		private String name;

		/// <summary>
		/// Create a foreign key constraint on two tables.
		/// </summary>
		/// <param name="dataModelSchema">The parent data model schema.</param>
		/// <param name="xmlSchemaKeyref">The XmlSchema object that describes the foreignn key relation.</param>
		public RelationSchema(DataModelSchema dataModelSchema, XmlSchemaKeyref xmlSchemaKeyref)
		{

			// Initialize the object.
			this.name = xmlSchemaKeyref.Name;

			// This will search through each of the tables looking for the parent and child components of the relation.
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
			{

				ConstraintSchema constraintSchema;

				// This is the parent component of the relation.
				if (keyValuePair.Value.Constraints.TryGetValue(xmlSchemaKeyref.Refer.Name, out constraintSchema))
				{
					UniqueConstraintSchema uniqueConstraintSchema = constraintSchema as UniqueConstraintSchema;
					this.parentColumns = uniqueConstraintSchema.Columns;
					this.parentTable = uniqueConstraintSchema.Table;
					this.parentKeyConstraint = uniqueConstraintSchema;
				}

				// This is the child part of the relation.
				if (keyValuePair.Value.Constraints.TryGetValue(xmlSchemaKeyref.Name, out constraintSchema))
				{
					ForeignKeyConstraintSchema foreignKeyConstraintSchema = constraintSchema as ForeignKeyConstraintSchema;
					this.childTable = foreignKeyConstraintSchema.Table;
					this.childColumns = foreignKeyConstraintSchema.Columns;
					this.childKeyConstraint = foreignKeyConstraintSchema;
				}

			}

		}

		/// <summary>
		/// Gets the constraint that binds the parent table to the child table.
		/// </summary>
		public ForeignKeyConstraintSchema ChildKeyConstraint
		{
			get { return this.childKeyConstraint; }
		}

		/// <summary>
		/// Gets the parent columns of this constraint.
		/// </summary>
		public ColumnSchema[] ChildColumns
		{
			get { return this.childColumns; }
		}

		/// <summary>
		/// Gets the parent table of this constraint.
		/// </summary>
		public TableSchema ChildTable
		{
			get { return this.childTable; }
		}

		/// <summary>
		/// The name of the relation.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Gets the UniqueConstraintSchema that guarantees that values in the parent column of a RelationSchema are unique.
		/// </summary>
		public UniqueConstraintSchema ParentKeyConstraint
		{
			get { return this.parentKeyConstraint; }
		}
	
		/// <summary>
		/// Gets the parent columns of this constraint.
		/// </summary>
		public ColumnSchema[] ParentColumns
		{
			get { return this.parentColumns; }
		}

		/// <summary>
		/// Gets the parent table of this constraint.
		/// </summary>
		public TableSchema ParentTable
		{
			get { return this.parentTable; }
		}

		/// <summary>
		/// Gets an indication of whether there is a single or multiple paths from the child to the the parent table.
		/// </summary>
		public Boolean IsDistinctPathToParent
		{

			get
			{

				// If any of the parent tables are the same then the path is not distinct.
				foreach (KeyValuePair<string, RelationSchema> relationPair in this.ChildTable.ParentRelations)
					if (relationPair.Value.ParentTable == this.ParentTable && relationPair.Value.Name != this.Name)
						return false;

				// There is only one path from the parent to the child table.
				return true;

			}

		}

		/// <summary>
		/// Gets an indication of whether there is a single or multiple paths from the parent to the child table.
		/// </summary>
		public Boolean IsDistinctPathToChild
		{

			get
			{

				// If any of the child tables are the same then the path is not distinct.
				foreach (KeyValuePair<string, RelationSchema> relationPair in this.ParentTable.ChildRelations)
					if (relationPair.Value.ChildTable == this.ChildTable && relationPair.Value.Name != this.Name)
						return false;

				// There is only one path from the parent table to the child.
				return true;

			}

		}

	}

}

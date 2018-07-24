namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
    using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// Creates foreign key constraint on two tables.
	/// </summary>
	public class ForeignKeyConstraintSchema : ConstraintSchema
	{

		// Public Enums
		public enum CascadeRules { None, Cascade, SetNull, SetDefault };

		// Private Instance Fields
		private ColumnSchema[] relatedColumns;
		private TableSchema relatedTable;
		private ForeignKeyConstraintSchema.CascadeRules updateRule;
		private ForeignKeyConstraintSchema.CascadeRules deleteRule;

		/// <summary>
		/// Create a foreign key constraint on two tables.
		/// </summary>
		/// <param name="dataModelSchema">The parent data model schema.</param>
		/// <param name="xmlSchemaKeyref">The XmlSchema object that describes the foreignn key relation.</param>
		public ForeignKeyConstraintSchema(DataModelSchema dataModelSchema, XmlSchemaKeyref xmlSchemaKeyref)
			: base(dataModelSchema, xmlSchemaKeyref)
		{

			// This will search through each of the tables looking for a key that matches the name of the reference.  Note that
			// there is no checking to make sure the 'Refer' key exists.  If it didn't exist, the XmlSchema would have caught it
			// and never have validated the schema.
			foreach (KeyValuePair<string, TableSchema> keyValuePair in dataModelSchema.Tables)
			{
				ConstraintSchema constraintSchema;
				if (keyValuePair.Value.Constraints.TryGetValue(xmlSchemaKeyref.Refer.Name, out constraintSchema))
				{
					this.relatedTable = constraintSchema.Table;
					this.relatedColumns = constraintSchema.Columns;
				}
			}

			// Parse the cascading update rule out of the keyref specification.
			XmlAttribute updateRuleAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaKeyref, QualifiedName.UpdateRule);
			this.updateRule = updateRuleAttribute == null ? CascadeRules.Cascade : (CascadeRules)Enum.Parse(typeof(CascadeRules), updateRuleAttribute.Value);

			// Parse the cascading delete rule out of the keyref specification.
			XmlAttribute deleteRuleAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaKeyref, QualifiedName.DeleteRule);
			this.deleteRule = deleteRuleAttribute == null ? CascadeRules.Cascade : (CascadeRules)Enum.Parse(typeof(CascadeRules), deleteRuleAttribute.Value);

		}

		/// <summary>
		/// Gets the parent table of this constraint.
		/// </summary>
		public TableSchema RelatedTable
		{
			get { return this.relatedTable; }
		}

		/// <summary>
		/// Gets the parent columns of this constraint.
		/// </summary>
		public ColumnSchema[] RelatedColumns
		{
			get { return this.relatedColumns; }
		}

		/// <summary>
		/// Gets the rule that describes what happens to child records when a record is updated.
		/// </summary>
		public ForeignKeyConstraintSchema.CascadeRules UpdateRule
		{
			get { return this.updateRule; }
		}

		/// <summary>
		/// Gets the rule that describes what happens to child records when a record is deleted.
		/// </summary>
		public ForeignKeyConstraintSchema.CascadeRules DeleteRule
		{
			get { return this.deleteRule; }
		}

		/// <summary>
		/// Gets an indication of whether there is a single or multiple paths from the child to the the parent table.
		/// </summary>
		public Boolean IsDistinctPathToParent
		{

			get
			{

				// If any of the parent tables are the same then the path is not distinct.
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in this.Table.Constraints)
					if (constraintPair.Value is ForeignKeyConstraintSchema)
					{
						ForeignKeyConstraintSchema foreignKeyConstraintSchema = constraintPair.Value as ForeignKeyConstraintSchema;
						if (foreignKeyConstraintSchema.RelatedTable == this.RelatedTable && foreignKeyConstraintSchema.Name != this.Name)
							return false;
					}

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
				foreach (KeyValuePair<string, ConstraintSchema> constraintPair in this.RelatedTable.Constraints)
					if (constraintPair.Value is ForeignKeyConstraintSchema)
					{
						ForeignKeyConstraintSchema foreignKeyConstraintSchema = constraintPair.Value as ForeignKeyConstraintSchema;
						if (foreignKeyConstraintSchema.Table == this.Table && foreignKeyConstraintSchema.Name != this.Name)
							return false;
					}

				// There is only one path from the parent table to the child.
				return true;

			}

		}

		/// <summary>
		/// Gets an indication of whether the foreign key is composed of other simpler foreign keys.
		/// </summary>
		public Boolean IsRedundant
		{

			get
			{

				// This is a list of candidate columns that can be replaced with other columns from other foreign key relations.
				// Each sibling foreign key relation will be examined to see if it contains the same columns as the ones collected
				// here.  If a redundant column is found, the one belonging to the dependent table will be removed.  A dependent 
				// table is one that also depends on one of the parent of another sibling relation.
				SortedList<string, ColumnSchema> keyColumns = new SortedList<string, ColumnSchema>();
				foreach (ColumnSchema columnSchema in this.Columns)
					keyColumns.Add(columnSchema.Name, columnSchema);

				// If all of the child columns can be obtained from other relations, then this foreign key constraint is considered
				// redundant.  All the other sibling foreign key relations will be examined.  When a redundant column is found, the
				// one belonging to a dependent table is removed from the list.
				foreach (ForeignKeyConstraintSchema parentForeignKey in this.RelatedTable.ForeignKeyConstraintSchemas)
					foreach (ForeignKeyConstraintSchema childForeignKey in this.Table.ForeignKeyConstraintSchemas)
						if (childForeignKey.Name != this.Name)
							if (childForeignKey.RelatedTable.Name == parentForeignKey.RelatedTable.Name)
								foreach (ColumnSchema columnSchema in childForeignKey.Columns)
									keyColumns.Remove(columnSchema.Name);

				// This foreign key is considered redundant when all the elements of the key can be obtained through other 
				// independent constraints.
				return keyColumns.Count == 0;

			}

		}

	}

}

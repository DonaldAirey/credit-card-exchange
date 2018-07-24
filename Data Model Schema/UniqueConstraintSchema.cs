namespace FluidTrade.Core
{

	using System;
    using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// Describes a set of columns that must be unique in a table.
	/// </summary>
	public class UniqueConstraintSchema : ConstraintSchema
	{

		// Private Members
		private System.Boolean isPrimaryKey;

		/// <summary>
		/// Creates a constraint that forces a set of columns to be unique.
		/// </summary>
		/// <param name="dataModelSchema">The parent data model schema.</param>
		/// <param name="xmlSchemaUnique">The XmlSchema description of the constraint.</param>
		public UniqueConstraintSchema(DataModelSchema dataModelSchema, XmlSchemaUnique xmlSchemaUnique)
			: base(dataModelSchema, xmlSchemaUnique)
		{

			// This determines whether the constraint should be used as the primary key for a table.
			XmlAttribute xmlAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaUnique, QualifiedName.PrimaryKey);
			this.isPrimaryKey = xmlAttribute == null ? false : Convert.ToBoolean(xmlAttribute.Value);

		}

		/// <summary>
		/// Gets an indication of whether the constraint is the primary key on a table.
		/// </summary>
		public Boolean IsPrimaryKey
		{
			get { return this.isPrimaryKey; }
		}

		/// <summary>
		/// Gets the foreign key that can be used to find a child element using this unique constraint.
		/// </summary>
		public ForeignKeyConstraintSchema ForeignKey
		{
			get
			{

				// Seach the foreign constraints to see if any of the column sets exactly match the column set of this constraint.
				foreach (ForeignKeyConstraintSchema foreignKeyConstraintSchema in this.Table.ForeignKeyConstraintSchemas)
					if (this.Table.GetUniqueConstraint(foreignKeyConstraintSchema.Columns) == this)
						return foreignKeyConstraintSchema;

				// At this point there are no foreign keys that can use this constraint to find a child element.
				return null;

			}
		}

	}

}

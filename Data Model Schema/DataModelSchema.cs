namespace FluidTrade.Core
{

	using System;
    using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// Describes a data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DataModelSchema
	{

		// Private Instance Fields
		private System.Collections.Generic.List<ObjectSchema> itemList;
		private System.Type generatorType;
		private String name;
		private System.Collections.Generic.SortedList<string, RelationSchema> relationList;
		private System.Collections.Generic.SortedList<string, TableSchema> tableList;
		private String targetNamespace;
		private System.Decimal version;
		private System.Xml.XmlNamespaceManager xmlNamespaceManager;

		/// <summary>
		/// Constructs a schema from the contents of an XML specification.
		/// </summary>
		/// <param name="fileContents">The contents of a file that specifies the schema in XML.</param>
		public DataModelSchema(string fileContents)
		{

			// Initialize the object
			this.itemList = new List<ObjectSchema>();
			this.relationList = new SortedList<string, RelationSchema>();
			this.tableList = new SortedList<string, TableSchema>();

			// Compile the schema to resolve all the types and qualified names.
			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(fileContents));
			XmlSchema primarySchema = XmlSchema.Read(xmlTextReader, new ValidationEventHandler(ValidationCallback));
			xmlSchemaSet.Add(primarySchema);
			xmlSchemaSet.Compile();

			// The namespace FluidTrade.Core is used to create qualified names from the XPath specifications.  These are most useful in
			// following the key schemas back to the relevant table schemas.
			this.xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
			foreach (XmlSchema xmlSchema in xmlSchemaSet.Schemas())
				foreach (XmlQualifiedName xmlQualifiedName in xmlSchema.Namespaces.ToArray())
					this.xmlNamespaceManager.AddNamespace(xmlQualifiedName.Name, xmlQualifiedName.Namespace);

			// Initialize the data structures from primary schema in the set.
			foreach (XmlSchema xmlSchema in xmlSchemaSet.Schemas(primarySchema.TargetNamespace))
			{
				this.name = xmlSchema.Id;
				this.targetNamespace = xmlSchema.TargetNamespace;
				this.version = Convert.ToDecimal(xmlSchema.Version);
				foreach (XmlSchemaObject xmlSchemaObject in xmlSchema.Items)
					if (xmlSchemaObject is XmlSchemaAnnotation)
						this.itemList.Add(new AnnotationSchema(this, xmlSchemaObject as XmlSchemaAnnotation));
			}

			// The schema is parsed in two passes.  The first evaluates the tables, keys and unique constraints.  The second 
			// evaluates the foreign keys and associates them with the parent and child tables.
			FirstPass(xmlSchemaSet);
			SecondPass(xmlSchemaSet);

		}

		/// <summary>
		/// A collection of annotations assigned to the schema.
		/// </summary>
		public List<ObjectSchema> Items
		{
			get { return this.itemList; }
		}

		/// <summary>
		/// The name of the data model.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// The relations between parent and child tables in the data model.
		/// </summary>
		public SortedList<string, RelationSchema> Relations
		{
			get { return this.relationList; }
		}

		/// <summary>
		/// A sorted list of the tables in the data model schema.
		/// </summary>
		public SortedList<string, TableSchema> Tables
		{
			get { return this.tableList; }
		}

		/// <summary>
		/// The target namespace for the generated data model.
		/// </summary>
		public string TargetNamespace { get { return this.targetNamespace; } set { this.targetNamespace = value; } }

		/// <summary>
		/// Identifies the version of the schema.
		/// </summary>
		public decimal Version { get { return this.version; } }

		/// <summary>
		/// Gets or sets the data type of the code generator used to construct the target.
		/// </summary>
		public Type GeneratorType
		{
			get { return this.generatorType; }
			set { this.generatorType = value; }
		}

		/// <summary>
		/// Used to resolve XPath specifications.
		/// </summary>
		public XmlNamespaceManager XmlNamespaceManager { get { return this.xmlNamespaceManager; } }

		/// <summary>
		/// Callback for parsing errors on the Xml Schema.
		/// </summary>
		/// <param name="sender">The object that originated the message.</param>
		/// <param name="args">The event arguments.</param>
		public void ValidationCallback(object sender, ValidationEventArgs args)
		{

			// Catch all parsing exceptions here.
			throw new Exception(args.Message);

		}

		/// <summary>
		/// Creates a collection of tables found in the schema.
		/// </summary>
		/// <param name="xmlSchema">The schema that describes the data model.</param>
		/// <returns>A list of TableSchema objects that describe the tables found in the data model schema.</returns>
		private void FirstPass(XmlSchemaSet xmlSchemaSet)
		{

			// Scan through the schema set looking for table elements.  These can either be defined at the root element for a standard 
			// schema or they can be found as choices of a special element describing a 'DataSet' for the Microsoft version of a
			// schema.
			foreach (XmlSchemaElement xmlSchemaElement in xmlSchemaSet.GlobalElements.Values)
			{

				// If the element read from the schema is the Microsoft DataSet element, then the tables are described as choices
				// associated with an implicit complex type on that element.
				if (ObjectSchema.IsDataSetElement(xmlSchemaElement))
				{

					// The tables are described as an choices of an implicit (nested) complex type.
					if (xmlSchemaElement.SchemaType is XmlSchemaComplexType)
					{

						// The complex type describes the table.
						XmlSchemaComplexType xmlSchemaComplexType = xmlSchemaElement.SchemaType as XmlSchemaComplexType;

						// The data model is described as a set of one or more choices of tables.
						if (xmlSchemaComplexType.Particle is XmlSchemaChoice)
						{

							// The data model is described as a set of choices.  Each choice represents a table.
							XmlSchemaChoice xmlSchemaChoice = xmlSchemaComplexType.Particle as XmlSchemaChoice;

							// Create a table for each of the choices described in the complex type.
							foreach (XmlSchemaObject choiceObject in xmlSchemaChoice.Items)
								if (choiceObject is XmlSchemaElement)
								{
									XmlSchemaElement tableElement = choiceObject as XmlSchemaElement;
									this.tableList.Add(tableElement.Name, new TableSchema(this, tableElement));
								}
						}

					}

					// The constraints describe the columns that are unique for a table.
					foreach (XmlSchemaIdentityConstraint xmlSchemaIdentityConstraint in xmlSchemaElement.Constraints)
					{

						// This describes the columns that must be unique within a table.
						if (xmlSchemaIdentityConstraint is XmlSchemaUnique)
						{
							XmlSchemaUnique xmlSchemaUnique = xmlSchemaIdentityConstraint as XmlSchemaUnique;
							UniqueConstraintSchema uniqueConstraintSchema = new UniqueConstraintSchema(this, xmlSchemaUnique);
							uniqueConstraintSchema.Table.Add(uniqueConstraintSchema);
						}

					}

				}

			}

		}

		/// <summary>
		/// The foreign constraints can only be evaluated after all the tables, keys and unique constraints have been evaluated.
		/// </summary>
		/// <param name="xmlSchema"></param>
		private void SecondPass(XmlSchemaSet xmlSchemaSet)
		{

			// This is the second pass through the schemas.  Once the tables, keys and unique constraints have been evaluated, 
			// then the foreign constraints can be constructed and applied to the parent and child tables.
			foreach (XmlSchemaElement xmlSchemaElement in xmlSchemaSet.GlobalElements.Values)
			{

				// Only the Microsoft DataSet element is evaluated for foreign keys.
				if (ObjectSchema.IsDataSetElement(xmlSchemaElement))
				{

					// This will examine each of the constraints looking for a foreign key description.
					foreach (XmlSchemaIdentityConstraint xmlSchemaIdentityConstraint in xmlSchemaElement.Constraints)
					{

						// Evaluate the foreign keys in the data model.
						if (xmlSchemaIdentityConstraint is XmlSchemaKeyref)
						{

							// This object can be used as a foreign key constraint and, optionally, can be used to describe a 
							// parent/child relationship.
							XmlSchemaKeyref xmlSchemaKeyref = xmlSchemaIdentityConstraint as XmlSchemaKeyref;

							// This creates a foreign key.
							ForeignKeyConstraintSchema foreignKeyConstraintSchema = new ForeignKeyConstraintSchema(this, xmlSchemaIdentityConstraint as XmlSchemaKeyref);

							// Foreign constraint schemas are always added to the list of constraints on a table.  They can also
							// conditionally become the source for a relationship between two tables.
							foreignKeyConstraintSchema.Table.Add(foreignKeyConstraintSchema);

							// Unless specifically instructed to supress the relation, it will be created add added to both the 
							// parent and child tables as well as the data model.
							XmlAttribute isConstraintOnlyAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaIdentityConstraint, QualifiedName.ConstraintOnly);
							if (isConstraintOnlyAttribute == null || !Convert.ToBoolean(isConstraintOnlyAttribute.Value))
							{
								RelationSchema relationSchema = new RelationSchema(this, xmlSchemaKeyref);
								relationSchema.ParentTable.ChildRelations.Add(relationSchema.Name, relationSchema);
								relationSchema.ChildTable.ParentRelations.Add(relationSchema.Name, relationSchema);
								this.Relations.Add(relationSchema.Name, relationSchema);
							}

						}

					}

				}

			}

		}

		/// <summary>
		/// Converts the text of a value into a CLR value.
		/// </summary>
		/// <param name="value">The text of a value.</param>
		/// <returns>The text converted into a base type object.</returns>
		public static object ConvertValue(Type type, object value)
		{

			// A missing default is an implicit declaration of a DBNull.Value default, which is how both the ADO DataSet and SQL
			// Database interpret the null value.  An actual 'null' value is only used to communicate that no explicit value is
			// being passed, but it is never actually stored in a persistent or volatile store.
			if (value == null)
				return DBNull.Value;

			// This uses the name of the type to drive a conversion to a CLR type.
			switch (type.ToString())
			{

			case "System.Boolean":
				return Convert.ToBoolean(value);

			case "System.Int16":
				return Convert.ToInt16(value);

			case "System.Int32":
				return Convert.ToInt32(value);

			case "System.Int64":
				return Convert.ToInt64(value);

			case "System.Decimal":
				return Convert.ToDecimal(value);

			case "System.Double":
				return Convert.ToDouble(value);

			case "System.String":
				return value;

			case "System.Guid":
				return new Guid(Convert.ToString(value));

			default:

				if (type.IsEnum)
					return Enum.Parse(type, Convert.ToString(value));

				break;

			}

			// Throw the exception to catch any data types that aren't converted above.
			throw new Exception(string.Format("The datatype {0} isn't handled by this compiler", value.GetType()));

		}

	}

}

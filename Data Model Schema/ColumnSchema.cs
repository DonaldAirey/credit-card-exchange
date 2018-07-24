namespace FluidTrade.Core
{

	using System;
    using System.Reflection;
	using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// Describes a column in a table in a data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ColumnSchema
	{

		// Protected Fields
		protected TableSchema tableSchema;
		protected System.Boolean isAutoIncrement;
		protected System.Boolean isEncrypted;
		protected System.Boolean isNullable;
		protected System.Boolean isPersistent;
		protected System.Boolean isPrimaryKey;
		protected System.Boolean isRowVersion;		
		protected System.Int32 autoIncrementSeed;
		protected System.Int32 autoIncrementStep;
		protected System.Int32 maximumLength;
		protected System.Object defaultValue;
		protected String name;
		protected System.Type dataType;

		/// <summary>
		/// Create a description of a column in a data model.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		/// <param name="xmlSchemaObject">The schema of the column.</param>
		public ColumnSchema(TableSchema tableSchema, string name, Type dataType, bool isNullable, bool isRowVersion,
			object defaultValue, int maximumLength)
		{

			// Initialize the object
			this.tableSchema = tableSchema;
			this.name = name;
			this.dataType = dataType;
			this.isEncrypted = false;
			this.isNullable = isNullable;
			this.isPersistent = true;
			this.isPrimaryKey = false;
			this.isRowVersion = isRowVersion;
			this.defaultValue = defaultValue;
			this.maximumLength = maximumLength;

		}

		/// <summary>
		/// Create a description of a column in a data model.
		/// </summary>
		/// <param name="dataModelSchema">The Schema of the entire data model.</param>
		/// <param name="xmlSchemaObject">The schema of the column.</param>
		public ColumnSchema(TableSchema tableSchema, XmlSchemaAnnotated xmlSchemaAnnotated)
		{

			// Initialize the object
			this.tableSchema = tableSchema;
			this.name = string.Empty;
			this.dataType = typeof(System.Object);
			this.isEncrypted = false;
			this.isNullable = false;
			this.isPrimaryKey = false;
			this.isPersistent = true;
			this.isRowVersion = false;
			this.defaultValue = DBNull.Value;
			this.maximumLength = int.MaxValue;

			// Initialize the fields from the element or attribute
			Initialize(xmlSchemaAnnotated);

		}

		/// <summary>
		/// Gets a value used to see an auto incrementing column.
		/// </summary>
		public int AutoIncrementSeed
		{
			get { return this.autoIncrementSeed; }
		}

		/// <summary>
		/// Gets the value used to increment the seed value for an auto-incrementing column.
		/// </summary>
		public int AutoIncrementStep
		{
			get { return this.autoIncrementStep; }
		}

		/// <summary>
		/// Gets the System.Type of the column.
		/// </summary>
		public Type DataType
		{
			get { return this.dataType; }
		}

		/// <summary>
		/// Gets the default value for a column.
		/// </summary>
		public Object DefaultValue
		{
			get { return this.defaultValue; }
		}

		/// <summary>
		/// Gets an indication of whether the column increments automatically as new rows are created.
		/// </summary>
		public Boolean IsAutoIncrement
		{
			get { return this.isAutoIncrement; }
		}

		/// <summary>
		/// Gets an indication of whether the column is encrypted or not
		/// </summary>
		public Boolean IsEncrypted
		{
			get { return this.isEncrypted; }
		}

		/// <summary>
		/// Gets the indication of whether the column is required to have a value or can be null.
		/// </summary>
		public Boolean IsNullable
		{
			get { return this.isNullable; }
		}

		/// <summary>
		/// Gets an indication of whether the column has a parent.
		/// </summary>
		public Boolean IsOrphan
		{
			get
			{

				// An orphan column is one without a parent column defined through some foreign key constraint.
				foreach (ForeignKeyConstraintSchema foreignKeyConstraintSchema in this.Table.ForeignKeyConstraintSchemas)
					foreach (ColumnSchema foreignColumn in foreignKeyConstraintSchema.Columns)
						if (foreignColumn == this)
							return false;

				// The column has no relation in another table.
				return true;

			}
		}

		/// <summary>
		/// Gets an indication of whether or not the column should be saved to a persistent store.
		/// </summary>
		public Boolean IsPersistent
		{
			get { return this.isPersistent; }
		}

		/// <summary>
		/// Gets or sets an indication of whether the column is part of a primary key.
		/// </summary>
		public Boolean IsPrimaryKey
		{
			get
			{

				// This will examine the primary key to see if the column is part of the key.
				foreach (UniqueConstraintSchema uniqueConstraint in this.Table.UniqueConstraintSchemas)
					if (uniqueConstraint.IsPrimaryKey)
						foreach (ColumnSchema uniqueColumn in uniqueConstraint.Columns)
							if (uniqueColumn == this)
								return true;

				// The column is not part of the primary key.
				return false;

			}
		}

		/// <summary>
		/// Gets a indication of whether the column contains the row's version.
		/// </summary>
		public Boolean IsRowVersion
		{
			get { return this.isRowVersion; }
		}

		/// <summary>
		/// Gets the maximum length of the data in a column.
		/// </summary>
		public int MaximumLength
		{
			get { return this.maximumLength; }
		}

		/// <summary>
		/// Gets the name of the column.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Gets the parent TableSchema of this ColumnSchema.
		/// </summary>
		public TableSchema Table
		{
			get { return this.tableSchema; }
		}

		/// <summary>
		/// Gets a display name for the column.
		/// </summary>
		/// <returns>The name of the column.</returns>
		public override string ToString() { return this.Name; }

		/// <summary>
		/// Initializes the column from an XmlSchemaElement or XmlSchemaAttribute.
		/// </summary>
		/// <param name="xmlSchemaAnnotated">An XmlSchema object containing the attributes of the column.</param>
		private void Initialize(XmlSchemaAnnotated xmlSchemaAnnotated)
		{

			// Extract the column properties from an XmlSchemaElement.
			if (xmlSchemaAnnotated is XmlSchemaElement)
			{

				// This element is used to describe the column.
				XmlSchemaElement xmlSchemaElement = xmlSchemaAnnotated as XmlSchemaElement;

				// This information is taken directly from the known XmlSchema attributes.  The rest of the information about the
				// column needs to be extracted using unhandled attributes and element facets.
				this.name = xmlSchemaElement.Name;
				this.isNullable = xmlSchemaElement.MinOccurs == 0.0m;

				// Extract the string that contains the type information from the 'DataType' custom attribute.  This is a
				// Microsoft extension to the XML Schema definition.
				XmlAttribute dataTypeAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaElement, QualifiedName.DataType);
				if (dataTypeAttribute != null)
				{

					// The type information is stored as a string.  This will rip apart the string to get the type, 
					// assembly, version, etc. information that can be used to access the type through reflection.
					string typeName = dataTypeAttribute.Value;
					string[] parts = typeName.Split(',');

					// There's no a lot of fancy parsing going on here.  If it doesn't match the expected format, then
					// reject the type.
					if (parts.Length != 5)
						throw new Exception(string.Format("Can't analyze the data type found on line {0}, {1}", xmlSchemaElement.LineNumber, typeName));

					// This will load the assembly into memory and use reflection to get the data type that was specified 
					// in the XML file as a string.
					string assemblyFullName = string.Format("{0},{1},{2},{3}", parts[1], parts[2], parts[3], parts[4]);
					Assembly assembly = Assembly.Load(assemblyFullName);
					if (assembly == null)
						throw new Exception(string.Format("Unable to load the type {0} from assembly {1}", parts[0], assemblyFullName));
					this.dataType = assembly.GetType(parts[0]);
					if (this.dataType == null)
						throw new Exception(string.Format("Unable to load the type {0} from assembly {1}", parts[0], assemblyFullName));

				}
				else
				{

					// This will extract the simple data type and the maximum field length from the facets associated with the element.
					if (xmlSchemaElement.ElementSchemaType is XmlSchemaSimpleType)
					{
						XmlSchemaSimpleType xmlSchemaSimpleType = xmlSchemaElement.ElementSchemaType as XmlSchemaSimpleType;
						this.dataType = xmlSchemaSimpleType.Datatype.ValueType;
						if (xmlSchemaSimpleType.Content is XmlSchemaSimpleTypeRestriction)
						{
							XmlSchemaSimpleTypeRestriction xmlSchemaSimpleTypeRestriction = xmlSchemaSimpleType.Content as XmlSchemaSimpleTypeRestriction;
							foreach (XmlSchemaFacet xmlSchemaFacet in xmlSchemaSimpleTypeRestriction.Facets)
								if (xmlSchemaFacet is XmlSchemaMaxLengthFacet)
								{
									XmlSchemaMaxLengthFacet xmlSchemaMaxLengthFacet = xmlSchemaFacet as XmlSchemaMaxLengthFacet;
									this.maximumLength = Convert.ToInt32(xmlSchemaMaxLengthFacet.Value);
								}
						}
					}

				}

				// The defalt value can only be evaluated after the data type has been determined.  Otherwise, there's no way to 
				// know to which data type the default value is converted.
				this.defaultValue = DataModelSchema.ConvertValue(this.dataType, xmlSchemaElement.DefaultValue);

			}

			// Extract the column properties from an Attribute.			
			if (xmlSchemaAnnotated is XmlSchemaAttribute)
			{

				// This attribute describes the column.
				XmlSchemaAttribute xmlSchemaAttribute = xmlSchemaAnnotated as XmlSchemaAttribute;

				// This information is taken directly from the known XmlSchema attributes.  The rest of the information about the
				// column needs to be extracted using unhandled attributes and element facets.
				this.name = xmlSchemaAttribute.Name;
				this.dataType = xmlSchemaAttribute.AttributeSchemaType.Datatype.ValueType;
				this.defaultValue = DataModelSchema.ConvertValue(this.dataType, xmlSchemaAttribute.DefaultValue);

			}

			// Determine the IsEncryptedColumn property.
			XmlAttribute isColumnEncrytedAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaAnnotated,
				QualifiedName.IsEncrypted);
			this.isEncrypted = isColumnEncrytedAttribute == null ? false : Convert.ToBoolean(isColumnEncrytedAttribute.Value);

			// Determine the IsIdentityColumn property.
			XmlAttribute autoIncrementAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaAnnotated,
				QualifiedName.AutoIncrement);
			this.isAutoIncrement = autoIncrementAttribute == null ? false : Convert.ToBoolean(autoIncrementAttribute.Value);

			// Determine the IsPersistent property.
			XmlAttribute isColumnPersistentAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaAnnotated,
				QualifiedName.IsPersistent);
			this.isPersistent = isColumnPersistentAttribute == null ? true : Convert.ToBoolean(isColumnPersistentAttribute.Value);

			// Determine the AutoIncrementSeed property.
			XmlAttribute autoIncrementSeedAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaAnnotated,
				QualifiedName.AutoIncrementSeed);
			this.autoIncrementSeed = autoIncrementSeedAttribute == null ? 0 : Convert.ToInt32(autoIncrementSeedAttribute.Value);

			// Determine the AutoIncrementStop property
			XmlAttribute autoIncrementStepAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaAnnotated,
				QualifiedName.AutoIncrementStep);
			this.autoIncrementStep = autoIncrementStepAttribute == null ? 0 : Convert.ToInt32(autoIncrementStepAttribute.Value);

		}

	}

}

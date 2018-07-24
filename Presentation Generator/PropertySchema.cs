namespace FluidTrade.PresentationGenerator
{

	using System;
	using System.Collections.Generic;
	using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// A Schema of a table.
	/// </summary>
	public class PropertySchema
	{

		// Private Instance Fields
		private ClassSchema classSchema;
		private Boolean isSimpleType;
		private Decimal maxOccurs;
		private Decimal minOccurs;
		private String name;
		private List<QuerySchema> querySchemaList;
		private List<SnippetSchema> snippetSchemaList;
		private String type;

		/// <summary>
		/// Create a table schema from the XML Schema specification.
		/// </summary>
		/// <param name="presentationSchema">The data model to which this table belongs.</param>
		/// <param name="xmlSchemaElement">The root of the XmlSchema element that describes the table.</param>
		public PropertySchema(ClassSchema classSchema, XmlSchemaElement xmlSchemaElement)
		{

			// Initialize the object.
			this.classSchema = classSchema;
			this.isSimpleType = true;
			this.maxOccurs = xmlSchemaElement.MaxOccurs;
			this.minOccurs = xmlSchemaElement.MinOccurs;
			this.name = xmlSchemaElement.Name;
			this.querySchemaList = new List<QuerySchema>();
			this.snippetSchemaList = new List<SnippetSchema>();
			this.type = String.Empty;

			// Initialize the data type from a complex type description.  The complex type can describe either a non-standard XML 
			// data type through the unhandled 'DataType' specification, or it can reference a data type that's declared in the
			// schema.
			if (xmlSchemaElement.ElementSchemaType is XmlSchemaComplexType)
			{
				XmlSchemaComplexType xmlSchemaComplexType = xmlSchemaElement.ElementSchemaType as XmlSchemaComplexType;
				if (xmlSchemaComplexType.QualifiedName == QualifiedName.AnyType)
				{
					foreach (XmlAttribute xmlAttribute in xmlSchemaElement.UnhandledAttributes)
						if (QualifiedName.DataType == new XmlQualifiedName(xmlAttribute.LocalName, xmlAttribute.NamespaceURI))
						{
							this.isSimpleType = true;
							this.type = xmlAttribute.Value;
						}
				}
				else
				{
					this.type = xmlSchemaComplexType.QualifiedName.ToString();
					this.isSimpleType = false;
				}

			}

			// Initialize the data type from a simple type description.
			if (xmlSchemaElement.ElementSchemaType is XmlSchemaSimpleType)
			{
				XmlSchemaSimpleType xmlSchemaSimpleType = xmlSchemaElement.ElementSchemaType as XmlSchemaSimpleType;
				this.isSimpleType = true;
				this.type = xmlSchemaSimpleType.Datatype.ValueType.FullName;
			}

			// This will run through each of the custom fields associated with the element and create fields that will sort, filter
			// and transform the data from the source list into the destination list.
			if (xmlSchemaElement.Annotation != null)
				foreach (XmlSchemaObject xmlSchemaObject in xmlSchemaElement.Annotation.Items)
					if (xmlSchemaObject is XmlSchemaAppInfo)
					{
						XmlSchemaAppInfo xmlSchemaAppInfo = xmlSchemaObject as XmlSchemaAppInfo;
						foreach (XmlNode xmlNode in xmlSchemaAppInfo.Markup)
						{
							XmlQualifiedName nodeName = new XmlQualifiedName(xmlNode.LocalName, xmlNode.NamespaceURI);
							if (QualifiedName.Where == nodeName)
								this.querySchemaList.Add(new WhereSchema(this, xmlNode));
							if (QualifiedName.Select == nodeName)
								this.querySchemaList.Add(new SelectSchema(this, xmlNode));
							if (QualifiedName.OrderBy == nodeName)
								this.querySchemaList.Add(new OrderBySchema(this, xmlNode));
							if (QualifiedName.Snippet == nodeName)
								this.snippetSchemaList.Add(new SnippetSchema(xmlNode));
							if (QualifiedName.Setter == nodeName)
								this.snippetSchemaList.Add(new SetterSchema(xmlNode));
						}
					}

		}

		/// <summary>
		/// Gets an indication of whether the type is a known CLR type or declared in the schemas.
		/// </summary>
		public Boolean IsSimpleType
		{
			get { return this.isSimpleType; }
		}

		/// <summary>
		/// Gets the maximum number of times this property can occur.
		/// </summary>
		public Decimal MaxOccurs
		{
			get { return this.maxOccurs; }
		}

		/// <summary>
		/// Gets the minimum number of times this property can occur.
		/// </summary>
		public Decimal MinOccurs
		{
			get { return this.minOccurs; }
		}

		/// <summary>
		/// Gets the name of the property.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// The full name of the property's type.
		/// </summary>
		public String Type
		{
			get { return this.Class.Presentation.ResolveQualifiedTypeName(this.type); }
		}
	
		/// <summary>
		/// Gets a list of QueriesSchemas items associated with this property.
		/// </summary>
		public List<QuerySchema> Queries
		{
			get { return this.querySchemaList; }
		}

		/// <summary>
		/// Gets a list of SetterSchema items associated with this property.
		/// </summary>
		public List<SnippetSchema> Snippets
		{
			get { return this.snippetSchemaList; }
		}

		/// <summary>
		/// Gets the parent ClassSchema for this property.
		/// </summary>
		public ClassSchema Class
		{
			get { return this.classSchema; }
		}

		/// <summary>
		/// Gets the previous source in the list of QuerySchema items.
		/// </summary>
		/// <param name="querySchema">A description of a query.</param>
		/// <returns>The name of the previous source in the list of queries associated with ths property.</returns>
		internal String GetPreviousSource(QuerySchema querySchema)
		{

			// When an item is found that matches the given query item, return the name of the previous query item.  This is used 
			// to chain together query items with the implicit source coming from the previous item in the list.
			for (int queryIndex = 1; queryIndex < this.querySchemaList.Count; queryIndex++)
			{
				QuerySchema innerQuerySchema = this.querySchemaList[queryIndex];
				if (innerQuerySchema == querySchema)
					return this.querySchemaList[queryIndex - 1].Name;
			}

			// An attempt was made to use a query item without an explicit source or an implicit previous source.
			throw new Exception(String.Format("The source is not defined for Query {0}", querySchema.Name));

		}

		/// <summary>
		/// Gets the resulting type of a query.
		/// </summary>
		/// <param name="querySchema">A description of a query.</param>
		/// <returns>The input type for the given query as determined by the source.</returns>
		internal String GetSourceType(QuerySchema querySchema)
		{

			// Search through the sibling queries until the source is found and return the resulting type of that query.
			foreach (QuerySchema innerQuerySchema in this.querySchemaList)
				if (innerQuerySchema.Name == querySchema.Source)
					return innerQuerySchema.ResultType;

			// An attempt was made to use and implicit source type from a sibling query that doesn't exist.
			throw new Exception(String.Format("The source is not defined for Query {0}", querySchema.Name));

		}

		/// <summary>
		/// The display text of the object.
		/// </summary>
		/// <returns></returns>
		public override string ToString() { return this.Name; }

	}

}

namespace FluidTrade.PresentationGenerator
{

    using System;
    using System.Xml;
    using FluidTrade.Core;

    /// <summary>
	/// An abstract description of an element in a collection of filters, sorts and transforms.
	/// </summary>
	public class QuerySchema
	{

		// Private Instance Fields
		private String name;
		private PropertySchema propertySchema;
		private String resultType;
		private String source;
		private String sourceType;

		/// <summary>
		/// Create an abstract description of an element in a collection of filters, sorts and transforms.
		/// </summary>
		/// <param name="propertySchema">A description of the property.</param>
		/// <param name="xmlNode">The XML Schema extensions that describe the query.</param>
		public QuerySchema(PropertySchema propertySchema, XmlNode xmlNode)
		{

			// Initialize the object.
			this.propertySchema = propertySchema;

			// Extract the name of the query.  This will default to an randomly generated name of not explicitly stated in the 
			// schema.
			XmlAttribute nameAttribute = xmlNode.Attributes[QualifiedName.VariableName.Name, QualifiedName.VariableName.Namespace];
			this.name = nameAttribute == null ? RandomVariableName.NewName() : nameAttribute.Value;

			// Extract the resulting type.  This will default to the property type if not explicitly stated in the schema.
			XmlAttribute resultTypeAttribute = xmlNode.Attributes[QualifiedName.ResultType.Name, QualifiedName.ResultType.Namespace];
			this.resultType = resultTypeAttribute == null ? String.Empty : resultTypeAttribute.Value;

			// Extract the source of data for the query.  This will default to the result of the previous query if not explicitly
			// stated in the schema.
			XmlAttribute sourceAttribute = xmlNode.Attributes[QualifiedName.Source.Name, QualifiedName.Source.Namespace];
			this.source = sourceAttribute == null ? String.Empty : sourceAttribute.Value;

			// Extract the type of the source data.  This will default to the type of the source of data for this query if not 
			// explicitly stated in the schema.
			XmlAttribute sourceTypeAttribute = xmlNode.Attributes[QualifiedName.SourceType.Name, QualifiedName.SourceType.Namespace];
			this.sourceType = sourceTypeAttribute == null ? String.Empty : sourceTypeAttribute.Value;

		}

		/// <summary>
		/// Gets the name of this query operation.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Gets the parent PropertySchema of this query operation.
		/// </summary>
		public PropertySchema Property
		{
			get { return this.propertySchema; }
		}

		/// <summary>
		/// Gets the result type of this query operation.
		/// </summary>
		public string ResultType
		{
			get
			{
				return this.sourceType == String.Empty ? this.propertySchema.Type :
					this.propertySchema.Class.Presentation.ResolvePrefixedTypeName(this.sourceType);
			}
		}

		/// <summary>
		/// Gets the source of data for the query operation.
		/// </summary>
		public String Source
		{
			get { return this.source == String.Empty ? this.propertySchema.GetPreviousSource(this) : this.source; }
		}

		/// <summary>
		/// Gets the parent PresentationSchema for this table.
		/// </summary>
		public string SourceType
		{
			get
			{
				return this.sourceType == String.Empty ? this.propertySchema.GetSourceType(this) :
					this.propertySchema.Class.Presentation.ResolvePrefixedTypeName(this.sourceType);
			}
		}

		/// <summary>
		/// The display text of the object.
		/// </summary>
		/// <returns></returns>
		public override string ToString() { return this.Name; }

	}

}

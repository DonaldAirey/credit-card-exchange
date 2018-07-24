namespace FluidTrade.PresentationGenerator
{

	using System;
    using System.Xml;

    /// <summary>
	/// A description of a LINQ clause that selectively removes items from a data source.
	/// </summary>
	public class WhereSchema : QuerySchema
	{

		// Private Instance Fields
		private String predicate;

		/// <summary>
		/// Creates a description of a LINQ clause that selectively removes items from a data source.
		/// </summary>
		/// <param name="propertySchema">A description of the property.</param>
		/// <param name="xmlNode">The XML Schema extensions that describe the query.</param>
		public WhereSchema(PropertySchema propertySchema, XmlNode xmlNode)
			: base(propertySchema, xmlNode)
		{

			// Extract the name of the predicate (the method that selectively qualifies records) from the XML Schema extension.
			XmlAttribute predicateAttribute = xmlNode.Attributes[QualifiedName.Predicate.Name, QualifiedName.Predicate.Namespace];
			this.predicate = predicateAttribute.Value;

		}

		/// <summary>
		/// Gets the name of the method that selectively qualifies records in the data source.
		/// </summary>
		public String Predicate
		{
			get { return this.predicate; }
		}

	}

}

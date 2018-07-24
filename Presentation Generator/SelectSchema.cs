namespace FluidTrade.PresentationGenerator
{

	using System;
    using System.Xml;

    /// <summary>
	/// An abstract description of an operation that selects one object from another.
	/// </summary>
	public class SelectSchema : QuerySchema
	{

		// Private Instance Fields
		private String selector;

		/// <summary>
		/// Create an abstract description of an operation that selects one object from another.
		/// </summary>
		/// <param name="propertySchema">A description of the property.</param>
		/// <param name="xmlNode">The XML Schema extensions that describe the query.</param>
		public SelectSchema(PropertySchema propertySchema, XmlNode xmlNode)
			: base(propertySchema, xmlNode)
		{

			// Extract the selector (the operation that provides the transformation of one object to another) from the XML 
			// extension.
			XmlAttribute selectorAttribute = xmlNode.Attributes[QualifiedName.Selector.Name, QualifiedName.Selector.Namespace];
			this.selector = selectorAttribute.Value;

		}

		/// <summary>
		/// Gets the name of the operation that transforms data from the data source to a target.
		/// </summary>
		public String Selector
		{
			get { return this.Property.Class.Presentation.ResolvePrefixedTypeName(this.selector); }
		}

	}

}

namespace FluidTrade.PresentationGenerator
{

	using System;
    using System.Xml;

    /// <summary>
	/// A description of a property setter.
	/// </summary>
	public class SetterSchema : SnippetSchema
	{

		// Private Instance Fields
		private String property;

		/// <summary>
		/// Creates a description of a property setter.
		/// </summary>
		/// <param name="xmlNode">The XML Schema extensions that describe the query.</param>
		public SetterSchema(XmlNode xmlNode) : base(xmlNode)
		{

			// Extract the name of the property to be set.
			XmlAttribute propertyAttribute = xmlNode.Attributes[QualifiedName.Property.Name, QualifiedName.Property.Namespace];
			this.property = propertyAttribute.Value;

			// Extract the value of the property either from the attribute of from the content of the element.
			XmlAttribute valueAttribute = xmlNode.Attributes[QualifiedName.Value.Name, QualifiedName.Value.Namespace];
			if (valueAttribute != null)
				base.value = valueAttribute.Value;

		}

		/// <summary>
		/// Gets the name of the property to be set.
		/// </summary>
		public String Property
		{
			get { return this.property; }
		}

	}

}

namespace FluidTrade.PresentationGenerator
{

	using System;
    using System.Xml;

    /// <summary>
	/// A description of an 'OrderBy' clause of a LINQ query.
	/// </summary>
	public class OrderBySchema : QuerySchema
	{

		// Private Instance Fields
		private String comparer;
		private String keySelector;
		private String keyType;

		/// <summary>
		/// Creates a description of an 'OrderBy' clause of a LINQ query.
		/// </summary>
		/// <param name="propertySchema">A description of a property.</param>
		/// <param name="xmlNode">The XML Schema extension describing the 'OrderBy' clause.</param>
		public OrderBySchema(PropertySchema propertySchema, XmlNode xmlNode)
			: base(propertySchema, xmlNode)
		{

			// Extract the 'Comparer' clause.
			XmlAttribute comparerAttribute = xmlNode.Attributes[QualifiedName.Comparer.Name, QualifiedName.Comparer.Namespace];
			this.comparer = comparerAttribute.Value;
	
			// Extract the 'KeySelector' clause.
			XmlAttribute keySelectorAttribute = xmlNode.Attributes[QualifiedName.KeySelector.Name, QualifiedName.KeySelector.Namespace];
			this.keySelector = keySelectorAttribute.Value;

			// Extract the 'KeyType' clause.
			XmlAttribute keyTypeAttribute = xmlNode.Attributes[QualifiedName.KeyType.Name, QualifiedName.KeyType.Namespace];
			this.keyType = keyTypeAttribute.Value;

		}

		/// <summary>
		/// Gets the name of the Comparer.
		/// </summary>
		public String Comparer
		{
			get { return this.comparer; }
		}

		/// <summary>
		/// Gets the full name of the type of the Key used to select the results.
		/// </summary>
		public String KeySelector
		{
			get { return this.Property.Class.Presentation.ResolvePropertyName(this.keySelector); }
		}

		/// <summary>
		/// Gets the name of the key used to select values from the source.
		/// </summary>
		public String KeyType
		{
			get { return this.keyType == string.Empty ? this.SourceType : this.Property.Class.Presentation.ResolvePrefixedTypeName(this.keyType); }
		}

	}

}

namespace FluidTrade.PresentationGenerator
{

	using System;
	using System.Collections.Generic;
	using System.Xml;
	using System.Xml.Schema;

	/// <summary>
	/// The description of a class.
	/// </summary>
	public class ClassSchema
	{

		// Private Instance Fields
		private PresentationSchema presentationSchema;
		private List<PropertySchema> propertyList;
		private ConstructorSchema constructorSchema;
		private String name;
		private String targetNamespace;
		private String type;

		/// <summary>
		/// The description of a class.
		/// </summary>
		/// <param name="presentationSchema">The PresentationSchema to which this class belongs.</param>
		/// <param name="xmlSchemaComplexType">The XML Schema description of the class.</param>
		public ClassSchema(PresentationSchema presentationSchema, XmlSchemaComplexType xmlSchemaComplexType)
		{

			// Initialize the object.
			this.name = xmlSchemaComplexType.QualifiedName.Name;
			this.presentationSchema = presentationSchema;
			this.propertyList = new List<PropertySchema>();
			this.targetNamespace = xmlSchemaComplexType.QualifiedName.Namespace;
			this.type = xmlSchemaComplexType.QualifiedName.ToString();

			// This will create the list of properties of this type.
			if (xmlSchemaComplexType.Particle is XmlSchemaSequence)
			{
				XmlSchemaSequence xmlSchemaSequence = xmlSchemaComplexType.Particle as XmlSchemaSequence;
				foreach (XmlSchemaObject item in xmlSchemaSequence.Items)
					if (item is XmlSchemaElement)
						this.propertyList.Add(new PropertySchema(this, item as XmlSchemaElement));
			}

			// This will create and initialize a description of the constructor for the class from the XML Schema extensions.
			this.constructorSchema = new ConstructorSchema(this);
			if (xmlSchemaComplexType.Annotation != null)
				foreach (XmlSchemaObject xmlSchemaObject in xmlSchemaComplexType.Annotation.Items)
					if (xmlSchemaObject is XmlSchemaAppInfo)
					{
						XmlSchemaAppInfo xmlSchemaAppInfo = xmlSchemaObject as XmlSchemaAppInfo;
						foreach (XmlNode constructorNode in xmlSchemaAppInfo.Markup)
							if (QualifiedName.Constructor == new XmlQualifiedName(constructorNode.LocalName, constructorNode.NamespaceURI))
								this.constructorSchema = new ConstructorSchema(this, constructorNode);
					}

		}

		/// <summary>
		/// Gets a description of the constructor of this class.
		/// </summary>
		public ConstructorSchema Constructor
		{
			get { return this.constructorSchema; }
		}

		/// <summary>
		/// Gets the name of this class.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Gets the parent PresentationSchema for this class.
		/// </summary>
		public PresentationSchema Presentation
		{
			get { return this.presentationSchema; }
		}

		/// <summary>
		/// A collection of properties of this class.
		/// </summary>
		public List<PropertySchema> Properties
		{
			get { return this.propertyList; }
		}

		/// <summary>
		/// The full name of the type of this class.
		/// </summary>
		public String Type
		{
			get { return this.Presentation.ResolveQualifiedTypeName(this.type); }
		}

		/// <summary>
		/// Gets the target namespace of the original schema used to build this class.
		/// </summary>
		public String TargetNamespace
		{
			get { return this.presentationSchema.ResolveNamespace(this.targetNamespace); }
		}

		/// <summary>
		/// The display text of the object.
		/// </summary>
		/// <returns></returns>
		public override string ToString() { return this.Name; }

	}

}

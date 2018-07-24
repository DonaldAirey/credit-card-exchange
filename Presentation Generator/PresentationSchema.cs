namespace FluidTrade.PresentationGenerator
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using FluidTrade.Core;

	/// <summary>
	/// Describes a data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class PresentationSchema
	{

		// Private Instance Fields
		private List<ClassSchema> classList;
		private List<String> importList;
		private Dictionary<String, String> namespaceNamespaceDictionary;
		private Dictionary<String, String> prefixNamespaceDictionary;
		private String sourceNamespace;
		private String targetNamespace;

		/// <summary>
		/// Constructs a schema from the contents of an XML specification.
		/// </summary>
		/// <param name="fileContents">The contents of a file that specifies the schema in XML.</param>
		public PresentationSchema(string inputFilePath, string fileContents)
		{

			// Initialize the object
			this.classList = new List<ClassSchema>();
			this.namespaceNamespaceDictionary = new Dictionary<string, string>();
			this.prefixNamespaceDictionary = new Dictionary<string, string>();

			// Compile the schema to resolve all the schema types.
			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(fileContents));
			XmlSchema primarySchema = XmlSchema.Read(xmlTextReader, new ValidationEventHandler(ValidationCallback));
			xmlSchemaSet.XmlResolver = new FileResolver(Path.GetDirectoryName(inputFilePath));
			xmlSchemaSet.Add(primarySchema);
			xmlSchemaSet.Compile();

			// These values are used to create an association between the XML namespace and the CLR namespace in the target 
			// assembly.
			this.sourceNamespace = primarySchema.TargetNamespace;
			this.targetNamespace = this.GetTargetNamespace(primarySchema);

			// This creates a list of import directives required by the output assembly.
			this.importList = GetUsingDirectives(primarySchema);

			// Compiling a schema can involved zero or more imported schemas.  The compilation will allow each of the schemas to
			// reference classes declared in the other schemas.  This loop will cycle through all the schemas creating ClassSchema
			// descriptions of every type found.  Creating these schemas makes it easier to create a CodeDOM and allows for the
			// generated code to reference the classes that will be created from this code.
			foreach (XmlSchema xmlSchema in xmlSchemaSet.Schemas())
			{

				// This creates a dictionary that is used to resolve an XML namespace given the prefix to that namespace.
				XmlQualifiedName[] qualifiedNames = xmlSchema.Namespaces.ToArray();
				foreach (XmlQualifiedName qualifiedName in xmlSchema.Namespaces.ToArray())
					if (!this.prefixNamespaceDictionary.ContainsKey(qualifiedName.Name))
						this.prefixNamespaceDictionary.Add(qualifiedName.Name, qualifiedName.Namespace);

				// This creates a dictionary that is used to translate an XML namespace into a CLR namespace.  When this dictionary
				// is combined with the prefixNamespaceDictionary, any type described as "ns:type" can be resolved to a CLR type.
				this.namespaceNamespaceDictionary.Add(xmlSchema.TargetNamespace, this.GetTargetNamespace(xmlSchema));

				// All the complex types found in the schema are turned into ClassSchema descriptions.  These descriptions are used
				// to generate the actual CLR versions of the classes.
				foreach (XmlSchemaObject xmlSchemaObject in xmlSchema.Items)
					if (xmlSchemaObject is XmlSchemaComplexType)
						this.classList.Add(new ClassSchema(this, xmlSchemaObject as XmlSchemaComplexType));

			}

		}

		/// <summary>
		/// Gets a collect of the classes.
		/// </summary>
		public List<ClassSchema> Classes
		{
			get { return this.classList; }
		}

		/// <summary>
		/// Gets a collection of imported namespaces.
		/// </summary>
		public List<String> Imports
		{
			get { return this.importList; }
		}

		/// <summary>
		/// Gets the source XML namespace.
		/// </summary>
		public string SourceNamespace
		{
			get { return this.sourceNamespace; }
		}

		/// <summary>
		/// The target CLR namespace.
		/// </summary>
		public string TargetNamespace
		{
			get { return this.targetNamespace; }
		}

		/// <summary>
		/// Gets the application specific markup extensions to the XML Schema.
		/// </summary>
		/// <param name="xmlSchema">The schema.</param>
		/// <returns>A set of nodes that describe the application specific extensions to the standard XML Schema.</returns>
		private XmlNode[] GetMarkup(XmlSchema xmlSchema)
		{

			// Search through all the items in the schema looking for annotations.  The annotations contain extensions to the
			// standard XML Schema.  In this case, such items as constructor parameters and CLR namespace import statements are
			// added to the schema to help produce the output assembly.
			foreach (XmlSchemaObject schemaItem in xmlSchema.Items)
				if (schemaItem is XmlSchemaAnnotation)
				{
					XmlSchemaAnnotation xmlSchemaAnnotation = schemaItem as XmlSchemaAnnotation;
					foreach (XmlSchemaObject annotationItem in xmlSchemaAnnotation.Items)
						if (annotationItem is XmlSchemaAppInfo)
						{
							XmlSchemaAppInfo xmlSchemaAppInfo = annotationItem as XmlSchemaAppInfo;
							return xmlSchemaAppInfo.Markup;
						}
				}

			// At this point the schema has no application specific markup.
			return new XmlNode[0];

		}

		/// <summary>
		/// Gets the target namespace from the custom attributes.
		/// </summary>
		/// <param name="xmlSchemaAnnotation">An node containing compliation instructions.</param>
		private String GetTargetNamespace(XmlSchema xmlSchema)
		{

			// This will parse the XML extensions looking for a description of the destination CLR namespace.
			foreach (XmlNode xmlNode in GetMarkup(xmlSchema))
				if (xmlNode is XmlElement)
				{
					XmlElement xmlElement = xmlNode as XmlElement;
					if (QualifiedName.GeneratedCode == new XmlQualifiedName(xmlElement.LocalName, xmlElement.NamespaceURI))
					{
						XmlAttribute destinationNamespace = xmlElement.Attributes[QualifiedName.DestinationNamespace.Name,
							QualifiedName.DestinationNamespace.Namespace];
						if (destinationNamespace != null)
							return destinationNamespace.Value;
					}
				}

			// At this point, no description of the destination CLR namespace was provided in the XML extensions.
			return string.Empty;

		}

		/// <summary>
		/// Parse the top level application annotations.
		/// </summary>
		/// <param name="xmlSchemaAnnotation">An node containing compliation instructions.</param>
		private List<String> GetUsingDirectives(XmlSchema xmlSchema)
		{

			// This will construct a list of imported CLR namespaces required by the destination assembly.
			List<String> usingList = new List<string>();
			foreach (XmlNode xmlNode in GetMarkup(xmlSchema))
			{
				if (xmlNode is XmlElement)
				{
					XmlElement xmlElement = xmlNode as XmlElement;
					if (QualifiedName.Using == new XmlQualifiedName(xmlElement.LocalName, xmlElement.NamespaceURI))
					{
						XmlAttribute namespaceAttribute = xmlElement.Attributes[QualifiedName.Namespace.Name, QualifiedName.Namespace.Namespace];
						if (namespaceAttribute != null)
							usingList.Add(namespaceAttribute.Value);
					}
				}
			}
			return usingList;

		}

		/// <summary>
		/// Resolves an XML namespace to a CLR namespace.
		/// </summary>
		/// <param name="sourceNamespace">The XML namespace.</param>
		/// <returns>The equivalent CLR namespace.</returns>
		public String ResolveNamespace(String sourceNamespace)
		{

			// This will return the CLR namespace mapped to the XML namespace.
			String destinationNamespace = string.Empty;
			if (this.namespaceNamespaceDictionary.TryGetValue(sourceNamespace, out destinationNamespace))
				return destinationNamespace;

			// At this point, there is no mapping for the given XML namespace.
			return String.Empty;

		}

		/// <summary>
		/// Resolves a partially qualified type name to the CLR equivalent.
		/// </summary>
		/// <param name="partialName">The prefixed name of an XML type.</param>
		/// <returns>The CLR equivalent of the given type name.</returns>
		public String ResolvePrefixedTypeName(String partialTypeName)
		{

			// The presence of a colon ':' indicates that an internal type name has been specified.  Conversely, the absense
			// indicates that a global type name has been specified.  Global type names do not need to be resolved.
			string[] typeParts = partialTypeName.Split(':');
			if (typeParts.Length == 1)
				return partialTypeName;

			// Unlike a fully qualified XML name, the prefixed type name requires an extra step to translate between the previx
			// and the XML namespace.  Once that is found, the CLR namespace can be applied to the given type.
			String targetNamespace = string.Empty;
			if (this.prefixNamespaceDictionary.TryGetValue(typeParts[0], out targetNamespace))
			{
				String destinationNamespace = string.Empty;
				if (this.namespaceNamespaceDictionary.TryGetValue(targetNamespace, out destinationNamespace))
					return destinationNamespace + '.' + typeParts[1];
			}

			// At this point there was no resolution possible of the given type name.
			return String.Empty;

		}

		/// <summary>
		/// Resolves an XML property name to a fully qualified CLR property name.
		/// </summary>
		/// <param name="partialPropertyName">The prefixed name of a property.</param>
		/// <returns>The CLR equivalent of the given property name.</returns>
		public String ResolvePropertyName(String partialPropertyName)
		{

			// The presence of a colon ':' indicates that an internal XML property has been specified.  Conversely, the absense
			// indicates that a global property name has been specified.  Global property names do not need to be resolved.
			string[] selectorParts = partialPropertyName.Split(':');
			if (selectorParts.Length == 1)
				return partialPropertyName;

			// The idea here is to reassemble the prefixed type name and resolve it.  The result is added to the original property 
			// name for the fully qualified CLR property name.
			string[] classParts = selectorParts[1].Split('.');
			string className = classParts[0];
			for (int index = 1; index < classParts.Length - 1; index++)
				className += '.' + classParts[index];
			return ResolvePrefixedTypeName(string.Format("{0}:{1}", selectorParts[0], className)) + '.' + classParts[classParts.Length - 1];

		}

		/// <summary>
		/// Resolves a qualified type name to the CLR equivalent.
		/// </summary>
		/// <param name="xmlTypeName">A fully qualified XML type name.</param>
		/// <returns>The CLR equivalent of the given type name.</returns>
		public String ResolveQualifiedTypeName(String typeName)
		{

			// The presence of a colon ':' indicates that an internal type name has been specified.  Conversely, the absense 
			// indicates that a global type name has been specified.  Global names do not need to be resolved.
			string[] typeParts = typeName.Split(':');
			if (typeParts.Length == 1)
				return typeName;

			// This puts the target namespace back together again after the disassembly above.  Note that the last element is the
			// actual name of the given type and is not part of a namespace specification.  This target namespace is passed through
			// the dictionaries in an attempt to find the CLR equivalent namespace.
			String targetNamespace = typeParts[0];
			for (int index = 1; index < typeParts.Length - 1; index++)
				targetNamespace += ':' + typeParts[index];

			// This effectively replaces the XML namespace with a CLR namespace and returns a fully qualified CLR type name.
			String destinationNamespace = string.Empty;
			if (this.namespaceNamespaceDictionary.TryGetValue(targetNamespace, out destinationNamespace))
				return destinationNamespace + '.' + typeParts[typeParts.Length - 1];

			// At this point there was no resolution possible of the given type name.
			return typeName;

		}

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

	}

}

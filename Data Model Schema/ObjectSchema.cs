namespace FluidTrade.Core
{

    using System;
    using System.Xml;
    using System.Xml.Schema;

	public class ObjectSchema
	{

		// Public Readonly Fields
		public DataModelSchema DataModelSchema;

		// Private Instance Fields
		private XmlSchemaObject xmlSchemaObject;

		public ObjectSchema(DataModelSchema dataModelSchema, XmlSchemaObject xmlSchemaObject)
		{

			// Initialize the object.
			this.DataModelSchema = dataModelSchema;
			this.xmlSchemaObject = xmlSchemaObject;

		}

		public XmlSchemaObject XmlSchemaObject
		{
			get { return this.xmlSchemaObject; }
		}

		protected XmlSchema RootSchema
		{

			get
			{

				XmlSchemaObject rootObject = this.xmlSchemaObject;
				while (rootObject.Parent != null)
					rootObject = rootObject.Parent;

				return rootObject as XmlSchema;

			}

		}

		/// <summary>
		/// Searches an element for unhandled extensions to the XmlSchema schema.
		/// </summary>
		/// <param name="xmlSchemaElement">The element to be searched.</param>
		/// <param name="xmlQualifiedName">The qualified name of an attribute.</param>
		/// <returns>An attribute with the qualified name, or null if there is no such attribute.</returns>
		public static XmlAttribute GetUnhandledAttribute(XmlSchemaAnnotated xmlSchemaAnnotated, XmlQualifiedName xmlQualifiedName)
		{

			// This will search any unhandled attributes associated with the element and return any that match the given qualified
			// name.
			if (xmlSchemaAnnotated.UnhandledAttributes != null)
				foreach (XmlAttribute xmlAttribute in xmlSchemaAnnotated.UnhandledAttributes)
				{
					XmlQualifiedName attributeName = new XmlQualifiedName(xmlAttribute.LocalName, xmlAttribute.NamespaceURI);
					if (attributeName == xmlQualifiedName)
						return xmlAttribute;
				}

			// At this point, there is no unhandled attribute that matches the given name.
			return null;

		}

		/// <summary>
		/// Determines if a schema element is the root data set.
		/// </summary>
		/// <param name="xmlSchemaObject">The XmlSchemaObject to be examined.</param>
		/// <returns>true if the object contains an 'IsDataSet' attribute.</returns>
		public static bool IsDataSetElement(XmlSchemaObject xmlSchemaObject)
		{

			// This searches the element for an extension to the Standard Schema that indicates the schema is a Microsoft DataSet.
			if (xmlSchemaObject is XmlSchemaElement)
			{
				XmlAttribute xmlAttribute = GetUnhandledAttribute(xmlSchemaObject as XmlSchemaElement, QualifiedName.IsDataSet);
				if (xmlAttribute != null)
					return Convert.ToBoolean(xmlAttribute.Value);
			}

			// This is not the DataSet element.
			return false;

		}

		public bool ContainsColumn(XmlSchemaComplexType xmlSchemaComplexType, XmlSchemaObject columnObject)
		{

			XmlQualifiedName columnName = columnObject is XmlSchemaAttribute ?
				(columnObject as XmlSchemaAttribute).QualifiedName : columnObject is XmlSchemaElement ?
				(columnObject as XmlSchemaElement).QualifiedName : XmlQualifiedName.Empty;

			// Columns can be specified as attributes.
			foreach (XmlSchemaAttribute xmlSchemaAttribute in xmlSchemaComplexType.Attributes)
				if (xmlSchemaAttribute.QualifiedName == columnName)
					return true;

			// Comlex content extends a base class.  The ComplexContent is mutually exclusive of the Particle, so if the 
			// particle is empty, the ComplexContent is present and should be parsed for columns.
			if (xmlSchemaComplexType.Particle == null)
			{

				// The Comlex Content describes an extension of a base class.
				if (xmlSchemaComplexType.ContentModel is XmlSchemaComplexContent)
				{

					// Strongly type the XmlSchemaContent.
					XmlSchemaComplexContent xmlSchemaComplexContent = xmlSchemaComplexType.ContentModel as XmlSchemaComplexContent;

					// A complex content can be derived by extension (adding columns) or restriction (removing columns).  This
					// section will look for the extensions to the base class.
					if (xmlSchemaComplexContent.Content is XmlSchemaComplexContentExtension)
					{

						// The Complex Content Extension describes a base class and the additional columns that make up a 
						// derived class.  This section will recursively collect the columns from the base class and then parse
						// out the extra columns in-line.
						XmlSchemaComplexContentExtension xmlSchemaComplexContentExtension =
							xmlSchemaComplexContent.Content as XmlSchemaComplexContentExtension;

						// The additional columns for this inherited table are found on the <Sequence> node that follows the
						// <Extension> node.
						if (xmlSchemaComplexContentExtension.Particle is XmlSchemaSequence)
						{

							// Strongly type the XmlSchemaSequence
							XmlSchemaSequence xmlSchemaSequence = xmlSchemaComplexContentExtension.Particle as XmlSchemaSequence;

							// Read through the sequence and replace any column from an inherited class with the column in the
							// derived class.  Also note that the columns are added in alphabetical order to give some amount of
							// predictability to the way the parameter lists are constructed when there are several layers of
							// inheritance.
							foreach (XmlSchemaObject xmlSchemaObject in xmlSchemaSequence.Items)
								if ((xmlSchemaObject as XmlSchemaElement).QualifiedName == columnName)
									return true;

						}

						// The Complex Content can also contain attributes that describe columns.
						foreach (XmlSchemaAttribute xmlSchemaAttribute in xmlSchemaComplexContentExtension.Attributes)
							if (xmlSchemaAttribute.QualifiedName == columnName)
								return true;

					}

				}

			}
			else
			{

				// This section will parse the simple particle.  The particle has no inheritiance to evaluate.
				if (xmlSchemaComplexType.Particle is XmlSchemaSequence)
				{

					// Strongly type the XmlSchemaSequence member.
					XmlSchemaSequence xmlSchemaSequence = xmlSchemaComplexType.Particle as XmlSchemaSequence;

					// Each XmlSchemaElement on the Particle describes a column.
					foreach (XmlSchemaObject xmlSchemaObject in xmlSchemaSequence.Items)
						if ((xmlSchemaObject as XmlSchemaElement).QualifiedName == columnName)
							return true;

				}

				// The ComplexType can also have attributes that describe table columns.
				foreach (XmlSchemaAttribute xmlSchemaAttribute in xmlSchemaComplexType.Attributes)
					if (xmlSchemaAttribute.QualifiedName == columnName)
						return true;

			}

			return false;

		}

		public XmlSchemaElement FindSelector(XmlSchemaIdentityConstraint xmlSchemaIdentityConstraint)
		{

			string[] tokens = xmlSchemaIdentityConstraint.Selector.XPath.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
			XmlSchemaObject navigationObject = xmlSchemaIdentityConstraint.Parent;
			foreach (string token in tokens)
			{
				switch (token)
				{
				case ".":

					break;

				case "..":

					navigationObject = navigationObject.Parent;
					break;

				default:

					string[] qNameParts = token.Split(':');
					XmlQualifiedName xmlQualifiedName = qNameParts.Length == 1 ?
						new XmlQualifiedName(qNameParts[0]) : new XmlQualifiedName(qNameParts[1],
						this.DataModelSchema.XmlNamespaceManager.LookupNamespace(qNameParts[0]));
						navigationObject = NavigateSchema(navigationObject, xmlQualifiedName);
					break;

				}

			}

			if (navigationObject is XmlSchemaElement)
				return navigationObject as XmlSchemaElement;

			return null;

		}

		private static XmlSchemaObject NavigateSchema(XmlSchemaObject navigationObject, XmlQualifiedName xmlQualifiedName)
		{

			XmlSchemaElement parentElement = navigationObject as XmlSchemaElement;
			if (parentElement.SchemaTypeName == XmlQualifiedName.Empty)
			{
				if (parentElement.SchemaType is XmlSchemaComplexType)
				{
					XmlSchemaComplexType xmlSchemaComplexType = parentElement.SchemaType as XmlSchemaComplexType;
					if (xmlSchemaComplexType.Particle is XmlSchemaChoice)
					{
						XmlSchemaChoice xmlSchemaChoice = xmlSchemaComplexType.Particle as XmlSchemaChoice;
						foreach (XmlSchemaObject xmlSchemaObject in xmlSchemaChoice.Items)
						{
							if (xmlSchemaObject is XmlSchemaElement)
							{
								XmlSchemaElement xmlSchemaElement = xmlSchemaObject as XmlSchemaElement;
								if (xmlSchemaElement.QualifiedName == xmlQualifiedName)
									return xmlSchemaElement;
							}
						}
					}
				}
			}

			return null;

		}

		public bool GetPrimaryKeyStatus(XmlSchemaIdentityConstraint xmlSchemaIdentityConstraint)
		{

			XmlAttribute xmlAttribute = ObjectSchema.GetUnhandledAttribute(xmlSchemaIdentityConstraint, QualifiedName.PrimaryKey);
			if (xmlAttribute != null)
				return Convert.ToBoolean(xmlAttribute.Value);

			return false;

		}

		public static XmlSchema GetXmlSchema(XmlSchemaObject xmlSchemaObject)
		{

			// Move up the tree to the root object.
			while (xmlSchemaObject.Parent != null)
				xmlSchemaObject = xmlSchemaObject.Parent;
			return xmlSchemaObject as XmlSchema;

		}

		public static XmlSchemaComplexType FindComplexType(XmlSchemaComplexContentExtension xmlSchemaComplexContentExtension)
		{

			foreach (XmlSchemaObject xmlSchemaObject in GetXmlSchema(xmlSchemaComplexContentExtension).Items)
				if (xmlSchemaObject is XmlSchemaComplexType)
				{
					XmlSchemaComplexType xmlSchemaComplexType = xmlSchemaObject as XmlSchemaComplexType;
					if (xmlSchemaComplexType.QualifiedName == xmlSchemaComplexContentExtension.BaseTypeName)
						return xmlSchemaComplexType;
				}

			return null;

		}

		public static XmlSchemaIdentityConstraint FindKey(XmlSchemaKeyref xmlSchemaKeyref)
		{

			foreach (XmlSchemaObject xmlSchemaObject in GetXmlSchema(xmlSchemaKeyref).Items)
				if (xmlSchemaObject is XmlSchemaElement)
				{
					XmlSchemaElement xmlSchemaElement = xmlSchemaObject as XmlSchemaElement;

					if (IsDataSetElement(xmlSchemaElement))
					{
						XmlSchemaElement dataSetElement = xmlSchemaObject as XmlSchemaElement;
						foreach (XmlSchemaIdentityConstraint xmlSchemaIdentityConstraint in dataSetElement.Constraints)
						{
							if (xmlSchemaIdentityConstraint is XmlSchemaUnique)
							{
								XmlSchemaUnique xmlSchemaUnique = xmlSchemaIdentityConstraint as XmlSchemaUnique;
								if (xmlSchemaUnique.QualifiedName.Namespace == xmlSchemaKeyref.Refer.Namespace &&
									xmlSchemaUnique.QualifiedName.Name == xmlSchemaKeyref.Refer.Name)
									return xmlSchemaUnique;
							}
							if (xmlSchemaIdentityConstraint is XmlSchemaKey)
							{
								XmlSchemaKey xmlSchemaKey = xmlSchemaIdentityConstraint as XmlSchemaKey;
								if (xmlSchemaKey.QualifiedName.Namespace == xmlSchemaKeyref.Refer.Namespace &&
									xmlSchemaKey.QualifiedName.Name == xmlSchemaKeyref.Refer.Name)
									return xmlSchemaKey;
							}
						}
					}
					else
					{
						foreach (XmlSchemaIdentityConstraint xmlSchemaIdentityConstraint in xmlSchemaElement.Constraints)
							if (xmlSchemaIdentityConstraint is XmlSchemaKey)
							{
								XmlSchemaKey xmlSchemaKey = xmlSchemaIdentityConstraint as XmlSchemaKey;
								if (xmlSchemaKey.QualifiedName.Namespace == xmlSchemaKeyref.Refer.Namespace &&
									xmlSchemaKey.QualifiedName.Name == xmlSchemaKeyref.Refer.Name)
									return xmlSchemaKey;
							}
					}

				}

			return null;

		}

	}

}

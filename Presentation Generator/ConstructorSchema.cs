namespace FluidTrade.PresentationGenerator
{

    using System.Collections.Generic;
	using System.Xml;

    /// <summary>
	/// Describes the constructor of a class.
	/// </summary>
	public class ConstructorSchema
	{

		// Private Instance Fields
		private List<ArgumentSchema> argumentSchemaList;
		private List<SnippetSchema> snippetSchemaList;
		private ClassSchema classSchema;

		/// <summary>
		/// Creates a description of a constructor of a class.
		/// </summary>
		/// <param name="classSchema">A description of a type.</param>
		public ConstructorSchema(ClassSchema classSchema)
		{

			// Initialize the object
			this.argumentSchemaList = new List<ArgumentSchema>();
			this.snippetSchemaList = new List<SnippetSchema>();
			this.classSchema = classSchema;

		}

		/// <summary>
		/// Creates a description of a constructor of a class using the XML Schema extensions.
		/// </summary>
		/// <param name="classSchema">A description of a type.</param>
		/// <param name="xmlNode">Extensions to the XML Schema specification.</param>
		public ConstructorSchema(ClassSchema classSchema, XmlNode xmlNode)
		{

			// Initialize the object
			this.argumentSchemaList = new List<ArgumentSchema>();
			this.snippetSchemaList = new List<SnippetSchema>();
			this.classSchema = classSchema;

			// Add arguments to the constructor.
			foreach (XmlNode argmentsNode in xmlNode.ChildNodes)
				if (QualifiedName.Arguments == new XmlQualifiedName(argmentsNode.LocalName, argmentsNode.NamespaceURI))
					foreach (XmlNode argumentNode in argmentsNode.ChildNodes)
						if (QualifiedName.Argument == new XmlQualifiedName(argumentNode.LocalName, argumentNode.NamespaceURI))
							this.argumentSchemaList.Add(new ArgumentSchema(this, argumentNode));

			// Add setters to the constructor.
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				XmlQualifiedName nodeName = new XmlQualifiedName(childNode.LocalName, childNode.NamespaceURI);
				if (nodeName == QualifiedName.Snippet)
					this.snippetSchemaList.Add(new SnippetSchema(childNode));
				if (nodeName == QualifiedName.Setter)
					this.snippetSchemaList.Add(new SetterSchema(childNode));
			}

		}

		/// <summary>
		/// Gets the arguments used to build an object of this class.
		/// </summary>
		public List<ArgumentSchema> Arguments
		{
			get { return this.argumentSchemaList; }
		}

		/// <summary>
		/// Gets the setters used to initialize an object of this class.
		/// </summary>
		public List<SnippetSchema> Snippets
		{
			get { return this.snippetSchemaList; }
		}

		/// <summary>
		/// Gets the type to which this constructor belongs.
		/// </summary>
		public ClassSchema Class
		{
			get { return this.classSchema; }
		}

	}

}

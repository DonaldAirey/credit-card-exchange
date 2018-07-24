namespace FluidTrade.PresentationGenerator
{

	using System;
    using System.Xml;

	/// <summary>
	/// Describes the arguments used by a constructor.
	/// </summary>
	public class ArgumentSchema
	{

		// Private Instance Fields
		private ConstructorSchema constructorSchema;
		private String name;
		private String type;

		/// <summary>
		/// Create a description of arguments used by a constructor.
		/// </summary>
		/// <param name="constructorSchema">A description of the constructor.</param>
		/// <param name="xmlNode">The XML Schema extensions that describe the arguments.</param>
		public ArgumentSchema(ConstructorSchema constructorSchema, XmlNode xmlNode)
		{

			// Initialize the object.
			this.constructorSchema = constructorSchema;

			// Extract the argument type from the XML extension.
			XmlAttribute typeAttribute = xmlNode.Attributes[QualifiedName.VariableType.Name, QualifiedName.VariableType.Namespace];
			this.type = typeAttribute.Value;

			// Extract the value of the argument from the XML Extension.
			XmlAttribute nameAttribute = xmlNode.Attributes[QualifiedName.VariableName.Name, QualifiedName.VariableName.Namespace];
			this.name = nameAttribute.Value;

		}

		/// <summary>
		/// Gets the name of the type of this argument.
		/// </summary>
		public String Type
		{
			get { return this.constructorSchema.Class.Presentation.ResolvePrefixedTypeName(this.type); ; }
		}

		/// <summary>
		/// Gets the name of the argument.
		/// </summary>
		public String Name
		{
			get { return this.name; }
		}

	}

}

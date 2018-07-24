namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using FluidTrade.Core;

    /// <summary>
	/// Generates the class that transforms a schema into a Presentation Layer.
	/// </summary>
	public class PresentationClass : System.CodeDom.CodeTypeDeclaration
	{

		/// <summary>
		/// Generates the class that transforms a schema into a Presentation Layer.
		/// </summary>
		/// <param name="classSchema">A description of a class.</param>
		public PresentationClass(ClassSchema classSchema)
		{

			//	/// <summary>
			//	/// This class provides the WorkingOrderHeader data for a report.
			//	/// </summary>
			//	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.Presentation.PresentationGenerator", "1.0.0.0")]
			//	[global::System.ComponentModel.DesignerCategoryAttribute("code")]
			//	[global::System.ComponentModel.ToolboxItemAttribute(true)]
			//	public class WorkingOrderHeader : FluidTrade.Core.IContent
			//	{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(String.Format("This class provides the {0} data for a report.", classSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(typeof(PresentationGenerator)));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.DesignerCategoryAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression("code"))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.ToolboxItemAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(true))));
			this.Name = classSchema.Name;
			this.BaseTypes.Add(new CodeTypeReference(typeof(IContent)));
			foreach (PropertySchema propertySchema in classSchema.Properties)
				if (propertySchema.IsSimpleType)
				{
					this.BaseTypes.Add(new CodeTypeReference(typeof(INotifyPropertyChanged)));
					break;
				}

			// Fields
			List<CodeMemberField> fieldList = new List<CodeMemberField>();
			fieldList.Add(new SelectSelfField(classSchema));
			fieldList.Add(new KeyField());
			foreach (PropertySchema propertySchema in classSchema.Properties)
			{
				if (propertySchema.MaxOccurs == 1)
					fieldList.Add(new SchemaScalarField(propertySchema));
				if (propertySchema.MaxOccurs > 1)
					fieldList.Add(new SchemaVectorField(propertySchema));
				foreach (QuerySchema querySchema in propertySchema.Queries)
				{
					if (querySchema is OrderBySchema)
						fieldList.Add(new ComplexComparerField(querySchema as OrderBySchema));
					if (querySchema is SelectSchema)
						fieldList.Add(new SelectField(querySchema as SelectSchema));
					if (querySchema is WhereSchema)
						fieldList.Add(new ComplexFilterField(querySchema as WhereSchema));
				}
			}
			fieldList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
			this.Members.AddRange(fieldList.ToArray());

			// Constructors
			this.Members.Add(new StaticConstructor(classSchema));
			foreach (PropertySchema propertySchema in classSchema.Properties)
				if (!propertySchema.IsSimpleType)
				{
					this.Members.Add(new SchemaConstructor(classSchema));
					break;
				}

			// Properties
			List<CodeMemberProperty> propertyList = new List<CodeMemberProperty>();
			propertyList.Add(new KeyProperty());
			propertyList.Add(new SelectSelfProperty(classSchema));
			foreach (PropertySchema propertySchema in classSchema.Properties)
			{
				propertyList.Add(new SchemaProperty(propertySchema));
				foreach (QuerySchema querySchema in propertySchema.Queries)
				{
					if (querySchema is WhereSchema)
						propertyList.Add(new ComplexFilterProperty(querySchema as WhereSchema));
					if (querySchema is OrderBySchema)
						propertyList.Add(new ComplexComparerProperty(querySchema as OrderBySchema));
					if (querySchema is SelectSchema)
						propertyList.Add(new SelectProperty(querySchema as SelectSchema));
				}
			}
			propertyList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
			this.Members.AddRange(propertyList.ToArray());

			// Events
			foreach (PropertySchema propertySchema in classSchema.Properties)
				if (propertySchema.IsSimpleType)
				{
					this.Members.Add(new PropertyChangedEvent());
					break;
				}

			// Methods
			List<CodeMemberMethod> methodList = new List<CodeMemberMethod>();
			methodList.Add(new CopyMethod(classSchema));
			if (classSchema.Constructor.Arguments.Count == 1)
				methodList.Add(new SchemaSelect(classSchema));
			methodList.Add(new SelectFromSelfMethod(classSchema));
			methodList.Sort((first, second) => { return first.Name.CompareTo(second.Name); });
			this.Members.AddRange(methodList.ToArray());

		}

	}

}

namespace FluidTrade.MiddleTierGenerator.ClusteredIndexClass
{

    using System.CodeDom;
    using System.Reflection;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a CodeDOM description of a strongly typed index.
	/// </summary>
	public class ClusteredIndexClass : CodeTypeDeclaration
	{

		/// <summary>
		/// Creates a CodeDOM description of a strongly typed index.
		/// </summary>
		/// <param name="constraintSchema">A description of a unique constraint.</param>
		public ClusteredIndexClass(UniqueConstraintSchema uniqueConstraintSchema)
		{

			//    /// <summary>
			//    /// Represents a means of identifying a Gender row using a set of columns in which all values must be unique.
			//    /// </summary>
			//    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.MiddleTier.MiddleTierGenerator", "1.3.0.0")]
			//    [global::System.ComponentModel.DesignerCategoryAttribute("code")]
			//    [global::System.ComponentModel.ToolboxItemAttribute(true)]
			//    public class GenderKeyIndex : global::FluidTrade.Core.ClusteredIndex, IGenderIndex {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Represents a means of identifying a {0} row using a set of columns in which all values must be unique.", uniqueConstraintSchema.Table.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeGeneratedCodeAttribute(typeof(MiddleTierGenerator)));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.DesignerCategoryAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression("code"))));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.ToolboxItemAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(true))));
			this.Name = string.Format("{0}Index", uniqueConstraintSchema.Name);
			this.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;
			this.BaseTypes.Add(new CodeGlobalTypeReference(typeof(ClusteredIndex)));
			this.BaseTypes.Add(new CodeTypeReference(string.Format("I{0}Index", uniqueConstraintSchema.Table.Name)));

			// Constructors
			this.Members.Add(new Constructor(uniqueConstraintSchema));

			// Methods
			this.Members.Add(new FindByMethod(uniqueConstraintSchema));
			this.Members.Add(new FindByKeyMethod(uniqueConstraintSchema));

			//		}

		}

	}

}
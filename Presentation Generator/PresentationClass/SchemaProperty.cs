namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a property for getting or setting the time a record lock is held.
	/// </summary>
	class SchemaProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property for from the XML Schema definition.
		/// </summary>
		public SchemaProperty(PropertySchema propertySchema)
		{

			//		/// <summary>
			//		/// Price
			//		/// <summary>
			//		public decimal Price
			//		{
			//			get
			//			{
			//				return this.price;
			//			}
			//			set
			//			{
			//
			//			}
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(propertySchema.Name, true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = propertySchema.MaxOccurs == 1 ?
				new CodeTypeReference(propertySchema.Type) :
				new CodeTypeReference(string.Format("System.Collections.Generic.IEnumerable<{0}>", propertySchema.Type));
			this.Name = propertySchema.Name;

			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(propertySchema.Name))));
			this.SetStatements.Add(new CodeSnippetStatement());

			//		}

		}

	}
}

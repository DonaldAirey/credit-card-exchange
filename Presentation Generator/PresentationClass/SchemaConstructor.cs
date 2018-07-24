namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Generates the constructor for the generated class.
	/// </summary>
	public class SchemaConstructor : System.CodeDom.CodeConstructor
	{

		/// <summary>
		/// Generates the constructor for the generated class.
		/// </summary>
		/// <param name="classSchema">A description of a Type.</param>
		public SchemaConstructor(ClassSchema classSchema)
		{

			//		/// <summary>
			//		/// Creates a FluidTrade.Sandbox.WorkingOrder.WorkingOrder.
			//		/// </summary>
			//		public WorkingOrder(FluidTrade.Sandbox.WorkingOrderRow workingOrderRow)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates a {0}.", classSchema.Type), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public;

			// This will run through each of the members of the class an initialize the complex data types.  If the element allows 
			// only one occurance of an element, then a single object is created.  If the element allows many, then a list is
			// created.  Basic data types are not constructred here.
			foreach (PropertySchema propertySchema in classSchema.Properties)
			{

				// For scalar properties, the property is created and then populated using the setters.
				if (propertySchema.MaxOccurs == 1)
				{

					//			this.askPrice = new FluidTrade.Sandbox.WorkingOrder.AskPrice();
					CodeFieldReferenceExpression fieldExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
						CommonConversion.ToCamelCase(propertySchema.Name));
					this.Statements.Add(new CodeAssignStatement(fieldExpression, new CodeObjectCreateExpression(new CodeTypeReference(propertySchema.Type))));

				}

			}

		}

	}

}

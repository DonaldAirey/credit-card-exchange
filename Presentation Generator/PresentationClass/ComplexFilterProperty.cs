namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a property for getting or setting the time a record lock is held.
	/// </summary>
	class ComplexFilterProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property for from the XML Schema definition.
		/// </summary>
		public ComplexFilterProperty(WhereSchema whereSchema)
		{

			//		/// <summary>
			//		/// Gets or sets the complex filter used to remove records from the result set.
			//		/// <summary>
			//		public ComplexFilter<Sandbox.WorkingOrder.WorkingOrder> Filter
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets the complex filter used to remove records from the result set.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference(string.Format("FluidTrade.Core.ComplexFilter<{0}>", whereSchema.ResultType));
			this.Name = whereSchema.Predicate;

			//			get
			//			{
			//				return FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.filter;
			//			}
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(whereSchema.Predicate))));
			this.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(whereSchema.Predicate)), new CodeArgumentReferenceExpression("value")));

			//		}

		}

	}
}

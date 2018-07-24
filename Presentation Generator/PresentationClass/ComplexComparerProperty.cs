namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a property for getting or setting the time a record lock is held.
	/// </summary>
	class ComplexComparerProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property for from the XML Schema definition.
		/// </summary>
		public ComplexComparerProperty(OrderBySchema orderBySchema)
		{

			//		/// <summary>
			//		/// Gets or sets the complex comparison for ordering records from the result set.
			//		/// <summary>
			//		public ComplexComparer<Sandbox.WorkingOrder.WorkingOrder> Comparer
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets the complex comparison for ordering records from the result set.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference(string.Format("FluidTrade.Core.ComplexComparer<{0}>", orderBySchema.ResultType));
			this.Name = orderBySchema.Comparer;

			//			get
			//			{
			//				return FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.comparer;
			//			}
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(orderBySchema.Comparer))));
			this.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(orderBySchema.Comparer)), new CodeArgumentReferenceExpression("value")));

			//		}

		}

	}
}

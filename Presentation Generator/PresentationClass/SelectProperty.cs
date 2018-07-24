namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a property used to get the delegate used to select a destination record from a source record.
	/// </summary>
	class SelectProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property used to get the delegate used to select a destination record from a source record.
		/// </summary>
		public SelectProperty(SelectSchema selectSchema)
		{

			//		/// <summary>
			//		/// Gets the selector delegate for System.Guid records.
			//		/// <summary>
			//		public static System.Func<System.Guid, FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader> Select
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets the delegate used to select a destination record from a source.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference(string.Format("System.Func<{0}, {1}>", selectSchema.SourceType, selectSchema.ResultType));
			this.Name = selectSchema.Selector;

			//			get
			//			{
			//				return FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.select;
			//			}
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(selectSchema.Selector))));
			this.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), CommonConversion.ToCamelCase(selectSchema.Selector)), new CodeArgumentReferenceExpression("value")));

			//		}

		}

	}
}

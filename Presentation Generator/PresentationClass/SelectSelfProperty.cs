namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a property used to get the delegate used to select an unmodified record for a query.
	/// </summary>
	class SelectSelfProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property used to get the delegate used to select an unmodified record for a query.
		/// </summary>
		public SelectSelfProperty(ClassSchema classSchema)
		{

			//		/// <summary>
			//		/// Gets the selector delegate for FluidTrade.Sandbox.WorkingOrder.AskPrice records.
			//		/// <summary>
			//		public static System.Func<Sandbox.WorkingOrder.AskPrice, FluidTrade.Sandbox.WorkingOrder.AskPrice> SelectSelf
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the selector delegate for {0} records.", classSchema.Type), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Type = new CodeTypeReference(string.Format("System.Func<{0}, {0}>", classSchema.Type));
			this.Name = "SelectSelf";

			//			get
			//			{
			//				return FluidTrade.Sandbox.WorkingOrder.AskPrice.selectSelf;
			//			}
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classSchema.Type), "selectSelf")));

			//		}

		}

	}
}

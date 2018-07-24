namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;

    /// <summary>
	/// Generates the static constructor for the class.
	/// </summary>
	public class StaticConstructor : System.CodeDom.CodeTypeConstructor
	{

		/// <summary>
		/// Generates the static constructor for the class.
		/// </summary>
		/// <param name="classSchema">A description of the type.</param>
		public StaticConstructor(ClassSchema classSchema)
		{

			//		/// <summary>
			//		/// Creates the static resources required for this class.
			//		/// </summary>
			//		static WorkingOrderHeader()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Creates the static resources required for this class.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));

			//			FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.selectSelf = new System.Func<Sandbox.WorkingOrderHeader.WorkingOrderHeader, FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader>(FluidTrade.Sandbox.WorkingOrderHeader.WorkingOrderHeader.SelectFromSelf);
			this.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classSchema.Type), "selectSelf"),
				new CodeObjectCreateExpression(new CodeTypeReference(string.Format("System.Func<{0}, {0}>", classSchema.Type)), new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(classSchema.Type), "SelectFromSelf"))));

			//		}

		}

	}

}

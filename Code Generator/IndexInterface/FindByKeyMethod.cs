namespace FluidTrade.Core.IndexInterface
{

    using System.CodeDom;

    /// <summary>
	/// Creates an abstract declaration of a method that finds a strongly typed row given an array of key elements.
	/// </summary>
	class FindByKeyMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates an abstract declaration of a method that finds a strongly typed row given an array of key elements.
		/// </summary>
		/// <param name="tableSchema">The description of the table.</param>
		public FindByKeyMethod(TableSchema tableSchema)
		{

			//        /// <summary>
			//        /// Finds a row in the Gender table containing the key elements.
			//        /// </summary>
			//        /// <param name="key">An array of key elements.</param>
			//        /// <returns>A DepartmentRow that contains the key elements, or null if there is no match.</returns>
			//        GenderRow Find(object[] key);
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Finds a row in the {0} table containing the key elements.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"key\">An array of key elements.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>A DepartmentRow that contains the key elements, or null if there is no match.</returns>", true));
			this.Attributes = MemberAttributes.Abstract;
			this.ReturnType = new CodeTypeReference(string.Format("{0}Row", tableSchema.Name));
			this.Name = "Find";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Object[])), "key"));

		}
	}
}

namespace FluidTrade.ClientGenerator.NonClusteredIndexClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method that finds a row containing the given elements of an index.
	/// </summary>
	class FindByKeyMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that finds a row containing the given elements of an index.
		/// </summary>
		/// <param name="uniqueConstraintSchema">A description of a unique constraint.</param>
		public FindByKeyMethod(UniqueConstraintSchema uniqueConstraintSchema)
		{

			//        /// <summary>
			//        /// Finds a row in the Gender table containing the key elements.
			//        /// </summary>
			//        /// <param name="key">An array of key elements.</param>
			//        /// <returns>A GenderKey row that contains the key elements, or null if there is no match.</returns>
			//        public new GenderRow Find(object[] key) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Finds a row in the {0} table containing the key elements.", uniqueConstraintSchema.Table.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"key\">An array of key elements.</param>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("<returns>A {0} row that contains the key elements, or null if there is no match.</returns>", uniqueConstraintSchema.Name), true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final;
			this.ReturnType = new CodeTypeReference(string.Format("{0}Row", uniqueConstraintSchema.Table.Name));
			this.Name = "Find";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Object[])), "key"));

			//            // Return the strongly typed Object row that matches the key element(s).
			//            return ((GenderRow)(base.Find(key)));
			this.Statements.Add(new CodeCommentStatement("Return the strongly typed Object row that matches the key element(s)."));
			this.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.ReturnType, new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Find", new CodeArgumentReferenceExpression("key")))));

			//        }

		}
	}
}

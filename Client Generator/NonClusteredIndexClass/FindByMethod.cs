namespace FluidTrade.ClientGenerator.NonClusteredIndexClass
{

    using System.CodeDom;
    using System.Collections.Generic;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method that finds a row containing the given elements of an index.
	/// </summary>
	class FindByMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that finds a row containing the given elements of an index.
		/// </summary>
		/// <param name="uniqueConstraintSchema">A description of a unique constraint.</param>
		public FindByMethod(UniqueConstraintSchema uniqueConstraintSchema)
		{

			//        /// <summary>
			//        /// Finds a row in the Gender table containing the key elements.
			//        /// </summary>
			//        /// <param name="genderCode">The GenderCode element of the key.</param>
			//        /// <returns>The Gender row that contains the key elements, or null if there is no match.</returns>
			//        public GenderRow Find(FluidTrade.UnitTest.Gender genderCode) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Finds a row in the {0} table containing the key elements.", uniqueConstraintSchema.Table.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			foreach (ColumnSchema columnSchema in uniqueConstraintSchema.Columns)
				this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"{0}\">The {1} element of the key.</param>", CommonConversion.ToCamelCase(columnSchema.Name), columnSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement(string.Format("<returns>The {0} row that contains the key elements, or null if there is no match.</returns>", uniqueConstraintSchema.Table.Name), true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeTypeReference(string.Format("{0}Row", uniqueConstraintSchema.Table.Name));
			this.Name = "Find";
			foreach (ColumnSchema columnSchema in uniqueConstraintSchema.Columns)
				this.Parameters.Add(new CodeParameterDeclarationExpression(columnSchema.DataType, CommonConversion.ToCamelCase(columnSchema.Name)));

			//            // Return the strongly typed Object row that matches the key element(s).
			//            return ((GenderRow)(base.Find(new object[] {
			//                        genderCode})));
			this.Statements.Add(new CodeCommentStatement("Return the strongly typed Object row that matches the key element(s)."));
			List<CodeExpression> findByArguments = new List<CodeExpression>();
			foreach (ColumnSchema columnSchema in uniqueConstraintSchema.Columns)
				findByArguments.Add(new CodeArgumentReferenceExpression(CommonConversion.ToCamelCase(columnSchema.Name)));
			this.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.ReturnType, new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Find", new CodeArrayCreateExpression(typeof(System.Object), findByArguments.ToArray())))));

			//        }

		}
	}
}

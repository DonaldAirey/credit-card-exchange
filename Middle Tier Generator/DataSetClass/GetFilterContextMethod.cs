namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a default method for getting a context for a filtered read operation.
	/// </summary>
	class GetFilterContextMethod : CodeMemberMethod
	{

		/// <summary>
		/// Provides a default handler for to get a Reader Context.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public GetFilterContextMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Provides a default handler for to get a Reader Context.
			//		/// </summary>
			//		private static object GetFilterContext()
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Provides a default handler for to get a Reader Context.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Private;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object));
			this.Name = "GetFilterContext";
			this.Parameters.Add(new CodeParameterDeclarationExpression(
				string.Format("{0}Transaction", dataModelSchema.Name),
				string.Format("{0}Transaction", CommonConversion.ToCamelCase(dataModelSchema.Name))));

			//			return null;
			this.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));

			//		}

		}

	}
}

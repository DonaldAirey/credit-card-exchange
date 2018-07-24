namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a CodeDOM description a method to lock tables for a filtered read operation.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class GetContainerMethod : CodeMemberMethod
	{

		/// <summary>
		/// Generates the method used to handle the Row Changed event.
		/// </summary>
		public GetContainerMethod(TableSchema tableSchema)
		{

			//		/// <summary>
			//		/// Gets the container Entity for a given records for filtering.
			//		/// </summary>
			//		/// <param name="iRow">The object for which a filtering Entity is required.</param>
			//		/// <returns>The Entity used for filtering, or a null if the object is it's own container.</returns>
			//		private bool GetContainer(object context, object[] transactionLogItem)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets the container Entity for a given records for filtering.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"iRow\">The object for which a filtering Entity is required.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<returns>The Entity used for filtering, or a null if the object is it's own container.</returns>", true));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Object));
			this.Name = "GetContainer";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(FluidTrade.Core.IRow)), "iRow"));

			//			return null;
			this.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));

			//		}

		}

	}

}

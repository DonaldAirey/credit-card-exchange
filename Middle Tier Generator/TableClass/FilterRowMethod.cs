namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a CodeDOM description a method to lock tables for a filtered read operation.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class FilterRowMethod : CodeMemberMethod
	{

		/// <summary>
		/// Generates the method used to handle the Row Changed event.
		/// </summary>
		public FilterRowMethod(TableSchema tableSchema)
		{

			//		/// <summary>
			//		/// Filters a row from the results returned to a client.
			//		/// </summary>
			//		/// <param name="dataModelTransaction">The transaction in which this filter takes place.</param>
			//		/// <param name="filterContext">A general purpose context for the read operation.</param>
			//		/// <param name="containerContext">The Entity which contains this objects and maps to an ACL entry.</param>
			//		/// <returns>True if the given row can be returned to the client, false otherwise.</returns>
			//		private bool FilterRow(object context, object[] transactionLogItem)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Filters a row from the results returned to a client.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"dataModelTransaction\">The transaction in which this filter takes place.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"filterContext\">A general purpose context for the read operation.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"containerContext\">The Entity which contains this objects and maps to an ACL entry.</param>", true));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(Boolean));
			this.Name = "FilterRow";
			this.Parameters.Add(
				new CodeParameterDeclarationExpression(
					new CodeGlobalTypeReference(typeof(FluidTrade.Core.IDataModelTransaction)),
					string.Format("{0}Transaction", CommonConversion.ToCamelCase(tableSchema.DataModel.Name))));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object)), "filterContext"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object)), "containerContext"));

			//			return true;
			this.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));

			//		}

		}

	}

}

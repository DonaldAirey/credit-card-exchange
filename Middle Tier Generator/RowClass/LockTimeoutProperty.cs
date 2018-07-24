namespace FluidTrade.MiddleTierGenerator.RowClass
{

	using System.CodeDom;
	using FluidTrade.Core;

	/// <summary>
	/// Creates a property that gets or sets the value of an item in a row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class LockTimeoutProperty :CodeMemberProperty
	{

		/// <summary>
		/// Creates a propert that gets the Locktimeout for the row.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public LockTimeoutProperty(TableSchema tableSchema)
		{

			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the LockTimeout for the row.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.BrowsableAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(false))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			this.Type = new CodeGlobalTypeReference(typeof(System.TimeSpan));
			this.Name = "LockTimeout";
			CodeMethodReturnStatement returnStmt =
				 new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), "LockTimeout"));

			this.GetStatements.Add(returnStmt);

		}

	}
}

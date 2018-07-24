namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a CodeDOM description a method to handle the Row Validate event.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class OnRowValidateMethod : CodeMemberMethod
	{

		/// <summary>
		/// Generates the method used to handle the Row Validate event.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public OnRowValidateMethod(TableSchema tableSchema)
		{

			// Construct the type names for the table and rows within the table.
			string rowTypeName = string.Format("{0}Row", tableSchema.Name);

			//            /// <summary>
			//            /// Raises the DepartmentRowValidate event.
			//            /// </summary>
			//            /// <param name="e">Provides data for the DepartmentRow changing and deleting events.</param>
			//            protected void OnRowValidate(global::System.Data.DataRowChangeEventArgs e) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Raises the {0}Validate event.", rowTypeName), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"e\">Provides data for the {0} changing and deleting events.</param>", rowTypeName), true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = "OnRowValidate";
			this.Parameters.Add(new CodeParameterDeclarationExpression(string.Format("{0}ChangeEventArgs", rowTypeName), "e"));

			//                base.OnRowValidate(e);
			//                if ((this.DepartmentRowValidate != null)) {
			//                    this.DepartmentRowValidate(this, e);
			//                }
			CodeConditionStatement ifEventExists = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("{0}Validate", rowTypeName)), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)));
			ifEventExists.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), string.Format("{0}Validate", rowTypeName), new CodeThisReferenceExpression(), new CodeArgumentReferenceExpression("e")));
			this.Statements.Add(ifEventExists);

			//            }

		}

	}

}

namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a CodeDOM description a method to handle the Row Deleting event.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class OnRowDeletingMethod : CodeMemberMethod
	{

		/// <summary>
		/// Generates the method used to handle the Row Deleting event.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public OnRowDeletingMethod(TableSchema tableSchema)
		{

			// Construct the type names for the table and rows within the table.
			string rowTypeName = string.Format("{0}Row", tableSchema.Name);

			//            /// <summary>
			//            /// Raises the DepartmentRowDeleting event.
			//            /// </summary>
			//            /// <param name="e">Provides data for the DepartmentRow changing and deleting events.</param>
			//            protected override void OnRowDeleting(global::System.Data.DataRowChangeEventArgs e) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Raises the {0}Deleting event.", rowTypeName), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"e\">Provides data for the {0} changing and deleting events.</param>", rowTypeName), true));
			this.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			this.Name = "OnRowDeleting";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Data.DataRowChangeEventArgs)), "e"));

			//                base.OnRowDeleting(e);
			//                if ((this.DepartmentRowDeleting != null)) {
			//                    this.DepartmentRowDeleting(this, new DepartmentRowChangeEvent(((DepartmentRow)(e.Row)), e.Action));
			//                }
			this.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "OnRowDeleting", new CodeArgumentReferenceExpression("e")));
			CodeConditionStatement ifEventExists = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("{0}Deleting", rowTypeName)), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)));
			ifEventExists.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), string.Format("{0}Deleting", rowTypeName), new CodeThisReferenceExpression(), new CodeObjectCreateExpression(string.Format("{0}ChangeEventArgs", rowTypeName), new CodeCastExpression(rowTypeName, new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("e"), "Row")), new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("e"), "Action"))));
			this.Statements.Add(ifEventExists);

			//            }

		}

	}

}

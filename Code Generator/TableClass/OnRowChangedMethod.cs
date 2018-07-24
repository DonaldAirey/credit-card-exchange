namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a CodeDOM description a method to handle the Row Changed event.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class OnRowChangedMethod : CodeMemberMethod
	{

		/// <summary>
		/// Generates the method used to handle the Row Changed event.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public OnRowChangedMethod(TableSchema tableSchema)
		{

			// Construct the type names for the table and rows within the table.
			string rowTypeName = string.Format("{0}Row", tableSchema.Name);

			//            /// <summary>
			//            /// Raises the DepartmentRowChanged event.
			//            /// </summary>
			//            /// <param name="e">Provides data for the DepartmentRow changing and deleting events.</param>
			//            protected override void OnRowChanged(global::System.Data.DataRowChangeEventArgs e) {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Raises the {0}Changed event.", rowTypeName), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("<param name=\"e\">Provides data for the {0} changing and deleting events.</param>", rowTypeName), true));
			this.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			this.Name = "OnRowChanged";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Data.DataRowChangeEventArgs)), "e"));

			//                base.OnRowChanged(e);
			//                if ((this.DepartmentRowChanged != null)) {
			//                    this.DepartmentRowChanged(this, new DepartmentRowChangeEvent(((DepartmentRow)(e.Row)), e.Action));
			//                }
			this.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "OnRowChanged", new CodeArgumentReferenceExpression("e")));
			CodeConditionStatement ifEventExists = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("{0}Changed", rowTypeName)), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)));
			ifEventExists.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), string.Format("{0}Changed", rowTypeName), new CodeThisReferenceExpression(), new CodeObjectCreateExpression(string.Format("{0}ChangeEventArgs", rowTypeName), new CodeCastExpression(rowTypeName, new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("e"), "Row")), new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("e"), "Action"))));
			this.Statements.Add(ifEventExists);

			//            }

		}

	}

}

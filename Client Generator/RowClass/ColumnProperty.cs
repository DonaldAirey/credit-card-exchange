namespace FluidTrade.ClientGenerator.RowClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a property that gets or sets the value of an item in a row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class ColumnProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property that gets or sets the value of an item in a row.
		/// </summary>
		/// <param name="tableSchema">The table to which this row belongs.</param>
		/// <param name="columnSchema">The nullable column.</param>
		public ColumnProperty(TableSchema tableSchema, ColumnSchema columnSchema)
		{

			//        /// <summary>
			//        /// Gets or sets the data in the RaceCode column.
			//        /// </summary>
			//        public int RaceCode {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets or sets the data in the {0} column.", columnSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(columnSchema.DataType);
			this.Name = columnSchema.Name;

			//            get {
			//                try {
			//                    return ((int)(this[this.tableEmployee.RaceCodeColumn]));
			//                }
			//                catch (global::System.InvalidCastException e) {
			//                    throw new global::System.Data.StrongTypingException("Cannot get value because it is DBNull.", e);
			//                }
			//            }
			if (columnSchema.IsNullable)
			{
				CodeTryCatchFinallyStatement tryCatchBlock = new CodeTryCatchFinallyStatement();
				tryCatchBlock.TryStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.Type, new CodeArrayIndexerExpression(new CodeThisReferenceExpression(), new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("table{0}", tableSchema.Name)), string.Format("{0}Column", columnSchema.Name))))));
				CodeCatchClause catchStrongTypeException = new CodeCatchClause("e", new CodeGlobalTypeReference(typeof(System.InvalidCastException)));
				catchStrongTypeException.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Data.StrongTypingException)), new CodePrimitiveExpression("Cannot get value because it is DBNull."), new CodeArgumentReferenceExpression("e"))));
				tryCatchBlock.CatchClauses.Add(catchStrongTypeException);
				this.GetStatements.Add(tryCatchBlock);
			}
			else
			{
				this.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(this.Type, new CodeArrayIndexerExpression(new CodeThisReferenceExpression(), new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("table{0}", tableSchema.Name)), string.Format("{0}Column", columnSchema.Name))))));
			}

			//            set {
			//                this[this.tableEmployee.RaceCodeColumn] = value;
			//            }
			this.SetStatements.Add(new CodeAssignStatement(new CodeArrayIndexerExpression(new CodeThisReferenceExpression(), new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("table{0}", tableSchema.Name)), string.Format("{0}Column", columnSchema.Name))), new CodePropertySetValueReferenceExpression()));

			//        }

		}

	}

}

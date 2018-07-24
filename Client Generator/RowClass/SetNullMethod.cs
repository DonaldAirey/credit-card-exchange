namespace FluidTrade.ClientGenerator.RowClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method that sets a nullable column to null.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class SetNullMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that sets a nullable column to null.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		/// <param name="columnSchema">A description of the column.</param>
		public SetNullMethod(TableSchema tableSchema, ColumnSchema columnSchema)
		{

			//            /// <summary>
			//            /// Sets the Null property of the RaceCode column.
			//            /// </summary>
			//            public void SetRaceCodeNull() {
			//                this[this.tableEmployee.RaceCodeColumn] = global::System.DBNull.Value;
			//            }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Sets the Null property of the {0} column.", columnSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Name = string.Format("Set{0}Null", columnSchema.Name);
			this.Statements.Add(new CodeAssignStatement(new CodeIndexerExpression(new CodeThisReferenceExpression(), new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("table{0}", tableSchema.Name)), string.Format("{0}Column", columnSchema.Name))), new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DBNull)), "Value")));

		}

	}

}

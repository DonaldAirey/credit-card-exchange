namespace FluidTrade.MiddleTierGenerator.RowClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method that checks whether a given nullable column has a null value.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class IsNullMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method that checks whether a given nullable column has a null value.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		/// <param name="columnSchema">A description of the column.</param>
		public IsNullMethod(TableSchema tableSchema, ColumnSchema columnSchema)
		{

			//            /// <summary>
			//            /// Gets the Null property of the RaceCode column.
			//            /// </summary>
			//            public bool IsRaceCodeNull() {
			//                return (this[this.tableEmployee.RaceCodeColumn] == global::System.DBNull.Value);
			//            }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the Null property of the {0} column.", columnSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.ReturnType = new CodeGlobalTypeReference(typeof(bool));
			this.Name = string.Format("Is{0}Null", columnSchema.Name);
			this.Statements.Add(new CodeMethodReturnStatement(new CodeBinaryOperatorExpression(new CodeIndexerExpression(new CodeThisReferenceExpression(), new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("table{0}", tableSchema.Name)), string.Format("{0}Column", columnSchema.Name))), CodeBinaryOperatorType.IdentityEquality, new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DBNull)), "Value"))));

		}

	}

}

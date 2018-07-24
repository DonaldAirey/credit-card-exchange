namespace FluidTrade.MiddleTierGenerator.RowClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Represents a declaration of a constuctor for a strongly typed row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class RowBuilderConstructor : CodeConstructor
	{

		/// <summary>
		/// Generates a constuctor for a strongly typed row.
		/// </summary>
		/// <param name="tableSchema">The table to which this constructor belongs.</param>
		public RowBuilderConstructor(TableSchema tableSchema)
		{

			// Construct the type names for the table and rows within the table.
			string tableTypeName = string.Format("{0}DataTable", tableSchema.Name);
			string rowTypeName = string.Format("{0}Row", tableSchema.Name);

			//		/// <summary>
			//		/// Creates a row of data from the AccessControl table schema.
			//		/// </summary>
			//		/// <param name="dataRowBuilder">An internal data structure used to build the row from the parent table schema.</param>
			//		internal DepartmentRow(DataRowBuilder dataRowBuilder) : 
			//					base(dataRowBuilder)
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Creates a row of data from the {0} table schema.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"dataRowBuilder\">An internal data structure used to build the row from the parent table schema.</param>", true));
			this.Attributes = MemberAttributes.Assembly;
			this.Name = rowTypeName;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Data.DataRowBuilder)), "dataRowBuilder"));
			this.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("dataRowBuilder"));

			//			this.tableAccessControl = ((AccessControlDataTable)(this.Table));
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), string.Format("table{0}", tableSchema.Name)),
					new CodeCastExpression(tableTypeName, new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Table"))));

			//			}

		}

	}

}

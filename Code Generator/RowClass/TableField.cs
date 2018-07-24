namespace FluidTrade.Core.RowClass
{

    using System.CodeDom;

    /// <summary>
	/// Represents a declaration of a field used to reference the table that owns this row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class TableField : CodeMemberField
	{

		/// <summary>
		/// Represents a declaration of a field used to reference the table that owns this row.
		/// </summary>
		/// <param name="tableSchema">The table that owns this row.</param>
		public TableField(TableSchema tableSchema)
		{

			//            // The parent Employee table.
			//            private EmployeeDataTable tableEmployee;
			this.Type = new CodeTypeReference(string.Format("{0}DataTable", tableSchema.Name));
			this.Name = string.Format("table{0}", tableSchema.Name);

		}

	}

}

namespace FluidTrade.Core.TargetClass
{

	using System;
	using System.CodeDom;

	/// <summary>
	/// Creates a field for each of the tables created for this data model.
	/// </summary>
	public class TableField : CodeMemberField
	{

		/// <summary>
		/// Creates a field for each of the tables created for this data model.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public TableField(TableSchema tableSchema)
		{

			//        // The Employee table
			//        private static EmployeeDataTable tableEmployee;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeTypeReference(String.Format("{0}DataTable", tableSchema.Name));
			this.Name = string.Format("table{0}", tableSchema.Name);

		}

	}

}

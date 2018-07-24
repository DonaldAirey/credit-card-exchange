namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

    /// <summary>
	/// Creates a field that holds the column.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ColumnField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the column.
		/// </summary>
		/// <param name="columnSchema">A description of the column.</param>
		public ColumnField(ColumnSchema columnSchema)
		{

			//        // The DepartmentId Column
			//        private global::System.Data.DataColumn columnDepartmentId;
			this.Type = new CodeGlobalTypeReference(typeof(System.Data.DataColumn));
			this.Name = string.Format("column{0}", columnSchema.Name);

		}

	}

}

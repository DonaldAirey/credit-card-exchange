namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

    /// <summary>
	/// Creates a field that holds a collection of FluidTrade.Core.DataIndex objects on a table.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DataIndicesField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds a collection of FluidTrade.Core.DataIndex objects on the a table.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public DataIndicesField(TableSchema tableSchema)
		{

			//            // A collection of FluidTrade.Core.DataIndex objects on the Department table.
			//            private global::FluidTrade.Core.RowFilterDelegate userFilter;
			this.Type = new CodeGlobalTypeReference(typeof(DataIndexCollection));
			this.Name = "dataIndices";

		}

	}

}

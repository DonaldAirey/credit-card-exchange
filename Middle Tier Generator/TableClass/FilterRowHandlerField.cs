namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class FilterRowHandlerField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds a delegate to a method that filters rows from the client data model.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public FilterRowHandlerField(TableSchema tableSchema)
		{

			//            private global::FluidTrade.Core.FilterRowDelegate filterRowHandler;
			this.Type = new CodeTypeReference("FilterRowDelegate");
			this.Name = "filterRowHandler";

		}

	}

}

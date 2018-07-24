namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class GetContainerHandlerField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds a delegate to a method that filters rows from the client data model.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public GetContainerHandlerField(TableSchema tableSchema)
		{

			//            private global::FluidTrade.Core.GetContainerDelegate filterRowHandler;
			this.Type = new CodeTypeReference("GetContainerDelegate");
			this.Name = "getContainerHandler";

		}

	}

}

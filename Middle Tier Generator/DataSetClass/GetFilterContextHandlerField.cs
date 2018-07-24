namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;

    /// <summary>
	/// Creates a field that holds a delegate to a method that returns context information for the read operation.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class GetFilterContextHandlerField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds a delegate to a method that returns context information for a read operation.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public GetFilterContextHandlerField()
		{

			//            private static global::FluidTrade.Core.ObjectDelegate getFilterContextHandler;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference("FilterContextDelegate");
			this.Name = "getFilterContextHandler";

		}

	}

}

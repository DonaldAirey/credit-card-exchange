namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Generates a property that gets or sets a delegate for filtering the rows of a table for the client.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class FilterRowHandlerProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets or sets a delegate for filtering the rows of a table for the client.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public FilterRowHandlerProperty(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Gets or sets a delegate used to filter the results passed back to a client from the server.
			//            /// </summary>
			//            public FilterRowDelegate FilterRowHandler {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets a delegate used to filter the results passed back to a client from the server.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference("FilterRowDelegate");
			this.Name = "FilterRowHandler";

			//                get {
			//                    return this.filterRowHandler;
			//                }
			//                set {
			//                    this.filterRowHandler = value;
			//                }
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "filterRowHandler")));
			this.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "filterRowHandler"), new CodeVariableReferenceExpression("value")));

			//            }

		}

	}

}

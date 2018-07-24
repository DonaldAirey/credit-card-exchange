namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Generates a property that gets or sets a delegate for filtering the rows of a table for the client.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class GetContainerHandlerProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets or sets a delegate for filtering the rows of a table for the client.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public GetContainerHandlerProperty(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Gets or sets a delegate used to filter the results passed back to a client from the server.
			//            /// </summary>
			//            public GetContainerDelegate GetContainerHandler {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets a delegate used to obtain the owner of an object.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference("GetContainerDelegate");
			this.Name = "GetContainerHandler";

			//                get {
			//                    return this.getContainerHandler;
			//                }
			//                set {
			//                    this.getContainerHandler = value;
			//                }
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "getContainerHandler")));
			this.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "getContainerHandler"), new CodeVariableReferenceExpression("value")));

			//            }

		}

	}

}

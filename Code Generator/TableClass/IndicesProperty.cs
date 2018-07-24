namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

    /// <summary>
	/// Generates a property that gets the collection of indices on a table.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class IndicesProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets the collection of indices on a table.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public IndicesProperty(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Gets a collection of FluidTrade.Core.DataIndices on a table.
			//            /// </summary>
			//            public RowFilterDelegate DataIndices {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets a collection of FluidTrade.Core.DataIndices on a table.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(DataIndexCollection));
			this.Name = "Indices";

			//                get {
			//                    return this.userFilter;
			//                }
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "dataIndices")));

			//            }

		}

	}

}

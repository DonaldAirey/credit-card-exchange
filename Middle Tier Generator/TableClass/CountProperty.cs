namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a propert that gets the count of rows in the table.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class CountProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a propert that gets the count of rows in the table.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public CountProperty(TableSchema tableSchema)
		{

			//            /// <summary>
			//            /// Gets the number of rows in the Employee table.
			//            /// </summary>
			//            [global::System.ComponentModel.BrowsableAttribute(false)]
			//            public int Count {
			//                get {
			//                    try {
			//                        // The table can't be modified while the number of rows is accessed.
			//                        this.AcquireLock();
			//                        return this.Rows.Count;
			//                    }
			//                    finally {
			//                        // The table can be accessed by other threads once the number of rows is returned to the caller.
			//                        this.ReleaseLock();
			//                    }
			//                }
			//            }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the number of rows in the {0} table.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeGlobalTypeReference(typeof(System.ComponentModel.BrowsableAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(false))));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "Count";
			CodeTryCatchFinallyStatement tryCatchFinallyStatement = new CodeTryCatchFinallyStatement();
			tryCatchFinallyStatement.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), "DataLock"),
					"EnterReadLock"));
			tryCatchFinallyStatement.TryStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Rows"), "Count")));
			tryCatchFinallyStatement.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), "DataLock"),
					"ExitReadLock"));
			this.GetStatements.Add(tryCatchFinallyStatement);

		}

	}

}

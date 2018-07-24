namespace FluidTrade.MiddleTierGenerator.TableClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Represents a declaration of a property that gets the parent row.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ItemInt32Property : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property to get a parent row.
		/// </summary>
		/// <param name="foreignKeyConstraintSchema">The foreign key that references the parent table.</param>
		public ItemInt32Property(TableSchema tableSchema)
		{

			// Construct the type names for the table and rows within the table.
			string rowTypeName = string.Format("{0}Row", tableSchema.Name);

			//            /// <summary>
			//            /// Indexer to a row in the Employee table.
			//            /// </summary>
			//            /// <param name="index">The integer index of the row.</param>
			//            /// <returns>The Employee row found at the given index.</returns>
			//            public EmployeeRow this[int index] {
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Indexer to a row in the {0} table.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"index\">The integer index of the row.</param>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("<returns>The {0} row found at the given index.</returns>", tableSchema.Name), true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference(rowTypeName);
			this.Name = "Item";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Int32)), "index"));

			//                get {
			//                    try {
			//                        // The table can't be modified while the row is accessed.
			//                        this.AcquireLock();
			//                        return ((EmployeeRow)(this.Rows[index]));
			//                    }
			//                    finally {
			//                        // The table can be accessed by other threads once the row is returned to the caller.
			//                        this.ReleaseLock();
			//                    }
			//                }
			CodeTryCatchFinallyStatement tryCatchFinallyStatement = new CodeTryCatchFinallyStatement();
			tryCatchFinallyStatement.TryStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), "DataLock"),
					"EnterReadLock"));
			tryCatchFinallyStatement.TryStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(new CodeTypeReference(String.Format("{0}Row", tableSchema.Name)), new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Rows"), new CodeArgumentReferenceExpression("index")))));
			tryCatchFinallyStatement.FinallyStatements.Add(
				new CodeMethodInvokeExpression(
					new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(tableSchema.DataModel.Name), "DataLock"),
					"ExitReadLock"));
			this.GetStatements.Add(tryCatchFinallyStatement);

			//            }

		}

	}

}

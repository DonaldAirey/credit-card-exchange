namespace FluidTrade.Core.TableClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a property that gets the column.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ColumnProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property that gets the column.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		/// <param name="columnSchema">A description of the column.</param>
		public ColumnProperty(TableSchema tableSchema, ColumnSchema columnSchema)
		{

			//        /// <summary>
			//        /// Gets the EmployeeId column of the Employee table.
			//        /// </summary>
			//        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//        [global::System.ComponentModel.BrowsableAttribute(false)]
			//        public global::System.Data.DataColumn EmployeeIdColumn {
			//            get {
			//                return this.columnEmployeeId;
			//            }
			//        }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the {0} column of the {1} table.", columnSchema.Name, tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(System.Data.DataColumn));
			this.Name = string.Format("{0}Column", columnSchema.Name);
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), string.Format("column{0}", columnSchema.Name))));

		}

	}

}

namespace FluidTrade.ClientGenerator.TableClass
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

			//        /// <summary>
			//        /// Gets the number of rows in the Department table.
			//        /// </summary>
			//        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
			//        [global::System.ComponentModel.BrowsableAttribute(false)]
			//        public int Count {
			//            get {
			//                return this.Rows.Count;
			//            }
			//        }
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("Gets the number of rows in the {0} table.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "Count";
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Rows"), "Count")));

		}

	}

}

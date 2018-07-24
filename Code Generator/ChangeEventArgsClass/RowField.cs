namespace FluidTrade.Core.ChangeEventArgsClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a field for the strongly typed row.
	/// </summary>
	class RowField : CodeMemberField
	{

		/// <summary>
		/// Creates a field for the strongly typed row.
		/// </summary>
		public RowField(TableSchema tableSchema)
		{

			//            
			//            /// <summary>
			//            /// The Department row that has been changed.
			//            /// </summary>
			//            private DepartmentRow departmentRow;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement(string.Format("The {0} row that has been changed.", tableSchema.Name), true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference(string.Format("{0}Row", tableSchema.Name));
			this.Name = string.Format("{0}Row", CommonConversion.ToCamelCase(tableSchema.Name));

		}

	}
}

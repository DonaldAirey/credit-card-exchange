namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Create a field for the data model.
	/// </summary>
	class TableRowsIndexField : CodeMemberField
	{

		/// <summary>
		/// Create a field for the data model.
		/// </summary>
		public TableRowsIndexField()
		{

			this.Comments.Add(new CodeCommentStatement("The index into the array of reconciled data where the rows of data are found."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Const;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "tableRowsIndex";
			this.InitExpression = new CodePrimitiveExpression(1);

		}

	}

}

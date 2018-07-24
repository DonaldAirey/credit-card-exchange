namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Create a field for the data model.
	/// </summary>
	class RowStateIndexField : CodeMemberField
	{

		/// <summary>
		/// Create a field for the data model.
		/// </summary>
		public RowStateIndexField()
		{

			this.Comments.Add(new CodeCommentStatement("The index into the array of reconciled data where the state of the row is found."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Const;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "rowStateIndex";
			this.InitExpression = new CodePrimitiveExpression(0);

		}

	}

}

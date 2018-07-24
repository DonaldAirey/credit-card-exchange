namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	class TableTableNameIndexField : CodeMemberField
	{

		/// <summary>
		/// A private field.
		/// </summary>
		public TableTableNameIndexField()
		{

			this.Comments.Add(new CodeCommentStatement("The index into the array of reconciled data where the name of the table is found."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Const;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "tableTableNameIndex";
			this.InitExpression = new CodePrimitiveExpression(0);

		}

	}

}

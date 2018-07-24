namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	class DeletedTimeColumnField : CodeMemberField
	{

		/// <summary>
		/// Create a member to wait for a thread to expire.
		/// </summary>
		public DeletedTimeColumnField()
		{

			//		// The time to wait for a thread to respond before aborting it.
			//		private static int threadWaitTime = 1000;
			this.Comments.Add(new CodeCommentStatement("The offset to the deleted time column in the deleted data model records."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Const;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "deletedTimeColumn";
			this.InitExpression = new CodePrimitiveExpression(1);

		}

	}

}

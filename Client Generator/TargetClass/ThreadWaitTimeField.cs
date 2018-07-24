namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a private field that holds the time to wait for a thread to exit before aborting it.
	/// </summary>
	class ThreadWaitTimeField : CodeMemberField
	{

		/// <summary>
		/// Creates a private field that holds the time to wait for a thread to exit before aborting it.
		/// </summary>
		public ThreadWaitTimeField()
		{

			//        // The time to wait for a thread to respond before aborting it.
			//        private const int threadWaitTime = 1000;
			this.Comments.Add(new CodeCommentStatement("The time to wait for a thread to respond before aborting it."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Const;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "threadWaitTime";
			this.InitExpression = new CodePrimitiveExpression(5000);

		}

	}

}

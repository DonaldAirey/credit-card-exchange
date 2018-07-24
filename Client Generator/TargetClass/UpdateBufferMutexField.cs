namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	class UpdateBufferMutexField : CodeMemberField
	{

		/// <summary>
		/// A private field.
		/// </summary>
		public UpdateBufferMutexField()
		{

			this.Comments.Add(new CodeCommentStatement("Provides mutually exclusive access to the buffer used to update the client data model."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.Threading.Mutex));
			this.Name = "updateBufferMutex";

		}

	}

}

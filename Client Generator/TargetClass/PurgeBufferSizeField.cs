namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	class PurgeBufferSizeField : CodeMemberField
	{

		/// <summary>
		/// Create a member to wait for a thread to expire.
		/// </summary>
		public PurgeBufferSizeField()
		{

			//		// The number of records that go into a single buffer for reconciling deleted records with the master data model.
			//		private static int purgeBufferSize;
			this.Comments.Add(new CodeCommentStatement("The number of records that go into a single buffer for reconciling deleted records with the master data model."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.Int32));
			this.Name = "purgeBufferSize";

		}

	}

}

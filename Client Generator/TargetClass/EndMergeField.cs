namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	class EndMergeField : CodeMemberField
	{

		/// <summary>
		/// A private field.
		/// </summary>
		public EndMergeField()
		{

			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Indicates that the client data model has completed merging records from the server.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.EventHandler));
			this.Name = "EndMerge";

		}

	}

}

namespace FluidTrade.ClientGenerator.TargetClass
{

	using System;
	using System.CodeDom;
    using FluidTrade.Core;

	class ProgressField : CodeMemberField
	{

		/// <summary>
		/// A private field.
		/// </summary>
		public ProgressField()
		{

			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Represents the method that will handle an event that indicates progress has been made.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(ProgressEventHandler));
			this.Name = "Progress";

		}

	}

}

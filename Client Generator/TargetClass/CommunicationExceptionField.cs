namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	class CommunicationExceptionField : CodeMemberField
	{

		/// <summary>
		/// A private field.
		/// </summary>
		public CommunicationExceptionField()
		{

			//		/// <summary>
			//		/// Handles a communication exception from the background process.
			//		/// </summary>
			//		public static global::FluidTrade.Core.ExceptionEventHandler CommunicationException;
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Handles a communication exception from the background process.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(ExceptionEventHandler));
			this.Name = "CommunicationException";

		}

	}

}

namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Create a field for the data model.
	/// </summary>
	class IsReadingField : CodeMemberField
	{

		/// <summary>
		/// Create a field for the data model.
		/// </summary>
		public IsReadingField()
		{

			this.Comments.Add(new CodeCommentStatement("Indicates whether the client is still reconciling with the server."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.Boolean));
			this.Name = "isReading";

		}

	}

}

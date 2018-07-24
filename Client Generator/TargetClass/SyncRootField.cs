namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates an object that can be used to synchrnoize the data model.
	/// </summary>
	class SyncRootField : CodeMemberField
	{

		/// <summary>
		/// Creates an object that can be used to synchrnoize the data model.
		/// </summary>
		public SyncRootField()
		{

			//		// This object is used to synchronize the data model.
			//		private static object syncRoot;
			this.Comments.Add(new CodeCommentStatement("This object is used to synchronize the data model."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.Object));
			this.Name = "syncRoot";

		}

	}

}

namespace FluidTrade.ClientGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field for each of the tables created for this data model.
	/// </summary>
	class SyncUpdateField : CodeMemberField
	{

		/// <summary>
		/// Creates a field for each of the tables created for this data model.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public SyncUpdateField()
		{

			//        // A sync object used to wait for an update of the client data model.
			//        private static global::System.Object syncUpdate;
			this.Comments.Add(new CodeCommentStatement("A sync object used to wait for an update of the client data model."));
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.Object));
			this.Name = "syncUpdate";

		}

	}

}

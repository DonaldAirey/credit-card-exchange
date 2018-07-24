namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a property for getting the unique identifier of a record.
	/// </summary>
	class KeyProperty : CodeMemberProperty
	{

		/// <summary>
		/// Creates a property for getting the unique identifier of a record.
		/// </summary>
		public KeyProperty()
		{

			//		/// <summary>
			//		/// Gets or sets the unique identifier of this content.
			//		/// <summary>
			//		public object Key
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets or sets the unique identifier of this content.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeTypeReference(typeof(System.Object));
			this.Name = "Key";

			//			get
			//			{
			//				return this.key;
			//			}
			this.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "key")));

			//			set
			//			{
			//				this.key = value;
			//			}
			this.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "key"), new CodeVariableReferenceExpression("value")));

			//		}

		}

	}
}

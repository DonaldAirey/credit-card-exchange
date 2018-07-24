namespace FluidTrade.ClientGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
	/// </summary>
	class OnEndMergeMethod : CodeMemberMethod
	{

		/// <summary>
		/// Creates a method to handle moving the deleted records from the active data model to the deleted data model.
		/// </summary>
		/// <param name="schema">The data model schema.</param>
		public OnEndMergeMethod(DataModelSchema dataModelSchema)
		{

			//		/// <summary>
			//		/// Broadcasts an event when the data model is has finished changing.
			//		/// </summary>
			//		/// <param name="sender">Object originating the event.</param>
			//		public static void OnEndMerge(object sender)
			//		{
			//
			//			// If a handler has been associated with this event, invoke it.
			//			if (DataModel.EndMerge != null)
			//				DataModel.EndMerge(sender, EventArgs.Empty);
			//
			//		}
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Broadcasts an event when the data model has finished changing.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"sender\">The object that originated the event.</param>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForMethods());
			this.Attributes = MemberAttributes.Final | MemberAttributes.Static | MemberAttributes.Public;
			this.Name = "OnEndMerge";
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(System.Object)), "sender"));
			this.Statements.Add(new CodeCommentStatement("Broadcast the event to any listeners."));
			this.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "EndMerge"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
				new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(dataModelSchema.Name), "EndMerge", new CodeArgumentReferenceExpression("sender"), new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(EventArgs)), "Empty")))));

		}

	}
}

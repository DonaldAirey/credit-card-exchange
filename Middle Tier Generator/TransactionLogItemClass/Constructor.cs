namespace FluidTrade.MiddleTierGenerator.TransactionLogItemClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a static constructor for the transaction.
	/// </summary>
	class Constructor : CodeConstructor
	{

		/// <summary>
		/// Creates a static constructor for the transaction.
		/// </summary>
		/// <param name="dataModelSchema">A description of the data model.</param>
		public Constructor(DataModelSchema dataModelSchema)
		{

			//	/// <summary>
			//	/// An item in the transaction log.
			//	/// </summary>
			//	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("FluidTrade.Core.MiddleTier.MiddleTierGenerator", "1.3.0.0")]
			//	[global::System.ComponentModel.DesignerCategoryAttribute("code")]
			//	public class TransactionLogItem
			//	{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Creates a collector of fields for the transaction log compression.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"containerContext\">An object that contains other objects.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"data\">The data for the transaction log item.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"keyLength\">the length of the key on the item.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"sequence\">The sequence of the item in the log.</param>", true));
			this.Comments.Add(new CodeCommentStatement("<param name=\"timeStamp\">The time the item was entered into the log.</param>", true));
			this.Attributes = MemberAttributes.Assembly;
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object)), "containerContext"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Object[])), "data"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Int32)), "keyLength"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(Int64)), "sequence"));
			this.Parameters.Add(new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(typeof(DateTime)), "timeStamp"));

			//			this.containerContext = containerContext;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "containerContext"), new CodeArgumentReferenceExpression("containerContext")));

			//			this.data = data;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "data"), new CodeArgumentReferenceExpression("data")));

			//			this.keyLength = keyLength;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "keyLength"), new CodeArgumentReferenceExpression("keyLength")));

			//			this.sequence = sequence;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "sequence"), new CodeArgumentReferenceExpression("sequence")));

			//			this.timeStamp = timeStamp;
			this.Statements.Add(
				new CodeAssignStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "timeStamp"), new CodeArgumentReferenceExpression("timeStamp")));

			//		}

		}

	}

}

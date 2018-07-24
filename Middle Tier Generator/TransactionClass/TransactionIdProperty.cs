namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Generates a property that gets transaction identifier.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionIdProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets transaction identifier.
		/// </summary>
		public TransactionIdProperty()
		{

			//		/// <summary>
			//		/// Gets the unique identifier of this transaction.
			//		/// </summary>
			//		public global::System.Guid TransactionId
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets the unique identifier of this transaction.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(Guid));
			this.Name = "TransactionId";

			//			get { return this.transactionId; }
			this.GetStatements.Add(
				new CodeMethodReturnStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionId")));

			//		}

		}

	}

}

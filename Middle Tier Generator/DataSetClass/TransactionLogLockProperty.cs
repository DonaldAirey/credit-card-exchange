namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Generates a property that gets or sets a delegate for filtering the rows of a table for the client.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionLogLockProperty : CodeMemberProperty
	{

		/// <summary>
		/// Generates a property that gets or sets a delegate for releasing tables that were locked for a filtered read operation.
		/// </summary>
		/// <param name="tableSchema">A description of the table.</param>
		public TransactionLogLockProperty()
		{

			//		/// <summary>
			//		/// Gets the Reader/Writer lock for the current data model.
			//		/// </summary>
			//		public global::System.Threading.ReaderWriterLockSlim TransactionLogLock
			//		{
			this.Comments.Add(new CodeCommentStatement("<summary>", true));
			this.Comments.Add(new CodeCommentStatement("Gets the Reader/Writer lock for the current data model.", true));
			this.Comments.Add(new CodeCommentStatement("</summary>", true));
			this.CustomAttributes.AddRange(new CodeCustomAttributesForProperties());
			this.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			this.Type = new CodeGlobalTypeReference(typeof(ReaderWriterLockSlim));
			this.Name = "TransactionLogLock";

			//			get
			//			{
			//				return this.transactionLogLock;
			//			}
			this.GetStatements.Add(
				new CodeMethodReturnStatement(
					new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "transactionLogLock")));

			//		}

		}

	}

}

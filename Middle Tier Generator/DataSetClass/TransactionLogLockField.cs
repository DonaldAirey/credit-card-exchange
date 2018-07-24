namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionLogLockField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the reader/writer lock for the current data model.
		/// </summary>
		public TransactionLogLockField()
		{

			//		internal static global::System.Threading.ReaderWriterLockSlim transactionLogLock;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(ReaderWriterLockSlim));
			this.Name = "transactionLogLock";

		}

	}

}

namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;

    /// <summary>
	/// Creates a field that holds a delegate to a method that filters rows from the client data model.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionLogField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the reader/writer lock for the current data model.
		/// </summary>
		public TransactionLogField()
		{

			//				private global::System.Collections.Generic.LinkedList<TransactionLogItem> transactionLog;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference("global::System.Collections.Generic.LinkedList<TransactionLogItem>");
			this.Name = "transactionLog";

		}

	}

}

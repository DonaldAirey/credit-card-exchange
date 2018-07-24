namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a field that holds the records used in a transaction.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class RecordListField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the records used in a transaction.
		/// </summary>
		public RecordListField()
		{

			//		private global::System.Collections.Generic.List<global::FluidTrade.Core.IRow> records;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference("global::System.Collections.Generic.List<global::FluidTrade.Core.IRow>");
			this.Name = "recordList";

		}

	}

}

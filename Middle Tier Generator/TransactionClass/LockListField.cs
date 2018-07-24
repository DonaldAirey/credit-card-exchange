namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System.CodeDom;

	/// <summary>
	/// Creates a field that holds the locks used in a transaction.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class LockListField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the locks used in a transaction.
		/// </summary>
		public LockListField()
		{

			//		private global::System.Collections.Generic.List<IRow> locks;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference("global::System.Collections.Generic.List<global::FluidTrade.Core.IRow>");
			this.Name = "lockList";

		}

	}

}

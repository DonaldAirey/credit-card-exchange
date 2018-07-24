namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that provides a way to look up a transaction based on the local transaction identifier.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionTableField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that provides a way to look up a transaction based on the local transaction identifier.
		/// </summary>
		public TransactionTableField(DataModelSchema dataModelSchema)
		{

			//		private static global::System.Collections.Generic.Dictionary<string, DataModelTransaction> transactionTable;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeTypeReference(String.Format("global::System.Collections.Generic.Dictionary<string, {0}Transaction>", dataModelSchema.Name));
			this.Name = "transactionTable";

		}

	}

}

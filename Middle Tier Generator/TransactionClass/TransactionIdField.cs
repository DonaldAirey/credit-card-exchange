namespace FluidTrade.MiddleTierGenerator.TransactionClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a field that holds the records used in a transaction.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionIdField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that holds the records used in a transaction.
		/// </summary>
		public TransactionIdField()
		{

			//		private global::System.Guid transactionId;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(Guid));
			this.Name = "transactionId";

		}

	}

}

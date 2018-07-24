namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that defines the number of transaction log items that will be examined before surrending the CPU.
	/// </summary>
	/// <copyright>Copyright © 2006-2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	class TransactionLogCompressionSizeField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that defines the number of transaction log items that will be examined before surrending the CPU.
		/// </summary>
		public TransactionLogCompressionSizeField()
		{

			//		private int transactionLogCompressionSize;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(Int32));
			this.Name = "transactionLogCompressionSize";

		}

	}

}

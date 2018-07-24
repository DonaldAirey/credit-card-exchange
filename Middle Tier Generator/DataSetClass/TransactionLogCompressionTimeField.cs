namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that defines the interval between compressing the transaction log.
	/// </summary>
	class TransactionLogCompressionTimeField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that defines the interval between compressing the transaction log.
		/// </summary>
		public TransactionLogCompressionTimeField()
		{

			//        private global::System.TimeSpan transactionLogCompressionTime;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(TimeSpan));
			this.Name = "transactionLogCompressionTime";

		}

	}

}

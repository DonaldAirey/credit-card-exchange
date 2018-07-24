namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Creates a field that defines how old an transaction item can be before it is consolodated.
	/// </summary>
	class TransactionLogAgeField : CodeMemberField
	{

		/// <summary>
		/// Creates a field that defines how old an transaction item can be before it is consolodated.
		/// </summary>
		public TransactionLogAgeField()
		{

			//        private static global::System.Threading.Thread garbageCollector;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(TimeSpan));
			this.Name = "transactionLogAge";

		}

	}

}

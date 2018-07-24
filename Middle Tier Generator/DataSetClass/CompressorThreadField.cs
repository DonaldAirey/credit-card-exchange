namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System.CodeDom;
    using System.Threading;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a private field that holds the thread that purges the data set of deleted records.
	/// </summary>
	class CompressorThreadField : CodeMemberField
	{

		/// <summary>
		/// Creates a private field that holds the thread that purges the data set of deleted records.
		/// </summary>
		public CompressorThreadField()
		{

			//        // A thread that purges deleted fields when they have become obsolete.
			//        private static global::System.Threading.Thread garbageCollector;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(Thread));
			this.Name = "compressorThread";

		}

	}

}

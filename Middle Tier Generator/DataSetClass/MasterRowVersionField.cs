namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a private field to hold the master row version counter.
	/// </summary>
	class MasterRowVersionField : CodeMemberField
	{

		/// <summary>
		/// Creates a private field to hold the master row version counter.
		/// </summary>
		public MasterRowVersionField()
		{

			//        // Keeps track of the row version for the entire data model.
			//        private static long masterRowVersion;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(Int64));
			this.Name = "rowVersion";

		}

	}

}

namespace FluidTrade.MiddleTierGenerator.DataSetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a private field that holds the amount of time a record lock is held.
	/// </summary>
	class IdentifierField : CodeMemberField
	{

		/// <summary>
		/// Creates a private field that holds the amount of time a record lock is held.
		/// </summary>
		public IdentifierField()
		{

			//        // The maximum amount of time that the server will wait for a lock.
			//        private static global::System.TimeSpan lockTimeout;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeGlobalTypeReference(typeof(Guid));
			this.Name = "identifier";

		}

	}

}

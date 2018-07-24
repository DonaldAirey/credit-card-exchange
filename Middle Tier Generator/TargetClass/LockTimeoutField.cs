namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a private field that holds the amount of time a record lock is held.
	/// </summary>
	class LockTimeoutField : CodeMemberField
	{

		/// <summary>
		/// Creates a private field that holds the amount of time a record lock is held.
		/// </summary>
		public LockTimeoutField()
		{

			//        // The maximum amount of time that the server will wait for a lock.
			//        private static global::System.TimeSpan lockTimeout;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.TimeSpan));
			this.Name = "lockTimeout";

		}

	}

}

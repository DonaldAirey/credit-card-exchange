namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Creates a private field that controls how long deleted records are kept until purged.
	/// </summary>
	class CompactTimeField : CodeMemberField
	{

		/// <summary>
		/// Creates a private field that controls how long deleted records are kept until purged.
		/// </summary>
		public CompactTimeField()
		{

			//        // The time that deleted records are allowed to hang around until they are purged for good.
			//        private static global::System.TimeSpan freshnessTime = new global::System.TimeSpan(0, 1, 0);
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(System.TimeSpan));
			this.Name = "compactTime";
			this.InitExpression = new CodeObjectCreateExpression(this.Type, new CodePrimitiveExpression(0), new CodePrimitiveExpression(1), new CodePrimitiveExpression(0));

		}

	}

}

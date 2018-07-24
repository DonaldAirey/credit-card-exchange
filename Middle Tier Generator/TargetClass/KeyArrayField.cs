namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// A private field.
	/// </summary>
	class KeyArrayField : CodeMemberField
	{

		/// <summary>
		/// A private field.
		/// </summary>
		public KeyArrayField(TableSchema tableSchema)
		{

			//			/// This is an array of indices used to find a record based on an external identifier.
			//			public Index[] externalKeyArray;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference(typeof(DataIndex[]));
			this.Name = "externalKeyArray";

		}

	}
}

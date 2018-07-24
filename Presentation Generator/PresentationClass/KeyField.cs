namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// Create an internal field to hold the unique identifier of the record.
	/// </summary>
	class KeyField : CodeMemberField
	{

		/// <summary>
		/// Create an internal field to hold the unique identifier of the record.
		/// </summary>
		public KeyField()
		{

			//		internal object key;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeGlobalTypeReference(typeof(System.Object));
			this.Name = "key";

		}

	}
}

namespace FluidTrade.ClientGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

	class DataSetIdField : CodeMemberField
	{

		/// <summary>
		/// A private field.
		/// </summary>
		public DataSetIdField()
		{

			//		private static global::System.Guid dataSetId;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeGlobalTypeReference(typeof(Guid));
			this.Name = "dataSetId";

		}

	}

}

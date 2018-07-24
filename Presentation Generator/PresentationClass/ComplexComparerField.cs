namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// A private field used to hold a complex sorting algorithm.
	/// </summary>
	class ComplexComparerField : CodeMemberField
	{

		/// <summary>
		/// A private field used to hold a complex sorting algorithm.
		/// </summary>
		public ComplexComparerField(OrderBySchema orderBySchema)
		{

			//		private ComplexComparer<Sandbox.WorkingOrder.WorkingOrder> comparer;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference(string.Format("FluidTrade.Core.ComplexComparer<{0}>", orderBySchema.ResultType));
			this.Name = CommonConversion.ToCamelCase(orderBySchema.Comparer);

		}

	}
}

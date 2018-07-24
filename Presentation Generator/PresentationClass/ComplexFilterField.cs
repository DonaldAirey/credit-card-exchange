namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// A private field used to hold a complex filtering algorithm.
	/// </summary>
	class ComplexFilterField : CodeMemberField
	{

		/// <summary>
		/// A private field used to hold a complex filtering algorithm.
		/// </summary>
		public ComplexFilterField(WhereSchema whereSchema)
		{

			//		private ComplexFilter<Sandbox.WorkingOrder.WorkingOrder> filter;
			this.Attributes = MemberAttributes.Private;
            this.Type = new CodeTypeReference(string.Format("FluidTrade.Core.ComplexFilter<{0}>", whereSchema.ResultType));
			this.Name = CommonConversion.ToCamelCase(whereSchema.Predicate);

		}

	}
}

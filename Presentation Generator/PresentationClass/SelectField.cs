namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// A private field used to hold a complex sorting algorithm.
	/// </summary>
	class SelectField : CodeMemberField
	{

		/// <summary>
		/// A private field used to hold a complex sorting algorithm.
		/// </summary>
		public SelectField(SelectSchema selectSchema)
		{

			//		private ComplexComparer<Sandbox.WorkingOrder.WorkingOrder> comparer;
			this.Attributes = MemberAttributes.Private;
			this.Type = new CodeTypeReference(string.Format("System.Func<{0}, {1}>", selectSchema.SourceType, selectSchema.ResultType));
			this.Name = CommonConversion.ToCamelCase(selectSchema.Selector);

		}

	}
}

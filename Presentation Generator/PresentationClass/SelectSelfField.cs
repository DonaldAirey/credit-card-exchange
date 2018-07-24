namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;

	/// <summary>
	/// Generates an internal field that holds a delegate to a function that selects an unmodified record for a query.
	/// </summary>
	class SelectSelfField : CodeMemberField
	{

		/// <summary>
		/// Generates an internal field that holds a delegate to a function that selects an unmodified record for a query.
		/// </summary>
		public SelectSelfField(ClassSchema classSchema)
		{

			//		private static System.Func<Sandbox.WorkingOrder.AskPrice, FluidTrade.Sandbox.WorkingOrder.AskPrice> selectSelf;
			this.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			this.Type = new CodeTypeReference(string.Format("System.Func<{0}, {0}>", classSchema.Type));
			this.Name = "selectSelf";

		}

	}

}

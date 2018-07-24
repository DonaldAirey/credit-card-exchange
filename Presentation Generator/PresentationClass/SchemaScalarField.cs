namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// An internal scalar field.
	/// </summary>
	class SchemaScalarField : CodeMemberField
	{

		/// <summary>
		/// An internal scalar field.
		/// </summary>
		public SchemaScalarField(PropertySchema propertySchema)
		{

			//		internal FluidTrade.Sandbox.WorkingOrder.AskPrice askPrice;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeTypeReference(propertySchema.Type);
			this.Name = CommonConversion.ToCamelCase(propertySchema.Name);

		}

	}
}

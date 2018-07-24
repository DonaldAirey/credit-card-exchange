namespace FluidTrade.PresentationGenerator.PresentationClass
{

    using System.CodeDom;
    using FluidTrade.Core;

	/// <summary>
	/// An internal vector field.
	/// </summary>
	class SchemaVectorField : CodeMemberField
	{

		/// <summary>
		/// An internal vector field.
		/// </summary>
		public SchemaVectorField(PropertySchema propertySchema)
		{

			//		internal System.Collections.Generic.IEnumerable<Sandbox.WorkingOrder.WorkingOrder> workingOrderList;
			this.Attributes = MemberAttributes.Assembly;
			this.Type = new CodeTypeReference(string.Format("{0}[]", propertySchema.Type));
			this.Name = CommonConversion.ToCamelCase(propertySchema.Name);

		}

	}
}

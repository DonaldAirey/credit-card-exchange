namespace FluidTrade.Core
{

	using System;
    using System.CodeDom;

	/// <summary>
	/// Describes a parameter that is exposed through an interface.
	/// </summary>
	public class ExternalParameterItem
	{

		// Public Instance Fields
		public System.Type ActualDataType;
		public System.Type DeclaredDataType;
		public String Description;
		public System.CodeDom.FieldDirection FieldDirection;
		public System.Boolean IsNullable;
		public String Name;

		/// <summary>
		/// Creates a CodeParameterDeclarationExpression from the description of the parameter.
		/// </summary>
		public CodeParameterDeclarationExpression CodeParameterDeclarationExpression
		{
			get
			{
				CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(new CodeGlobalTypeReference(this.DeclaredDataType), this.Name);
				codeParameterDeclarationExpression.Direction = this.FieldDirection;
				return codeParameterDeclarationExpression;
			}
		}

	}

}

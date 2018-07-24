namespace FluidTrade.Core
{

	using System;
    using System.CodeDom;

	/// <summary>
	/// Creates an expression of a representation of a reference to a global type.
	/// </summary>
	public class CodeGlobalTypeReferenceExpression : CodeTypeReferenceExpression
	{

		/// <summary>
		/// Creates an expression of a representation of a reference to a global type.
		/// </summary>
		/// <param name="type">The CLR type used to create a reference.</param>
		public CodeGlobalTypeReferenceExpression(Type type)
		{

			// global::System.Data.DataRow
			this.Type = new CodeGlobalTypeReference(type);

		}

	}

}

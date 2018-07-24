namespace FluidTrade.Core
{

	using System;
    using System.CodeDom;

	/// <summary>
	/// Creates a representation of a reference to a global type.
	/// </summary>
	public class CodeGlobalTypeReference : CodeTypeReference
	{

		/// <summary>
		/// Creates a representation of a reference to a global type.
		/// </summary>
		/// <param name="type">The CLR type used to create a reference.</param>
		public CodeGlobalTypeReference(Type type)
		{

			// global::System.Data.DataRow
			this.BaseType = type.FullName;
			this.Options = CodeTypeReferenceOptions.GlobalReference;

		}

	}

}

namespace FluidTrade.Core
{

    using System.CodeDom;

	/// <summary>
	/// Represents an expression that creates an object array using an array of ColumnSchemas.
	/// </summary>
	public class CodeKeyCreateExpression : CodeArrayCreateExpression
	{

		/// <summary>
		/// Represents an expression that creates an object array using an array of ColumnSchemas.
		/// </summary>
		/// <param name="columns">The columns used to create the array expression.</param>
		public CodeKeyCreateExpression(ColumnSchema[] columns)
		{

			this.CreateType = new CodeGlobalTypeReference(typeof(System.Object));
			foreach (ColumnSchema columnSchema in columns)
				this.Initializers.Add(new CodeArgumentReferenceExpression(CommonConversion.ToCamelCase(columnSchema.Name)));

		}

		/// <summary>
		/// Represents a statement that creates an array of values for use as a key.
		/// </summary>
		/// <param name="rowExpression">An expression representing the row that owns the columns.</param>
		/// <param name="columns">An array describing a set of columns.</param>
		public CodeKeyCreateExpression(CodeExpression rowExpression, ColumnSchema[] columns)
		{

			this.CreateType = new CodeGlobalTypeReference(typeof(System.Object));
			foreach (ColumnSchema columnSchema in columns)
				this.Initializers.Add(new CodePropertyReferenceExpression(rowExpression, columnSchema.Name));

		}

		/// <summary>
		/// Represents a statement that creates an array of values for use as a key.
		/// </summary>
		/// <param name="rowExpression">An expression representing the row that owns the columns.</param>
		/// <param name="columns">An array describing a set of columns.</param>
		public CodeKeyCreateExpression(params CodeExpression[] codeExpressionArray)
		{

			this.CreateType = new CodeGlobalTypeReference(typeof(System.Object));
			foreach (CodeExpression codeExpression in codeExpressionArray)
				this.Initializers.Add(codeExpression);

		}

	}

}

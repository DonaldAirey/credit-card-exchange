namespace FluidTrade.Core
{

	using System;
    using System.CodeDom;

	/// <summary>
	/// Converts data types from and to CodeDOM values.
	/// </summary>
	public class CodeConvert
	{

		/// <summary>
		/// Generates a code expression from a generic value.
		/// </summary>
		/// <returns>A CodeDOM expression representing the value of the object.</returns>
		public static CodeExpression CreateConstantExpression(object value)
		{

			// If the column provides a fixed value, then translate the default text into the proper datatype for the column.
			switch (value.GetType().ToString())
			{
			case "System.Boolean":
			case "System.Int16":
			case "System.Int32":
			case "System.Int64":
			case "System.Decimal":
			case "System.Double":
			case "System.String":
				return new CodePrimitiveExpression(value);
			case "System.Guid":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Guid)), "NewGuid");
			case "System.DBNull":
				return new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(DBNull)), "Value");
			default:
				if (value.GetType().IsEnum)
					return new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(value.GetType()), Enum.GetName(value.GetType(), value));
				break;
			}

			throw new Exception(string.Format("There is no CodeDOM expression for {0}", value));

		}

		/// <summary>
		/// Creates a method to convert a value to a CLR type.
		/// </summary>
		/// <param name="type">The CLR type to which the value is converted.</param>
		/// <param name="sourceExpression">The CodeDOM expression of the value to be converted.</param>
		/// <returns>A CodeDOM expression to convert a value from a string to a CLR type.</returns>
		public static CodeExpression ConversionMethod(Type type, CodeExpression sourceExpression)
		{

			// Use the destination type to drive the creation of a statement that will convert text into a CLR value.
			switch (type.ToString())
			{

			case "System.Object":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToString", sourceExpression);

			case "System.Boolean":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToBoolean", sourceExpression);

			case "System.Int16":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToInt16", sourceExpression);

			case "System.Int32":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToInt32", sourceExpression);

			case "System.Int64":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToInt64", sourceExpression);

			case "System.Decimal":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToDecimal", sourceExpression);

			case "System.Double":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToDouble", sourceExpression);

			case "System.DateTime":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToDateTime", sourceExpression);

			case "System.String":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToString", sourceExpression);

			case "System.Guid":
				return new CodeObjectCreateExpression(new CodeGlobalTypeReference(typeof(System.Guid)), new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "ToString", sourceExpression));

			case "System.Byte[]":
				return new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Convert)), "FromBase64String", new CodeCastExpression(typeof(String), sourceExpression));

			default:

				if (type.IsEnum)
					return new CodeCastExpression(type, new CodeMethodInvokeExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Enum)), "Parse", new CodeTypeOfExpression(type), new CodeCastExpression(typeof(String), sourceExpression)));

				break;

			}

			// Throw the exception to catch any data types that aren't converted above.
			throw new Exception(string.Format("There is no conversion expression that can be created for a {0} type.", type));

		}

	}

}

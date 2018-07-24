namespace FluidTrade.MiddleTierGenerator.TargetClass
{

    using System;
    using System.CodeDom;
    using FluidTrade.Core;

    /// <summary>
	/// Converts CLR types to SQL types.
	/// </summary>
	public class TypeConverter
	{

		/// <summary>
		/// Creates an expression of a CLR type as an SQL type.
		/// </summary>
		/// <param name="type">The native CLR data type to be converted.</param>
		/// <returns>A CodeDOM expression of that data type as an SQL type.</returns>
		public static CodeExpression Convert(Type type)
		{

			string sqlDataType = string.Empty;

			// This will convert the native CLR type to an SQL data type.
			switch (type.ToString())
			{
				case "System.Boolean": sqlDataType = "Bit"; break;
				case "System.Byte": sqlDataType = "TinyInt"; break;
				case "System.Byte[]": sqlDataType = "Image"; break;
				case "System.DateTime": sqlDataType = "DateTime"; break;
				case "System.Decimal": sqlDataType = "Decimal"; break;
				case "System.Double": sqlDataType = "Float"; break;
				case "System.Float": sqlDataType = "Real"; break;
				case "System.Guid": sqlDataType = "UniqueIdentifier"; break;
				case "System.Int16": sqlDataType = "SmallInt"; break;
				case "System.Int32": sqlDataType = "Int"; break;
				case "System.Int64": sqlDataType = "BigInt"; break;
				case "System.Object": sqlDataType = "Variant"; break;
				case "System.String": sqlDataType = "NVarChar"; break;

			default:

				// Enums are converted into a generic data type on the SQL Server.
				if (type.IsSubclassOf(typeof(System.Enum)))
					return Convert(Enum.GetUnderlyingType(type));
				sqlDataType = "Variant";
				break;

			}

			// This expression can be used when constructing parameters for an SQL query.
			return new CodePropertyReferenceExpression(new CodeGlobalTypeReferenceExpression(typeof(System.Data.SqlDbType)), sqlDataType);

		}

	}

}

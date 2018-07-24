namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection.Emit;

	internal abstract class LiteralElement : ExpressionElement
	{

		protected void OnParseOverflow(string image)
		{
			base.ThrowCompileException(CompileErrorResourceKeys.ValueNotRepresentableInType, CompileExceptionReason.ConstantOverflow, image, this.ResultType.Name);
		}

		public static void EmitInt32Load(Int32 value, FleeILGenerator ilg)
		{
			if (value >= -1 & value <= 8)
			{
				EmitSuperShort(value, ilg);
			}
			else if (value >= sbyte.MinValue & value <= sbyte.MaxValue)
			{
				ilg.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
			}
			else
			{
				ilg.Emit(OpCodes.Ldc_I4, value);
			}
		}

		protected static void EmitInt64Load(Int64 value, FleeILGenerator ilg)
		{
			if (value >= Int32.MinValue & value <= Int32.MaxValue)
			{
				EmitInt32Load((int)value, ilg);
				ilg.Emit(OpCodes.Conv_I8);
			}
			else if (value >= 0 & value <= UInt32.MaxValue)
			{
				EmitInt32Load((int)value, ilg);
				ilg.Emit(OpCodes.Conv_U8);
			}
			else
			{
				ilg.Emit(OpCodes.Ldc_I8, value);
			}
		}

		private static void EmitSuperShort(Int32 value, FleeILGenerator ilg)
		{
			OpCode ldcOpcode = default(OpCode);

			switch (value)
			{
			case 0:
				ldcOpcode = OpCodes.Ldc_I4_0;
				break;
			case 1:
				ldcOpcode = OpCodes.Ldc_I4_1;
				break;
			case 2:
				ldcOpcode = OpCodes.Ldc_I4_2;
				break;
			case 3:
				ldcOpcode = OpCodes.Ldc_I4_3;
				break;
			case 4:
				ldcOpcode = OpCodes.Ldc_I4_4;
				break;
			case 5:
				ldcOpcode = OpCodes.Ldc_I4_5;
				break;
			case 6:
				ldcOpcode = OpCodes.Ldc_I4_6;
				break;
			case 7:
				ldcOpcode = OpCodes.Ldc_I4_7;
				break;
			case 8:
				ldcOpcode = OpCodes.Ldc_I4_8;
				break;
			case -1:
				ldcOpcode = OpCodes.Ldc_I4_M1;
				break;
			default:
				Debug.Assert(false, "value out of range");
				break;
			}

			ilg.Emit(ldcOpcode);
		}
	}

	internal abstract class IntegralLiteralElement : LiteralElement
	{

		protected IntegralLiteralElement()
		{

		}

		// Attempt to find the first type of integer that a number can fit into
		public static LiteralElement Create(string image, bool isHex, bool negated, IServiceProvider services)
		{
			StringComparison comparison = StringComparison.OrdinalIgnoreCase;

			ExpressionOptions options = services.GetService(typeof(ExpressionOptions)) as ExpressionOptions;

			// Convert to a double if option is set
			if (options.IntegersAsDoubles == true)
			{
				return DoubleLiteralElement.Parse(image, services);
			}

			bool hasUSuffix = image.EndsWith("u", comparison) & !image.EndsWith("lu", comparison);
			bool hasLSuffix = image.EndsWith("l", comparison) & !image.EndsWith("ul", comparison);
			bool hasULSuffix = image.EndsWith("ul", comparison) | image.EndsWith("lu", comparison);
			bool hasSuffix = hasUSuffix | hasLSuffix | hasULSuffix;

			LiteralElement constant = default(LiteralElement);
			NumberStyles numStyles = NumberStyles.Integer;

			if (isHex == true)
			{
				numStyles = NumberStyles.AllowHexSpecifier;
				image = image.Remove(0, 2);
			}

			if (hasSuffix == false)
			{
				// If the literal has no suffix, it has the first of these types in which its value can be represented: int, uint, long, ulong.
				constant = Int32LiteralElement.TryCreate(image, isHex, negated);

				if ((constant != null))
				{
					return constant;
				}

				constant = UInt32LiteralElement.TryCreate(image, numStyles);

				if ((constant != null))
				{
					return constant;
				}

				constant = Int64LiteralElement.TryCreate(image, isHex, negated);

				if ((constant != null))
				{
					return constant;
				}

				return new UInt64LiteralElement(image, numStyles);
			}
			else if (hasUSuffix == true)
			{
				image = image.Remove(image.Length - 1);
				// If the literal is suffixed by U or u, it has the first of these types in which its value can be represented: uint, ulong.

				constant = UInt32LiteralElement.TryCreate(image, numStyles);

				if ((constant != null))
				{
					return constant;
				}
				else
				{
					return new UInt64LiteralElement(image, numStyles);
				}
			}
			else if (hasLSuffix == true)
			{
				// If the literal is suffixed by L or l, it has the first of these types in which its value can be represented: long, ulong.
				image = image.Remove(image.Length - 1);

				constant = Int64LiteralElement.TryCreate(image, isHex, negated);

				if ((constant != null))
				{
					return constant;
				}
				else
				{
					return new UInt64LiteralElement(image, numStyles);
				}
			}
			else
			{
				// If the literal is suffixed by UL, Ul, uL, ul, LU, Lu, lU, or lu, it is of type ulong.
				Debug.Assert(hasULSuffix == true, "expecting ul suffix");
				image = image.Remove(image.Length - 2);
				return new UInt64LiteralElement(image, numStyles);
			}
		}
	}

}
namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Globalization;
	using System.Reflection.Emit;

	internal class Int32LiteralElement : IntegralLiteralElement
	{

		private Int32 MyValue;
		private const string MinValue = "2147483648";
		private bool MyIsMinValue;

		public Int32LiteralElement(Int32 value)
		{
			MyValue = value;
		}

		private Int32LiteralElement()
		{
			MyIsMinValue = true;
		}

		public static Int32LiteralElement TryCreate(string image, bool isHex, bool negated)
		{
			if (negated == true & image == MinValue)
			{
				return new Int32LiteralElement();
			}
			else if (isHex == true)
			{
				Int32 value = default(Int32);

				// Since Int32.TryParse will succeed for a string like 0xFFFFFFFF we have to do some special handling
				if (Int32.TryParse(image, NumberStyles.AllowHexSpecifier, null, out value) == false)
				{
					return null;
				}
				else if (value >= 0 & value <= Int32.MaxValue)
				{
					return new Int32LiteralElement(value);
				}
				else
				{
					return null;
				}
			}
			else
			{
				Int32 value = default(Int32);

				if (Int32.TryParse(image, out value) == true)
				{
					return new Int32LiteralElement(value);
				}
				else
				{
					return null;
				}
			}
		}

		public void Negate()
		{
			if (MyIsMinValue == true)
			{
				MyValue = Int32.MinValue;
			}
			else
			{
				MyValue = -MyValue;
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			EmitInt32Load(MyValue, ilg);
		}

		public override System.Type ResultType
		{
			get { return typeof(Int32); }
		}
	}

	internal class UInt32LiteralElement : IntegralLiteralElement
	{

		private UInt32 MyValue;

		public UInt32LiteralElement(UInt32 value)
		{
			MyValue = value;
		}

		public static UInt32LiteralElement TryCreate(string image, NumberStyles ns)
		{
			UInt32 value = default(UInt32);
			if (UInt32.TryParse(image, ns, null, out value) == true)
			{
				return new UInt32LiteralElement(value);
			}
			else
			{
				return null;
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			EmitInt32Load((int)MyValue, ilg);
		}

		public override System.Type ResultType
		{
			get { return typeof(UInt32); }
		}
	}

	internal class Int64LiteralElement : IntegralLiteralElement
	{

		private Int64 MyValue;
		private const string MinValue = "9223372036854775808";
		private bool MyIsMinValue;

		public Int64LiteralElement(Int64 value)
		{
			MyValue = value;
		}

		private Int64LiteralElement()
		{
			MyIsMinValue = true;
		}

		public static Int64LiteralElement TryCreate(string image, bool isHex, bool negated)
		{
			if (negated == true & image == MinValue)
			{
				return new Int64LiteralElement();
			}
			else if (isHex == true)
			{
				Int64 value = default(Int64);

				if (Int64.TryParse(image, NumberStyles.AllowHexSpecifier, null, out value) == false)
				{
					return null;
				}
				else if (value >= 0 & value <= Int64.MaxValue)
				{
					return new Int64LiteralElement(value);
				}
				else
				{
					return null;
				}
			}
			else
			{
				Int64 value = default(Int64);

				if (Int64.TryParse(image, out value) == true)
				{
					return new Int64LiteralElement(value);
				}
				else
				{
					return null;
				}
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			EmitInt64Load(MyValue, ilg);
		}

		public void Negate()
		{
			if (MyIsMinValue == true)
			{
				MyValue = Int64.MinValue;
			}
			else
			{
				MyValue = -MyValue;
			}
		}

		public override System.Type ResultType
		{
			get { return typeof(Int64); }
		}
	}

	internal class UInt64LiteralElement : IntegralLiteralElement
	{

		private UInt64 MyValue;

		public UInt64LiteralElement(string image, NumberStyles ns)
		{
			try
			{
				MyValue = UInt64.Parse(image, ns);
			}
			catch (OverflowException)
			{
				base.OnParseOverflow(image);
			}
		}

		public UInt64LiteralElement(UInt64 value)
		{
			MyValue = value;
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			EmitInt64Load((long)MyValue, ilg);
		}

		public override System.Type ResultType
		{
			get { return typeof(UInt64); }
		}
	}

}
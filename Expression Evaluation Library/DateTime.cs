namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Globalization;

	internal class DateTimeLiteralElement : LiteralElement
	{

		private DateTime MyValue;

		public DateTimeLiteralElement(string image, ExpressionContext context)
		{
			ExpressionParserOptions options = context.ParserOptions;

			if (DateTime.TryParseExact(image, options.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out MyValue) == false)
			{
				base.ThrowCompileException(CompileErrorResourceKeys.CannotParseType, CompileExceptionReason.InvalidFormat, typeof(DateTime).Name);
			}
		}

		public override void Emit(FleeILGenerator ilg, System.IServiceProvider services)
		{
			int index = ilg.GetTempLocalIndex(typeof(DateTime));

			Utility.EmitLoadLocalAddress(ilg, index);

			LiteralElement.EmitInt64Load(MyValue.Ticks, ilg);

			ConstructorInfo ci = typeof(DateTime).GetConstructor(new Type[] { typeof(long) });

			ilg.Emit(OpCodes.Call, ci);

			Utility.EmitLoadLocal(ilg, index);
		}

		public override System.Type ResultType
		{
			get { return typeof(DateTime); }
		}
	}

	internal class TimeSpanLiteralElement : LiteralElement
	{

		private TimeSpan MyValue;

		public TimeSpanLiteralElement(string image)
		{
			if (TimeSpan.TryParse(image, out MyValue) == false)
			{
				base.ThrowCompileException(CompileErrorResourceKeys.CannotParseType, CompileExceptionReason.InvalidFormat, typeof(TimeSpan).Name);
			}
		}

		public override void Emit(FleeILGenerator ilg, System.IServiceProvider services)
		{
			int index = ilg.GetTempLocalIndex(typeof(TimeSpan));

			Utility.EmitLoadLocalAddress(ilg, index);

			LiteralElement.EmitInt64Load(MyValue.Ticks, ilg);

			ConstructorInfo ci = typeof(TimeSpan).GetConstructor(new Type[] { typeof(long) });

			ilg.Emit(OpCodes.Call, ci);

			Utility.EmitLoadLocal(ilg, index);
		}

		public override System.Type ResultType
		{
			get { return typeof(TimeSpan); }
		}
	}

}
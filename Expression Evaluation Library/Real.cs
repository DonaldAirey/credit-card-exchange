namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Reflection.Emit;

	internal class DoubleLiteralElement : LiteralElement
	{

		private double MyValue;

		private DoubleLiteralElement()
		{

		}

		public DoubleLiteralElement(double value)
		{
			MyValue = value;
		}

		public static DoubleLiteralElement Parse(string image, IServiceProvider services)
		{
			ExpressionParserOptions options = services.GetService(typeof(ExpressionParserOptions)) as ExpressionParserOptions;
			DoubleLiteralElement element = new DoubleLiteralElement();

			try
			{
				double value = options.ParseDouble(image);
				return new DoubleLiteralElement(value);
			}
			catch (OverflowException)
			{
				element.OnParseOverflow(image);
				return null;
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			ilg.Emit(OpCodes.Ldc_R8, MyValue);
		}

		public override System.Type ResultType
		{
			get { return typeof(double); }
		}
	}

	internal class SingleLiteralElement : LiteralElement
	{

		private float MyValue;

		private SingleLiteralElement()
		{

		}

		public SingleLiteralElement(float value)
		{
			MyValue = value;
		}

		public static SingleLiteralElement Parse(string image, IServiceProvider services)
		{
			ExpressionParserOptions options = services.GetService(typeof(ExpressionParserOptions)) as ExpressionParserOptions;
			SingleLiteralElement element = new SingleLiteralElement();

			try
			{
				float value = options.ParseSingle(image);
				return new SingleLiteralElement(value);
			}
			catch (OverflowException)
			{
				element.OnParseOverflow(image);
				return null;
			}
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			ilg.Emit(OpCodes.Ldc_R4, MyValue);
		}

		public override System.Type ResultType
		{
			get { return typeof(float); }
		}
	}

}
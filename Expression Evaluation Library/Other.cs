namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Reflection.Emit;

	internal class StringLiteralElement : LiteralElement
	{

		private string MyValue;

		public StringLiteralElement(string value)
		{
			MyValue = value;
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			ilg.Emit(OpCodes.Ldstr, MyValue);
		}

		public override System.Type ResultType
		{
			get { return typeof(string); }
		}
	}

	internal class BooleanLiteralElement : LiteralElement
	{

		private bool MyValue;

		public BooleanLiteralElement(bool value)
		{
			MyValue = value;
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			if (MyValue == true)
			{
				ilg.Emit(OpCodes.Ldc_I4_1);
			}
			else
			{
				ilg.Emit(OpCodes.Ldc_I4_0);
			}
		}

		public override System.Type ResultType
		{
			get { return typeof(bool); }
		}
	}

	internal class CharLiteralElement : LiteralElement
	{

		private char MyValue;

		public CharLiteralElement(char value)
		{
			MyValue = value;
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			int intValue = Convert.ToInt32(MyValue);
			EmitInt32Load(intValue, ilg);
		}

		public override System.Type ResultType
		{
			get { return typeof(char); }
		}
	}

	internal class NullLiteralElement : LiteralElement
	{

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			ilg.Emit(OpCodes.Ldnull);
		}

		public override System.Type ResultType
		{
			get { return typeof(Null); }
		}
	}

}
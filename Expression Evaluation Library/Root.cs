namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Reflection.Emit;
	using System.Reflection;

	// The expression element at the top of the expression tree
	internal class RootExpressionElement : ExpressionElement
	{

		private ExpressionElement MyChild;
		private Type MyResultType;

		public RootExpressionElement(ExpressionElement child, Type resultType)
		{
			MyChild = child;
			MyResultType = resultType;
			this.Validate();
		}

		public override void Emit(FleeILGenerator ilg, IServiceProvider services)
		{
			MyChild.Emit(ilg, services);
			ImplicitConverter.EmitImplicitConvert(MyChild.ResultType, MyResultType, ilg);

			ExpressionOptions options = services.GetService(typeof(ExpressionOptions)) as ExpressionOptions;

			if (options.IsGeneric == false)
			{
				ImplicitConverter.EmitImplicitConvert(MyResultType, typeof(object), ilg);
			}

			ilg.Emit(OpCodes.Ret);
		}

		private void Validate()
		{
			if (ImplicitConverter.EmitImplicitConvert(MyChild.ResultType, MyResultType, null) == false)
			{
				base.ThrowCompileException(CompileErrorResourceKeys.CannotConvertTypeToExpressionResult, CompileExceptionReason.TypeMismatch, MyChild.ResultType.Name, MyResultType.Name);
			}
		}

		public override System.Type ResultType
		{
			get { return typeof(object); }
		}
	}

}
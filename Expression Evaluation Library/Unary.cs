namespace FluidTrade.ExpressionEvaluation
{

	using System;

	// Base class for an expression element that operates on one value
	internal abstract class UnaryElement : ExpressionElement
	{

		protected ExpressionElement MyChild;
		private Type MyResultType;

		public void SetChild(ExpressionElement child)
		{
			MyChild = child;
			MyResultType = this.GetResultType(child.ResultType);

			if (MyResultType == null)
			{
				base.ThrowCompileException(CompileErrorResourceKeys.OperationNotDefinedForType, CompileExceptionReason.TypeMismatch, MyChild.ResultType.Name);
			}
		}

		protected abstract Type GetResultType(Type childType);

		public override System.Type ResultType
		{
			get { return MyResultType; }
		}
	}

}
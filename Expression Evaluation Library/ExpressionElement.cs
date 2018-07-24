namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Diagnostics;
	using System.Reflection.Emit;

	// The base class for all elements of an expression.
	internal abstract class ExpressionElement
	{

		internal ExpressionElement()
		{

		}

		// All expression elements must be able to emit their IL
		public abstract void Emit(FleeILGenerator ilg, IServiceProvider services);
		// All expression elements must expose the Type they evaluate to
		public abstract Type ResultType
		{
			get;
		}

		public override string ToString()
		{
			return this.Name;
		}

		protected void ThrowCompileException(string messageKey, CompileExceptionReason reason, params object[] arguments)
		{
			string messageTemplate = FleeResourceManager.Instance.GetCompileErrorString(messageKey);
			string message = string.Format(messageTemplate, arguments);
			throw new ExpressionCompileException(message, reason);
		}

		protected void ThrowAmbiguousCallException(Type leftType, Type rightType, object operation)
		{
			this.ThrowCompileException(CompileErrorResourceKeys.AmbiguousOverloadedOperator, CompileExceptionReason.AmbiguousMatch, leftType.Name, rightType.Name, operation);
		}

		protected FleeILGenerator CreateTempFleeILGenerator(FleeILGenerator ilgCurrent)
		{
			DynamicMethod dm = new DynamicMethod("temp", typeof(Int32), null, this.GetType());
			return new FleeILGenerator(dm.GetILGenerator(), ilgCurrent.Length, true);
		}

		protected string Name
		{
			get
			{
				string key = this.GetType().Name;
				string value = FleeResourceManager.Instance.GetElementNameString(key);
				Debug.Assert(value != null, string.Format("Element name for '{0}' not in resource file", key));
				return value;
			}
		}
	}

}
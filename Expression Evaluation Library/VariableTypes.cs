namespace FluidTrade.ExpressionEvaluation
{

	using System;

	internal interface IVariable
	{
		IVariable Clone();
		Type VariableType
		{
			get;
		}
		object ValueAsObject
		{
			get;
			set;
		}
	}

	internal interface IGenericVariable<T>
	{
		T GetValue();
	}

	internal class DynamicExpressionVariable<T> : IVariable, IGenericVariable<T>
	{

		private IDynamicExpression MyExpression;

		public IVariable Clone()
		{
			DynamicExpressionVariable<T> copy = new DynamicExpressionVariable<T>();
			copy.MyExpression = MyExpression;
			return copy;
		}

		public T GetValue()
		{
			return (T)MyExpression.Evaluate();
		}

		public object ValueAsObject
		{
			get { return MyExpression; }
			set { MyExpression = value as IDynamicExpression; }
		}

		public System.Type VariableType
		{
			get { return MyExpression.Context.Options.ResultType; }
		}
	}

	internal class GenericExpressionVariable<T> : IVariable, IGenericVariable<T>
	{

		private IGenericExpression<T> MyExpression;

		public IVariable Clone()
		{
			GenericExpressionVariable<T> copy = new GenericExpressionVariable<T>();
			copy.MyExpression = MyExpression;
			return copy;
		}

		public T GetValue()
		{
			return MyExpression.Evaluate();
		}

		public object ValueAsObject
		{
			get { return MyExpression; }
			set { MyExpression = value as IGenericExpression<T>; }
		}

		public System.Type VariableType
		{
			get { return MyExpression.Context.Options.ResultType; }
		}
	}

	internal class GenericVariable<T> : IVariable, IGenericVariable<T>
	{

		public T MyValue;

		public IVariable Clone()
		{
			GenericVariable<T> copy = new GenericVariable<T>();
			copy.MyValue = MyValue;
			return copy;
		}

		public T GetValue()
		{
			return MyValue;
		}

		public System.Type VariableType
		{
			get { return typeof(T); }
		}

		public object ValueAsObject
		{
			get { return MyValue; }
			set
			{
				if (value == null)
				{
					MyValue = default(T);
				}
				else
				{
					MyValue = (T)value;
				}
			}
		}
	}

}
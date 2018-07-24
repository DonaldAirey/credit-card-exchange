namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections.Generic;

	/// <include file='Resources/DocComments.xml' path='DocComments/IExpression/Class/*' /> 
	public interface IExpression
	{

		/// <include file='Resources/DocComments.xml' path='DocComments/IExpression/Clone/*' /> 
		IExpression Clone();
		/// <include file='Resources/DocComments.xml' path='DocComments/IExpression/Text/*' /> 
		string Text
		{
			get;
		}
		/// <include file='Resources/DocComments.xml' path='DocComments/IExpression/Info/*' /> 
		ExpressionInfo Info
		{
			get;
		}
		/// <include file='Resources/DocComments.xml' path='DocComments/IExpression/Context/*' /> 
		ExpressionContext Context
		{
			get;
		}
		/// <include file='Resources/DocComments.xml' path='DocComments/IExpression/Owner/*' /> 
		object Owner
		{
			get;
			set;
		}
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/IDynamicExpression/Class/*' /> 
	public interface IDynamicExpression : IExpression
	{
		/// <include file='Resources/DocComments.xml' path='DocComments/IDynamicExpression/Evaluate/*' /> 
		object Evaluate();
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/IGenericExpression/Class/*' /> 
	public interface IGenericExpression<T> : IExpression
	{
		/// <include file='Resources/DocComments.xml' path='DocComments/IGenericExpression/Evaluate/*' /> 
		T Evaluate();
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionInfo/Class/*' /> 
	public sealed class ExpressionInfo
	{

		private IDictionary<string, object> MyData;

		internal ExpressionInfo()
		{
			MyData = new Dictionary<string, object>();
			MyData.Add("ReferencedVariables", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}

		internal void AddReferencedVariable(string name)
		{
			IDictionary<string, string> dict = MyData["ReferencedVariables"] as IDictionary<string, string>;
			dict[name] = name;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionInfo/GetReferencedVariables/*' /> 
		public string[] GetReferencedVariables()
		{
			IDictionary<string, string> dict = MyData["ReferencedVariables"] as IDictionary<string, string>;
			string[] arr = new string[dict.Count];
			dict.Keys.CopyTo(arr, 0);
			return arr;
		}
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionOwnerMemberAccessAttribute/Class/*' /> 
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class ExpressionOwnerMemberAccessAttribute : Attribute
	{

		private bool MyAllowAccess;

		/// <include file='Resources/DocComments.xml' path='DocComments/ExpressionOwnerMemberAccessAttribute/New/*' /> 
		public ExpressionOwnerMemberAccessAttribute(bool allowAccess)
		{
			MyAllowAccess = allowAccess;
		}

		internal bool AllowAccess
		{
			get { return MyAllowAccess; }
		}
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/ResolveVariableTypeEventArgs/Class/*' /> 
	public class ResolveVariableTypeEventArgs : EventArgs
	{

		private string MyName;
		private Type MyType;

		internal ResolveVariableTypeEventArgs(string name)
		{
			this.MyName = name;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveVariableTypeEventArgs/VariableName/*' /> 
		public string VariableName
		{
			get { return MyName; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveVariableTypeEventArgs/VariableType/*' /> 
		public Type VariableType
		{
			get { return MyType; }
			set { MyType = value; }
		}
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/ResolveVariableValueEventArgs/Class/*' /> 
	public class ResolveVariableValueEventArgs : EventArgs
	{

		private string MyName;
		private Type MyType;
		private object MyValue;

		internal ResolveVariableValueEventArgs(string name, Type t)
		{
			MyName = name;
			MyType = t;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveVariableValueEventArgs/VariableName/*' /> 
		public string VariableName
		{
			get { return MyName; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveVariableValueEventArgs/VariableType/*' /> 
		public Type VariableType
		{
			get { return MyType; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveVariableValueEventArgs/VariableValue/*' /> 
		public object VariableValue
		{
			get { return MyValue; }
			set { MyValue = value; }
		}
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/ResolveFunctionEventArgs/Class/*' /> 
	public class ResolveFunctionEventArgs : EventArgs
	{

		private string MyName;
		private Type[] MyArgumentTypes;
		private Type MyReturnType;

		internal ResolveFunctionEventArgs(string name, Type[] argumentTypes)
		{
			MyName = name;
			MyArgumentTypes = argumentTypes;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveFunctionEventArgs/FunctionName/*' /> 
		public string FunctionName
		{
			get { return MyName; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveFunctionEventArgs/ArgumentTypes/*' /> 
		public Type[] ArgumentTypes
		{
			get { return MyArgumentTypes; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/ResolveFunctionEventArgs/ReturnType/*' /> 
		public Type ReturnType
		{
			get { return MyReturnType; }
			set { MyReturnType = value; }
		}
	}

	/// <include file='Resources/DocComments.xml' path='DocComments/InvokeFunctionEventArgs/Class/*' /> 
	public class InvokeFunctionEventArgs : EventArgs
	{

		private string MyName;
		private object[] MyArguments;
		private object MyFunctionResult;

		internal InvokeFunctionEventArgs(string name, object[] arguments)
		{
			MyName = name;
			MyArguments = arguments;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/InvokeFunctionEventArgs/FunctionName/*' /> 
		public string FunctionName
		{
			get { return MyName; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/InvokeFunctionEventArgs/Arguments/*' /> 
		public object[] Arguments
		{
			get { return MyArguments; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/InvokeFunctionEventArgs/Result/*' /> 
		public object Result
		{
			get { return MyFunctionResult; }
			set { MyFunctionResult = value; }
		}
	}

}
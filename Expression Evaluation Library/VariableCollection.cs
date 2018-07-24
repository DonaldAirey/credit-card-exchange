namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.ComponentModel;

	/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Class/*' /> 
	public sealed class VariableCollection : IDictionary<string, object>
	{

		private IDictionary<string, IVariable> MyVariables;
		private ExpressionContext MyContext;

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/ResolveVariableType/*' /> 
		public event EventHandler<ResolveVariableTypeEventArgs> ResolveVariableType;
		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/ResolveVariableValue/*' /> 
		public event EventHandler<ResolveVariableValueEventArgs> ResolveVariableValue;

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/ResolveFunction/*' /> 
		public event EventHandler<ResolveFunctionEventArgs> ResolveFunction;
		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/InvokeFunction/*' /> 
		public event EventHandler<InvokeFunctionEventArgs> InvokeFunction;

		internal VariableCollection(ExpressionContext context)
		{
			MyContext = context;
			this.CreateDictionary();
			this.HookOptions();
		}

		#region "Methods - Non Public"
		private void HookOptions()
		{
			MyContext.Options.CaseSensitiveChanged += OnOptionsCaseSensitiveChanged;
		}

		private void CreateDictionary()
		{
			MyVariables = new Dictionary<string, IVariable>(MyContext.Options.StringComparer);
		}

		private void OnOptionsCaseSensitiveChanged(object sender, EventArgs e)
		{
			this.CreateDictionary();
		}

		internal void Copy(VariableCollection dest)
		{
			dest.CreateDictionary();
			dest.HookOptions();

			foreach (KeyValuePair<string, IVariable> pair in MyVariables)
			{
				IVariable copyVariable = pair.Value.Clone();
				dest.MyVariables.Add(pair.Key, copyVariable);
			}
		}

		internal void DefineVariableInternal(string name, Type variableType, object variableValue)
		{
			Utility.AssertNotNull(variableType, "variableType");

			if (MyVariables.ContainsKey(name) == true)
			{
				string msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.VariableWithNameAlreadyDefined, name);
				throw new ArgumentException(msg);
			}

			IVariable v = this.CreateVariable(variableType, variableValue);
			MyVariables.Add(name, v);
		}

		internal Type GetVariableTypeInternal(string name)
		{
			IVariable value = null;
			bool success = MyVariables.TryGetValue(name, out value);

			if (success == true)
			{
				return value.VariableType;
			}

			ResolveVariableTypeEventArgs args = new ResolveVariableTypeEventArgs(name);
			if (ResolveVariableType != null)
			{
				ResolveVariableType(this, args);
			}

			return args.VariableType;
		}

		private IVariable GetVariable(string name, bool throwOnNotFound)
		{
			IVariable value = null;
			bool success = MyVariables.TryGetValue(name, out value);

			if (success == false & throwOnNotFound == true)
			{
				string msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.UndefinedVariable, name);
				throw new ArgumentException(msg);
			}
			else
			{
				return value;
			}
		}

		/// <summary> 
		/// Create a variable 
		/// </summary> 
		/// <param name="variableValueType">The variable's type</param> 
		/// <param name="variableValue">The actual value; may be null</param> 
		/// <returns>A new variable for the value</returns> 
		/// <remarks></remarks> 
		private IVariable CreateVariable(Type variableValueType, object variableValue)
		{
			Type variableType = default(Type);

			// Is the variable value an expression? 
			IExpression expression = variableValue as IExpression;
			ExpressionOptions options = null;

			if (expression != null)
			{
				options = expression.Context.Options;
				// Get its result type 
				variableValueType = options.ResultType;
			}

			if (expression != null)
			{
				// Create a variable that wraps the expression 

				if (options.IsGeneric == false)
				{
					variableType = typeof(DynamicExpressionVariable<>);
				}
				else
				{
					variableType = typeof(GenericExpressionVariable<>);
				}
			}
			else
			{
				// Create a variable for a regular value 
				MyContext.AssertTypeIsAccessible(variableValueType);
				variableType = typeof(GenericVariable<>);
			}

			// Create the generic variable instance 
			variableType = variableType.MakeGenericType(variableValueType);
			IVariable v = Activator.CreateInstance(variableType) as IVariable;

			return v;
		}

		internal Type ResolveOnDemandFunction(string name, Type[] argumentTypes)
		{
			ResolveFunctionEventArgs args = new ResolveFunctionEventArgs(name, argumentTypes);
			if (ResolveFunction != null)
			{
				ResolveFunction(this, args);
			}
			return args.ReturnType;
		}

		private static T ReturnGenericValue<T>(object value)
		{
			if (value == null)
			{
				return default(T);
			}
			else
			{
				return (T)value;
			}
		}

		private static void ValidateSetValueType(Type requiredType, object value)
		{
			if (value == null)
			{
				// Can always assign null value 
				return;
			}

			Type valueType = value.GetType();

			if (requiredType.IsAssignableFrom(valueType) == false)
			{
				string msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.VariableValueNotAssignableToType, valueType.Name, requiredType.Name);
				throw new ArgumentException(msg);
			}
		}

		static internal MethodInfo GetVariableLoadMethod(Type variableType)
		{
			MethodInfo mi = typeof(VariableCollection).GetMethod("GetVariableValueInternal", BindingFlags.Public | BindingFlags.Instance);
			mi = mi.MakeGenericMethod(variableType);
			return mi;
		}

		static internal MethodInfo GetFunctionInvokeMethod(Type returnType)
		{
			MethodInfo mi = typeof(VariableCollection).GetMethod("GetFunctionResultInternal", BindingFlags.Public | BindingFlags.Instance);
			mi = mi.MakeGenericMethod(returnType);
			return mi;
		}

		static internal MethodInfo GetVirtualPropertyLoadMethod(Type returnType)
		{
			MethodInfo mi = typeof(VariableCollection).GetMethod("GetVirtualPropertyValueInternal", BindingFlags.Public | BindingFlags.Instance);
			mi = mi.MakeGenericMethod(returnType);
			return mi;
		}

		private Dictionary<string, object> GetNameValueDictionary()
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();

			foreach (KeyValuePair<string, IVariable> pair in MyVariables)
			{
				dict.Add(pair.Key, pair.Value.ValueAsObject);
			}

			return dict;
		}
		#endregion

		#region "Methods - Public"
		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/GetVariableType/*' /> 
		public Type GetVariableType(string name)
		{
			IVariable v = this.GetVariable(name, true);
			return v.VariableType;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/DefineVariable/*' /> 
		public void DefineVariable(string name, Type variableType)
		{
			this.DefineVariableInternal(name, variableType, null);
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/GetVariableValueInternal/*' /> 
		public T GetVariableValueInternal<T>(string name)
		{
			IVariable iVariable = default(IVariable);
			IGenericVariable<T> v = null;

			if (MyVariables.TryGetValue(name, out iVariable) == true)
			{
				v = iVariable as IGenericVariable<T>;
				return v.GetValue();
			}

			GenericVariable<T> vTemp = new GenericVariable<T>();
			ResolveVariableValueEventArgs args = new ResolveVariableValueEventArgs(name, typeof(T));
			if (ResolveVariableValue != null)
			{
				ResolveVariableValue(this, args);
			}

			ValidateSetValueType(typeof(T), args.VariableValue);
			vTemp.ValueAsObject = args.VariableValue;
			v = vTemp;

			return v.GetValue();
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/GetVirtualPropertyValueInternal/*' /> 
		public T GetVirtualPropertyValueInternal<T>(string name, object component)
		{
			PropertyDescriptorCollection coll = TypeDescriptor.GetProperties(component);
			PropertyDescriptor pd = coll.Find(name, true);

			object value = pd.GetValue(component);
			ValidateSetValueType(typeof(T), value);
			return ReturnGenericValue<T>(value);
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/GetFunctionResultInternal/*' /> 
		public T GetFunctionResultInternal<T>(string name, object[] arguments)
		{
			InvokeFunctionEventArgs args = new InvokeFunctionEventArgs(name, arguments);
			if (InvokeFunction != null)
			{
				InvokeFunction(this, args);
			}

			object result = args.Result;
			ValidateSetValueType(typeof(T), result);

			return ReturnGenericValue<T>(result);
		}
		#endregion

		#region "IDictionary Implementation"
		private void Add1(System.Collections.Generic.KeyValuePair<string, object> item)
		{
			this.Add(item.Key, item.Value);
		}
		void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, object>>.Add(System.Collections.Generic.KeyValuePair<string, object> item)
		{
			Add1(item);
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Clear/*' /> 
		public void Clear()
		{
			MyVariables.Clear();
		}

		private bool Contains1(System.Collections.Generic.KeyValuePair<string, object> item)
		{
			return this.ContainsKey(item.Key);
		}
		bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, object>>.Contains(System.Collections.Generic.KeyValuePair<string, object> item)
		{
			return Contains1(item);
		}

		public void CopyTo(System.Collections.Generic.KeyValuePair<string, object>[] array, int arrayIndex)
		{
			Dictionary<string, object> dict = this.GetNameValueDictionary();
			ICollection<KeyValuePair<string, object>> coll = dict;
			coll.CopyTo(array, arrayIndex);
		}

		private bool Remove1(System.Collections.Generic.KeyValuePair<string, object> item)
		{
			this.Remove(item.Key);
			return false;
		}
		bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, object>>.Remove(System.Collections.Generic.KeyValuePair<string, object> item)
		{
			return Remove1(item);
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Add/*' /> 
		public void Add(string name, object value)
		{
			Utility.AssertNotNull(value, "value");
			this.DefineVariableInternal(name, value.GetType(), value);
			this[name] = value;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/ContainsKey/*' /> 
		public bool ContainsKey(string name)
		{
			return MyVariables.ContainsKey(name);
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Remove/*' /> 
		public bool Remove(string name)
		{
			MyVariables.Remove(name);
			return false;
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/TryGetValue/*' /> 
		public bool TryGetValue(string key, out object value)
		{
			value = default(object);
			IVariable v = this.GetVariable(key, false);
			if ((v != null))
			{
				value = v.ValueAsObject;
			}

			return (v != null);
		}

		public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object>> GetEnumerator()
		{
			Dictionary<string, object> dict = this.GetNameValueDictionary();
			return dict.GetEnumerator();
		}

		private System.Collections.IEnumerator GetEnumerator1()
		{
			return this.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator1();
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Count/*' /> 
		public int Count
		{
			get { return MyVariables.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Item/*' /> 
		public object this[string name]
		{
			get
			{
				IVariable v = this.GetVariable(name, true);
				return v.ValueAsObject;
			}
			set
			{
				IVariable v = null;

				if (MyVariables.TryGetValue(name, out v) == true)
				{
					v.ValueAsObject = value;
				}
				else
				{
					this.Add(name, value);
				}
			}
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Keys/*' /> 
		public System.Collections.Generic.ICollection<string> Keys
		{
			get { return MyVariables.Keys; }
		}

		/// <include file='Resources/DocComments.xml' path='DocComments/VariableCollection/Values/*' /> 
		public System.Collections.Generic.ICollection<object> Values
		{
			get
			{
				Dictionary<string, object> dict = this.GetNameValueDictionary();
				return dict.Values;
			}
		}
		#endregion
	}

}
namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Reflection.Emit;
	using System.Reflection;
	using System.ComponentModel.Design;

	internal class Expression<T> : IExpression, IDynamicExpression, IGenericExpression<T>
	{

		private string MyExpression;
		private ExpressionContext MyContext;
		private ExpressionOptions MyOptions;
		private ExpressionInfo MyInfo;
		private ExpressionEvaluator<T> MyEvaluator;
		private object MyOwner;

		private const string EmitAssemblyName = "FleeExpression";
		private const string DynamicMethodName = "Flee Expression";

		public Expression(string expression, ExpressionContext context, bool isGeneric)
		{
			Utility.AssertNotNull(expression, "expression");
			MyExpression = expression;
			MyOwner = context.ExpressionOwner;

			MyContext = context;

			if (context.NoClone == false)
			{
				MyContext = context.CloneInternal(false);
			}

			MyInfo = new ExpressionInfo();

			this.SetupOptions(MyContext.Options, isGeneric);

			MyContext.Imports.ImportOwner(MyOptions.OwnerType);

			this.ValidateOwner(MyOwner);

			this.Compile(expression, MyOptions);

			if ((MyContext.CalculationEngine != null))
			{
				MyContext.CalculationEngine.FixTemporaryHead(this, MyContext, MyOptions.ResultType);
			}
		}

		private void SetupOptions(ExpressionOptions options, bool isGeneric)
		{
			// Make sure we clone the options
			MyOptions = options;
			MyOptions.IsGeneric = isGeneric;

			if (isGeneric)
			{
				MyOptions.ResultType = typeof(T);
			}

			MyOptions.SetOwnerType(MyOwner.GetType());
		}

		private void Compile(string expression, ExpressionOptions options)
		{
			// Add the services that will be used by elements during the compile
			IServiceContainer services = new ServiceContainer();
			this.AddServices(services);

			// Parse and get the root element of the parse tree
			ExpressionElement topElement = MyContext.Parse(expression, services);

			if (options.ResultType == null)
			{
				options.ResultType = topElement.ResultType;
			}

			RootExpressionElement rootElement = new RootExpressionElement(topElement, options.ResultType);

			DynamicMethod dm = this.CreateDynamicMethod();

			FleeILGenerator ilg = new FleeILGenerator(dm.GetILGenerator(), 0, false);

			// Emit the IL
			rootElement.Emit(ilg, services);

			ilg.ValidateLength();

			// Emit to an assembly if required
			if (options.EmitToAssembly == true)
			{
				EmitToAssembly(rootElement, services);
			}

			Type delegateType = typeof(ExpressionEvaluator<>).MakeGenericType(typeof(T));
			MyEvaluator = dm.CreateDelegate(delegateType) as ExpressionEvaluator<T>;
		}

		private DynamicMethod CreateDynamicMethod()
		{
			// Create the dynamic method
			Type[] parameterTypes = { typeof(object), typeof(ExpressionContext), typeof(VariableCollection) };
			DynamicMethod dm = default(DynamicMethod);

			dm = new DynamicMethod(DynamicMethodName, typeof(T), parameterTypes, MyOptions.OwnerType);

			return dm;
		}

		private void AddServices(IServiceContainer dest)
		{
			dest.AddService(typeof(ExpressionOptions), MyOptions);
			dest.AddService(typeof(ExpressionParserOptions), MyContext.ParserOptions);
			dest.AddService(typeof(ExpressionContext), MyContext);
			dest.AddService(typeof(IExpression), this);
			dest.AddService(typeof(ExpressionInfo), MyInfo);
		}

		private static void EmitToAssembly(ExpressionElement rootElement, IServiceContainer services)
		{
			AssemblyName assemblyName = new AssemblyName(EmitAssemblyName);

			string assemblyFileName = string.Format("{0}.dll", EmitAssemblyName);

			AssemblyBuilder assemblyBuilder = System.AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyFileName, assemblyFileName);

			MethodBuilder mb = moduleBuilder.DefineGlobalMethod("Evaluate", MethodAttributes.Public | MethodAttributes.Static, typeof(T), new Type[] { typeof(object), typeof(ExpressionContext), typeof(VariableCollection) });
			FleeILGenerator ilg = new FleeILGenerator(mb.GetILGenerator(), 0, false);

			rootElement.Emit(ilg, services);

			moduleBuilder.CreateGlobalFunctions();
			assemblyBuilder.Save(assemblyFileName);
		}

		private void ValidateOwner(object owner)
		{
			Utility.AssertNotNull(owner, "owner");

			if (MyOptions.OwnerType.IsAssignableFrom(owner.GetType()) == false)
			{
				string msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.NewOwnerTypeNotAssignableToCurrentOwner);
				throw new ArgumentException(msg);
			}
		}

		public object Evaluate()
		{
			return MyEvaluator(MyOwner, MyContext, MyContext.Variables);
		}

		public T EvaluateGeneric()
		{
			return MyEvaluator(MyOwner, MyContext, MyContext.Variables);
		}
		T IGenericExpression<T>.Evaluate()
		{
			return EvaluateGeneric();
		}

		public IExpression Clone()
		{
			Expression<T> copy = this.MemberwiseClone() as Expression<T>;
			copy.MyContext = MyContext.CloneInternal(true);
			copy.MyOptions = copy.MyContext.Options;
			return copy;
		}

		public override string ToString()
		{
			return MyExpression;
		}

		internal Type ResultType
		{
			get { return MyOptions.ResultType; }
		}

		public string Text
		{
			get { return MyExpression; }
		}

		public ExpressionInfo Info1
		{
			get { return MyInfo; }
		}
		ExpressionInfo IExpression.Info
		{
			get { return Info1; }
		}

		public object Owner
		{
			get { return MyOwner; }
			set
			{
				this.ValidateOwner(value);
				MyOwner = value;
			}
		}

		public ExpressionContext Context
		{
			get { return MyContext; }
		}
	}

}
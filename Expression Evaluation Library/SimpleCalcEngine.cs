namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections.Generic;
	using System.IO;

	namespace FluidTrade.ExpressionEvaluation
	{

		public class SimpleCalcEngine
		{

			#region "Fields"

			private IDictionary<string, IExpression> MyExpressions;
			private ExpressionContext MyContext;

			#endregion

			#region "Constructor"

			public SimpleCalcEngine()
			{
				MyExpressions = new Dictionary<string, IExpression>(StringComparer.OrdinalIgnoreCase);
				MyContext = new ExpressionContext();
			}

			#endregion

			#region "Methods - Private"

			private void AddCompiledExpression(string expressionName, IExpression expression)
			{
				if (MyExpressions.ContainsKey(expressionName) == true)
				{
					throw new InvalidOperationException(string.Format("The calc engine already contains an expression named '{0}'", expressionName));
				}
				else
				{
					MyExpressions.Add(expressionName, expression);
				}
			}

			private ExpressionContext ParseAndLink(string expressionName, string expression)
			{
				IdentifierAnalyzer analyzer = Context.ParseIdentifiers(expression);

				ExpressionContext context2 = MyContext.CloneInternal(true);
				this.LinkExpression(expressionName, context2, analyzer);

				// Tell the expression not to clone the context since it's already been cloned
				context2.NoClone = true;

				// Clear our context's variables
				MyContext.Variables.Clear();

				return context2;
			}

			private void LinkExpression(string expressionName, ExpressionContext context, IdentifierAnalyzer analyzer)
			{
				foreach (string identifier in analyzer.GetIdentifiers(context))
				{
					this.LinkIdentifier(identifier, expressionName, context);
				}
			}

			private void LinkIdentifier(string identifier, string expressionName, ExpressionContext context)
			{
				IExpression child = null;

				if (MyExpressions.TryGetValue(identifier, out child) == false)
				{
					string msg = string.Format("Expression '{0}' references unknown name '{1}'", expressionName, identifier);
					throw new InvalidOperationException(msg);
				}

				context.Variables.Add(identifier, child);
			}

			#endregion

			#region "Methods - Public"

			public void AddDynamic(string expressionName, string expression)
			{
				ExpressionContext linkedContext = this.ParseAndLink(expressionName, expression);
				IExpression e = linkedContext.CompileDynamic(expression);
				this.AddCompiledExpression(expressionName, e);
			}

			public void AddGeneric<T>(string expressionName, string expression)
			{
				ExpressionContext linkedContext = this.ParseAndLink(expressionName, expression);
				IExpression e = linkedContext.CompileGeneric<T>(expression);
				this.AddCompiledExpression(expressionName, e);
			}

			public void Clear()
			{
				MyExpressions.Clear();
			}

			#endregion

			#region "Properties - Public"

			public IExpression this[string name]
			{
				get
				{
					IExpression e = null;
					MyExpressions.TryGetValue(name, out e);
					return e;
				}
			}

			public ExpressionContext Context
			{
				get { return MyContext; }
				set { MyContext = value; }
			}

			#endregion
		}
	}

}
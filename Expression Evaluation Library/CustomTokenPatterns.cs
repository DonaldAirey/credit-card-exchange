namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Globalization;

	internal abstract class CustomTokenPattern : TokenPattern
	{

		public void Initialize(int id, string name, PatternType type, string pattern, ExpressionContext context)
		{
			this.ComputeToken(id, name, type, pattern, context);
		}

		protected abstract void ComputeToken(int id, string name, PatternType type, string pattern, ExpressionContext context);
	}

	internal class RealPattern : CustomTokenPattern
	{

		private const string CorePattern = "\\d{0}\\{1}\\d+([e][+-]\\d{{1,3}})?f?";

		protected override void ComputeToken(int id, string name, PatternType type, string pattern, ExpressionContext context)
		{
			ExpressionParserOptions options = context.ParserOptions;

			char digitsBeforePattern = (options.RequireDigitsBeforeDecimalPoint ? '+' : '*');

			pattern = string.Format(CorePattern, digitsBeforePattern, options.DecimalSeparator);

			this.SetData(id, name, type, pattern);
		}
	}

	internal class ArgumentSeparatorPattern : CustomTokenPattern
	{

		protected override void ComputeToken(int id, string name, PatternType type, string pattern, ExpressionContext context)
		{
			ExpressionParserOptions options = context.ParserOptions;
			this.SetData(id, name, type, Convert.ToString(options.FunctionArgumentSeparator));
		}
	}

}
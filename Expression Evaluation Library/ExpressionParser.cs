﻿namespace FluidTrade.ExpressionEvaluation
{

	using System.IO;

	///<remarks>A token stream parser.</remarks> 
	internal class ExpressionParser : RecursiveDescentParser
	{

		///<summary>An enumeration with the generated production node 
		///identity constants.</summary> 
		private enum SynteticPatterns
		{
			SUBPRODUCTION_1 = 3001,
			SUBPRODUCTION_2 = 3002,
			SUBPRODUCTION_3 = 3003,
			SUBPRODUCTION_4 = 3004,
			SUBPRODUCTION_5 = 3005,
			SUBPRODUCTION_6 = 3006,
			SUBPRODUCTION_7 = 3007,
			SUBPRODUCTION_8 = 3008,
			SUBPRODUCTION_9 = 3009,
			SUBPRODUCTION_10 = 3010,
			SUBPRODUCTION_11 = 3011,
			SUBPRODUCTION_12 = 3012,
			SUBPRODUCTION_13 = 3013,
			SUBPRODUCTION_14 = 3014,
			SUBPRODUCTION_15 = 3015,
			SUBPRODUCTION_16 = 3016
		}

		public ExpressionParser(TextReader input, Analyzer analyzer, ExpressionContext context)
			: base(new ExpressionTokenizer(input, context), analyzer)
		{
			CreatePatterns();
		}

		///<summary>Creates a new parser.</summary> 
		/// 
		///<param name='input'>the input stream to read from</param> 
		/// 
		///<exception cref='ParserCreationException'>if the parser 
		///couldn't be initialized correctly</exception> 
		public ExpressionParser(TextReader input)
			: base(new ExpressionTokenizer(input))
		{
			CreatePatterns();
		}

		///<summary>Creates a new parser.</summary> 
		/// 
		///<param name='input'>the input stream to read from</param> 
		/// 
		///<param name='analyzer'>the analyzer to parse with</param> 
		/// 
		///<exception cref='ParserCreationException'>if the parser 
		///couldn't be initialized correctly</exception> 
		public ExpressionParser(TextReader input, Analyzer analyzer)
			: base(new ExpressionTokenizer(input), analyzer)
		{
			CreatePatterns();
		}

		///<summary>Initializes the parser by creating all the production 
		///patterns.</summary> 
		/// 
		///<exception cref='ParserCreationException'>if the parser 
		///couldn't be initialized correctly</exception> 
		private void CreatePatterns()
		{
			ProductionPattern pattern = default(ProductionPattern);
			ProductionPatternAlternative alt = default(ProductionPatternAlternative);

			pattern = new ProductionPattern((int)ExpressionConstants.EXPRESSION, "Expression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.XOR_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.XOR_EXPRESSION, "XorExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.OR_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_1, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.OR_EXPRESSION, "OrExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.AND_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_2, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.AND_EXPRESSION, "AndExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.NOT_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_3, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.NOT_EXPRESSION, "NotExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.NOT, 0, 1);
			alt.AddProduction((int)ExpressionConstants.IN_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.IN_EXPRESSION, "InExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.COMPARE_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_4, 0, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.IN_TARGET_EXPRESSION, "InTargetExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.IN_LIST_TARGET_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.IN_LIST_TARGET_EXPRESSION, "InListTargetExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.LEFT_PAREN, 1, 1);
			alt.AddProduction((int)ExpressionConstants.ARGUMENT_LIST, 1, 1);
			alt.AddToken((int)ExpressionConstants.RIGHT_PAREN, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.COMPARE_EXPRESSION, "CompareExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.SHIFT_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_6, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.SHIFT_EXPRESSION, "ShiftExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.ADDITIVE_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_8, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.ADDITIVE_EXPRESSION, "AdditiveExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.MULTIPLICATIVE_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_10, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.MULTIPLICATIVE_EXPRESSION, "MultiplicativeExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.POWER_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_12, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.POWER_EXPRESSION, "PowerExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.NEGATE_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_13, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.NEGATE_EXPRESSION, "NegateExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.SUB, 0, 1);
			alt.AddProduction((int)ExpressionConstants.MEMBER_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.MEMBER_EXPRESSION, "MemberExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.BASIC_EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_14, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.MEMBER_ACCESS_EXPRESSION, "MemberAccessExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.DOT, 1, 1);
			alt.AddProduction((int)ExpressionConstants.MEMBER_FUNCTION_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.BASIC_EXPRESSION, "BasicExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.LITERAL_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.EXPRESSION_GROUP, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.MEMBER_FUNCTION_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.MEMBER_FUNCTION_EXPRESSION, "MemberFunctionExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.FUNCTION_CALL_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION, "FieldPropertyExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.IDENTIFIER, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION, "SpecialFunctionExpression");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.IF_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.CAST_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.IF_EXPRESSION, "IfExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.IF, 1, 1);
			alt.AddToken((int)ExpressionConstants.LEFT_PAREN, 1, 1);
			alt.AddProduction((int)ExpressionConstants.EXPRESSION, 1, 1);
			alt.AddToken((int)ExpressionConstants.ARGUMENT_SEPARATOR, 1, 1);
			alt.AddProduction((int)ExpressionConstants.EXPRESSION, 1, 1);
			alt.AddToken((int)ExpressionConstants.ARGUMENT_SEPARATOR, 1, 1);
			alt.AddProduction((int)ExpressionConstants.EXPRESSION, 1, 1);
			alt.AddToken((int)ExpressionConstants.RIGHT_PAREN, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.CAST_EXPRESSION, "CastExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.CAST, 1, 1);
			alt.AddToken((int)ExpressionConstants.LEFT_PAREN, 1, 1);
			alt.AddProduction((int)ExpressionConstants.EXPRESSION, 1, 1);
			alt.AddToken((int)ExpressionConstants.ARGUMENT_SEPARATOR, 1, 1);
			alt.AddProduction((int)ExpressionConstants.CAST_TYPE_EXPRESSION, 1, 1);
			alt.AddToken((int)ExpressionConstants.RIGHT_PAREN, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.CAST_TYPE_EXPRESSION, "CastTypeExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.IDENTIFIER, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_15, 0, -1);
			alt.AddToken((int)ExpressionConstants.ARRAY_BRACES, 0, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.INDEX_EXPRESSION, "IndexExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.LEFT_BRACE, 1, 1);
			alt.AddProduction((int)ExpressionConstants.ARGUMENT_LIST, 1, 1);
			alt.AddToken((int)ExpressionConstants.RIGHT_BRACE, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.FUNCTION_CALL_EXPRESSION, "FunctionCallExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.IDENTIFIER, 1, 1);
			alt.AddToken((int)ExpressionConstants.LEFT_PAREN, 1, 1);
			alt.AddProduction((int)ExpressionConstants.ARGUMENT_LIST, 0, 1);
			alt.AddToken((int)ExpressionConstants.RIGHT_PAREN, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.ARGUMENT_LIST, "ArgumentList");
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.EXPRESSION, 1, 1);
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_16, 0, -1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.LITERAL_EXPRESSION, "LiteralExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.INTEGER, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.REAL, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.STRING_LITERAL, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.HEX_LITERAL, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.CHAR_LITERAL, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.NULL_LITERAL, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.DATETIME, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.TIMESPAN, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION, "BooleanLiteralExpression");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.TRUE, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.FALSE, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)ExpressionConstants.EXPRESSION_GROUP, "ExpressionGroup");
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.LEFT_PAREN, 1, 1);
			alt.AddProduction((int)ExpressionConstants.EXPRESSION, 1, 1);
			alt.AddToken((int)ExpressionConstants.RIGHT_PAREN, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_1, "Subproduction1");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.XOR, 1, 1);
			alt.AddProduction((int)ExpressionConstants.OR_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_2, "Subproduction2");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.OR, 1, 1);
			alt.AddProduction((int)ExpressionConstants.AND_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_3, "Subproduction3");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.AND, 1, 1);
			alt.AddProduction((int)ExpressionConstants.NOT_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_4, "Subproduction4");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.IN, 1, 1);
			alt.AddProduction((int)ExpressionConstants.IN_TARGET_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_5, "Subproduction5");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.EQ, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.GT, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.LT, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.GTE, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.LTE, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.NE, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_6, "Subproduction6");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_5, 1, 1);
			alt.AddProduction((int)ExpressionConstants.SHIFT_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_7, "Subproduction7");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.LEFT_SHIFT, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.RIGHT_SHIFT, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_8, "Subproduction8");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_7, 1, 1);
			alt.AddProduction((int)ExpressionConstants.ADDITIVE_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_9, "Subproduction9");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.ADD, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.SUB, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_10, "Subproduction10");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_9, 1, 1);
			alt.AddProduction((int)ExpressionConstants.MULTIPLICATIVE_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_11, "Subproduction11");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.MUL, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.DIV, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.MOD, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_12, "Subproduction12");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)SynteticPatterns.SUBPRODUCTION_11, 1, 1);
			alt.AddProduction((int)ExpressionConstants.POWER_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_13, "Subproduction13");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.POWER, 1, 1);
			alt.AddProduction((int)ExpressionConstants.NEGATE_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_14, "Subproduction14");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.MEMBER_ACCESS_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			alt = new ProductionPatternAlternative();
			alt.AddProduction((int)ExpressionConstants.INDEX_EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_15, "Subproduction15");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.DOT, 1, 1);
			alt.AddToken((int)ExpressionConstants.IDENTIFIER, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);

			pattern = new ProductionPattern((int)SynteticPatterns.SUBPRODUCTION_16, "Subproduction16");
			pattern.Synthetic = true;
			alt = new ProductionPatternAlternative();
			alt.AddToken((int)ExpressionConstants.ARGUMENT_SEPARATOR, 1, 1);
			alt.AddProduction((int)ExpressionConstants.EXPRESSION, 1, 1);
			pattern.AddAlternative(alt);
			AddPattern(pattern);
		}
	}

}
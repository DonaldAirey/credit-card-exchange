namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.IO;

	///<remarks>A character stream tokenizer.</remarks> 
	internal class ExpressionTokenizer : Tokenizer
	{

		private ExpressionContext MyContext;

		///<summary>Creates a new tokenizer for the specified input 
		///stream.</summary> 
		/// 
		///<param name='input'>the input stream to read</param> 
		/// 
		///<exception cref='ParserCreationException'>if the tokenizer 
		///couldn't be initialized correctly</exception> 
		public ExpressionTokenizer(TextReader input, ExpressionContext context)
			: base(input, true)
		{
			MyContext = context;
			CreatePatterns();
		}

		///<summary>Creates a new tokenizer for the specified input 
		///stream.</summary> 
		/// 
		///<param name='input'>the input stream to read</param> 
		/// 
		///<exception cref='ParserCreationException'>if the tokenizer 
		///couldn't be initialized correctly</exception> 
		public ExpressionTokenizer(TextReader input)
			: base(input, true)
		{
			CreatePatterns();
		}

		///<summary>Initializes the tokenizer by creating all the token 
		///patterns.</summary> 
		/// 
		///<exception cref='ParserCreationException'>if the tokenizer 
		///couldn't be initialized correctly</exception> 
		private void CreatePatterns()
		{
			TokenPattern pattern = default(TokenPattern);
			CustomTokenPattern customPattern = default(CustomTokenPattern);

			pattern = new TokenPattern((int)ExpressionConstants.ADD, "ADD", TokenPattern.PatternType.STRING, "+");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.SUB, "SUB", TokenPattern.PatternType.STRING, "-");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.MUL, "MUL", TokenPattern.PatternType.STRING, "*");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.DIV, "DIV", TokenPattern.PatternType.STRING, "/");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.POWER, "POWER", TokenPattern.PatternType.STRING, "^");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.MOD, "MOD", TokenPattern.PatternType.STRING, "%");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.LEFT_PAREN, "LEFT_PAREN", TokenPattern.PatternType.STRING, "(");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.RIGHT_PAREN, "RIGHT_PAREN", TokenPattern.PatternType.STRING, ")");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.LEFT_BRACE, "LEFT_BRACE", TokenPattern.PatternType.STRING, "[");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.RIGHT_BRACE, "RIGHT_BRACE", TokenPattern.PatternType.STRING, "]");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.EQ, "EQ", TokenPattern.PatternType.STRING, "=");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.LT, "LT", TokenPattern.PatternType.STRING, "<");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.GT, "GT", TokenPattern.PatternType.STRING, ">");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.LTE, "LTE", TokenPattern.PatternType.STRING, "<=");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.GTE, "GTE", TokenPattern.PatternType.STRING, ">=");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.NE, "NE", TokenPattern.PatternType.STRING, "<>");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.AND, "AND", TokenPattern.PatternType.STRING, "AND");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.OR, "OR", TokenPattern.PatternType.STRING, "OR");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.XOR, "XOR", TokenPattern.PatternType.STRING, "XOR");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.NOT, "NOT", TokenPattern.PatternType.STRING, "NOT");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.IN, "IN", TokenPattern.PatternType.STRING, "in");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.DOT, "DOT", TokenPattern.PatternType.STRING, ".");
			AddPattern(pattern);

			customPattern = new ArgumentSeparatorPattern();
			customPattern.Initialize((int)ExpressionConstants.ARGUMENT_SEPARATOR, "ARGUMENT_SEPARATOR", TokenPattern.PatternType.STRING, ",", MyContext);
			AddPattern(customPattern);

			pattern = new TokenPattern((int)ExpressionConstants.ARRAY_BRACES, "ARRAY_BRACES", TokenPattern.PatternType.STRING, "[]");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.LEFT_SHIFT, "LEFT_SHIFT", TokenPattern.PatternType.STRING, "<<");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.RIGHT_SHIFT, "RIGHT_SHIFT", TokenPattern.PatternType.STRING, ">>");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.WHITESPACE, "WHITESPACE", TokenPattern.PatternType.REGEXP, "\\s+");
			pattern.Ignore = true;
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.INTEGER, "INTEGER", TokenPattern.PatternType.REGEXP, "\\d+(u|l|ul|lu)?");
			AddPattern(pattern);

			customPattern = new RealPattern();
			customPattern.Initialize((int)ExpressionConstants.REAL, "REAL", TokenPattern.PatternType.REGEXP, "\\d*\\.\\d+([e][+-]\\d{1,3})?f?", MyContext);
			AddPattern(customPattern);

			pattern = new TokenPattern((int)ExpressionConstants.STRING_LITERAL, "STRING_LITERAL", TokenPattern.PatternType.REGEXP, "\"([^\"\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])*\"");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.CHAR_LITERAL, "CHAR_LITERAL", TokenPattern.PatternType.REGEXP, "'([^'\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])'");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.TRUE, "TRUE", TokenPattern.PatternType.STRING, "True");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.FALSE, "FALSE", TokenPattern.PatternType.STRING, "False");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.IDENTIFIER, "IDENTIFIER", TokenPattern.PatternType.REGEXP, "[a-z_]\\w*");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.HEX_LITERAL, "HEX_LITERAL", TokenPattern.PatternType.REGEXP, "0x[0-9a-f]+(u|l|ul|lu)?");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.NULL_LITERAL, "NULL_LITERAL", TokenPattern.PatternType.STRING, "null");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.TIMESPAN, "TIMESPAN", TokenPattern.PatternType.REGEXP, "##(\\d+\\.)?\\d{2}:\\d{2}(:\\d{2}(\\.\\d{1,7})?)?#");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.DATETIME, "DATETIME", TokenPattern.PatternType.REGEXP, "#[^#]+#");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.IF, "IF", TokenPattern.PatternType.STRING, "if");
			AddPattern(pattern);

			pattern = new TokenPattern((int)ExpressionConstants.CAST, "CAST", TokenPattern.PatternType.STRING, "cast");
			AddPattern(pattern);
		}
	}

}
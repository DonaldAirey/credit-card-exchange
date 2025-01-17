﻿namespace FluidTrade.ExpressionEvaluation
{


	///<remarks>A class providing callback methods for the 
	///parser.</remarks> 
	internal abstract class ExpressionAnalyzer : Analyzer
	{

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public override void Enter(Node node)
		{
			switch ((ExpressionConstants)node.Id)
			{
			case ExpressionConstants.ADD:
				EnterAdd((Token)node);

				break;
			case ExpressionConstants.SUB:
				EnterSub((Token)node);

				break;
			case ExpressionConstants.MUL:
				EnterMul((Token)node);

				break;
			case ExpressionConstants.DIV:
				EnterDiv((Token)node);

				break;
			case ExpressionConstants.POWER:
				EnterPower((Token)node);

				break;
			case ExpressionConstants.MOD:
				EnterMod((Token)node);

				break;
			case ExpressionConstants.LEFT_PAREN:
				EnterLeftParen((Token)node);

				break;
			case ExpressionConstants.RIGHT_PAREN:
				EnterRightParen((Token)node);

				break;
			case ExpressionConstants.LEFT_BRACE:
				EnterLeftBrace((Token)node);

				break;
			case ExpressionConstants.RIGHT_BRACE:
				EnterRightBrace((Token)node);

				break;
			case ExpressionConstants.EQ:
				EnterEq((Token)node);

				break;
			case ExpressionConstants.LT:
				EnterLt((Token)node);

				break;
			case ExpressionConstants.GT:
				EnterGt((Token)node);

				break;
			case ExpressionConstants.LTE:
				EnterLte((Token)node);

				break;
			case ExpressionConstants.GTE:
				EnterGte((Token)node);

				break;
			case ExpressionConstants.NE:
				EnterNe((Token)node);

				break;
			case ExpressionConstants.AND:
				EnterAnd((Token)node);

				break;
			case ExpressionConstants.OR:
				EnterOr((Token)node);

				break;
			case ExpressionConstants.XOR:
				EnterXor((Token)node);

				break;
			case ExpressionConstants.NOT:
				EnterNot((Token)node);

				break;
			case ExpressionConstants.IN:
				EnterIn((Token)node);

				break;
			case ExpressionConstants.DOT:
				EnterDot((Token)node);

				break;
			case ExpressionConstants.ARGUMENT_SEPARATOR:
				EnterArgumentSeparator((Token)node);

				break;
			case ExpressionConstants.ARRAY_BRACES:
				EnterArrayBraces((Token)node);

				break;
			case ExpressionConstants.LEFT_SHIFT:
				EnterLeftShift((Token)node);

				break;
			case ExpressionConstants.RIGHT_SHIFT:
				EnterRightShift((Token)node);

				break;
			case ExpressionConstants.INTEGER:
				EnterInteger((Token)node);

				break;
			case ExpressionConstants.REAL:
				EnterReal((Token)node);

				break;
			case ExpressionConstants.STRING_LITERAL:
				EnterStringLiteral((Token)node);

				break;
			case ExpressionConstants.CHAR_LITERAL:
				EnterCharLiteral((Token)node);

				break;
			case ExpressionConstants.TRUE:
				EnterTrue((Token)node);

				break;
			case ExpressionConstants.FALSE:
				EnterFalse((Token)node);

				break;
			case ExpressionConstants.IDENTIFIER:
				EnterIdentifier((Token)node);

				break;
			case ExpressionConstants.HEX_LITERAL:
				EnterHexLiteral((Token)node);

				break;
			case ExpressionConstants.NULL_LITERAL:
				EnterNullLiteral((Token)node);

				break;
			case ExpressionConstants.TIMESPAN:
				EnterTimespan((Token)node);

				break;
			case ExpressionConstants.DATETIME:
				EnterDatetime((Token)node);

				break;
			case ExpressionConstants.IF:
				EnterIf((Token)node);

				break;
			case ExpressionConstants.CAST:
				EnterCast((Token)node);

				break;
			case ExpressionConstants.EXPRESSION:
				EnterExpression((Production)node);

				break;
			case ExpressionConstants.XOR_EXPRESSION:
				EnterXorExpression((Production)node);

				break;
			case ExpressionConstants.OR_EXPRESSION:
				EnterOrExpression((Production)node);

				break;
			case ExpressionConstants.AND_EXPRESSION:
				EnterAndExpression((Production)node);

				break;
			case ExpressionConstants.NOT_EXPRESSION:
				EnterNotExpression((Production)node);

				break;
			case ExpressionConstants.IN_EXPRESSION:
				EnterInExpression((Production)node);

				break;
			case ExpressionConstants.IN_TARGET_EXPRESSION:
				EnterInTargetExpression((Production)node);

				break;
			case ExpressionConstants.IN_LIST_TARGET_EXPRESSION:
				EnterInListTargetExpression((Production)node);

				break;
			case ExpressionConstants.COMPARE_EXPRESSION:
				EnterCompareExpression((Production)node);

				break;
			case ExpressionConstants.SHIFT_EXPRESSION:
				EnterShiftExpression((Production)node);

				break;
			case ExpressionConstants.ADDITIVE_EXPRESSION:
				EnterAdditiveExpression((Production)node);

				break;
			case ExpressionConstants.MULTIPLICATIVE_EXPRESSION:
				EnterMultiplicativeExpression((Production)node);

				break;
			case ExpressionConstants.POWER_EXPRESSION:
				EnterPowerExpression((Production)node);

				break;
			case ExpressionConstants.NEGATE_EXPRESSION:
				EnterNegateExpression((Production)node);

				break;
			case ExpressionConstants.MEMBER_EXPRESSION:
				EnterMemberExpression((Production)node);

				break;
			case ExpressionConstants.MEMBER_ACCESS_EXPRESSION:
				EnterMemberAccessExpression((Production)node);

				break;
			case ExpressionConstants.BASIC_EXPRESSION:
				EnterBasicExpression((Production)node);

				break;
			case ExpressionConstants.MEMBER_FUNCTION_EXPRESSION:
				EnterMemberFunctionExpression((Production)node);

				break;
			case ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
				EnterFieldPropertyExpression((Production)node);

				break;
			case ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION:
				EnterSpecialFunctionExpression((Production)node);

				break;
			case ExpressionConstants.IF_EXPRESSION:
				EnterIfExpression((Production)node);

				break;
			case ExpressionConstants.CAST_EXPRESSION:
				EnterCastExpression((Production)node);

				break;
			case ExpressionConstants.CAST_TYPE_EXPRESSION:
				EnterCastTypeExpression((Production)node);

				break;
			case ExpressionConstants.INDEX_EXPRESSION:
				EnterIndexExpression((Production)node);

				break;
			case ExpressionConstants.FUNCTION_CALL_EXPRESSION:
				EnterFunctionCallExpression((Production)node);

				break;
			case ExpressionConstants.ARGUMENT_LIST:
				EnterArgumentList((Production)node);

				break;
			case ExpressionConstants.LITERAL_EXPRESSION:
				EnterLiteralExpression((Production)node);

				break;
			case ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION:
				EnterBooleanLiteralExpression((Production)node);

				break;
			case ExpressionConstants.EXPRESSION_GROUP:
				EnterExpressionGroup((Production)node);

				break;
			}
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public override Node Exit(Node node)
		{
			switch ((ExpressionConstants)node.Id)
			{
			case ExpressionConstants.ADD:

				return ExitAdd((Token)node);
			case ExpressionConstants.SUB:

				return ExitSub((Token)node);
			case ExpressionConstants.MUL:

				return ExitMul((Token)node);
			case ExpressionConstants.DIV:

				return ExitDiv((Token)node);
			case ExpressionConstants.POWER:

				return ExitPower((Token)node);
			case ExpressionConstants.MOD:

				return ExitMod((Token)node);
			case ExpressionConstants.LEFT_PAREN:

				return ExitLeftParen((Token)node);
			case ExpressionConstants.RIGHT_PAREN:

				return ExitRightParen((Token)node);
			case ExpressionConstants.LEFT_BRACE:

				return ExitLeftBrace((Token)node);
			case ExpressionConstants.RIGHT_BRACE:

				return ExitRightBrace((Token)node);
			case ExpressionConstants.EQ:

				return ExitEq((Token)node);
			case ExpressionConstants.LT:

				return ExitLt((Token)node);
			case ExpressionConstants.GT:

				return ExitGt((Token)node);
			case ExpressionConstants.LTE:

				return ExitLte((Token)node);
			case ExpressionConstants.GTE:

				return ExitGte((Token)node);
			case ExpressionConstants.NE:

				return ExitNe((Token)node);
			case ExpressionConstants.AND:

				return ExitAnd((Token)node);
			case ExpressionConstants.OR:

				return ExitOr((Token)node);
			case ExpressionConstants.XOR:

				return ExitXor((Token)node);
			case ExpressionConstants.NOT:

				return ExitNot((Token)node);
			case ExpressionConstants.IN:

				return ExitIn((Token)node);
			case ExpressionConstants.DOT:

				return ExitDot((Token)node);
			case ExpressionConstants.ARGUMENT_SEPARATOR:

				return ExitArgumentSeparator((Token)node);
			case ExpressionConstants.ARRAY_BRACES:

				return ExitArrayBraces((Token)node);
			case ExpressionConstants.LEFT_SHIFT:

				return ExitLeftShift((Token)node);
			case ExpressionConstants.RIGHT_SHIFT:

				return ExitRightShift((Token)node);
			case ExpressionConstants.INTEGER:

				return ExitInteger((Token)node);
			case ExpressionConstants.REAL:

				return ExitReal((Token)node);
			case ExpressionConstants.STRING_LITERAL:

				return ExitStringLiteral((Token)node);
			case ExpressionConstants.CHAR_LITERAL:

				return ExitCharLiteral((Token)node);
			case ExpressionConstants.TRUE:

				return ExitTrue((Token)node);
			case ExpressionConstants.FALSE:

				return ExitFalse((Token)node);
			case ExpressionConstants.IDENTIFIER:

				return ExitIdentifier((Token)node);
			case ExpressionConstants.HEX_LITERAL:

				return ExitHexLiteral((Token)node);
			case ExpressionConstants.NULL_LITERAL:

				return ExitNullLiteral((Token)node);
			case ExpressionConstants.TIMESPAN:

				return ExitTimeSpan((Token)node);
			case ExpressionConstants.DATETIME:

				return ExitDateTime((Token)node);
			case ExpressionConstants.IF:

				return ExitIf((Token)node);
			case ExpressionConstants.CAST:

				return ExitCast((Token)node);
			case ExpressionConstants.EXPRESSION:

				return ExitExpression((Production)node);
			case ExpressionConstants.XOR_EXPRESSION:

				return ExitXorExpression((Production)node);
			case ExpressionConstants.OR_EXPRESSION:

				return ExitOrExpression((Production)node);
			case ExpressionConstants.AND_EXPRESSION:

				return ExitAndExpression((Production)node);
			case ExpressionConstants.NOT_EXPRESSION:

				return ExitNotExpression((Production)node);
			case ExpressionConstants.IN_EXPRESSION:

				return ExitInExpression((Production)node);
			case ExpressionConstants.IN_TARGET_EXPRESSION:

				return ExitInTargetExpression((Production)node);
			case ExpressionConstants.IN_LIST_TARGET_EXPRESSION:

				return ExitInListTargetExpression((Production)node);
			case ExpressionConstants.COMPARE_EXPRESSION:

				return ExitCompareExpression((Production)node);
			case ExpressionConstants.SHIFT_EXPRESSION:

				return ExitShiftExpression((Production)node);
			case ExpressionConstants.ADDITIVE_EXPRESSION:

				return ExitAdditiveExpression((Production)node);
			case ExpressionConstants.MULTIPLICATIVE_EXPRESSION:

				return ExitMultiplicativeExpression((Production)node);
			case ExpressionConstants.POWER_EXPRESSION:

				return ExitPowerExpression((Production)node);
			case ExpressionConstants.NEGATE_EXPRESSION:

				return ExitNegateExpression((Production)node);
			case ExpressionConstants.MEMBER_EXPRESSION:

				return ExitMemberExpression((Production)node);
			case ExpressionConstants.MEMBER_ACCESS_EXPRESSION:

				return ExitMemberAccessExpression((Production)node);
			case ExpressionConstants.BASIC_EXPRESSION:

				return ExitBasicExpression((Production)node);
			case ExpressionConstants.MEMBER_FUNCTION_EXPRESSION:

				return ExitMemberFunctionExpression((Production)node);
			case ExpressionConstants.FIELD_PROPERTY_EXPRESSION:

				return ExitFieldPropertyExpression((Production)node);
			case ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION:

				return ExitSpecialFunctionExpression((Production)node);
			case ExpressionConstants.IF_EXPRESSION:

				return ExitIfExpression((Production)node);
			case ExpressionConstants.CAST_EXPRESSION:

				return ExitCastExpression((Production)node);
			case ExpressionConstants.CAST_TYPE_EXPRESSION:

				return ExitCastTypeExpression((Production)node);
			case ExpressionConstants.INDEX_EXPRESSION:

				return ExitIndexExpression((Production)node);
			case ExpressionConstants.FUNCTION_CALL_EXPRESSION:

				return ExitFunctionCallExpression((Production)node);
			case ExpressionConstants.ARGUMENT_LIST:

				return ExitArgumentList((Production)node);
			case ExpressionConstants.LITERAL_EXPRESSION:

				return ExitLiteralExpression((Production)node);
			case ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION:

				return ExitBooleanLiteralExpression((Production)node);
			case ExpressionConstants.EXPRESSION_GROUP:

				return ExitExpressionGroup((Production)node);
			}
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public override void Child(Production node, Node child)
		{
			switch ((ExpressionConstants)node.Id)
			{
			case ExpressionConstants.EXPRESSION:
				ChildExpression(node, child);

				break;
			case ExpressionConstants.XOR_EXPRESSION:
				ChildXorExpression(node, child);

				break;
			case ExpressionConstants.OR_EXPRESSION:
				ChildOrExpression(node, child);

				break;
			case ExpressionConstants.AND_EXPRESSION:
				ChildAndExpression(node, child);

				break;
			case ExpressionConstants.NOT_EXPRESSION:
				ChildNotExpression(node, child);

				break;
			case ExpressionConstants.IN_EXPRESSION:
				ChildInExpression(node, child);

				break;
			case ExpressionConstants.IN_TARGET_EXPRESSION:
				ChildInTargetExpression(node, child);

				break;
			case ExpressionConstants.IN_LIST_TARGET_EXPRESSION:
				ChildInListTargetExpression(node, child);

				break;
			case ExpressionConstants.COMPARE_EXPRESSION:
				ChildCompareExpression(node, child);

				break;
			case ExpressionConstants.SHIFT_EXPRESSION:
				ChildShiftExpression(node, child);

				break;
			case ExpressionConstants.ADDITIVE_EXPRESSION:
				ChildAdditiveExpression(node, child);

				break;
			case ExpressionConstants.MULTIPLICATIVE_EXPRESSION:
				ChildMultiplicativeExpression(node, child);

				break;
			case ExpressionConstants.POWER_EXPRESSION:
				ChildPowerExpression(node, child);

				break;
			case ExpressionConstants.NEGATE_EXPRESSION:
				ChildNegateExpression(node, child);

				break;
			case ExpressionConstants.MEMBER_EXPRESSION:
				ChildMemberExpression(node, child);

				break;
			case ExpressionConstants.MEMBER_ACCESS_EXPRESSION:
				ChildMemberAccessExpression(node, child);

				break;
			case ExpressionConstants.BASIC_EXPRESSION:
				ChildBasicExpression(node, child);

				break;
			case ExpressionConstants.MEMBER_FUNCTION_EXPRESSION:
				ChildMemberFunctionExpression(node, child);

				break;
			case ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
				ChildFieldPropertyExpression(node, child);

				break;
			case ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION:
				ChildSpecialFunctionExpression(node, child);

				break;
			case ExpressionConstants.IF_EXPRESSION:
				ChildIfExpression(node, child);

				break;
			case ExpressionConstants.CAST_EXPRESSION:
				ChildCastExpression(node, child);

				break;
			case ExpressionConstants.CAST_TYPE_EXPRESSION:
				ChildCastTypeExpression(node, child);

				break;
			case ExpressionConstants.INDEX_EXPRESSION:
				ChildIndexExpression(node, child);

				break;
			case ExpressionConstants.FUNCTION_CALL_EXPRESSION:
				ChildFunctionCallExpression(node, child);

				break;
			case ExpressionConstants.ARGUMENT_LIST:
				ChildArgumentList(node, child);

				break;
			case ExpressionConstants.LITERAL_EXPRESSION:
				ChildLiteralExpression(node, child);

				break;
			case ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION:
				ChildBooleanLiteralExpression(node, child);

				break;
			case ExpressionConstants.EXPRESSION_GROUP:
				ChildExpressionGroup(node, child);

				break;
			}
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterAdd(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitAdd(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterSub(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitSub(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterMul(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitMul(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterDiv(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitDiv(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterPower(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitPower(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterMod(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitMod(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterLeftParen(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitLeftParen(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterRightParen(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitRightParen(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterLeftBrace(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitLeftBrace(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterRightBrace(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitRightBrace(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterEq(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitEq(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterLt(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitLt(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterGt(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitGt(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterLte(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitLte(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterGte(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitGte(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterNe(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitNe(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterAnd(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitAnd(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterOr(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitOr(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterXor(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitXor(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterNot(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitNot(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterIn(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitIn(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterDot(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitDot(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterArgumentSeparator(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitArgumentSeparator(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterArrayBraces(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitArrayBraces(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterLeftShift(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitLeftShift(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterRightShift(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitRightShift(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterInteger(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitInteger(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterReal(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitReal(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterStringLiteral(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitStringLiteral(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterCharLiteral(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitCharLiteral(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterTrue(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitTrue(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterFalse(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitFalse(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterIdentifier(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitIdentifier(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterHexLiteral(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitHexLiteral(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterNullLiteral(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitNullLiteral(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterTimespan(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitTimeSpan(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterDatetime(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitDateTime(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterIf(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitIf(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterCast(Token node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitCast(Token node)
		{
			return node;
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterXorExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitXorExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildXorExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterOrExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitOrExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildOrExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterAndExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitAndExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildAndExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterNotExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitNotExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildNotExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterInExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitInExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildInExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterInTargetExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitInTargetExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildInTargetExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterInListTargetExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitInListTargetExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildInListTargetExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterCompareExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitCompareExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildCompareExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterShiftExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitShiftExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildShiftExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterAdditiveExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitAdditiveExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildAdditiveExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterMultiplicativeExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitMultiplicativeExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildMultiplicativeExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterPowerExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitPowerExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildPowerExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterNegateExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitNegateExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildNegateExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterMemberExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitMemberExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildMemberExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterMemberAccessExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitMemberAccessExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildMemberAccessExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterBasicExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitBasicExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildBasicExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterMemberFunctionExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitMemberFunctionExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildMemberFunctionExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterFieldPropertyExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitFieldPropertyExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildFieldPropertyExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterSpecialFunctionExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitSpecialFunctionExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildSpecialFunctionExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterIfExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitIfExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildIfExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterCastExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitCastExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildCastExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterCastTypeExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitCastTypeExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildCastTypeExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterIndexExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitIndexExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildIndexExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterFunctionCallExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitFunctionCallExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildFunctionCallExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterArgumentList(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitArgumentList(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildArgumentList(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterLiteralExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitLiteralExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildLiteralExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterBooleanLiteralExpression(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitBooleanLiteralExpression(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildBooleanLiteralExpression(Production node, Node child)
		{
			node.AddChild(child);
		}

		///<summary>Called when entering a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being entered</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void EnterExpressionGroup(Production node)
		{
		}

		///<summary>Called when exiting a parse tree node.</summary> 
		/// 
		///<param name='node'>the node being exited</param> 
		/// 
		///<returns>the node to add to the parse tree, or 
		/// null if no parse tree should be created</returns> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual Node ExitExpressionGroup(Production node)
		{
			return node;
		}

		///<summary>Called when adding a child to a parse tree 
		///node.</summary> 
		/// 
		///<param name='node'>the parent node</param> 
		///<param name='child'>the child node, or null</param> 
		/// 
		///<exception cref='ParseException'>if the node analysis 
		///discovered errors</exception> 
		public virtual void ChildExpressionGroup(Production node, Node child)
		{
			node.AddChild(child);
		}
	}
}
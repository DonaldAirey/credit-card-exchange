namespace FluidTrade.ExpressionEvaluation
{

	using System;
	using System.Collections.Generic;

	internal class IdentifierAnalyzer : Analyzer
	{

		private IDictionary<int, string> MyIdentifiers;
		private int MyMemberExpressionCount;
		private bool MyInFieldPropertyExpression;

		public IdentifierAnalyzer()
		{
			MyIdentifiers = new Dictionary<int, string>();
		}

		public override Node Exit(Node node)
		{
			switch ((ExpressionConstants)node.Id)
			{
			case ExpressionConstants.IDENTIFIER:
				this.ExitIdentifier((Token)node);
				break;
			case ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
				this.ExitFieldPropertyExpression();
				break;
			}

			return node;
		}

		public override void Enter(Node node)
		{
			switch ((ExpressionConstants)node.Id)
			{
			case ExpressionConstants.MEMBER_EXPRESSION:
				this.EnterMemberExpression();
				break;
			case ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
				this.EnterFieldPropertyExpression();
				break;
			}
		}

		private void ExitIdentifier(Token node)
		{
			if (MyInFieldPropertyExpression == false)
			{
				return;
			}

			if (MyIdentifiers.ContainsKey(MyMemberExpressionCount) == false)
			{
				MyIdentifiers.Add(MyMemberExpressionCount, node.Image);
			}
		}

		private void EnterMemberExpression()
		{
			MyMemberExpressionCount += 1;
		}

		private void EnterFieldPropertyExpression()
		{
			MyInFieldPropertyExpression = true;
		}

		private void ExitFieldPropertyExpression()
		{
			MyInFieldPropertyExpression = false;
		}

		public void Reset()
		{
			MyIdentifiers.Clear();
			MyMemberExpressionCount = -1;
		}

		public ICollection<string> GetIdentifiers(ExpressionContext context)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			ExpressionImports ei = context.Imports;

			foreach (string identifier in MyIdentifiers.Values)
			{
				// Skip names registered as namespaces
				if (ei.HasNamespace(identifier) == true)
				{
					continue;
				}
				else if (context.Variables.ContainsKey(identifier) == true)
				{
					// Identifier is a variable
					continue;
				}

				// Get only the unique values
				dict[identifier] = null;
			}

			return dict.Keys;
		}
	}

}
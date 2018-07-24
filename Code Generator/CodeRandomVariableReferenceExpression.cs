namespace FluidTrade.Core
{

	using System;
	using System.CodeDom;

	/// <summary>
	/// Generates a unique variable reference expression.
	/// </summary>
	public class CodeRandomVariableReferenceExpression : CodeVariableReferenceExpression
	{

		// Private Static Fields
		private static Random random;
#if DEBUG
		private static Int64 sequence;
#endif

		/// <summary>
		/// Creates the static resources required for this class.
		/// </summary>
		static CodeRandomVariableReferenceExpression()
		{

			// Create a random number generator for the first letter of the variable name.
#if DEBUG
			CodeRandomVariableReferenceExpression.random = new Random(0);
#else
			CodeRandomVariableReferenceExpression.random = new Random(Convert.ToInt32(DateTime.Now.TimeOfDay.TotalMilliseconds));
#endif

		}

		/// <summary>
		/// Creates a unique variable reference expression.
		/// </summary>
		public CodeRandomVariableReferenceExpression()
		{

			// This variable name is guaranteed to be unique and will not conflict with the human-readable variables created by the
			// code generator.
			int startingChar = CodeRandomVariableReferenceExpression.random.Next('a', 'z');
#if DEBUG
			this.VariableName = String.Format("{0}{1}", Char.ConvertFromUtf32(startingChar), CodeRandomVariableReferenceExpression.sequence++);
#else
			this.VariableName = String.Format("{0}{1}", Char.ConvertFromUtf32(startingChar), Guid.NewGuid().ToString("N"));
#endif

		}

	}

}

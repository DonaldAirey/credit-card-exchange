namespace FluidTrade.Core
{

	using System;

    /// <summary>
	/// Generates a unique variable reference expression.
	/// </summary>
	public class RandomVariableName
	{

		// Private Static Fields
		private static Random random;

		/// <summary>
		/// Creates the static resources required for this class.
		/// </summary>
		static RandomVariableName()
		{

			// Create a random number generator for the first letter of the variable name.
			RandomVariableName.random = new Random(Convert.ToInt32(DateTime.Now.TimeOfDay.TotalMilliseconds));

		}

		/// <summary>
		/// Creates a unique variable reference expression.
		/// </summary>
		public static string NewName()
		{

			// This variable name is guaranteed to be unique and will not conflict with the human-readable variables created by the
			// code generator.
			int startingChar = RandomVariableName.random.Next('a', 'z');
			return String.Format("{0}{1}", Char.ConvertFromUtf32(startingChar), Guid.NewGuid().ToString("N"));

		}

	}

}

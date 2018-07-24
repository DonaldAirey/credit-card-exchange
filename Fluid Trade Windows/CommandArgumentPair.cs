namespace FluidTrade.Core.Windows
{


    /// <summary>
	/// A pair of elements used to execute an generic command with a set of arguments.
	/// </summary>
	public class CommandArgumentPair
	{

		// Public Instance Fields
		public readonly GenericEventHandler GenericEventHandler;
		public readonly GenericEventArgs GenericEventArgs;

		/// <summary>
		/// Create a description of a method that should be invoked with the specified arguments.
		/// </summary>
		/// <param name="argumentDelegate">The method that should be invoked.</param>
		/// <param name="arguments">The arguments of the method invokation.</param>
		public CommandArgumentPair(GenericEventHandler genericEventHandler, GenericEventArgs genericEventArgs)
		{

			// Initialize the object
			this.GenericEventHandler = genericEventHandler;
			this.GenericEventArgs = genericEventArgs;

		}

		/// <summary>
		/// Create a description of a method that should be invoked with the specified arguments.
		/// </summary>
		/// <param name="argumentDelegate">The method that should be invoked.</param>
		/// <param name="arguments">The arguments of the method invokation.</param>
		public CommandArgumentPair(GenericEventHandler genericEventHandler, params object[] arguments)
		{

			// Initialize the object
			this.GenericEventHandler = genericEventHandler;
			this.GenericEventArgs = new GenericEventArgs(arguments);

		}

	}

}

namespace FluidTrade.Core.Windows
{


    /// <summary>
	/// A pair of elements used to execute an generic command with a set of arguments.
	/// </summary>
	public class TimelineSegment
	{

		// Public Instance Fields
		public Storyline Storyline;
		public readonly StorylineHandler TimelineHandler;
        public readonly FluidTrade.Core.Windows.GenericEventArgs GenericEventArgs;

		/// <summary>
		/// Create a description of a method that should be invoked with the specified arguments.
		/// </summary>
		/// <param name="argumentDelegate">The method that should be invoked.</param>
		/// <param name="arguments">The arguments of the method invokation.</param>
		public TimelineSegment(StorylineHandler genericEventHandler, GenericEventArgs genericEventArgs)
		{

			// Initialize the object
			this.TimelineHandler = genericEventHandler;
			this.GenericEventArgs = genericEventArgs;

		}

		/// <summary>
		/// Create a description of a method that should be invoked with the specified arguments.
		/// </summary>
		/// <param name="argumentDelegate">The method that should be invoked.</param>
		/// <param name="arguments">The arguments of the method invokation.</param>
		public TimelineSegment(StorylineHandler genericEventHandler, params object[] arguments)
		{

			// Initialize the object
			this.TimelineHandler = genericEventHandler;
			this.GenericEventArgs = new GenericEventArgs(arguments);

		}

	}

}

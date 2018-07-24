namespace FluidTrade.Core.Windows
{

	using System;

    /// <summary>
	/// Arguments for an asynchronous command event.
	/// </summary>
	public class GenericEventArgs : EventArgs
	{

		/// <summary>
		/// The column that was deleted.
		/// </summary>
		public readonly object[] Arguments;

		/// <summary>
		/// Creates an object used to describe a column deletion event.
		/// </summary>
		/// <param name="column">The column that was deleted.</param>
		public GenericEventArgs(params object[] arguments)
		{

			// Initialize the object.
			this.Arguments = arguments;

		}

	}

}

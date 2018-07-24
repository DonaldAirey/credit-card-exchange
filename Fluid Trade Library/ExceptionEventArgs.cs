namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Arguments for a background exception handler.
	/// </summary>
	public class ExceptionEventArgs : EventArgs
	{

		// Public Instance Fields
		public System.Exception Exception;

		/// <summary>
		/// Create the arguments for a background exception handler.
		/// </summary>
		/// <param name="exception">The exception that occurred in a background process.</param>
		public ExceptionEventArgs(Exception exception)
		{

			// Initialize the object
			this.Exception = exception;

		}

	}

}

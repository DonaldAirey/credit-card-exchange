namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// An event that indicates a background process incurred an exception that the foreground should process.
	/// </summary>
	/// <param name="sender">The object that originated the event.</param>
	/// <param name="exceptionHandlerArgs">The event arguments.</param>
	public delegate void ExceptionEventHandler(Object sender, ExceptionEventArgs exceptionEventArgs);

}

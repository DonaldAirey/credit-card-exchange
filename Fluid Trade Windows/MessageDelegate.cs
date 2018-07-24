namespace FluidTrade.Core.Windows
{

	using System;

	/// <summary>
	/// Used to transmit a message from the background to the foreground.
	/// </summary>
	/// <param name="message">A message to be displayed in the foreground.</param>
	public delegate void MessageDelegate(String message);

}

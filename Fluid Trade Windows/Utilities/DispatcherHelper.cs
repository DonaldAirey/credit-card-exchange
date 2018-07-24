namespace FluidTrade.Core.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Threading;

	/// <summary>
	/// Useful methods for dispatchers.
	/// </summary>
	public static class DispatcherHelper
	{

		/// <summary>
		/// Pump events until all of the events at a particular priority have completed.
		/// </summary>
		/// <param name="dispatcher">The dispatcher to pump messages on.</param>
		/// <param name="minimumPriority">The priority to stop pumping at.</param>
		public static void DoEvents(this Dispatcher dispatcher, DispatcherPriority minimumPriority)
		{

			DispatcherFrame nested = new DispatcherFrame();

			dispatcher.BeginInvoke(
				new Action(() =>
					nested.Continue = false),
				minimumPriority);

			Dispatcher.PushFrame(nested);

		}

	}

}

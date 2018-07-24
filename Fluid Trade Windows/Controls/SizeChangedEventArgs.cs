namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Windows;

	public class SizeChangedEventArgs : EventArgs
	{

		public Size OldSize;
		public Size NewSize;

		public SizeChangedEventArgs(Size oldSize, Size newSize)
		{

			// Initialize the object.
			this.OldSize = oldSize;
			this.NewSize = newSize;

		}

	}
}

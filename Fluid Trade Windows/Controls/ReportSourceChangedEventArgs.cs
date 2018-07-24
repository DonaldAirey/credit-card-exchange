namespace FluidTrade.Core.Windows.Controls
{

	using System;

	/// <summary>
	/// Contains the arguments used to describe new XAML source code for a report.
	/// </summary>
	public class ReportSourceChangedEventArgs : EventArgs
	{

		// Public Instance Fields
		public System.String Source;

		/// <summary>
		/// Create an object that indicates new XAML source code for a report.
		/// </summary>
		/// <param name="source"></param>
		public ReportSourceChangedEventArgs(string source)
		{

			// Initialize the object
			this.Source = source;

		}

	}

}

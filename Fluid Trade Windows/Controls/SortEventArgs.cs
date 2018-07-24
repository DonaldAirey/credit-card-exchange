namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Describes the arguments for an event to sort the report.
	/// </summary>
	public class SortEventArgs : EventArgs
	{

		/// <summary>
		/// A collection of items that describe how to sort the report.
		/// </summary>
		public readonly List<SortItem> Items;

		/// <summary>
		/// Create an object that describes how to sort a report.
		/// </summary>
		/// <param name="items"></param>
		public SortEventArgs(List<SortItem> items)
		{

			// Initialize the object
			this.Items = items;

		}

	}

}

namespace FluidTrade.Core.Windows.Controls
{

	using System;

	/// <summary>
	/// Arguments that are passed when the mouse is moved during a resizing operation.
	/// </summary>
	public class ResizeRowEventArgs : EventArgs
	{

		/// <summary>
		/// The column that is being resized.
		/// </summary>
		public ReportRow RowDefinition;

		/// <summary>
		/// Indicates that this is the final movement.
		/// </summary>
		public System.Boolean IsFinal;

		/// <summary>
		/// The width of the column.
		/// </summary>
		public System.Double Width;

		/// <summary>
		/// Describes the movement of the mouse during a column resizing operation.
		/// </summary>
		/// <param name="reportColumn">The column that is being resized.</param>
		/// <param name="width">The width of the column.</param>
		/// <param name="isFinal">Indicates that this is the final movement of the sequence.</param>
		public ResizeRowEventArgs(ReportRow reportColumn, double width, bool isFinal)
		{

			// Initialize the object.
			this.RowDefinition = reportColumn;
			this.Width = width;
			this.IsFinal = isFinal;

		}

	}

}

namespace FluidTrade.Core.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core.Windows.Controls;

	/// <summary>
	/// The event arguments of a cell changed event.
	/// </summary>
	public class ReportCellPropertyChangedEventArgs : EventArgs
	{

		private ReportRow row;
		private ReportCell cell;
		private String property;

		/// <summary>
		/// Create the event arguments.
		/// </summary>
		/// <param name="row">The row the cell is in.</param>
		/// <param name="cell">The cell itself.</param>
		/// <param name="property">The property that changed.</param>
		public ReportCellPropertyChangedEventArgs(ReportRow row, ReportCell cell, String property)
		{

			this.row = row;
			this.cell = cell;
			this.property = property;

		}

		/// <summary>
		/// The row the cell is in.
		/// </summary>
		public ReportRow Row
		{
			get { return this.row; }
		}

		/// <summary>
		/// The cell itself.
		/// </summary>
		public ReportCell Cell
		{
			get { return this.cell; }
		}

		/// <summary>
		/// The property that changed.
		/// </summary>
		public String Property
		{
			get { return this.property; }
		}

	}
}

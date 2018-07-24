namespace FluidTrade.Core.Windows.Controls
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
using System.Windows.Controls;
using System.Windows;

	public class ReportGridtToolTipEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// Event Args for Report Tooltip events
		/// </summary>
		/// <param name="routedEvent"></param>
		/// <param name="source"></param>
		/// <param name="reportCell"></param>
		/// <param name="inputElementAtPoint"></param>
		/// <param name="toolTip"></param>
		public ReportGridtToolTipEventArgs(RoutedEvent routedEvent, object source, ReportCell reportCell, IInputElement inputElementForTooltip, ToolTip toolTip)
			:base(routedEvent, source)
		{
			this.ReportCell = reportCell;
			this.InputElementForTooltip = inputElementForTooltip;
			this.ToolTip = toolTip;
		}

		/// <summary>
		/// Get report cell that the mouse is over when tooltip is to be shown
		/// </summary>
		public ReportCell ReportCell { get; private set; }

		/// <summary>
		/// Get report cell Input Element that the mouse is over when tooltip is to be shown
		/// </summary>
		public IInputElement InputElementForTooltip { get; private set; }

		/// <summary>
		/// Tooltip control for the grid. Subscriber needs to set the content to show the tooltip
		/// </summary>
		public ToolTip ToolTip { get; private set; }
	}


	/// <summary>
	/// Event handler delegate for Report Tooltip events
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="undoPropertyChangedEventArgs"></param>
	public delegate void ReportGridtToolTipEventHandler(object sender, ReportGridtToolTipEventArgs undoPropertyChangedEventArgs);
	
}

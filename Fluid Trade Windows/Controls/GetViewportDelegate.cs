namespace FluidTrade.Core.Windows.Controls
{

	using System.Windows;

	/// <summary>
	/// Used to invoke events that require a row definition.
	/// </summary>
	/// <param name="reportColumn"></param>
	public delegate Rect GetViewportDelegate();

	public delegate ReportGrid GetReportGridDelegate();

}

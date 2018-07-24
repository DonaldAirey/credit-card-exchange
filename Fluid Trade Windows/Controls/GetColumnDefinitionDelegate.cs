namespace FluidTrade.Core.Windows.Controls
{

	using System;

	/// <summary>
	/// Used to invoke events that require a column definition.
	/// </summary>
	/// <param name="reportColumn"></param>
	public delegate ReportColumn GetColumnDefinitionDelegate(Type type);

}

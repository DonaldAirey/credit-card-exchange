namespace FluidTrade.Core.Windows.Controls
{
    using System.Xml.Linq;

	/// <summary>
	/// XAML element for describing a ColumnDefinition
	/// </summary>
	class ColumnDefinitionElement : XElement
	{

		/// <summary>
		/// Create an XAML element describing the column.
		/// </summary>
		/// <param name="reportColumn">The CLR description of the column.</param>
		public ColumnDefinitionElement(ReportColumn reportColumn, ReportField fieldDefinition)
			: base(DynamicReport.namespacePresentation + "ColumnDefinition")
		{

			// The 'Width' attribute defaults to the width specified by the FieldDefinition.
			Add(new XAttribute("ColumnId", reportColumn.ColumnId));
			if (reportColumn.Width != fieldDefinition.Width)
				Add(new XAttribute("Width", reportColumn.Width));


		}

	}
}

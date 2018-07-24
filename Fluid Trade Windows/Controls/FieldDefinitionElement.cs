namespace FluidTrade.Core.Windows.Controls
{
    using System.Xml.Linq;

	/// <summary>
	/// XAML element for describing a FieldDefinition
	/// </summary>
	class FieldDefinitionElement : XElement
	{

		/// <summary>
		/// Create an XAML element describing the field.
		/// </summary>
		/// <param name="fieldDefinition">The CLR description of the field.</param>
		public FieldDefinitionElement(ReportField fieldDefinition)
			: base(DynamicReport.namespacePresentation + "FieldDefinition")
		{

			// Initialize the object.
			Add(new XAttribute("ColumnId", fieldDefinition.ColumnId));
			Add(new XAttribute("Description", fieldDefinition.Description));
			Add(new XAttribute("Width", fieldDefinition.Width));

		}

	}
}

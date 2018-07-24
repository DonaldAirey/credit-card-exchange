namespace FluidTrade.Core.Windows.Controls
{

	using System;
    using System.Windows;
	using System.Windows.Media.Animation;

	/// <summary>
	/// Describes the attributes of a column.
	/// </summary>
	public class ReportColumn : Animatable
	{

		// Public Static Columns
		public static readonly DependencyProperty ColumnIdProperty;
		public static readonly DependencyProperty LeftProperty;
		public static readonly DependencyProperty WidthProperty;
		public static readonly DependencyProperty ActualLeftProperty;
		public static readonly DependencyProperty ActualWidthProperty;

		/// <summary>
		/// Create the static resources for a column definition.
		/// </summary>
		static ReportColumn()
		{

			// ColumnId Property
			ReportColumn.ColumnIdProperty = DependencyProperty.Register("ColumnId", typeof(String), typeof(ReportColumn));

			// Left Property
			ReportColumn.LeftProperty = DependencyProperty.Register("Left", typeof(Double), typeof(ReportColumn));

			// Width Property
			ReportColumn.WidthProperty = DependencyProperty.Register("Width", typeof(Double), typeof(ReportColumn), new PropertyMetadata(Double.NaN));

			// Actual Left Property
			ReportColumn.ActualLeftProperty = DependencyProperty.Register("ActualLeft", typeof(Double), typeof(ReportColumn));

			// Actual Width Property
			ReportColumn.ActualWidthProperty = DependencyProperty.Register("ActualWidth", typeof(Double), typeof(ReportColumn));

		}

		/// <summary>
		/// Gets the rendered left edge of the column.
		/// </summary>
		public Double ActualLeft
		{
			get { return (Double)GetValue(ReportColumn.ActualLeftProperty); }
		}

		/// <summary>
		/// Gets the rendered width of the column.
		/// </summary>
		public Double ActualWidth
		{
			get { return (Double)GetValue(ReportColumn.ActualWidthProperty); }
		}

		/// <summary>
		/// Gets or sets the column identifier.
		/// </summary>
		public String ColumnId
		{
			get { return (String)GetValue(ReportColumn.ColumnIdProperty); }
			set { SetValue(ReportColumn.ColumnIdProperty, value); }
		}

		/// <summary>
		/// Gets or sets the left edge of the column.
		/// </summary>
		public Double Left
		{
			get { return (Double)GetValue(ReportColumn.LeftProperty); }
			set { SetValue(ReportColumn.LeftProperty, value); }
		}

		/// <summary>
		/// Gets the location of the right side of the column.
		/// </summary>
		public Double Right
		{
			get { return this.Left + this.Width; }
		}

		/// <summary>
		/// Gets or sets the current width of the column.
		/// </summary>
		public Double Width
		{
			get { return (Double)GetValue(ReportColumn.WidthProperty); }
			set { SetValue(ReportColumn.WidthProperty, value); }
		}

		/// <summary>
		/// Create a freezable copy of the object.
		/// </summary>
		/// <returns>A new, freezable copy of the object.</returns>
		protected override Freezable CreateInstanceCore()
		{
			return new ReportColumn();
		}

		/// <summary>
		/// Returns a System.String that represents the current object.
		/// </summary>
		/// <returns>A System.String that represents the current object.</returns>
		public override String ToString()
		{
			return String.Format("{{{0}}}", this.ColumnId);
		}

	}

}

namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.ComponentModel;
	using System.Windows;

	/// <summary>
	/// Represents a virtual allocation of space used to display content in the report.
	/// </summary>
	public class ReportCell : INotifyPropertyChanged
	{

		// Private Static Fields
		private static ReportCell emptyCell;

		// Public Instance Fields
		public ReportColumn ReportColumn;
		public IContent Content;
		public ReportRow ReportRow;
		public RowTemplate RowTemplate;

		// Private Instance Fields
		private Boolean isFocused;
		private Boolean isObsolete;
		private Boolean isSelected;

		// Public Events
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Create the static resources required for this class.
		/// </summary>
		static ReportCell()
		{

			// This cell can be used when an explicitly empty cell is required for an operation.  For example, the MeasureOverride
			// method uses an explicitly Empty ReportCell as an indication that the visual element is a container for a cell, even
			// though it doesn't currently have any virtual cell associated with it.
			ReportCell.emptyCell = new ReportCell();

		}

		/// <summary>
		/// Creates an explicitly empty cell for the report.
		/// </summary>
		private ReportCell()
		{

			// Initialize the object.
			this.IsObsolete = false;
			this.isFocused = false;

			this.Content = null;
			this.ReportColumn = null;
			this.ReportRow = null;
		}

		/// <summary>
		/// Creates a new virtual cell for the report.
		/// </summary>
		/// <param name="iContent">The content of the cell.</param>
		/// <param name="reportColumn">The row to which the cell belongs.</param>
		/// <param name="reportRow">The column to which the cell belongs.</param>
		public ReportCell(IContent iContent, ReportColumn reportColumn, ReportRow reportRow)
		{

			// Initialize the object.
			this.IsObsolete = false;
			this.isFocused = false;
			this.Content = iContent;
			this.ReportColumn = reportColumn;
			this.ReportRow = reportRow;

		}

		/// <summary>
		/// An Empty ReportCell.
		/// </summary>
		public static ReportCell Empty
		{
			get { return ReportCell.emptyCell; }
		}
	
		/// <summary>
		/// Gets or sets an indication of whether the cell has the input focus.
		/// </summary>
		public bool IsFocused
		{
			get { return this.isFocused; }
			set
			{
				if (this.isFocused != value)
				{
					this.isFocused = value;
					if (this.PropertyChanged != null)
						this.PropertyChanged(this, new PropertyChangedEventArgs("IsFocused"));
				}
			}
		}

		/// <summary>
		/// Gets or sets an indication of whether the cell has been removed from the report.
		/// </summary>
		public bool IsObsolete
		{
			get { return this.isObsolete; }
			set { this.isObsolete = value; }
		}

		/// <summary>
		/// Gets or sets an indication of whether the cell is selected or not.
		/// </summary>
		public bool IsSelected
		{
			get { return this.isSelected; }
			set
			{
				if (this.isSelected != value)
				{
					this.isSelected = value;
					if (this.PropertyChanged != null)
						this.PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
				}
			}
		}

		/// <summary>
		/// Gets the area occupied by the cell.
		/// </summary>
		public Rect Rect
		{
			get { return new Rect(this.ReportColumn.Left, this.ReportRow.Top, this.ReportColumn.Width, this.ReportRow.Height); }
		}

		/// <summary>
		/// Gets the area occupied by the cell.
		/// </summary>
		public Rect ActualRect
		{
			get
			{
				return new Rect(this.ReportColumn.ActualLeft, this.ReportRow.ActualTop, this.ReportColumn.ActualWidth,
						   this.ReportRow.ActualHeight);
			}
		}

	}

}

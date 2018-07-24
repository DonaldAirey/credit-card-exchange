namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections.Generic;
    using System.Windows;
	using System.Windows.Media.Animation;
	using System.ComponentModel;

	/// <summary>
	/// Represents a virtual allocation of space used to display content in the report.
	/// </summary>
	public class ReportRow : Animatable
	{

		// Public Static Columns
		public static readonly DependencyProperty ActualHeightProperty;
		public static readonly DependencyProperty ActualTopProperty;
		public static readonly DependencyProperty HeightProperty;
		public static readonly DependencyProperty IsEvenProperty;
		public static readonly DependencyProperty OrdinalProperty;
		public static readonly DependencyProperty TopProperty;
		public static readonly DependencyProperty ZIndexProperty;

		// Private Instance Fields
		public IContent IContent;
		public Boolean IsActive;
		public Boolean IsObsolete;
		public Boolean IsSelected;

		// Public Events
		public event ReportCellPropertyChangedEventHandler CellChanged;

		// Public Instance Fields
		private Dictionary<ReportColumn, ReportCell> reportCellDictionary;

		/// <summary>
		/// Creates the static resources used by this row.
		/// </summary>
		static ReportRow()
		{

			// Actual Height Property
			ReportRow.ActualHeightProperty = DependencyProperty.Register("ActualHeight", typeof(Double), typeof(ReportRow));

			// Actual Top Property
			ReportRow.ActualTopProperty = DependencyProperty.Register("ActualTop", typeof(Double), typeof(ReportRow));

			// Height Property
			ReportRow.HeightProperty = DependencyProperty.Register("Height", typeof(Double), typeof(ReportRow));

			// IsEven Property
			ReportRow.IsEvenProperty = DependencyProperty.Register("IsEven", typeof(Boolean), typeof(ReportRow));

			// Ordinal Property
			ReportRow.OrdinalProperty = DependencyProperty.Register("Ordinal", typeof(Int32), typeof(ReportRow));

			// Top Property
			ReportRow.TopProperty = DependencyProperty.Register("Top", typeof(Double), typeof(ReportRow));

			// ZIndex Property
			ReportRow.ZIndexProperty = DependencyProperty.Register("ZIndex", typeof(Int32), typeof(ReportRow));

		}

		/// <summary>
		/// Creates a new tile for the virtual space of the report.
		/// </summary>
		/// <param name="iContent">The content of the tile.</param>
		/// <param name="rect">The virtual space occupied by the tile.</param>
		public ReportRow()
		{

			// Initialize the object.
			this.IsObsolete = false;
			this.reportCellDictionary = new Dictionary<ReportColumn, ReportCell>();

		}

		/// <summary>
		/// Gets or sets the height of the row.
		/// </summary>
		public Double ActualHeight
		{
			get { return (Double)this.GetValue(ReportRow.ActualHeightProperty); }
			set { this.SetValue(ReportRow.ActualHeightProperty, value); }
		}

		/// <summary>
		/// Gets or sets the left edge of the column.
		/// </summary>
		public Double ActualTop
		{
			get { return (Double)this.GetValue(ReportRow.ActualTopProperty); }
			set { this.SetValue(ReportRow.ActualTopProperty, value); }
		}

		/// <summary>
		/// The cells in the row, in no particular order.
		/// </summary>
		public IEnumerable<ReportCell> Cells
		{
			get { return this.reportCellDictionary.Values; }
		}

		/// <summary>
		/// Gets the bottom of the row.
		/// </summary>
		public Double Bottom
		{
			get { return this.Top + this.Height; }
		}

		/// <summary>
		/// Gets the report cell at the given column in the row.
		/// </summary>
		/// <param name="reportColumn"></param>
		/// <returns></returns>
		public ReportCell this[ReportColumn reportColumn]
		{
			get { return this.reportCellDictionary[reportColumn]; }
		}

		/// <summary>
		/// Gets or sets the width of the column.
		/// </summary>
		public Double Height
		{
			get { return (Double)this.GetValue(ReportRow.HeightProperty); }
			set {this.SetValue(ReportRow.HeightProperty, value);}
		}

		/// <summary>
		/// if the row has no cell values 
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return this.reportCellDictionary == null ||
					this.reportCellDictionary.Count == 0;
			}
		}

		/// <summary>
		/// Gets an indication of whether this row has an even or odd ordinal value.
		/// </summary>
		public bool IsEven
		{
			get { return (bool)this.GetValue(ReportRow.IsEvenProperty); }
		}

		/// <summary>
		/// Sets or sets a value indicating the current order of this row in the report.
		/// </summary>
		public Int32 Ordinal
		{
			get { return (Int32)this.GetValue(ReportRow.OrdinalProperty); }
			set
			{
				this.SetValue(ReportRow.IsEvenProperty, value % 2 == 0);
				this.SetValue(ReportRow.OrdinalProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the left edge of the column.
		/// </summary>
		public Double Top
		{
			get { return (Double)this.GetValue(ReportRow.TopProperty); }
			set { this.SetValue(ReportRow.TopProperty, value); }
		}

		/// <summary>
		/// Gets or sets the left edge of the column.
		/// </summary>
		public Int32 ZIndex
		{
			get { return (Int32)this.GetValue(ReportRow.ZIndexProperty); }
			set { this.SetValue(ReportRow.ZIndexProperty, value); }
		}

		public void Add(ReportColumn reportColumn, ReportCell reportCell)
		{
			reportCell.PropertyChanged += OnCellPropertyChanged;
			this.reportCellDictionary.Add(reportColumn, reportCell);
		}

		/// <summary>
		/// Create a freezable copy of the object.
		/// </summary>
		/// <returns>A new, freezable copy of the object.</returns>
		protected override Freezable CreateInstanceCore()
		{
			return new ReportRow();
		}

		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{

			if (this.CellChanged != null)
			{

				ReportCellPropertyChangedEventArgs eventArgs = new ReportCellPropertyChangedEventArgs(this, sender as ReportCell, e.PropertyName);
				this.CellChanged(this, eventArgs);

			}
			
		}

		public void Remove(ReportColumn reportColumn)
		{
			this.reportCellDictionary.Remove(reportColumn);
		}

		public Boolean TryGetValue(ReportColumn key, out ReportCell value)
		{
			return this.reportCellDictionary.TryGetValue(key, out value);
		}

		public Boolean ContainsKey(ReportColumn key)
		{
			return this.reportCellDictionary.ContainsKey(key);
		}

	}

}

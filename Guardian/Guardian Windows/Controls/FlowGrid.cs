namespace FluidTrade.Guardian.Windows.Controls
{

    using System;
	using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
	using System.Windows.Data;

    /// <summary>
    /// A FrameworkElement specific Collection.
    /// </summary>
    [ContentProperty("Cells")]
    public class FlowGridRow : DependencyObject, ICollection<FrameworkElement>
    {

        /// <summary>
        /// The identifier for the ColumnDefinitions dependency property.
        /// </summary>
        public static DependencyProperty ColumnDefinitionsProperty =
            DependencyProperty.Register("ColumnDefinitions", typeof(List<ColumnDefinition>), typeof(FlowGridRow));

        private ObservableCollection<FrameworkElement> cells = new ObservableCollection<FrameworkElement>();
		private List<Border> visualCells = new List<Border>();
        private double height = 0.0;

		/// <summary>
		/// Create a new FlowGridRow.
		/// </summary>
		public FlowGridRow()
		{

			this.cells.CollectionChanged += CellsChanged;

		}

        /// <summary>
        /// Gets the cells of the row.
        /// </summary>
        public ObservableCollection<FrameworkElement> Cells
        {

            get { return this.cells; }

        }

		/// <summary>
		/// The column definitions of for this row. This is used by FlowGrid.
		/// </summary>
		internal List<ColumnDefinition> ColumnDefinitions
		{

			get { return this.GetValue(FlowGridRow.ColumnDefinitionsProperty) as List<ColumnDefinition>; }
			set { this.SetValue(FlowGridRow.ColumnDefinitionsProperty, value); }

		}

		/// <summary>
		/// The number of filled cells.
		/// </summary>
		public int Count
		{

			get { return cells.Count; }

		}

		/// <summary>
		/// The desired height of the row as a whole.
		/// </summary>
		internal double Height
		{

			get { return this.height; }

		}

		/// <summary>
		/// The visual elements that should be used to display the contents of the row.
		/// </summary>
		internal List<Border> VisualCells
		{

			get { return this.visualCells; }

		}

		/// <summary>
		/// Get the element in a particular column.
		/// </summary>
		/// <param name="column">The index of the desired column.</param>
		/// <returns>The element in the desired column, or null if there is no element in that column.</returns>
		internal FrameworkElement this[int column]
		{

			get
			{

				if (this.Cells.Count > column)
					return this.visualCells[column];
				else
					return null;

			}
			set
			{

				this.Cells[column] = value;

			}

		}

        /// <summary>
        /// Add a new element to the next open cell.
        /// </summary>
        /// <param name="element">The ui element to add.</param>
        public void Add(FrameworkElement element)
        {

            this.Cells.Add(element);

        }

		private void CellsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArgs)
		{

			if (eventArgs.NewItems != null)
				foreach (FrameworkElement element in eventArgs.NewItems)
				{

					Border border = new Border();

					border.SetBinding(Border.VerticalAlignmentProperty, new Binding("VerticalAlignment") { Source = element });
					border.SetBinding(Border.HorizontalAlignmentProperty, new Binding("HorizontalAlignment") { Source = element });
					border.Child = element;

					this.visualCells.Add(border);

				}

			if (eventArgs.OldItems != null)
				foreach (FrameworkElement element in eventArgs.OldItems)
					this.visualCells.Remove(this.visualCells.FirstOrDefault(border => border.Child == element));

		}

        /// <summary>
        /// Empty all filled cells.
        /// </summary>
        public void Clear()
        {

            this.Cells.Clear();

        }

        /// <summary>
        /// Determine the largest width that could fit in a column.
        /// </summary>
        /// <param name="column">The index of the column.</param>
        /// <returns>The determined width.</returns>
        private double ColumnDefinitionMaxWidth(int column)
        {

            if (this.ColumnDefinitions[column].Width.IsAbsolute)
                return this.ColumnDefinitions[column].Width.Value;
            else
                return Double.MaxValue;

        }

        /// <summary>
        /// Determine the smallest width that could fit in a column.
        /// </summary>
        /// <param name="column">The index of the column.</param>
        /// <returns>The determined width.</returns>
        private double ColumnDefinitionMinWidth(int column)
        {

            if (this.Cells[column].DesiredSize.Width > 0)
                return this.Cells[column].DesiredSize.Width;
            else
                return this.ColumnDefinitions[column].MinWidth;

        }

        /// <summary>
        /// Determine the desired size of a particular column. This is used by FlowGrid.
        /// </summary>
        /// <param name="column">The index of the column to size up.</param>
        /// <returns>The desired size of the column.</returns>
        internal double ColumnWidth(int column)
        {

            if (this.VisualCells.Count > column)
            {

                Size size = new Size(this.ColumnDefinitionMaxWidth(column), Double.MaxValue);

                this[column].Measure(size);

                size = this[column].DesiredSize;
				if (size.Width < this.ColumnDefinitionMinWidth(column))
					size.Width = this.ColumnDefinitionMinWidth(column);

                return size.Width;

            }
            else
            {

                return 0.0;

            }

        }

        /// <summary>
        /// Determin whether an element is in one of the cells of the row.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True if the element is in the row, false otherwise.</returns>
        public Boolean Contains(FrameworkElement element)
        {

            return this.Cells.Contains(element);

        }

        /// <summary>
        /// Copy the contents of the row to an array.
        /// </summary>
        /// <param name="array">The array to copy into.</param>
        /// <param name="arrayIndex">The index at which copying begins.</param>
        public void CopyTo(FrameworkElement[] array, int arrayIndex)
        {

            this.Cells.CopyTo(array, arrayIndex);

        }

        /// <summary>
        /// Get an enumerator over all of the elements in the row.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<FrameworkElement> GetEnumerator()
        {

            return this.Cells.GetEnumerator();

        }

        /// <summary>
        /// Get an enumerator over all of the elements in the row.
        /// </summary>
        /// <returns>The enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {

            return this.Cells.GetEnumerator();

        }

        /// <summary>
        /// Get whether this row is readonly. Always false.
        /// </summary>
        public Boolean IsReadOnly
        {

            get { return false; }

        }

        /// <summary>
        /// Measure the elements in the row and determine the row's overall height.
        /// </summary>
        public void Measure()
        {

            this.height = 0.0;

            foreach (FrameworkElement element in this.VisualCells)
            {

				element.Visibility = System.Windows.Visibility.Visible;
                element.Measure(new Size(Double.MaxValue, Double.MaxValue));
                if (this.height < element.DesiredSize.Height) this.height = element.DesiredSize.Height;

            }

        }

        /// <summary>
        /// Remove an element from the row.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public Boolean Remove(FrameworkElement element)
        {

			if (this.Cells.Contains(element))
				this.visualCells.RemoveAt(this.Cells.IndexOf(element));

            return this.Cells.Remove(element);

        }

    }

    /// <summary>
    /// A grid whose rows wrap to the right when the available height is exceeded, similarly to WrapPanel.
    /// </summary>
    [ContentProperty("Rows")]
    public class FlowGrid : Canvas
    {

        // Private fields:
        private ObservableCollection<FlowGridRow> children = new ObservableCollection<FlowGridRow>();
        private List<ColumnDefinition> columnDefinitions = new List<ColumnDefinition>();

        /// <summary>
        /// Create a new FlowGrid.
        /// </summary>
        public FlowGrid()
        {

            this.Children.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnChildrenChanged);

        }

		/// <summary>
		/// Gets the children of the FlowGrid. Mostly, this is here for ContentProperty and Xaml, which retrieves the ContentProperty via reflection.
		/// This causes a problem (well, causes an exception and could cause a Problem in the future) because the Xaml framework relies on the order
		/// the properties it requests from the reflection framework to determine which property to use as the ContentProperty. So, this property,
		/// Rows, is here to unambiguously indicate the children of the FlowGrid for Xaml. We could just change the name of FlowGrid's Children property,
		/// but we want to hide the Canvas's Children property so users of FlowGrid can't mess with the layout directly.
		/// </summary>
		public ObservableCollection<FlowGridRow> Rows
		{

			get { return this.children; }

		}

		/// <summary>
        /// Gets the children of the FlowGrid. Each child is essentially a row in the grid, with each element in the row accupying its own column.
        /// </summary>
        public new ObservableCollection<FlowGridRow> Children
        {

            get { return this.children; }

        }

        /// <summary>
        /// Gets the column definitions for the FlowGrid.
        /// </summary>
        public List<ColumnDefinition> ColumnDefinitions
        {

            get { return this.columnDefinitions; }

        }

        /// <summary>
        /// Respond to additions to and removals from the Children collection.
        /// </summary>
        /// <param name="sender">The Children collection</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArgs)
        {

            if (eventArgs.NewItems != null)
                foreach (FlowGridRow row in eventArgs.NewItems)
                {

                    row.ColumnDefinitions = this.ColumnDefinitions;
                    foreach (FrameworkElement element in row.VisualCells)
                        base.Children.Add(element);

                }
            if (eventArgs.OldItems != null)
                foreach (FlowGridRow row in eventArgs.OldItems)
                {

                    row.ColumnDefinitions = null;
                    foreach (FrameworkElement element in row.VisualCells)
                        base.Children.Remove(element);

                }

            this.InvalidateArrange();

        }

        /// <summary>
        /// Determine the appropriate size for the FlowGrid control. Since the layout of the rows and columns is required to determine an appropriate
        /// size for the parent grid control, the child elements are arranged here as well.
        /// </summary>
        /// <param name="availableSize">The maximum available size for the control.</param>
        /// <returns>The total size necessary for the control.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {

            // Unfortunately, because we'll place our children based on our size, we need to literally arrange them in order to know how much space
            // they'll take up.
            this.ArrangeItems(availableSize);
            return base.MeasureOverride(availableSize);

        }

        /// <summary>
        /// Do the final layout for the panel.
        /// </summary>
        /// <param name="finalSize">The final size the flow grid needs to fit in.</param>
        /// <returns>The size we actually used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {

            base.ArrangeOverride(finalSize);

            return base.ArrangeOverride(finalSize);

        }

        /// <summary>
        /// Arrange the child elements in the parent grid based on the virtual FlowGrid column and row arrangement.
        /// </summary>
        /// <param name="size">The size available to the control.</param>
        private void ArrangeItems(Size size)
        {

            List<FlowGridRow> rows = new List<FlowGridRow>(this.Children);
            // columnStop is essentially the horizontal offset for a vertical set of rows.
            double columnStop = 0.0;
			Visibility visibility = System.Windows.Visibility.Visible;

            while (rows.Count > 0 && columnStop < size.Width)
            {

                // The column is literally the column of rows we're going to layout at this columnStop in the vertical space we have.
                List<FlowGridRow> column = this.GetColumn(rows, size.Height);
                double height = 0.0;
                double[] widths = new double[this.ColumnDefinitions.Count + 1];
				double totalWidth = 0;

				// Calculate how big everything in the column is.
                foreach (FlowGridRow row in column)
                {

                    for (int element = 0; element < row.Count && element < this.ColumnDefinitions.Count; ++element)
                    {

                        double width = row.ColumnWidth(element);

                        if (widths[element + 1] < width)
                            widths[element + 1] = width;

                    }

                }

				// Tally up the width of the column.
                foreach (double width in widths)
					totalWidth += width;

				// If the column won't fit in the available space, it, and everything after it, shouldn't get displayed.
				if (columnStop + totalWidth >= size.Width)
					visibility = System.Windows.Visibility.Collapsed;

				// Afix each of the elements in the column to their the correct places on the canvas.
                foreach (FlowGridRow row in column)
                {

                    for (int element = 0; element < row.Count && element < this.ColumnDefinitions.Count; ++element)
                    {

                        FlowGrid.SetLeft(row[element], GetLeft(row[element], columnStop + widths[element], widths[element + 1]));
                        FlowGrid.SetTop(row[element], GetTop(row[element], height, row.Height));
						row[element].Visibility = visibility;

                    }

                    height += row.Height;

                }

				columnStop += totalWidth;

            }

			// Any rows left over couldn't be placed for some reason or another, so they shouldn't be displayed at all.
			foreach (FlowGridRow row in rows)
				foreach (FrameworkElement element in row.VisualCells)
					element.Visibility = System.Windows.Visibility.Collapsed;

        }

		/// <summary>
		/// Get the rows that should go in the next visual column.
		/// </summary>
		/// <param name="rows">The rows left to arrange.</param>
		/// <param name="maxHeight">The height of the FlowGrid.</param>
		/// <returns>The largest set of rows that firsts within the height.</returns>
        private List<FlowGridRow> GetColumn(List<FlowGridRow> rows, double maxHeight)
        {

            List<FlowGridRow> column = new List<FlowGridRow>();
            double height = 0;
            double nextHeight = 0;

            rows[0].Measure();
            nextHeight = rows[0].Height;

            do
            {

                height += nextHeight;
                column.Add(rows[0]);
                rows.Remove(rows[0]);

                if (rows.Count > 0)
                {

                    rows[0].Measure();
                    nextHeight = rows[0].Height;

                }

            } while (rows.Count > 0 && height + nextHeight < maxHeight);

            return column;

        }

		/// <summary>
		/// Get the final size Rect of a child element.
		/// </summary>
		/// <param name="element">The element to measure.</param>
		/// <returns>The calculated size Rect.</returns>
        private Rect GetFinalRect(FrameworkElement element)
        {

            double top = FlowGrid.GetTop(element);
            double bottom = FlowGrid.GetBottom(element);
            double left = FlowGrid.GetLeft(element);
            double right = FlowGrid.GetRight(element);

            return new Rect(left, top, right - left, bottom - top);

        }

		/// <summary>
		/// Get the left-hand edge of a child element.
		/// </summary>
		/// <param name="element">The child element.</param>
		/// <param name="leftEdge">The left edge of the current column.</param>
		/// <param name="width">The width allowed to the control.</param>
		/// <returns>The position of the left edge of the control.</returns>
        private double GetLeft(FrameworkElement element, double leftEdge, double width)
        {

            double left = leftEdge;
            System.Windows.HorizontalAlignment alignment = element.HorizontalAlignment;
            Size size = element.DesiredSize;

            if (alignment == System.Windows.HorizontalAlignment.Right)
                left += width - size.Width;
            else if (alignment == System.Windows.HorizontalAlignment.Center)
                left += (width - size.Width) / 2;

            return left;

        }

		/// <summary>
		/// Get the right-hand edge of a child element.
		/// </summary>
		/// <param name="element">The child element.</param>
		/// <param name="leftEdge">The left edge of the current column.</param>
		/// <param name="width">The width allowed to the control.</param>
		/// <returns>The position of the right edge of the control.</returns>
        private double GetRight(FrameworkElement element, double leftEdge, double width)
        {

            double right = leftEdge + width;
            System.Windows.HorizontalAlignment alignment = element.HorizontalAlignment;
            Size size = element.DesiredSize;

            if (alignment == System.Windows.HorizontalAlignment.Left)
                right -= width - size.Width;
            else if (alignment == System.Windows.HorizontalAlignment.Center)
                right -= (width - size.Width) / 2;

            return right;

        }

		/// <summary>
		/// Get the top edge of a child element.
		/// </summary>
		/// <param name="element">The child element.</param>
		/// <param name="topEdge">The top edge of the column.</param>
		/// <param name="height">The height allowed to the control.</param>
		/// <returns>The position of the top edge of the control.</returns>
        private double GetTop(FrameworkElement element, double topEdge, double height)
        {

            double top = topEdge;
            System.Windows.VerticalAlignment alignment = element.VerticalAlignment;
            Size size = element.DesiredSize;

            if (alignment == System.Windows.VerticalAlignment.Bottom)
                top += height - size.Height;
            else if (alignment == System.Windows.VerticalAlignment.Center)
                top += (height - size.Height) / 2;

            return top;

        }

		/// <summary>
		/// Get the bottom edge of a child element.
		/// </summary>
		/// <param name="element">The child element.</param>
		/// <param name="topEdge">The bottom edge of the column.</param>
		/// <param name="height">The height allowed to the control.</param>
		/// <returns>The position of the bottom edge of the control.</returns>
		private double GetBottom(FrameworkElement element, double topEdge, double height)
        {

            double bottom = topEdge + height;
            System.Windows.VerticalAlignment alignment = element.VerticalAlignment;
            Size size = element.DesiredSize;

            if (alignment == System.Windows.VerticalAlignment.Top)
                bottom -= height - size.Height;
            else if (alignment == System.Windows.VerticalAlignment.Center)
                bottom -= (height - size.Height) / 2;

            return bottom;

        }

    }

}

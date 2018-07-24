namespace FluidTrade.Core.Windows.Controls
{

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Shapes;

	/// <summary>
	/// An indicator to the user where a column will be located after it is moved.
	/// </summary>
	/// <copyright>Copyright © 2006 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class DestinationPopup : Popup
	{

		// Private Constants
		private const System.Double arrowHeight = 9.0;
		private const System.Double arrowWidth = 9.0;

		// Private Instance Members
		private System.Double targetDistance;
		private System.Windows.Controls.Orientation orientation;
		private System.Windows.Media.ScaleTransform scaleTransform;
		private System.Windows.Shapes.Path path;

		/// <summary>
		/// Create the cursor used to indicate the destination of a column drag-and-drop operation.
		/// </summary>
		/// <param name="headerHeight"></param>
		public DestinationPopup()
		{

			// Initialize the object.
			this.AllowsTransparency = true;
			this.Placement = PlacementMode.Relative;

		}

		/// <summary>
		/// Generate the glyphs that point out the location of the destination.
		/// </summary>
		private void GenerateGlyphs()
		{

			// The destination is marked with a set of red arrows.
			this.path = new Path();
			path.Fill = Brushes.Red;

			// A pair of paths is used to make the arrows that point to the destination.
			PathGeometry pathGeometry = new PathGeometry();

			// This creates a set of arrows for a vertical destination.
			if (this.orientation == Orientation.Vertical)
			{

				// The total height of the control is fixed by the size of the indicator arrows and the gap between them.
				this.Height = this.targetDistance + DestinationPopup.arrowHeight * 2.0;

				// The  name of the constants that describe the arrows has little bearing on the actual dimension on which they 
				// are used.  Rather they describe an abstract arrow that is drawn according to the orientation.
				double arrowHeight = DestinationPopup.arrowHeight;
				double arrowWidth = DestinationPopup.arrowWidth;

				// This is the top arrow of the destination indicator.
				PathFigure topArrow = new PathFigure();
				topArrow.IsClosed = true;
				topArrow.StartPoint = new Point(arrowWidth / 3.0, 0.0);
				topArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 1.5, 0.0), true));
				topArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 1.5, arrowHeight / 2.25), true));
				topArrow.Segments.Add(new LineSegment(new Point(arrowWidth, arrowHeight / 2.25), true));
				topArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 2.0, arrowHeight), true));
				topArrow.Segments.Add(new LineSegment(new Point(0.0, arrowHeight / 2.25), true));
				topArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 3.0, arrowHeight / 2.25), true));
				pathGeometry.Figures.Add(topArrow);

				// This is the bottom arrow of the destination indicator.
				PathFigure bottomArrow = new PathFigure();
				double totalHeight = this.targetDistance + arrowHeight * 2.0;
				bottomArrow.IsClosed = true;
				bottomArrow.StartPoint = new Point(arrowWidth / 3.0, totalHeight);
				bottomArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 1.5, totalHeight), true));
				bottomArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 1.5, totalHeight - arrowHeight / 2.25), true));
				bottomArrow.Segments.Add(new LineSegment(new Point(arrowWidth, totalHeight - arrowHeight / 2.25), true));
				bottomArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 2.0, totalHeight - arrowHeight), true));
				bottomArrow.Segments.Add(new LineSegment(new Point(0.0, totalHeight - arrowHeight / 2.25), true));
				bottomArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 3.0, totalHeight - arrowHeight / 2.25), true));
				pathGeometry.Figures.Add(bottomArrow);


			}

			// This creates a set of arrows for a horizontal destination.
			if (this.orientation == Orientation.Horizontal)
			{

				// The total height of the control is fixed by the size of the indicator arrows and the gap between them.
				this.Width = this.targetDistance + DestinationPopup.arrowWidth * 2.0;

				// The  name of the constants that describe the arrows has little bearing on the actual dimension on which they 
				// are used.  Rather they describe an abstract arrow that is drawn according to the orientation.
				double arrowHeight = DestinationPopup.arrowWidth;
				double arrowWidth = DestinationPopup.arrowHeight;

				// This is the left arrow of the destination indicator.
				PathFigure leftArrow = new PathFigure();
				leftArrow.IsClosed = true;
				leftArrow.StartPoint = new Point(0.0, arrowHeight / 3.0);
				leftArrow.Segments.Add(new LineSegment(new Point(0.0, arrowHeight / 1.5), true));
				leftArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 2.25, arrowHeight / 1.5), true));
				leftArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 2.25, arrowHeight), true));
				leftArrow.Segments.Add(new LineSegment(new Point(arrowWidth, arrowHeight / 2.0), true));
				leftArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 2.25, 0.0), true));
				leftArrow.Segments.Add(new LineSegment(new Point(arrowWidth / 2.25, arrowHeight / 3.0), true));
				pathGeometry.Figures.Add(leftArrow);

				// This is the bottom arrow of the destination indicator.
				PathFigure rightArrow = new PathFigure();
				double totalWidth = this.targetDistance + arrowWidth * 2.0;
				rightArrow.IsClosed = true;
				rightArrow.StartPoint = new Point(totalWidth, arrowHeight / 3.0);
				rightArrow.Segments.Add(new LineSegment(new Point(totalWidth, arrowHeight / 1.5), true));
				rightArrow.Segments.Add(new LineSegment(new Point(totalWidth - arrowWidth / 2.25, arrowHeight / 1.5), true));
				rightArrow.Segments.Add(new LineSegment(new Point(totalWidth - arrowWidth / 2.25, arrowHeight), true));
				rightArrow.Segments.Add(new LineSegment(new Point(totalWidth - arrowWidth, arrowHeight / 2.0), true));
				rightArrow.Segments.Add(new LineSegment(new Point(totalWidth - arrowWidth / 2.25, 0.0), true));
				rightArrow.Segments.Add(new LineSegment(new Point(totalWidth - arrowWidth / 2.25, arrowHeight / 3.0), true));
				pathGeometry.Figures.Add(rightArrow);

			}

			// The path contains the two distinct geometries created above: an arrow above pointing down and an arrow below
			// pointing up.
			path.Data = pathGeometry;

			// Because I haven't found a way to control the relative order of the Popup windows, the destination Popup is open for
			// the same amount of time as the column Popup (the image that indicates which column is being moved).  Opening the two
			// Popups allow some amount of control of the relative Z order.  Making the interior Path visible and invisible allows
			// for the indicator to disappear when there is no destination.  If it were possible to set the Z order of the controls
			// ahead of time, then this function would be replaced by manipulating the 'IsOpen' indicator.
			path.Visibility = this.IsOpen ? Visibility.Visible : Visibility.Hidden;

			// The contents of the Popup is the path containing the red arrows.
			this.Child = path;

		}

		/// <summary>
		/// Gets or sets the orienation of the destination arrows.
		/// </summary>
		public Orientation Orientation
		{

			get { return this.orientation; }

			set
			{

				// This prevents redundant regeneration of the glyphs.
				if (this.orientation != value)
				{

					this.orientation = value;

					// This will adjust the size of the popup window for a vertical destination cursor.
					if (this.orientation == Orientation.Vertical)
					{
						this.Width = DestinationPopup.arrowWidth;
						this.Height = DestinationPopup.arrowHeight * 2.0;
					}

					// This will adjust the size of the popup window for a horizontal destination cursor.
					if (this.orientation == Orientation.Horizontal)
					{
						this.Width = DestinationPopup.arrowWidth * 2.0;
						this.Height = DestinationPopup.arrowHeight;
					}

					// Regenerate the glyphs with the new orientation.
					GenerateGlyphs();

				}

			}

		}

		/// <summary>
		/// Gets or sets the magnification for this Popup indicator.
		/// </summary>
		public double Scale
		{

			get { return this.scaleTransform.ScaleX; }
			set
			{
				this.scaleTransform = new ScaleTransform(value, value);
				this.RenderTransform = this.scaleTransform;
			}

		}

		/// <summary>
		/// Gets or sets the visibility of the contents of the Popup indicator.
		/// </summary>
		public new Visibility Visibility
		{
			get { return this.path.Visibility; }
			set { this.path.Visibility = value; }
		}

		/// <summary>
		/// Gets or sets the distance between the arrows in this Popup indicator.
		/// </summary>
		public double TargetDistance
		{

			get { return this.targetDistance; }

			set
			{

				// The target height is the distance between the two red arrows.  It is generally the height of the header row and 
				// the arrows appear to straddle the header row.
				if (this.targetDistance != value)
				{

					// Save the target height for the 'get' operation.
					this.targetDistance = value;

					// The glyphs need to be regenerated when the distance (width or height) of the target has changed.
					GenerateGlyphs();

				}

			}

		}

		/// <summary>
		/// Gets or sets the horizontal offset to the indicator arrows.
		/// </summary>
		public new double HorizontalOffset
		{
			set
			{
				if (this.orientation == Orientation.Vertical)
					base.HorizontalOffset = Math.Floor(value - DestinationPopup.arrowWidth / 2.0);
				if (this.orientation == Orientation.Horizontal)
					base.HorizontalOffset = value - DestinationPopup.arrowWidth;
			}
		}

		/// <summary>
		/// Gets or sets the vertical offset to the indicator arrows.
		/// </summary>
		public new double VerticalOffset
		{
			set
			{
				if (this.orientation == Orientation.Vertical)
					base.VerticalOffset = value - DestinationPopup.arrowHeight;
				if (this.orientation == Orientation.Horizontal)
					base.VerticalOffset = Math.Floor(value - DestinationPopup.arrowHeight / 2.0);
			}
		}

	}

}

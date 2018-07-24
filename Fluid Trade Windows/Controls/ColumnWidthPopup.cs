namespace FluidTrade.Core.Windows.Controls
{

    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

	class ColumnWidthPopup : Popup
	{

		// Private Instance Fields
		private System.Windows.Controls.TextBlock textBlock;
		private System.Double columnWidth;

		public ColumnWidthPopup()
		{

			this.Placement = PlacementMode.Top;
			this.AllowsTransparency = true;
			this.IsOpen = false;
			this.Visibility = Visibility.Hidden;

			// The Popup contains a border object which, in turn, holds a text block which describes how wide the column will be.
			Border border = new Border();
			border.SnapsToDevicePixels = true;
			border.Background = new LinearGradientBrush(Colors.White, Color.FromRgb(206, 221, 240), 90.0);
			border.BorderBrush = Brushes.Black;
			border.CornerRadius = new CornerRadius(2);
			border.BorderThickness = new Thickness(1);
			this.Child = border;

			// This text block contains the description of the column width as it is being resized.
			this.textBlock = new TextBlock();
			this.textBlock.Margin = new Thickness(5, 0, 5, 0);
			border.Child = this.textBlock;

		}

		public double Content
		{

			get { return this.columnWidth; }
			set
			{

				this.columnWidth = value < 0.0 ? 0.0 : value;
				double excelUnits = columnWidth > 12.0 ? (columnWidth - 5.0) / 7.0 : columnWidth / 12.0;
				this.textBlock.Text = String.Format("Width: {0:0.00} ({1:0} pixels)", excelUnits, columnWidth);

			}

		}

	}

}

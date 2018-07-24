namespace FluidTrade.Guardian.Windows.Controls
{

    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
	using System.Threading;
	using System.ComponentModel;

    /// <summary>
    /// An explorer-like status area, with a set style, an entity icon and information, and an area for including type-specific information.
    /// </summary>
    [ContentProperty("Children")]
    public class StatusBar : System.Windows.Controls.Primitives.StatusBar
    {

		/// <summary>
        /// The type-specific children dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildrenProperty;
        /// <summary>
        /// The 'extra' control dependency property.
        /// </summary>
		public static readonly DependencyProperty ExtraProperty;
		/// <summary>
		/// The Icon dependency property.
		/// </summary>
		public static readonly DependencyProperty IconProperty;

        private Grid parentGrid;
        private FlowGrid flowGrid;

        static StatusBar()
        {

            StatusBar.ChildrenProperty = DependencyProperty.Register("Children", typeof(ObservableCollection<FlowGridRow>), typeof(StatusBar));
            StatusBar.ExtraProperty = DependencyProperty.Register("Extra", typeof(FrameworkElement), typeof(StatusBar), new PropertyMetadata(OnExtraChanged));
			StatusBar.IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(StatusBar));

        }

        /// <summary>
        /// Initialize a new status bar.
        /// </summary>
        public StatusBar()
        {

            ControlTemplate template = new ControlTemplate();
            FrameworkElementFactory  templateGrid = new FrameworkElementFactory (typeof(Grid));
            LinearGradientBrush backgroundBrush = new LinearGradientBrush() { StartPoint = new Point(0.0, 0.5), EndPoint = new Point(1.0, 0.5) };
            FrameworkElementFactory itemPresenter = new FrameworkElementFactory(typeof(ItemsPresenter));
            StatusBarItem imageItem = new StatusBarItem();
			Image image = new Image();
			Border imageBorder = new Border()
			{
				MaxHeight = 248,
				MaxWidth = 248,
				MinHeight = 40,
				MinWidth = 40,
				Margin = new Thickness(1, 0, 0, 0)
			};
            StatusBarItem flowItem = new StatusBarItem() {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                Margin = new Thickness(0, 4, 0, 0) };
            Image ribbon = new Image() {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Source = new BitmapImage(new Uri(@"..\Resources\Ribbon.png", UriKind.Relative))};
            Grid panel = new Grid();

            // Set the visual template for the status bar.
            backgroundBrush.GradientStops.Add(new GradientStop() { Color = Color.FromRgb(0xF3, 0xFB, 0xFE), Offset = 0.0 });
            backgroundBrush.GradientStops.Add(new GradientStop() { Color = Color.FromRgb(0xF1, 0xFB, 0xFE), Offset = 0.25 });
            backgroundBrush.GradientStops.Add(new GradientStop() { Color = Color.FromRgb(0xEE, 0xFB, 0xFE), Offset = 0.5 });
            backgroundBrush.GradientStops.Add(new GradientStop() { Color = Color.FromRgb(0xEB, 0xFA, 0xFD), Offset = 0.75 });
            backgroundBrush.GradientStops.Add(new GradientStop() { Color = Color.FromRgb(0xBB, 0xD9, 0xF0), Offset = 1.0 });
            templateGrid.AppendChild(itemPresenter);
            templateGrid.SetValue(Control.BackgroundProperty, backgroundBrush);
            template.VisualTree = templateGrid;
            base.Template = template;

            // Add the icon.
			image.SetBinding(Image.SourceProperty, new Binding("Icon") { Source = this });
            imageBorder.Child = image;
			//imageBorder.Padding = new Thickness(0, 0, 3, 0);
            imageItem.Content = imageBorder;
			imageItem.Margin = new Thickness(0,0,0,-1);
            base.AddChild(imageItem);

			panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(300, GridUnitType.Star) });
			panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Build the flow grid where all the information goes.
            this.flowGrid = new FlowGrid() {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                Margin = new Thickness(1, 0, 0, 10),
				SnapsToDevicePixels = true };
			this.flowGrid.SetValue(Grid.ColumnProperty, 0);
            this.flowGrid.ColumnDefinitions.Add(new ColumnDefinition());
            this.flowGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 80 });
			this.flowGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			panel.Children.Add(this.flowGrid);

            // Build the ribbon/Extra area.
			ribbon.SetValue(Panel.ZIndexProperty, -1);
			ribbon.SetValue(Grid.ColumnSpanProperty, 2);
			ribbon.SetValue(Grid.ColumnProperty, 0);
            panel.Children.Add(ribbon);
			this.parentGrid = new Grid();
			this.parentGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
			this.parentGrid.SetValue(Grid.ColumnProperty, 1);
            panel.Children.Add(this.parentGrid);

            flowItem.Content = panel;
            base.AddChild(flowItem);

            this.Children = this.flowGrid.Children;

        }

        /// <summary>
        /// The entity specific information. Each collection in Children is a row in the two-column FlowGrid of the status bar's center area.
        /// </summary>
        public ObservableCollection<FlowGridRow> Children
        {

            get { return this.GetValue(StatusBar.ChildrenProperty) as ObservableCollection<FlowGridRow>; }
            set { this.SetValue(StatusBar.ChildrenProperty, value); }

        }

        /// <summary>
        /// The element that is placed in the right side of status bar, over the "ribbon" image.
        /// </summary>
        public FrameworkElement Extra
        {

            get { return this.GetValue(StatusBar.ExtraProperty) as FrameworkElement; }
            set { this.SetValue(StatusBar.ExtraProperty, value); }

        }

		/// <summary>
		/// The icon on the left of the status bar.
		/// </summary>
		public ImageSource Icon
		{

			get { return this.GetValue(StatusBar.IconProperty) as ImageSource; }
			set { this.SetValue(StatusBar.IconProperty, value); }

		}

		/// <summary>
        /// Handle the changing of the Extra element. This places the new element over the ribbon icon, and removes the old value.
        /// </summary>
        private static void OnExtraChanged(DependencyObject statusBar, DependencyPropertyChangedEventArgs eventArgs)
        {

            FrameworkElement extra = statusBar.GetValue(StatusBar.ExtraProperty) as FrameworkElement;

            extra.SetValue(Panel.ZIndexProperty, 1);
            (statusBar as StatusBar).parentGrid.Children.Add(extra);
            (statusBar as StatusBar).parentGrid.Children.Remove(eventArgs.OldValue as UIElement);

        }

    }

}

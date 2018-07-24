namespace FluidTrade.Actipro 
{

    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Threading;

    using ActiproSoftware.Windows.Controls.Navigation;
    using System.Diagnostics;
    using ActiproSoftware.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using ActiproSoftware.Windows.Data;

	/// <summary>
	/// Provides wrapper for Actipro Breadcrumb control.
	/// </summary>
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ContentControl))]
    public class FolderNavBar : System.Windows.Controls.UserControl 
    {

        /// <summary>
        /// Identifies the MarkThree.Guardian.FolderTreeView.MaxHistoryCount property.
        /// </summary>
        public static readonly DependencyProperty MaxHistoryCountProperty;
        /// <summary>
        /// Identifies the MarkThree.Guardian.FolderTreeView.IsEditing property.
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty;
        /// <summary>
        /// Identifies the MarkThree.Guardian.FolderTreeView.RootNode property.
        /// </summary>
        public static readonly DependencyProperty RootNodeProperty;
        /// <summary>
        /// Identifies the MarkThree.Guardian.FolderTreeView.SelectedValue property.
        /// </summary>
        public static readonly DependencyProperty SelectedValueProperty;
        /// <summary>
        /// Routed event triggered when the there is a change in the nav.
        /// </summary>
        public static readonly RoutedEvent NodeSelectionChangedEvent;

        private Breadcrumb breadcrumb;
        private ObservableCollection<FolderNavNode> history;

		/// <summary>
		/// Create the static resources required by this class.
		/// </summary>
        static FolderNavBar()
		{

            FolderNavBar.MaxHistoryCountProperty = DependencyProperty.Register(
                "MaxHistoryCount",
                typeof(int),
                typeof(FolderNavBar),
                new PropertyMetadata(0, OnMaxHistoryCountChanged));
            FolderNavBar.IsEditingProperty = DependencyProperty.Register(
                "IsEditing",
                typeof(bool),
                typeof(FolderNavBar));
            FolderNavBar.RootNodeProperty = DependencyProperty.Register(
                "RootNode",
                typeof(FolderNavNode),
                typeof(FolderNavBar));
            FolderNavBar.SelectedValueProperty = DependencyProperty.Register(
				"SelectedValue",
				typeof(FolderNavNode),
				typeof(FolderNavBar),
				new FrameworkPropertyMetadata(OnSelectedValueChanged));

            FolderNavBar.NodeSelectionChangedEvent = EventManager.RegisterRoutedEvent(
                "NodeSelectionChanged",
                RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), 
                typeof(FolderNavBar));

		}


		/// <summary>
		/// Initializes an instance of the <c>FolderNavBar</c> class.
		/// </summary>
		public FolderNavBar() {

            PopupButton popupButton = new PopupButton() {
                    DisplayMode = PopupButtonDisplayMode.ButtonOnly,
                    IsRounded = false,
                    IsTransparencyModeEnabled = true
                };
            Image refresh = new Image() {
                    Source =  new BitmapImage(new Uri("Resources/Refresh16.png", UriKind.Relative)),
                    Stretch = Stretch.None,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
            MultiBinding tooltip = new MultiBinding() { Converter = new StringFormatConverter(), ConverterParameter = "{}Refresh {0}" };
            PathConverter pathConverter = new PathConverter();
            FolderIconConverter imageConverter = new FolderIconConverter();
            FolderMenuIconConverter imageMenuConverter = new FolderMenuIconConverter();

            this.breadcrumb = new Breadcrumb();

            this.breadcrumb.Margin = new Thickness(0);
            this.breadcrumb.MenuVerticalOffset = -2;
            this.breadcrumb.AutoMinimizeItemCount = 0;
            this.breadcrumb.ConvertItem += OnBreadcrumbConvertItem;
            this.breadcrumb.ImageMinWidth = 16;

            this.breadcrumb.SetBinding(Breadcrumb.RootItemProperty, new Binding() { Source = this, Path = new PropertyPath("RootNode") });

            this.breadcrumb.ItemContainerStyle = CreateRootBreadcrumbItemStyle(imageConverter);
            this.breadcrumb.ItemTemplate = CreateBreadcrumbItemTemplate();
            this.breadcrumb.MenuItemTemplate = CreateMenuItemNormalTemplate();
            this.breadcrumb.MenuItemContainerStyle = CreateMenuItemContainerStyle(imageMenuConverter);
            this.breadcrumb.MenuItemExpandedTemplate = CreateMenuItemExpandedTemplate();
            this.breadcrumb.ComboBoxItemTemplate = CreateComboBoxItemTemplate(pathConverter);

            this.breadcrumb.SetBinding(Control.BackgroundProperty, new Binding("Background") { Source = this, Mode = BindingMode.TwoWay });
            this.breadcrumb.SetBinding(Control.BorderThicknessProperty, new Binding("BorderThickness") { Source = this, Mode = BindingMode.TwoWay });
            this.breadcrumb.SetBinding(Control.BorderBrushProperty, new Binding("BorderBrush") { Source = this, Mode = BindingMode.TwoWay });

            tooltip.Bindings.Add(new Binding() {
                    Source = this.breadcrumb,
                    Path = new PropertyPath("SelectedItem.Entity.Name"),
                    Mode = BindingMode.OneWay });
            popupButton.Content = refresh;
            this.breadcrumb.ActionButtons.Add(popupButton);

            this.Content = this.breadcrumb;

            this.history = new ObservableCollection<FolderNavNode>();
            this.breadcrumb.ComboBoxItemsSource = history;
            this.breadcrumb.SelectedItemChanged += OnSelectedItemChanged;

            this.SetBinding(FolderNavBar.IsEditingProperty, new Binding("IsEditing") { Source = this.breadcrumb });

            if (pathConverter != null)
                pathConverter.SetBinding(PathConverter.RootProperty, new Binding("RootNode") { Source = this, Mode=BindingMode.OneWay });

		}

        /// <summary>
        /// Create the template for the items in the history drop-down.
        /// </summary>
        private DataTemplate CreateComboBoxItemTemplate(PathConverter pathConverter)
        {

            DataTemplate navComboBoxItemTemplate = new DataTemplate();
            FrameworkElementFactory comboBoxItemPanel = new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory comboBoxItemImage = new FrameworkElementFactory(typeof(Image));
            FrameworkElementFactory comboBoxItemText = new FrameworkElementFactory(typeof(TextBlock));

            comboBoxItemImage.SetValue(Image.WidthProperty, 16.0);
            comboBoxItemImage.SetValue(Image.HeightProperty, 16.0);
            comboBoxItemImage.SetBinding(Image.SourceProperty, new Binding("Entity.ImageSource"));
            comboBoxItemPanel.AppendChild(comboBoxItemImage);
            comboBoxItemText.SetBinding(TextBlock.TextProperty, new Binding() { Converter = pathConverter });
            comboBoxItemPanel.AppendChild(comboBoxItemText);
            comboBoxItemPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            navComboBoxItemTemplate.VisualTree = comboBoxItemPanel;

            return navComboBoxItemTemplate;

        }

        /// <summary>
        /// Create the template for the items in the breadcrumb display.
        /// </summary>
        private DataTemplate CreateBreadcrumbItemTemplate()
        {

            DataTemplate navBreadcrumbItemTemplate = new DataTemplate();
            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));

            text.SetBinding(TextBlock.TextProperty, new Binding("Entity.Name"));
            text.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            text.SetValue(TextBlock.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
            navBreadcrumbItemTemplate.VisualTree = text;

            return navBreadcrumbItemTemplate;

        }

        /// <summary>
        /// Create the template for the items in the navigation drop-down.
        /// </summary>
        private DataTemplate CreateMenuItemNormalTemplate()
        {

            DataTemplate navMenuItemNormalTemplate = new DataTemplate();
            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));

            text.SetBinding(TextBlock.TextProperty, new Binding("Entity.Name"));
            text.SetValue(TextBlock.MarginProperty, new Thickness(2, 0, 0, 0));
            navMenuItemNormalTemplate.VisualTree = text;

            return navMenuItemNormalTemplate ;

        }

        /// <summary>
        /// Create the template for the expanded item in the navigation drop-down.
        /// </summary>
        private DataTemplate CreateMenuItemExpandedTemplate()
        {

            DataTemplate navMenuItemExpandedTemplate = new DataTemplate();
            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));

            text.SetBinding(TextBlock.TextProperty, new Binding("Entity.Name"));
            text.SetValue(TextBlock.MarginProperty, new Thickness(2, 0, 0, 0));
            text.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            navMenuItemExpandedTemplate.VisualTree = text;

            return navMenuItemExpandedTemplate ;

        }

        /// <summary>
        /// Create the base style for the all breadcrumb items.
        /// </summary>
        private Style CreateCommonBreadcrumbItemStyle(FolderIconConverter imageConverter)
        {

            Style breadcrumbItemStyle = new Style() { TargetType = typeof(BreadcrumbItem) };
            Setter imageSource = new Setter() {
                    Property = BreadcrumbItem.ImageSourceProperty,
                    Value = new Binding("Entity.ImageData") { Converter = imageConverter }
                };

            breadcrumbItemStyle.Setters.Add(imageSource);

            return breadcrumbItemStyle;

        }

        /// <summary>
        /// Create the style for breadcrumb items.
        /// </summary>
        private Style CreateBreadcrumbItemStyle(FolderIconConverter imageConverter)
        {

            Style breadcrumbItemStyle = CreateCommonBreadcrumbItemStyle(imageConverter);
            Setter itemsSource = new Setter() {
                    Property = BreadcrumbItem.ItemsSourceProperty,
                    Value = new Binding("Children")
                };

            breadcrumbItemStyle.Setters.Add(itemsSource);

            return breadcrumbItemStyle;

        }

        /// <summary>
        /// Create the style for breadcrumb items.
        /// </summary>
        private Style CreateRootBreadcrumbItemStyle(FolderIconConverter imageConverter)
        {

            Style breadcrumbItemStyle = CreateCommonBreadcrumbItemStyle(imageConverter);
            Setter itemsSource = new Setter() {
                    Property = BreadcrumbItem.ItemsSourceProperty,
                    Value = new Binding("Children")
                };
            Setter itemContainerStyle = new Setter() {
                    Property = BreadcrumbItem.ItemContainerStyleProperty,
                    Value = CreateBreadcrumbItemStyle(imageConverter)
                };

            breadcrumbItemStyle.Setters.Add(itemsSource);
            breadcrumbItemStyle.Setters.Add(itemContainerStyle);

            return breadcrumbItemStyle;

        }

        /// <summary>
        /// Create the style for the navigation menu items.
        /// </summary>
        private Style CreateMenuItemContainerStyle(FolderMenuIconConverter imageMenuConverter)
        {

            Style menuItemContainerStyle = new Style() { TargetType = typeof(MenuItem) };
            Setter icon = new Setter() {
                    Property = MenuItem.IconProperty,
                    Value = new Binding("Entity.ImageSource") { Converter = imageMenuConverter }
                };

            menuItemContainerStyle.Setters.Add(icon);

            return menuItemContainerStyle;

        }

        /// <summary>
        /// True if path is being edited, False otherwise.
        /// </summary>
        public bool IsEditing
        {
            get { return (bool)this.GetValue(FolderNavBar.IsEditingProperty); }
            set { this.SetValue(FolderNavBar.IsEditingProperty, value); }
        }

        /// <summary>
        /// The length of the breadcrumb history.
        /// </summary>
        public int MaxHistoryCount
        {
            get { return (int)this.GetValue(FolderNavBar.MaxHistoryCountProperty); }
            set { this.SetValue(FolderNavBar.MaxHistoryCountProperty, value); }
        }

        /// <summary>
        /// CLR accesors for the event
        /// </summary>
        public event RoutedEventHandler NodeSelectionChanged
        {
            add { AddHandler(NodeSelectionChangedEvent, value); }
            remove { RemoveHandler(NodeSelectionChangedEvent, value); }
        }

        /// <summary>
        /// Fill in our parts of the template when it changes.
        /// </summary>
        public override void OnApplyTemplate()
        {

            base.OnApplyTemplate();

            ContentControl content = base.GetTemplateChild("PART_ContentHost") as ContentControl;

            if (content != null)
                content.Content = this.breadcrumb;

        }

        /// <summary>
        /// Handle a change to the max history count.
        /// </summary>
        /// <param name="dependencyObject">The FolderNavBar.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void OnMaxHistoryCountChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {

            FolderNavBar folderNavBar = dependencyObject as FolderNavBar;

            if (folderNavBar != null && folderNavBar.MaxHistoryCount != 0)
                while (folderNavBar.history.Count > folderNavBar.MaxHistoryCount)
                    folderNavBar.history.RemoveAt(folderNavBar.history.Count - 1);

        }
        
        /// <summary>
        /// Handles a change to the Navigation change property.
        /// </summary>
        /// <param name="dependencyObject">The object that owns the property.</param>
        /// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
        private static void OnSelectedValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            // Extract the variables from the generic arguments.
            FolderNavBar folderNavBar = dependencyObject as FolderNavBar;
            FolderNavNode newValue = dependencyPropertyChangedEventArgs.NewValue as FolderNavNode;

            folderNavBar.breadcrumb.SelectedItem = newValue;
            folderNavBar.history.Remove(newValue);
            folderNavBar.history.Insert(0, newValue);
            if (folderNavBar.MaxHistoryCount != 0 && folderNavBar.history.Count > folderNavBar.MaxHistoryCount)
                folderNavBar.history.RemoveAt(folderNavBar.history.Count - 1);
        }

        /// <summary>
        /// Gets the root data for the Breadcrumb.
        /// </summary>
        public FolderNavNode RootNode
        {
            get { return this.GetValue(FolderNavBar.RootNodeProperty) as FolderNavNode; }
            set { this.SetValue(FolderNavBar.RootNodeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected value shown in the TreeView.
        /// </summary>
        public  Object SelectedValue
        {
            get { return this.GetValue(FolderNavBar.SelectedValueProperty); }
            set { this.SetValue(FolderNavBar.SelectedValueProperty, value); }
        }


        #region Private Fields

        private void OnSelectedItemChanged(object sender, ActiproSoftware.Windows.ObjectPropertyChangedRoutedEventArgs routedPropertyChangedEventArgs)
        {
            try
            {
                if (this.RootNode != null)
                {
                    // Extract the selected node from the event arguments.
                    FolderNavNode selectedNode = routedPropertyChangedEventArgs.NewValue as FolderNavNode;
                    if (selectedNode != null)
                    {
                        RoutedEventArgs newEventArgs = new RoutedEventArgs(FolderNavBar.NodeSelectionChangedEvent);
                        newEventArgs.Source = selectedNode;
                        RaiseEvent(newEventArgs);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex.Message);
                throw;
            }
        }


		/// <summary>
		/// Handles the ConvertItem event of the breadcrumb control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="BreadcrumbConvertItemEventArgs"/> instance containing the event data.</param>
		private void OnBreadcrumbConvertItem(object sender, BreadcrumbConvertItemEventArgs e) {
			ConvertItemHelper.HandleConvertItem(sender, e);
        }

        #endregion
    }
}

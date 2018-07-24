namespace FluidTrade.Guardian
{
    using FluidTrade.Core.Windows.Controls;
    using FluidTrade.Guardian.Windows;
    using FluidTrade.Guardian.Windows.Controls;    
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
	using System.Xml.Linq;
    
	/// <summary>
	/// Interaction logic for ViewerFolder.xaml
	/// </summary>
	public partial class ViewerFolder : Viewer
	{

		// Private Instance Fields
		private Folder folder;

		// Private Delegates
		private delegate void SetStatusBarDelegate(String imageData, String title, String date);

		/// <summary>
		/// Identifies the MarkThree.Sandbox.ViewerPrototype.IsNavigationPaneVisible dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNavigationPaneVisibleProperty;

		/// <summary>
		/// Create the static resources required for the MarkThree.Sandbox.ViewerFolder.
		/// </summary>
		static ViewerFolder()
		{
			// IsNavigationPaneVisible Property
			ViewerFolder.IsNavigationPaneVisibleProperty = DependencyProperty.Register(
				"IsNavigationPaneVisible",
				typeof(Boolean),
				typeof(ViewerFolder),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsNavigationPaneVisibleChanged)));

		}
	
		/// <summary>
		/// Create a viewer for displaying the contents of folders.
		/// </summary>
		/// <param name="obj">Describes the content of the viewer.</param>
		public ViewerFolder(Object obj)
		{

			this.Content = obj;

			// The content must be a folder.
			if (obj is Folder)
				this.folder = obj as Folder;

			// The IDE managed resources are initialized here.
			InitializeComponent();

			// Create a canvas with a single image on it.  This canvas will eventually be replaced with an application that works
			// like the directory window in the Windows Explorer.
			Grid grid = new Grid();
			Image image = new Image();
			image.Source = new BitmapImage(new Uri("/FluidTrade.FolderViewer;component/Resources/DebtTrak.png", UriKind.Relative));
			image.Stretch = Stretch.UniformToFill;
			grid.Children.Add(image);						
			this.SetValue(Page.ContentProperty, grid);

			// Once loaded, the frame elements are updated with information about the object that is displayed in this viewer.
			this.Loaded += new RoutedEventHandler(OnLoaded);

		}

		/// <summary>
		/// Gets or sets an indication of whether the navigation pane is visible or not.
		/// </summary>
		private Boolean IsNavigationPaneVisible
		{
			get { return (Boolean)this.GetValue(ViewerFolder.IsNavigationPaneVisibleProperty); }
			set { this.SetValue(ViewerFolder.IsNavigationPaneVisibleProperty, value); }
		}

		/// <summary>
		/// Occurs when the Viewer is laid out, rendered and ready for interaction.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The unused routed event arguments.</param>
		void OnLoaded(object sender, RoutedEventArgs e)
		{

			// Bind the "IsNavigationPaneVisible" property to the settings.
			Binding bindingIsNavigationPaneVisible = new Binding("IsNavigationPaneVisible");
			bindingIsNavigationPaneVisible.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingIsNavigationPaneVisible.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, ViewerFolder.IsNavigationPaneVisibleProperty, bindingIsNavigationPaneVisible);

			(this.StatusBar as Guardian.Windows.Controls.EntityStatusBar).Entity = this.folder;
		
		}

		/// <summary>
		/// Handles a change to the IsNavigationPaneVisible property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnIsNavigationPaneVisibleChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed variables from the generic parameters.
			ViewerFolder viewerFolder = dependencyObject as ViewerFolder;
			Boolean isNavigationPaneVisible = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// The real logic is handled by the application window.  This handler is only got to update the menu user interface to
			// reflect the state of the navigation pane.
			if (viewerFolder.menuItemIsNavigationPaneVisible.IsChecked != isNavigationPaneVisible)
				viewerFolder.menuItemIsNavigationPaneVisible.IsChecked = isNavigationPaneVisible;

		}

		/// <summary>
		/// Checks or clears the 'IsNavigationPaneVisible' menu item.
		/// </summary>
		/// <param name="sender">The object that originated this event.</param>
		/// <param name="e">The unused event argments.</param>
		private void OnSetIsNavigationPaneVisible(object sender, RoutedEventArgs routedEventArgs)
		{

			// Freeze or thaw the the ability to change the layout of the headings.
			this.IsNavigationPaneVisible = !this.IsNavigationPaneVisible;

		}

	}

}

namespace FluidTrade.Guardian
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Security;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Data;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Navigation;
	using System.Windows.Threading;
	using FluidTrade.Actipro;
	using FluidTrade.Core;
	using FluidTrade.Core.Windows;
	using FluidTrade.Core.Windows.Controls;
	using FluidTrade.Guardian.Windows; 
	
	/// <summary>
	/// Main window of the Guardian Explorer application.
	/// </summary>
	public partial class WindowMain : NavigationWindow
	{

		// Private Instance Fields
		private Menu currentMenu;
		private StatusBar currentStatusBar;
		private ToolBar currentToolBar;
		private FolderTreeView folderTreeView;
		private FrameworkElement frameMain;
		private Grid gridMain;
		private GridSplitter gridSplitter1;
		private GridSplitter gridSplitter2;
		private Menu menuMain;
		private MenuItem menuItemIsNavigationPaneVisible;
		private Double progressMinimum;
		private StatusBar statusBarMain;
		private ToolBar toolBarMain;
		private NegotiationService negotiationService;
        private FolderNavBar folderNavBar;
        private TextBox searchBox;
		private Button searchCancelButton;
		private ProgressBar progressBar;
		private TextBlock statusText;

		private Object currentViewer;

		/// <summary>
		/// Identifies the the CurrentViewer dependency property;
		/// </summary>
		public static readonly DependencyPropertyKey CurrentViewerProperty;
		/// <summary>
		/// Identifies the MarkThree.Sandbox.ViewerPrototype.IsNavigationPaneVisible dependency property.
		/// </summary>
		public static readonly DependencyProperty IsNavigationPaneVisibleProperty;
        /// <summary>
        /// Identifies the the MaxStatusBarHeight dependency property;
        /// </summary>
        public static readonly DependencyProperty MaxStatusBarHeightProperty;

		private static readonly double MinimumClientHeight = 190;

		/// <summary>
		/// Create the static resources required for the MarkThree.Sandbox.WindowMain.
		/// </summary>
		static WindowMain()
		{
			// IsNavigationPaneVisible Property
			WindowMain.IsNavigationPaneVisibleProperty = DependencyProperty.Register(
				"IsNavigationPaneVisible",
				typeof(Boolean),
				typeof(WindowMain),
				new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsNavigationPaneVisibleChanged)));
            // MaxStatusBarHeight Property
            WindowMain.MaxStatusBarHeightProperty = DependencyProperty.Register(
                "MaxStatusBarHeight",
                typeof(double),
                typeof(WindowMain));
			// CurrentViewer Property
			WindowMain.CurrentViewerProperty = DependencyProperty.RegisterReadOnly(
				"CurrenViewer",
				typeof(Object),
				typeof(WindowMain),
				new PropertyMetadata(null));

			// Get these going early.
			TimeUnitList timeUnitList = TimeUnitList.Default;
			SettlementUnitList settlementUnitList = SettlementUnitList.Default;
       
		}

		/// <summary>
		/// Create the main window for the application.
		/// </summary>
		public WindowMain()
		{

			// Initialize the object
			this.progressMinimum = Double.MinValue;

			// The IDE created components are created here.
			InitializeComponent();

			// The 'WindowStartupLocation' is not a dependency property and so it must be set manually.
			this.WindowStartupLocation = FluidTrade.Guardian.Properties.Settings.Default.MainWindowStartupLocation;

			// This event will keep the folder tree view synchronized with the content when the navigator tries to move forward or
			// backward through the journal.
			this.Navigated += new NavigatedEventHandler(OnNavigated);

			// Install a common handler to deal with communication exceptions to the shared data model.
			DataModel.CommunicationException += OnCommunicationException;
			DataModel.Progress += OnProgress;
			
			this.negotiationService = new NegotiationService();
			this.negotiationService.OpenObject += new FluidTrade.Guardian.Windows.OpenObjectEventHandler(OnOpenObject);				
		}

		/// <summary>
		/// The currently shown viewer.
		/// </summary>
		public Object CurrentViewer
		{

			get { return this.currentViewer; }

		}

		/// <summary>
		/// Handle the AddressBar command. Give keyboard focus to the navigation bar.
		/// </summary>
		/// <param name="sender">The main window.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void OnAddressBar(object sender, EventArgs eventArgs)
		{


		}

		/// <summary>
		/// Event Handler for Progress made while reconcilling with the server.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="eventArgs">The progress event arguments.</param>
		private void OnProgress(object sender, ProgressEventArgs progressEventArgs)
        {

			// Remove the progress bar when the client has reconciled itself to the server.  When the client is behind the server, initialize and display the
			// progress.
			if (progressEventArgs.Current == progressEventArgs.Maximum)
			{

				// The 'progressMinimum' is the starting point for the progress bar.  It is set the first time the client falls behind the server.
				this.progressMinimum = Double.MinValue;

				// This will hide the progress bar when the client has caught up to the server.
				if (this.progressBar.Visibility == Visibility.Visible)
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
					{
						this.progressBar.Visibility = Visibility.Hidden;						
						DataModel.Progress -= OnProgress;
					});

			}
			else
			{

				// The visual elements of the progress bar are initialized the first time the client falls behind the server.
				if (this.progressMinimum == Double.MinValue)
				{
					this.progressMinimum =  0;
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
					{						
						this.progressBar.Visibility = Visibility.Visible;						
					});
				}

				// The progress of the reconcilliation is updated in the foreground thread.
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
				{
					this.statusText.Text = "Loading...";
					this.progressBar.IsIndeterminate = false;
					this.progressBar.Minimum = this.progressMinimum;
					this.progressBar.Maximum = progressEventArgs.Maximum;
					this.progressBar.Value = progressEventArgs.Current;
				});

			}

        }

        /// <summary>
        /// Handle the find command. Move keyboard focus to the search box.
        /// </summary>
        /// <param name="sender">The main window.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnFind(object sender, RoutedEventArgs eventArgs)
        {
			// Got the 'F3' Find command.
			// Call the search routine.
			OnSearchButton(sender, eventArgs);
        }

        /// <summary>
        /// Handle the MaximizeRestore command. Switch the state of the window between maximized and restored.
        /// </summary>
        /// <param name="sender">The main window.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnMaximizeRestore(object sender, EventArgs eventArgs)
        {

            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;

        }

		private void OnOpenObject(object sender, OpenObjectEventArgs openObjectEventArgs)
		{

			this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FluidTrade.Guardian.Windows.OpenObjectEventHandler(MyOnOpenObject), sender, openObjectEventArgs);

		}

		private void MyOnOpenObject(Object sender, OpenObjectEventArgs openObjectEventArgs)
		{

			try
			{

				// When the navigator is used to move forward or backward through the journal, the folder tree view will attempt to synchronize the selected item
				// in the tree to the content of the application window.  This check insures that the navigator doesn't fire more than once.
				Boolean isAlreadyViewed = false;

				if (!Application.Current.MainWindow.IsActive)
					Application.Current.MainWindow.Activate();

				if (this.currentViewer is Viewer)
				{

					Viewer currentViewer = this.currentViewer as Viewer;
					Entity currentEntity = currentViewer.Content as Entity;
					isAlreadyViewed = currentEntity == openObjectEventArgs.Entity;

				}

				// Navigate to the new viewer.
				if (!isAlreadyViewed)
				{

					try
					{

						this.currentViewer = openObjectEventArgs.Entity.CreateViewer(openObjectEventArgs.Arguments);
						this.SetValue(WindowMain.CurrentViewerProperty, this.currentViewer);
						this.Navigate(this.currentViewer);

					}
					catch (Exception exception)
					{

						// Log this event so it can be debugged in a production environment.
						if (exception.InnerException != null)
						{
							EventLog.Error("{0}, {1}, {2}", exception.Message, exception.InnerException.Message, exception.StackTrace);
						}
						else
						{
							EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
						}

						// This lets the user know that there's been a problem loading up a viewer.  It could be that the user doesn't have
						// permission to use the viewer, or it could be a problem loading a dependent assembly.
						MessageBox.Show(
							String.Format(FluidTrade.Client.Properties.Resources.UnableToLoadViewer, openObjectEventArgs.Entity.Name),
							this.Title,
							MessageBoxButton.OK);

					}

				}

			}
			catch (Exception exception)
			{
				EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
			}

		}

		/// <summary>
		/// Gets or sets an indication of whether the navigation pane is visible or not.
		/// </summary>
		private Boolean IsNavigationPaneVisible
		{
			get { return (Boolean)this.GetValue(WindowMain.IsNavigationPaneVisibleProperty); }
			set { this.SetValue(WindowMain.IsNavigationPaneVisibleProperty, value); }
		}

        /// <summary>
        /// Gets or sets the maximum allowed height of the status bar.
        /// </summary>
        public double MaxStatusBarHeight
        {
            get { return (double)this.GetValue(WindowMain.MaxStatusBarHeightProperty); }
            set { this.SetValue(WindowMain.MaxStatusBarHeightProperty, value); }
		}

		/// <summary>
		/// Provides a background color for the client area when the application is active.
		/// </summary>
        /// <param name="sender">The object that originated the event</param>
		/// <param name="e">Unused event arguments.</param>
		private void OnActivated(object sender, EventArgs e)
		{

			// This provides a background color for the client area when the application is active.  This makes the navigation
			// controls appear to be part of the frame rather than part of the client window.
			this.Background = FindResource(SystemColors.GradientActiveCaptionBrushKey) as System.Windows.Media.Brush;

		}

		/// <summary>
		/// Handles a request to terminate the application.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnClose(object sender, RoutedEventArgs e)
		{

			// Closing the main window will set of a series of events that will shut down the application.
			this.Close();

		}

		/// <summary>
		/// Handles the closing of the main maindow.
		/// </summary>
		/// <param name="e">The unused event arguments.</param>
		protected override void OnClosed(EventArgs e)
		{

			try
			{
				FluidTrade.Core.Utilities.ApplicationHelper.IsAppExiting = true;
				// Terminate the search thread if it is still running.
				searchCancelButton_Click(this, null);

				// Save the data model.
				Guid currentUserId = Guid.Empty;
				if (ChannelStatus.LoginEvent.WaitOne(0, false))
					currentUserId = UserContext.Instance.UserId;

				if (currentUserId != Guid.Empty)
					DataModel.Write(currentUserId.ToString());
				else
					EventLog.Warning("Failed to save cache to disk: Invalid UserId.");

			}
			catch (Exception exception)
			{

				EventLog.Warning("Failed to save cache to disk: {0}: {1}", exception.GetType(), exception.Message);

			}

			// The 'WindowStartupLocation' is not a dependency property and so it must be saved manually.
            FluidTrade.Guardian.Properties.Settings.Default.MainWindowStartupLocation = this.WindowStartupLocation;

		}

		/// <summary>
		/// Handles a communication error from the background.
		/// </summary>
		/// <param name="sender">The object that originated the exception.</param>
		/// <param name="exceptionEventArgs">The event arguments containing the exception.</param>
		private void OnCommunicationException(System.Object sender, FluidTrade.Core.ExceptionEventArgs exceptionEventArgs)
		{

			// The message must be passed to the foreground and there a message box will display the text of the exception.

			if (exceptionEventArgs.Exception is MessageSecurityException)
			{
				string errorMessage = exceptionEventArgs.Exception.Message;
				if (exceptionEventArgs.Exception.InnerException != null)
					errorMessage = exceptionEventArgs.Exception.InnerException.Message;

				this.Dispatcher.Invoke(DispatcherPriority.Normal,
				(MessageDelegate)((string message) => { MessageBox.Show(message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error); }),
				errorMessage);
			}
			else
			{

				this.Dispatcher.Invoke(DispatcherPriority.Normal,
					(MessageDelegate)((string message) => { MessageBox.Show(message, this.Title); }),
					"Server not responding");
			}
		}

		/// <summary>
		/// Provides a background color for the client area when the application is not active.
		/// </summary>
        /// <param name="sender">The object that originated the event</param>
        /// <param name="e">Unused event arguments.</param>
		private void OnDeactivated(object sender, EventArgs e)
		{

			// This provides a background color for the client area when the application is not active.  This makes the navigation
			// controls appear to be part of the frame rather than part of the client window.
			this.Background = FindResource(SystemColors.GradientInactiveCaptionBrushKey) as System.Windows.Media.Brush;

		}

		/// <summary>
		/// Handles the change of the keyboard focus to the main frame window of the browser.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The unused keyboard focus arguments.</param>
		private void OnFrameGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{

			// The Frame window used to navigation between viewers is a FocusScope and must take care of advising the application's
			// Main Window FocusScope when anything inside the Frame has changed the focus.  This update is done recursively for
			// all nested FocusScopes so that commands can be routed properly to their destination.  Without this recursive update,
			// the tunnelling algorithm gets lost at the first FocusScope it finds.
			if (e.NewFocus is DependencyObject)
			{
				DependencyObject dependencyObject = e.NewFocus as DependencyObject;
				DependencyObject focusScope = FocusManager.GetFocusScope(dependencyObject);
				while (focusScope != Application.Current.MainWindow)
				{
					focusScope = FocusManager.GetFocusScope(VisualTreeHelper.GetParent(focusScope));
					FocusManager.SetFocusedElement(focusScope, e.NewFocus);
				}
			}

		}

		/// <summary>
		/// Force Garbage Collection.
		/// </summary>
		/// <param name="sender">The object that created this event.</param>
		/// <param name="routedEventArgs">The event arguments.</param>
		public void OnResyncData(object sender, RoutedEventArgs routedEventArgs)
		{
			ThreadPoolHelper.QueueUserWorkItem(ResyncDataThread);
		}

		/// <summary>
		/// Background thread to access the datamodel
		/// </summary>
		/// <param name="state"></param>
		private static void ResyncDataThread(object state)
		{
			lock (DataModel.SyncRoot)
				DataModel.Reset();
		}
		

		/// <summary>
		/// Force Garbage Collection.
		/// </summary>
		/// <param name="sender">The object that created this event.</param>
		/// <param name="routedEventArgs">The event arguments.</param>
		public void OnGarbageCollect(object sender, RoutedEventArgs routedEventArgs)
		{
			//This is a very expensive operation, will block the app and is here for diagnostics only
			//Force the Garbage collector to collect immediately using the max generation
			EventLog.Information("Before Garbage Collection : estimated bytes on heap: {0}", GC.GetTotalMemory(false));			
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			GC.WaitForPendingFinalizers();
			// Collect anything that's just been finalized
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			EventLog.Information("After Garbage Collection : estimated bytes on heap: {0}", GC.GetTotalMemory(false));			
		}

		/// <summary>
		/// Show Help About dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnHelpAbout(object sender, RoutedEventArgs e)
		{
			About helpAbout = new About() { ConnectToServer = (sender != null) };
			helpAbout.Owner = this;
			helpAbout.ShowDialog();
		}

		/// <summary>
		/// Handles a change to the IsNavigationPaneVisible property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnIsNavigationPaneVisibleChanged(
			DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the strongly typed variables from the generic parameters.
			WindowMain windowMain = dependencyObject as WindowMain;
			Boolean isNavigationPaneVisible = (Boolean)dependencyPropertyChangedEventArgs.NewValue;

			// The state of the navigation windows is not easily stored as a simple value in the settings.  It needs to be coded and decoded from values that
			// can't simply be bound to a property.
			if (isNavigationPaneVisible)
			{

				// This will bind the "NavigationPaneWidth" property to the settings and add the grid splitter back into the visual tree.  As the grid splitter
				// changes the size of the navigation pane that change is reflected in the settings.
				Binding bindingNavigationPaneWidth = new Binding("NavigationPaneWidth");
                bindingNavigationPaneWidth.Source = FluidTrade.Guardian.Properties.Settings.Default;
				bindingNavigationPaneWidth.Mode = BindingMode.TwoWay;
				BindingOperations.SetBinding(windowMain.gridMain.ColumnDefinitions[0], ColumnDefinition.WidthProperty,
					bindingNavigationPaneWidth);
				if (!windowMain.gridMain.Children.Contains(windowMain.gridSplitter1))
					windowMain.gridMain.Children.Add(windowMain.gridSplitter1);

			}
			else
			{

				// Remove the binding between the setting and the width of the grid column.  This is necessary because the grid is going to be shurnk and that
				// change shouldn't show up in the settings.  The settings retains the unhidden width so it can be restored when the navigation pane is made
				// visible again.  Once the coupling between the splitter and the grid column is removed the navigation pane is hidden and the grid splitter is
				// removed from the visual tree hierarchy which effectively hides the navigation pane.
				BindingOperations.ClearBinding(windowMain.gridMain.ColumnDefinitions[0], ColumnDefinition.WidthProperty);
				windowMain.gridMain.ColumnDefinitions[0].Width = new GridLength(0.0);
				if (windowMain.gridMain.Children.Contains(windowMain.gridSplitter1))
					windowMain.gridMain.Children.Remove(windowMain.gridSplitter1);
			}

			// The real logic is handled by the application window.  This handler is only got to update the menu user interface to reflect the state of the
			// navigation pane.  Note that there's no guarantee that this menu is part of the visible tree hierarchy.  It is very likely that a viewer has its
			// own user interface installed for a menu.
			if (windowMain.menuItemIsNavigationPaneVisible.IsChecked != isNavigationPaneVisible)
				windowMain.menuItemIsNavigationPaneVisible.IsChecked = isNavigationPaneVisible;

		}

		/// <summary>
		/// Handles the window loaded event.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The routed event arguments.</param>
		private void OnLoaded(object sender, RoutedEventArgs e)
		{

			// In order to get all the functionality of a browser, the main window is constructed using a template for a NavigationWindow.  This means that the
			// compiler won't attempt to create fields automatically for the visual elements declared inside the template.  To get around this, the fields for
			// the visual elements of the application window are extracted here manually from the template using their names.
			this.folderTreeView = this.Template.FindName("folderTreeView", this) as FolderTreeView;
			this.frameMain = this.Template.FindName("frameMain", this) as FrameworkElement;
			this.gridMain = this.Template.FindName("gridMain", this) as Grid;
			this.gridSplitter1 = this.Template.FindName("gridSplitter1", this) as GridSplitter;
			this.gridSplitter2 = this.Template.FindName("gridSplitter2", this) as GridSplitter;
			this.menuItemIsNavigationPaneVisible = this.Template.FindName("menuItemIsNavigationPaneVisible", this) as MenuItem;
			this.menuMain = this.Template.FindName("menuMain", this) as Menu;
			this.toolBarMain = this.Template.FindName("toolBarMain", this) as ToolBar;
			this.statusBarMain = this.Template.FindName("statusBarMain", this) as StatusBar;
            this.folderNavBar = this.Template.FindName("folderNavBar", this) as FolderNavBar;
            this.searchBox = this.Template.FindName("searchBox", this) as TextBox;
			this.searchCancelButton = this.Template.FindName("searchCancelButton", this) as Button;
			this.progressBar = this.Template.FindName("progressBar", this) as ProgressBar;
			this.statusText = this.Template.FindName("statusText", this) as TextBlock;
            
			TextBlock statusBarServerNameBox = this.Template.FindName("statusBarServerName", this) as TextBlock;
            statusBarServerNameBox.Text = UserContext.Instance.ServerPath;
            this.statusText.Text = "Waiting for connection...";
			
			// The FocusManager is brain damaged so this event handler will move the keyboard focus into the frame's content when the frame gets the keyboard
			// focus.  The main idea is to allow the keyboard to navigate into and around in the page in the frame.
			this.frameMain.PreviewGotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnFramePreviewGotKeyboardFocus);
			this.frameMain.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnFrameGotKeyboardFocus);

			// Bind the "IsNavigationPaneVisible" property to the settings when the navigation pane is visible.  The binding is removed when the pane is
			// invisible to preserve the last known width of the navigation pane.  The binding is set here conditionally because the property change event
			// handler isn't called until the state actually changes.
			Binding bindingIsNavigationPaneVisible = new Binding("IsNavigationPaneVisible");
            bindingIsNavigationPaneVisible.Source = FluidTrade.Guardian.Properties.Settings.Default;
			bindingIsNavigationPaneVisible.Mode = BindingMode.TwoWay;
			BindingOperations.SetBinding(this, WindowMain.IsNavigationPaneVisibleProperty, bindingIsNavigationPaneVisible);

			// Bind the "NavigationPaneWidth" property to the settings.  Normally this would be done in the property change handler for the visibility state
			// (see above), but that method is only called when the state changes.  When the initial state is the same as the saved state, then that handler is
			// never called.  This does what the handler would normally do when the saved state is the same as the default state.
			if (this.IsNavigationPaneVisible)
			{
				Binding bindingNavigationPaneWidth = new Binding("NavigationPaneWidth");
                bindingNavigationPaneWidth.Source = FluidTrade.Guardian.Properties.Settings.Default;
				bindingNavigationPaneWidth.Mode = BindingMode.TwoWay;
				BindingOperations.SetBinding(this.gridMain.ColumnDefinitions[0], ColumnDefinition.WidthProperty,
					bindingNavigationPaneWidth);
			}

			// The frame controls -- menu, status bar and tool bar -- can be replaced by the viewers.  The controls are removed here and added back through the
			// same mechanism that the loaded pages will use to install their user interface into the application frame.
			this.gridMain.Children.Remove(this.menuMain);
			this.gridMain.Children.Remove(this.statusBarMain);
			this.gridMain.Children.Remove(this.toolBarMain);
			FluidTradeCommands.SetFrame.Execute(new FrameCommandArgs(this.menuMain, this.statusBarMain, this.toolBarMain), this);

            this.SizeChanged += OnSizeChanged;

			if (!String.IsNullOrEmpty(FluidTrade.Guardian.Properties.Settings.Default.LandingPageURI))
				this.Content = new Page() { Content = new Frame() { Source = new Uri(FluidTrade.Guardian.Properties.Settings.Default.LandingPageURI) } };
		
			string landPageURI = FluidTrade.Guardian.Properties.Settings.Default.LandingPageURI;

			if (String.IsNullOrEmpty(landPageURI) == false)
			{
				Grid grid = new Grid();
				Frame frame = new Frame();

				string formatedUri = String.Format(landPageURI, 
					FluidTrade.Guardian.Properties.Settings.Default.ApplicationName, UserContext.Instance.ClientVersion,
					UserContext.Instance.ServerPath);
				frame.Source = new Uri(Uri.EscapeUriString(formatedUri));
				grid.Children.Add(frame);
				this.SetValue(Page.ContentProperty, grid);
			}
		
			//Start the login dialog box from the background thread so that the main window has time to start.	
			ThreadPoolHelper.QueueUserWorkItem(DoLogin);
			ThreadPoolHelper.QueueUserWorkItem(WaitForLogin);

			WindowUserName.LoginAbout += new EventHandler(delegate(object s, EventArgs args)
			{				
				OnHelpAbout(null, e);
			});
		}

		/// <summary>
		/// Handles a request to reset the login credentials.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The unused event arguments.</param>
		private void OnLogin(object sender, RoutedEventArgs e)
		{
			ThreadPoolHelper.QueueUserWorkItem(DoLogin);
		}

		private void DoLogin(Object state)
		{

			// This will instruct the communication channel to prompt the user the next time a request is made of the middle tier.  Since there is a background
			// thread running that keeps the client synchronized with the server, the prompt should happen almost immediately.
			DataModel.IsReading = false;
			ChannelStatus.LoginEvent.Set();
			ChannelStatus.IsPrompted = true;

			while (true)
			{

				//This will lock until the credentials are entered.
				try
				{					

					//If the user canceled the dialog box, then exit the app since we can no longer log in
					if (!ChannelStatus.LoginEvent.WaitOne(0, false))
					{
						// The record is busy.
						this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
							delegate()
							{
								Application.Current.Shutdown();
							}
						));												
					}
					// The current user's identifier is used as a file name when reading the cached version of the data model.  This allows several logins to
					// simultaneously keep cached results on a machine.  This is used primarily by administrators that might keep several ids around for
					// navigating through different customer mappings.
					Guid currentUserId = Information.UserId;

					// Make sure the user id is ok.
					if (currentUserId == Guid.Empty)
					{
						ChannelStatus.IsPrompted = true;
					}
					else
					{
					
						DataModel.Read(currentUserId.ToString());
						Properties.Settings.Default.UserId = currentUserId;
					
					}

					if (currentUserId != Guid.Empty)
					{
						ChannelStatus.LogggedInEvent.Set();
						UserContext.Instance.UserId = currentUserId;
						break;
					}

				}
				catch (SecurityAccessDeniedException)
				{				
					MessageBox.Show("User credentials could not verified", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
					ChannelStatus.IsPrompted = true;
				}
				catch (MessageSecurityException securityException)
				{
					string errorMessage = securityException.Message;
					if (securityException.InnerException != null)
						errorMessage = securityException.InnerException.Message;

					// The record is busy.
					this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
						delegate()
						{
							AlertMessageBox.Instance.Show(errorMessage, AlertMessageBoxType.General);
						}
					));
					
					ChannelStatus.IsPrompted = true;
				}
				catch (FaultException<RecordNotFoundFault>)
				{					
					// The record is busy.
					this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
						delegate()
						{
							AlertMessageBox.Instance.Show("You are authorized to use the server but are not mapped to a user", AlertMessageBoxType.General);
						}
					));
					ChannelStatus.IsPrompted = true;
				}
				catch (EndpointNotFoundException)
				{
					this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
						delegate()
						{
							MessageBoxResult result = AlertMessageBox.Instance.Show("Server is currently offline. Do you wish to retry?", AlertMessageBoxType.LostConnectionToServer, MessageBoxButton.YesNo);
							if(result == MessageBoxResult.No)
							{
								Application.Current.Shutdown();
							}

						}
					));
					Thread.Sleep(1000);
				}
				// COMMENTED OUT - probably should be handling this.
				//catch (CommunicationObjectAbortedException communicationObjectAbortedException)
				//{
				//    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
				//        delegate()
				//        {
				//            AlertMessageBox.Instance.Show("Login operation canceled or the server was not able to obtain your user login information. Please try again.", AlertMessageBoxType.LostConnectionToServer);
				//        }
				//    ));
				//    EventLog.Information(string.Format("{0} {1}\n {2}", communicationObjectAbortedException.Message, communicationObjectAbortedException.ToString(), communicationObjectAbortedException.StackTrace));
				//}
				catch (Exception ex)
				{
					EventLog.Error(ex);
				}
			}

			// This will tell the data model to attempt to reload using a new set of credentials.	
			if (ChannelStatus.LogggedInEvent.WaitOne(0, false))
			{				
				DataModel.IsReading = true;
			}						
		}

		/// <summary>
		/// Invoked when an unhandled System.Windows.Input..GotKeyboardFocus attached event reaches this element.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">Arguments describing the change of focus event.</param>
		void OnFramePreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{

			// When the input focus reaches a frame, pass the focus on to the content.
			if (e.NewFocus is Frame)
			{
				Frame frame = e.NewFocus as Frame;
				if (frame.Content is IInputElement)
					Keyboard.Focus(frame.Content as IInputElement);
				e.Handled = true;
			}

		}

		/// <summary>
		/// Handles a successful navigation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnNavigated(object sender, NavigationEventArgs e)
		{

			// When the user selects an item using the folder tree view, its selected value will be the same as the content of this navigation window by the
			// time this event handler is called.  When the forward and backward buttons are used to navigate, then these properties will be different and
			// setting the selected value will synchronize them again.
			if (e.Content is Viewer)
			{
				Viewer viewer = e.Content as Viewer;
				if (this.folderTreeView.SelectedValue != viewer.Content)
					this.folderTreeView.SelectedValue = viewer.Content;
			}

		}

        private void OnNavBarNodeChanged(object sender, RoutedEventArgs routedEventargs)
        {
            FolderNavNode selectedNavNode = routedEventargs.OriginalSource as FolderNavNode;
            if (selectedNavNode != null )
            {
                FolderTreeNode rootNode = this.folderTreeView.ItemsSource as FolderTreeNode;
                TreeTraverseHelper.Node<FolderTreeNode> leafNode = rootNode.FindLeafNode(
                            t => t.Entity.EntityId == selectedNavNode.Entity.EntityId, (parent) => parent.Children);


                this.folderTreeView.SelectedValue = leafNode.Item.Entity;

            }
        }

		/// <summary>
		/// Resets the user settings to the factor defaults.
		/// </summary>
		/// <param name="sender">The object that created this event.</param>
        /// <param name="routedEventArgs">The event arguments.</param>
		public void OnResetSettings(object sender, RoutedEventArgs routedEventArgs)
		{

			// This will reset the settings object to the hard-coded values and then read them into the application the way it does
			// when the application is started.
			Guardian.Properties.Settings.Default.Reset();

			// This will force the client to reload from the server thus overwriting whatever is in cache.
			lock (DataModel.SyncRoot)
				DataModel.Reset();

		}


		/// <summary>
		/// Handles setting the frame resources: the main menu, the tool bar and the status bar.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		private void OnSetFrame(object sender, RoutedEventArgs routedEventArgs)
		{

			// Extract the arguments.
			ExecutedRoutedEventArgs executedRoutedEventArgs = routedEventArgs as ExecutedRoutedEventArgs;
			FrameCommandArgs frameCommandArgs = executedRoutedEventArgs.Parameter as FrameCommandArgs;

			// Sets the viewer specific menu.  Note that the menu gets its layout properties from the menu of the main window. Also note the delicate way that
			// the previous menu is removed.  The 'FindName' may find the resource, but it needs to be a valid child of the grid for it to be removed properly.
			if (this.currentMenu != frameCommandArgs.Menu)
			{
				if (frameCommandArgs.Menu != null)
				{
					Grid.SetRow(frameCommandArgs.Menu, Grid.GetRow(this.menuMain));
					Grid.SetColumn(frameCommandArgs.Menu, Grid.GetColumn(this.menuMain));
					Grid.SetColumnSpan(frameCommandArgs.Menu, Grid.GetColumnSpan(this.menuMain));
					this.gridMain.Children.Add(frameCommandArgs.Menu);
				}
				if (this.currentMenu != null)
					this.gridMain.Children.Remove(this.currentMenu);
			}
			this.currentMenu = frameCommandArgs.Menu;

			// Sets the viewer specific tool bar.  Note that the tool bar gets its layout properties from the tool bar of the main window.  Also note the
			// delicate way that the previous status bar is removed.  The 'FindName' may find the resource, but it needs to be a valid child of the grid for it
			// to be removed properly.
			if (this.currentToolBar != frameCommandArgs.ToolBar)
			{
				if (frameCommandArgs.ToolBar != null)
				{
					Grid.SetRow(frameCommandArgs.ToolBar, Grid.GetRow(this.toolBarMain));
					Grid.SetColumn(frameCommandArgs.ToolBar, Grid.GetColumn(this.toolBarMain));
					Grid.SetColumnSpan(frameCommandArgs.ToolBar, Grid.GetColumnSpan(this.toolBarMain));
					this.gridMain.Children.Add(frameCommandArgs.ToolBar);
				}
				if (this.currentToolBar != null)
					this.gridMain.Children.Remove(this.currentToolBar);
			}
			this.currentToolBar = frameCommandArgs.ToolBar;

			// Set the viewer specific status bar.  Note that the status bar gets its layout properties from the status bar of the main window.  Also note the
			// delicate way that the previous status bar is removed.  The 'FindName' may find the resource, but it needs to be a valid child of the grid for it
			// to be removed properly.
			if (this.currentStatusBar != frameCommandArgs.StatusBar)
			{

				if (frameCommandArgs.StatusBar != null)
				{

					frameCommandArgs.StatusBar.Name = this.statusBarMain.Name;
					Grid.SetRow(frameCommandArgs.StatusBar, Grid.GetRow(this.statusBarMain));
					Grid.SetColumn(frameCommandArgs.StatusBar, Grid.GetColumn(this.statusBarMain));
					Grid.SetColumnSpan(frameCommandArgs.StatusBar, Grid.GetColumnSpan(this.statusBarMain));
					this.gridMain.Children.Add(frameCommandArgs.StatusBar);
				
                }
				if (this.currentStatusBar != null)
					this.gridMain.Children.Remove(this.currentStatusBar);
			}
			this.currentStatusBar = frameCommandArgs.StatusBar;
            this.MaxStatusBarHeight = this.Height - MinimumClientHeight;

			this.InputBindings.Clear();
			if (frameCommandArgs.InputBindings != null)
				foreach (InputBinding inputBinding in frameCommandArgs.InputBindings)
					this.InputBindings.Add(inputBinding);

		}

		/// <summary>
		/// Handles a change to the visibility of the navigation pane.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedEventArgs">The routed event arguments.</param>
		private void OnSetIsNavigationPaneVisible(object sender, RoutedEventArgs routedEventArgs)
		{

			// Show or hide the navigation window.  Note that the real work is done in the property change handler.
			this.IsNavigationPaneVisible = !this.IsNavigationPaneVisible;

		}


        /// <summary>
        /// Handler for FolderTreeView update.  Called first time the data is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventargs"></param>
        private void OnFolderTreeViewUpdated(object sender, RoutedEventArgs routedEventargs)
        {

            try
			{				
            
                //Update the Navbar with the data from the TreeView.
                FolderTreeView folderTreeView = sender as FolderTreeView;
                
                if (folderTreeView != null)
                {
                
                    FolderTreeNode rootNode = folderTreeView.ItemsSource as FolderTreeNode;
                    // This assumes that there is only one top-level entity, ie. the current user's folder.
					FolderTreeNodeAdapter adapter = new FolderTreeNodeAdapter(rootNode[0]);
                    this.folderNavBar.RootNode = adapter.FolderNavNode;
                }

            }
            catch (Exception exception)
            {

                // Make sure that any errors trying to create of navigate to a page are logged.
                EventLog.Error("{0}, {1}", exception.Message, exception.Message);

            }

        }

        
		/// <summary>
		/// Handles a change to the selected item in the main navigation window for the application.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="routedPropertyChangedEventArgs">The routed event arguments.</param>
		private void OnFolderTreeViewItemChanged(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
		{
			try
			{
				// Extract the selected node from the event arguments.
				FolderTreeNode selectedNode = routedPropertyChangedEventArgs.NewValue as FolderTreeNode;

				this.Cursor = Cursors.Wait;

				if (selectedNode != null && this.folderNavBar.RootNode != null)
				{
					//Check to see if this the rootNode
					if (this.folderNavBar.RootNode.Entity.EntityId == selectedNode.Entity.EntityId)
					{
						this.folderNavBar.SelectedValue = this.folderNavBar.RootNode;
					}
					else
					{
						TreeTraverseHelper.Node<FolderNavNode> leafNode = this.folderNavBar.RootNode.FindLeafNode(
								t => t.Entity.EntityId == selectedNode.Entity.EntityId, (parent) => parent.Children);
						this.folderNavBar.SelectedValue = leafNode.Item;
					}

					this.SelectViewer(selectedNode);
				}

			}
			catch (Exception exception)
			{

				// Make sure that any errors trying to create of navigate to a page are logged.
				EventLog.Error("{0}, {1}", exception.Message, exception.Message);

			}
			finally
			{

				this.Cursor = Cursors.Arrow;

			}

		}

        /// <summary>
        /// Handle the search box losing focus. If there's no text in the in the box, revert to the default text ("Search") and make the font stand
        /// out.
        /// </summary>
        /// <param name="sender">The search box.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSearchBoxLostFocus(object sender, RoutedEventArgs e)
        {

            TextBox search = sender as TextBox;

            if (String.IsNullOrEmpty(search.Text))
            {

                //search.Text = "Search";
                search.FontStyle = FontStyles.Italic;

            }

        }

        /// <summary>
        /// Handle the search box gaining focus. Clear out the text string (if only the default text is there).
        /// </summary>
        /// <param name="sender">The search box.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSearchBoxGotFocus(object sender, RoutedEventArgs e)
        {

            TextBox search = sender as TextBox;

            if (search.FontStyle == FontStyles.Italic)
            {

                search.Text = "";

            }

            // We do this here instead of in the xaml to make sure we know to clear the text box.
            search.FontStyle = FontStyles.Normal;

        }

		/// <summary>
		/// Search allows for getting the string that was enter to be searched and then locating each cell in the report grid.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSearchButton(Object sender, RoutedEventArgs e)
		{
			string searchString = this.searchBox.Text;
			this.searchCancelButton.IsEnabled = true;

			// Validate that we have a non-empty search string.
			if (String.IsNullOrEmpty(searchString))
			{
				string message = "Search string is empty.";
				this.Dispatcher.BeginInvoke(new Action(() =>
					MessageBox.Show(Application.Current.MainWindow, message, Application.Current.MainWindow.Title)));
				return;
			}

			if (this.currentViewer != null && this.currentViewer is Viewer)
			{
				Viewer currentViewer = this.currentViewer as Viewer;
				currentViewer.OnSearchHandler(sender, e, searchString, new Action(this.CompletedSearchAction));
			}
			this.searchBox.IsEnabled = false;

		}

		private void CompletedSearchAction()
		{
			this.searchCancelButton.IsEnabled = false;
			this.searchBox.IsEnabled = true;
		}


		/// <summary>
		/// Capture/Preview the key down and process it.
		/// </summary>
		/// <param name="e">Key event args</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{

			// The base class handles any keys not handled above.
			base.OnKeyDown(e);

			switch (e.Key)
			{
				case Key.Enter:
					if (this.searchBox.IsFocused)
						this.OnSearchButton(this, e);
					break;
			}
		}

		private void searchCancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.currentViewer != null && this.currentViewer is Viewer)
			{
				Viewer currentViewer = this.currentViewer as Viewer;
				currentViewer.OnSearchRequestCancel();
			}
		}

		/// <summary>
		/// Selects a viewer from a FolderTreeNode.
		/// </summary>
		/// <param name="selectedNode">A node which describes which viewer should be opened to view the given Entity.</param>
		private void SelectViewer(FolderTreeNode selectedNode)
        {

            // When the navigator is used to move forward or backward through the journal, the folder tree view will attempt to synchronize the selected item
            // in the tree to the content of the application window.  This check insures that the navigator doesn't fire more than once.
            Boolean isAlreadyViewed = false;
            if (this.currentViewer is Viewer)
            {
                Viewer currentViewer = this.currentViewer as Viewer;
				Entity currentEntity = currentViewer.Content as Entity;
                isAlreadyViewed = currentEntity == selectedNode.Entity;
            }

            // Navigate to the new viewer if it is not already visible.
			if (!isAlreadyViewed)
			{

				try
				{

                    this.currentViewer = selectedNode.Entity.CreateViewer();
					this.SetValue(WindowMain.CurrentViewerProperty, this.currentViewer);

                    Viewer content = this.Content as Viewer;
                    Viewer current = this.currentViewer as Viewer;

                    current.Title = (current.Content as Entity).Name;

                    // Only Navigate to the viewer if it isn't already the current viewer (otherwise the navigation history gets truncated).
                    if (content == null || content != null && current != null && !content.Content.Equals(current.Content))
    					this.Navigate(this.currentViewer);

				}
				catch (Exception exception)
				{

					// Log this event so it can be debugged in a production environment.
					if (exception.InnerException != null)
					{
						EventLog.Error("{0}, {1}, {2}", exception.Message, exception.InnerException.Message, exception.StackTrace);
					}
					else
					{
						EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);
					}

					Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
					{
                        MessageBox.Show(
							String.Format(FluidTrade.Client.Properties.Resources.UnableToLoadViewer, selectedNode.Entity.Name),
                            this.Title,
                            MessageBoxButton.OK); });
                    this.Navigate(null);

				}

			}

        }

        /// <summary>
        /// Handle the window changing size. Make sure the status bar doesn't get too big.
        /// </summary>
        /// <param name="sender">The main window.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnSizeChanged(object sender, System.Windows.SizeChangedEventArgs eventArgs)
        {

            this.MaxStatusBarHeight = eventArgs.NewSize.Height - MinimumClientHeight;

        }

		/// <summary>
		/// Wait for login event
		/// </summary>
		/// <param name="state"></param>
		private void WaitForLogin(object state)
		{			

			ChannelStatus.LogggedInEvent.WaitOne();			
			// This will tell the data model to attempt to reload using a new set of credentials.	
			if (ChannelStatus.LogggedInEvent.WaitOne(0, false))
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
				{
					this.statusText.Text = "Loading...";
				});
			}						
		}

	}

	
}

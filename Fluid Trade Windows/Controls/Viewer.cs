namespace FluidTrade.Core.Windows.Controls
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;

	/// <summary>
	/// A Page with elements for interacting with an application container such as menus, tool bars, status bars, etc.
	/// </summary>
	[Serializable]
	public class Viewer : Page
	{

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.Viewer.Content property.
		/// </summary>
		public static new readonly DependencyProperty ContentProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.Viewer.Menu property.
		/// </summary>
		public static readonly DependencyProperty MenuProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.Viewer.StatusBar property.
		/// </summary>
		public static readonly DependencyProperty StatusBarProperty;

		/// <summary>
		/// Identifies the MarkThree.Windows.Controls.Viewer.ToolBar property.
		/// </summary>
		public static readonly DependencyProperty ToolBarProperty;

		//
		protected readonly static CacheDictionary<Guid, Int32?> lastSelectedtabItem = new CacheDictionary<Guid, Int32?>();

		// Private Instance Fields
		private UIElementCollection uiElementCollection;

        /// <summary>
        /// This is the same color used in the ReportGrid.
        /// </summary>
        protected readonly System.Windows.Media.Color splitColor = System.Windows.Media.Color.FromArgb(0xFF, 0xD7, 0xE6, 0xF7);
        protected readonly System.Windows.Media.Color splitColor2 = System.Windows.Media.Color.FromArgb(0xFF, 0x84, 0xA6, 0xD2);
        protected readonly System.Windows.Media.Color splitColor3 = System.Windows.Media.Color.FromArgb(0xFF, 0xA7, 0xCD, 0xF0);


		/// <summary>
		/// Initializes the static elements of the document viewer.
		/// </summary>
		static Viewer()
		{

			// Content
			Viewer.ContentProperty = DependencyProperty.Register("Content", typeof(Object), typeof(Viewer));

			// Menu
			Viewer.MenuProperty = DependencyProperty.Register("Menu", typeof(Menu), typeof(Viewer),
				new FrameworkPropertyMetadata(OnMenuChanged));

			// StatusBar
			Viewer.StatusBarProperty = DependencyProperty.Register("StatusBar", typeof(StatusBar), typeof(Viewer),
				new FrameworkPropertyMetadata(OnStatusBarChanged));

			// ToolBar
			Viewer.ToolBarProperty = DependencyProperty.Register("ToolBar", typeof(ToolBar), typeof(Viewer),
				new FrameworkPropertyMetadata(OnToolBarChanged));

		}

		/// <summary>
		/// Create a page with elements for interacting with a container.
		/// </summary>
		public Viewer()
		{

			// All viewers can get have the focus.
			this.Focusable = true;

			// The frame elements for this viewer need to temporarily be part of the window tree in order for binding to work
			// property.  Once the user interface elements are compiled, the frame elements for this viewer are removed from the 
			// native collection and passed to the application window to be integrated into the application frame window.
			this.uiElementCollection = new UIElementCollection(this, this);

			// Provide default values for the user interface frame elements for this viewer.  These are window elements and so must
			// be associated with the foreground thread.  That is, they can't be declared as defaults for a property.  Note that 
			// the property triggers will automatically add them to the user interface collection and make them children of the
			// viewer.
			this.Menu = new Menu();
			this.StatusBar = new StatusBar();
			this.ToolBar = new ToolBar();

			// When this viewer is activated in a frame, it will load the container application with its frame controls.
			this.Loaded += new RoutedEventHandler(OnLoaded);

		}

		/// <summary>
		/// The content of a viewer.
		/// </summary>
		public new Object Content
		{
			get { return this.GetValue(Viewer.ContentProperty); }
			set { this.SetValue(Viewer.ContentProperty, value); }
		}

		/// <summary>
		/// The menu of a viewer.
		/// </summary>
		public Menu Menu
		{
			get { return (Menu)this.GetValue(Viewer.MenuProperty); }
			set { this.SetValue(Viewer.MenuProperty, value); }
		}

		/// <summary>
		/// The status bar of a viewer.
		/// </summary>
		public StatusBar StatusBar
		{
			get { return (StatusBar)this.GetValue(Viewer.StatusBarProperty); }
			set { this.SetValue(Viewer.StatusBarProperty, value); }
		}

		/// <summary>
		/// The tool bar of a viewer.
		/// </summary>
		public ToolBar ToolBar
		{
			get { return (ToolBar)this.GetValue(Viewer.ToolBarProperty); }
			set { this.SetValue(Viewer.ToolBarProperty, value); }
		}

		/// <summary>
		/// Handles the loading of this control into the visual tree.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The unused routed event arguments.</param>
		void OnLoaded(object sender, RoutedEventArgs e)
		{

			// There is no way in XAML to set the target for an input binding because it is not a dependency object, so it is done
			// here.  Since the application can have many FocusScopes and it is easy for a command to get lost on its way to a
			// target, the commands are forced back to this window.
			foreach (InputBinding inputBinding in this.InputBindings)
				inputBinding.CommandTarget = this;

			// At this point the user interface elements for the frame window have been compiled.  More importantly, all the
			// bindings have been resolved in an intuitive (for a WPF programmer) way.  In order for the frame elements to be
			// integrated into the main frame window, however, they must be removed from the collection that binds them to the 
			// viewer.
			this.uiElementCollection.Remove(this.Menu);
			this.uiElementCollection.Remove(this.StatusBar);
			this.uiElementCollection.Remove(this.ToolBar);

			// This will install the menu, status bar, tool bar and input bindings in the application window.  All commands will be
			// routed back to this window as if it were the primary window in a static application.
			FrameCommandArgs frameCommandArgs = new FrameCommandArgs(this.Menu, this.StatusBar, this.ToolBar, this.InputBindings);
			FluidTradeCommands.SetFrame.Execute(frameCommandArgs, this);

		}

		/// <summary>
		/// Handles a change to the Menu property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnMenuChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the variables from the generic arguments.
			Viewer viewer = dependencyObject as Viewer;
			Menu newMenu = dependencyPropertyChangedEventArgs.NewValue as Menu;
			Menu oldMenu = dependencyPropertyChangedEventArgs.OldValue as Menu;

			// The user interface elements require a parent window in order for bindings to compile properly.  Once compiled, they
			// can be removed from this local collection and passed on to the application window where the become part of the user
			// interface.
			if (oldMenu != null)
				viewer.uiElementCollection.Remove(oldMenu);
			if (newMenu != null)
				viewer.uiElementCollection.Add(newMenu);

		}

		/// <summary>
		/// Handles the tunneling of the action of getting the keyboard focus.
		/// </summary>
		/// <param name="e">The keyboard focus event arguments.</param>
		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{

			// When the focus is given to this viewer it is passed on to the content.
			if (e.NewFocus is Viewer && this.Content is IInputElement)
			{
				Keyboard.Focus(this.Content as IInputElement);
				e.Handled = true;
			}

		}

		/// <summary>
		/// Search allows for getting the string that was enter to be searched and then locating each cell in the report grid.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void OnSearchHandler(Object sender, RoutedEventArgs e, string searchString, Action completeAction) { }

		/// <summary>
		/// Allows for search operation to be cancelled.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void OnSearchRequestCancel() { }


		/// <summary>
		/// Handles a change to the StatusBar property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnStatusBarChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the variables from the generic arguments.
			Viewer viewer = dependencyObject as Viewer;
			StatusBar newStatusBar = dependencyPropertyChangedEventArgs.NewValue as StatusBar;
			StatusBar oldStatusBar = dependencyPropertyChangedEventArgs.OldValue as StatusBar;

			// The user interface elements require a parent window in order for bindings to compile properly.  Once compiled, they
			// can be removed from this local collection and passed on to the application window where the become part of the user
			// interface.
			if (oldStatusBar != null)
				viewer.uiElementCollection.Remove(oldStatusBar);
			if (newStatusBar != null)
				viewer.uiElementCollection.Add(newStatusBar);

		}

		/// <summary>
		/// Handles a change to the ToolBar property.
		/// </summary>
		/// <param name="dependencyObject">The object that owns the property.</param>
		/// <param name="dependencyPropertyChangedEventArgs">A description of the changed property.</param>
		private static void OnToolBarChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{

			// Extract the variables from the generic arguments.
			Viewer viewer = dependencyObject as Viewer;
			ToolBar newToolBar = dependencyPropertyChangedEventArgs.NewValue as ToolBar;
			ToolBar oldToolBar = dependencyPropertyChangedEventArgs.OldValue as ToolBar;

			// The user interface elements require a parent window in order for bindings to compile properly.  Once compiled, they
			// can be removed from this local collection and passed on to the application window where the become part of the user
			// interface.
			if (oldToolBar != null)
				viewer.uiElementCollection.Remove(oldToolBar);
			if (newToolBar != null)
				viewer.uiElementCollection.Add(newToolBar);

		}

	}

}

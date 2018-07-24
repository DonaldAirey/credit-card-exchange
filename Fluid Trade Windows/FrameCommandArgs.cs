namespace FluidTrade.Core.Windows
{

    using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	
	/// <summary>
	/// Commands for setting the resources of a frame window.
	/// </summary>
	public class FrameCommandArgs
	{

		/// <summary>
		/// The main menu.
		/// </summary>
		public Menu Menu;

		/// <summary>
		/// The status bar.
		/// </summary>
		public StatusBar StatusBar;

		/// <summary>
		/// The tool bar.
		/// </summary>
		public ToolBar ToolBar;

		/// <summary>
		/// The input bindings.
		/// </summary>
		public InputBindingCollection InputBindings;

		/// <summary>
		/// Create an object for setting the resources of a frame window.
		/// </summary>
		/// <param name="menu">The main menu.</param>
		/// <param name="statusBar">The status bar.</param>
		/// <param name="toolBar">The tool bar.</param>
		public FrameCommandArgs(Menu menu, StatusBar statusBar, ToolBar toolBar)
		{

			// Initialize the object
			this.ToolBar = toolBar;
			this.Menu = menu;
			this.StatusBar = statusBar;
			this.InputBindings = null;

		}

		/// <summary>
		/// Create an object for setting the resources of a frame window.
		/// </summary>
		/// <param name="menu">The main menu.</param>
		/// <param name="statusBar">The status bar.</param>
		/// <param name="toolBar">The tool bar.</param>
		public FrameCommandArgs(Menu menu, StatusBar statusBar, ToolBar toolBar, InputBindingCollection inputBindings)
		{

			// Initialize the object
			this.ToolBar = toolBar;
			this.Menu = menu;
			this.StatusBar = statusBar;
			this.InputBindings = inputBindings;

		}

	}

}

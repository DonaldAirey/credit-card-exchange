namespace FluidTrade.Guardian
{

	using System;
	using System.Windows;

	/// <summary>
	/// Additional logic for the unit test module for the Report class.
	/// </summary>
	public partial class AssetExplorer : Application
	{

		/// <summary>
		/// Raises the System.Windows.Application.Startp event.
		/// </summary>
		/// <param name="e">The arguments for starting an application.</param>
		protected override void OnStartup(StartupEventArgs e)
		{

			// Allow the base class to initialize the application.
			base.OnStartup(e);

			// Since the Visual Studio Designer never runs this code, the 'IsDesignModeProperty' property will never be set in code
			// that is run on the design surface.  This property can be used to inhibit the background processes and data bound 
			// resources that can kill the design environment.
			AppDomain.CurrentDomain.SetData("IsDesignModeProperty", false);

			//hookup exception event handlers
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(FluidTrade.Core.UnhandledExceptionHelper.CurrentDomain_UnhandledException);
			Dispatcher.UnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(FluidTrade.Core.UnhandledExceptionHelper.Dispatcher_UnhandledException);
			Dispatcher.UnhandledExceptionFilter += new System.Windows.Threading.DispatcherUnhandledExceptionFilterEventHandler(FluidTrade.Core.UnhandledExceptionHelper.Dispatcher_UnhandledExceptionFilter);

		}

		/// <summary>
		/// Raises the System.Windows.Application.Exit event.
		/// </summary>
		/// <param name="e">The arguments for exiting an application.</param>
		protected override void OnExit(ExitEventArgs e)
		{

			// This will shut down the background updating of the shared data model.
			DataModel.IsReading = false;

			// This commits the settings to a persistant store.
			FluidTrade.Core.Properties.Settings.Default.Save();
			FluidTrade.Guardian.Properties.Settings.Default.Save();

			// Allow the base class to complete the shutdown.
			base.OnExit(e);

		}

	}

}

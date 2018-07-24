namespace FluidTrade.Guardian
{

	using System;
	using System.Windows;

	/// <summary>
	/// Additional logic for the unit test module for the Report class.
	/// </summary>
	public partial class TradeGenerator : Application
	{

		/// <summary>
		/// Raises the System.Windows.Application.Startp event.
		/// </summary>
		/// <param name="e">The arguments for starting an application.</param>
		protected override void OnStartup(StartupEventArgs e)
		{

			// Allow the base class to initialize the application.
			base.OnStartup(e);

			// This turns on the automatic updating of the data model.
			DataModel.IsReading = true;

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

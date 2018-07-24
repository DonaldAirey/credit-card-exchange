namespace FluidTrade.Guardian
{

	using System;
	using System.ComponentModel;
	using System.Configuration.Install;
	using System.Diagnostics;

	/// <summary>
	/// Installs the Web Service Host as a Windows Service.
	/// </summary>
	[RunInstaller(true)]
	public class InstallerEventLog : Installer
	{

		// Private Constants
		private const String log = "DebtTrak";
		private const String source = "DebtTrak Client";

		/// <summary>
		/// Installs the Web Service Host as a Windows Service.
		/// </summary>
		public InstallerEventLog()
		{

			// Generate a custom log.  Note that this log need to match the name of the log actuall used by the service.  It would be nice if these values
			// could come from the same configuration file as used by the reset of the service, but the installer doesn't have access to the configuration
			// files.
			// Remove Event Source if already there
			if (EventLog.SourceExists(InstallerEventLog.source))
				EventLog.DeleteEventSource(InstallerEventLog.source);

			EventLogInstaller eventLogInstaller = new EventLogInstaller();
			eventLogInstaller.Log = InstallerEventLog.log;
			eventLogInstaller.Source = InstallerEventLog.source;
			eventLogInstaller.Committed += new InstallEventHandler(EventLogInstallerCommitted);

			// Add installers to collection. Order is not important.
			this.Installers.Add(eventLogInstaller);

		}

		/// <summary>
		/// Set the EventLog properties
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void EventLogInstallerCommitted(object sender, InstallEventArgs e)
		{
			string logName = ((EventLogInstaller)sender).Log;
			if (EventLog.Exists(logName))
			{
				using (EventLog exampleEventLog = new EventLog(logName))
				{
					//Set it to 10MB
					exampleEventLog.MaximumKilobytes = 10240;
					exampleEventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
				}
			}
		}

	}

}
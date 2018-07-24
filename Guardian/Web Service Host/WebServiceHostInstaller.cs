namespace FluidTrade.Guardian
{

	using System.ComponentModel;
    using System.Configuration.Install;
    using System.Diagnostics;
    using System.ServiceProcess;

    /// <summary>
    /// Installs the Web Service Host as a Windows Service.
    /// </summary>
    [RunInstaller(true)]
    public class WebServiceHostInstaller : Installer
    {

        /// <summary>
        /// Installs the Web Service Host as a Windows Service.
        /// </summary>
        public WebServiceHostInstaller()
        {

            // The services run under the system account.
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.LocalService;

            // This is the service that is installed.  Note that it must match the name of the service actually created.  This would be a nice place to use a
            // configuration file, but these installers don't appear to work with configuration files.
            ServiceInstaller serviceInstaller = new ServiceInstaller();
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = Service.serviceName;
			
            // An event log is created automatically for the service that runs out of the 'Application' log and has the same source name as the web service.
            // Unfortunately, that is the same source name that is going to be used in the custom log.  This will remove the installation of a default event
            // log so the custom log will install properly using the service name.
            for (int index = 0; index < serviceInstaller.Installers.Count; index++)
                if (serviceInstaller.Installers[index] is EventLogInstaller)
                {
                    serviceInstaller.Installers.Remove(serviceInstaller.Installers[index]);
                    index--;
                }

			// Remove Event Source if already there
			if (EventLog.SourceExists(Service.source))
				EventLog.DeleteEventSource(Service.source);

            // Generate a custom log.  Note that this log need to match the name of the log actuall used by the service.  It would be nice if these values
            // could come from the same configuration file as used by the reset of the service, but the installer doesn't have access to the configuration
            // files.
            EventLogInstaller eventLogInstaller = new EventLogInstaller();
            eventLogInstaller.Log = Service.log;
            eventLogInstaller.Source = Service.source;
			//We need to set size of the eventlog. We cannot do that through the installer, so we will do it after the commits
			eventLogInstaller.Committed += new InstallEventHandler(EventLogInstallerCommitted);
						

            // Add installers to collection. Order is critical here.  The event log must be installed first or it will lead to a condition service appears
			// where theunable to uninstall after a failed install.
			this.Installers.Add(eventLogInstaller);
			this.Installers.Add(serviceInstaller);
            this.Installers.Add(serviceProcessInstaller);

        }

		/// <summary>
		/// Set the EventLog properties
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void EventLogInstallerCommitted(object sender, InstallEventArgs e)
		{
			string logName = ((EventLogInstaller)sender).Log;
			try
			{
				if (EventLog.Exists(logName))
				{
					using (EventLog eventLog = new EventLog(logName))
					{
						//Set it to 1 GB
						eventLog.MaximumKilobytes = 1048576;
						eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
					}
				}
			}
			catch
			{
			}
		}

    }

}
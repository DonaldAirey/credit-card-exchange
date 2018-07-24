namespace FluidTrade.Guardian
{

	using FluidTrade.Guardian;
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics;
	using System.ServiceProcess;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;

	/// <summary>
	/// Windows Service that provides a shared, in-memory data model.
	/// </summary>
	public partial class Service : ServiceBase
	{

		// The installer doesn't have access to the application configuration file.  These values must be changed when the
		// settings for the web service are changed.
		public const String serviceName = "Web Service";
		public const String log = "DebtTrak";
		public const String source = "Web Service";

		// Private Instance Fields
		private List<ServiceHost> serviceHosts;
		private DataModelFilters dataModelFilters;
		private SettlementDocumentFactory settlementDocumentFactory;

		/// <summary>
		/// Construct a service that provides a shared, in-memory data model.
		/// </summary>
		public Service()
		{

			// Initialize the object.
			this.ServiceName = serviceName;

			try
			{

				// This Windows Service can host one or many WCF Endpoints.  They are started and stopped as a unit.
				this.serviceHosts = new List<ServiceHost>();
				this.serviceHosts.Add(new ServiceHost(typeof(ServerAdmin), new Uri[] { }));
				this.serviceHosts.Add(new ServiceHost(typeof(ServerAdminCallbackManager), new Uri[] { }));
				this.serviceHosts.Add(new ServiceHost(typeof(ServerAdminStreamManager), new Uri[] { }));
				this.serviceHosts.Add(new ServiceHost(typeof(AdminSupport), new Uri[] { }));
				this.serviceHosts.Add(new ServiceHost(typeof(DataModel), new Uri[] { }));
				this.serviceHosts.Add(new ServiceHost(typeof(TradingSupport), new Uri[] { }));

				this.dataModelFilters = new DataModelFilters();
				this.settlementDocumentFactory = new SettlementDocumentFactory();

			}
			catch(Exception exception)
			{

				// Any problems initializing should be sent to the Event Log.
				EventLog.WriteEntry(
					Service.source,
					string.Format("{0}: {1}", exception.Message, exception.StackTrace),
					EventLogEntryType.Error);

			}

		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if DEBUG_SERVICE
			// This will run the project as an executable rather than as a service when debugging.
			Service service = new Service();
			service.OnStart(new string[] { });
			System.Windows.MessageBox.Show("Hit any key to exit Service", FluidTrade.Guardian.Properties.Settings.Default.ServiceName);
			service.Stop();
#else
			// This will run the Web Service as Windows Service.
			ServiceBase.Run(new ServiceBase[] { new Service() });
#endif

		}

		/// <summary>
		/// Start the service.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		protected override void OnStart(string[] args)
		{

			// Log the start of the service.
			EventLog.WriteEntry(
				Service.source,
				string.Format(Properties.Resources.ServiceStartingMessage, Service.serviceName),
				EventLogEntryType.Information);

			// Hack - This should eventually be implicit.  It seems to be required now because all the crossing is done dynamically.
			// Start the business rules and crossing automatically when debugging.
			OperationParameters operationParameter = new OperationParameters();
			operationParameter.AreBusinessRulesActive = true;
			operationParameter.IsCrossingActive = true;
			operationParameter.IsChatActive = true;
			OperationManager.OperatingParameters = operationParameter;

			try
			{

				// Start each of the WCF Web Services hosted by this Windows Service.
				foreach(ServiceHost serviceHost in this.serviceHosts)
					serviceHost.Open();

			}
			catch(Exception exception)
			{

				// Any problems initializing should be sent to the Event Log.
				EventLog.WriteEntry(
					Service.source,
					string.Format("{0}: {1}", exception.Message, exception.StackTrace),
					EventLogEntryType.Error);

			}

			// Log the start of the service.
			EventLog.WriteEntry(
				Service.source,
				string.Format(Properties.Resources.ServiceStarted, Service.serviceName),
				EventLogEntryType.Information);
		}

		/// <summary>
		/// Called when the service is stopped.
		/// </summary>
		protected override void OnStop()
		{

			try
			{

				// Shut down each of the Web Services hosted by this Windows Service.
				foreach(ServiceHost serviceHost in this.serviceHosts)
					serviceHost.Close();

			}
			catch(Exception exception)
			{

				// Any problems initializing should be sent to the Event Log.
				EventLog.WriteEntry(
					Service.source,
					string.Format("{0}: {1}", exception.Message, exception.StackTrace),
					EventLogEntryType.Error);

			}

#if DEBUG
            // Start the business rules and crossing automatically when debugging.
            OperationParameters operationParameter = new OperationParameters();
			operationParameter.AreBusinessRulesActive = false;
			operationParameter.IsCrossingActive = false;
			OperationManager.OperatingParameters = operationParameter;
#endif

            // Log the end of the service.
            EventLog.WriteEntry(
				Service.source,
				string.Format(Properties.Resources.ServiceStoppingMessage, Service.serviceName),
				EventLogEntryType.Information);

		}

	}

}

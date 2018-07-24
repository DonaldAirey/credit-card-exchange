namespace FluidTrade.Core
{

	using System;
    using System.Diagnostics;

	/// <summary>
	/// An Event Log for Web Services.
	/// </summary>
	/// <copyright>Copyright © 2002 - 2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class EventLog
	{

		/// <summary>
		/// Determines what level of detail is included in the event log.
		/// </summary>
		[Flags]
		public enum ErrorLogLevel
		{
			None = 0,
			Information = 1,
			Warning = 2,
			Error = 4,
			Exception = 8,
			Verbose = 16,
			Default = Information | Warning | Error | Exception
		}

		// Private Static Members
		private static Boolean isMultiuser;
		private static System.Diagnostics.EventLog eventLog;
		private static ErrorLogLevel logLevel;

		/// <summary>
		/// Initialize the Web Service Event Log.
		/// </summary>
		static EventLog()
		{

			// Read the settings for the log from the configuration file.
			string logName = Properties.Settings.Default.EventLog;
			string sourceName = Properties.Settings.Default.EventLogSource;
			EventLog.isMultiuser = Properties.Settings.Default.IsMultiuserLog;

			// Try to get the log level from the app config, but if we can't for whatever reason (likely because it's out of range), use the default.
			try
			{
				EventLog.logLevel = (ErrorLogLevel)Properties.Settings.Default.LogLevel;
			}
			catch
			{
				EventLog.logLevel = ErrorLogLevel.Default;
			}

			// If the configuration file doesn't contain specifications for the log, then use the application log with
			// an undefined source.
			if (logName == null)
				logName = "Application";
			if (sourceName == null)
				sourceName = Process.GetCurrentProcess().MainModule.ModuleName;

			// Initialize the Event Log.
			EventLog.eventLog = new System.Diagnostics.EventLog();
			EventLog.eventLog.Log = logName;
			EventLog.eventLog.Source = sourceName;

		}

		/// <summary>
		/// Write a formatted error entry into the event log.
		/// </summary>
		/// <param name="message">The format string for the message.</param>
		/// <param name="arguments">An array of optional arguments for the format string.</param>
		public static void Error(string format, params object[] arguments)
		{

			// Note that if we can't write to the log we should ignore the exception.  To do otherwise causes cascading exceptions telling us we can't write to
			// the log.
			try
			{
				// Construct an error message.
				if ((EventLog.logLevel & ErrorLogLevel.Error) == ErrorLogLevel.Error)
				{
					string message = CreateFormattedMessage(format, arguments);
					EventLog.eventLog.WriteEntry(message, EventLogEntryType.Error);
				}
			}
			catch { }

		}

		/// <summary>
		/// Write an exception entry into the event log.
		/// </summary>
		/// <param name="message">The format string for the message.</param>
		/// <param name="arguments">An array of optional arguments for the format string.</param>
		public static void Error(Exception exception)
		{

			// Note that if we can't write to the log we should ignore the exception.  To do otherwise causes cascading exceptions telling us we can't write to
			// the log.
			try
			{
				if (exception is global::System.ServiceModel.FaultException<FluidTrade.Core.OptimisticConcurrencyFault>)
				{
					if ((EventLog.logLevel & ErrorLogLevel.Verbose) == ErrorLogLevel.Verbose)
					{
						//exception.ToString() includes message and callstack
						string message = CreateFormattedMessage("OptimisticConcurrencyException in {0}, {1}", exception.Message, exception.ToString());
						EventLog.eventLog.WriteEntry(message, EventLogEntryType.Error);
					}
				}
				else if ((EventLog.logLevel & ErrorLogLevel.Exception) == ErrorLogLevel.Exception)
				{
					//exception.ToString() includes message and callstack
					string message = CreateFormattedMessage("Exception in {0}, {1}", exception.Message, exception.ToString());
					EventLog.eventLog.WriteEntry(message, EventLogEntryType.Error);
				}
			}
			catch { }

		}
				
		/// <summary>
		/// Write an informational message to the event log.
		/// </summary>
		/// <param name="message">The format string for the message.</param>
		/// <param name="arguments">An array of optional arguments for the format string.</param>
		public static void Information(string format, params object[] arguments)
		{

			// Note that if we can't write to the log we should ignore the exception.  To do otherwise causes cascading exceptions telling us we can't write to
			// the log.
			try
			{
				if ((EventLog.logLevel & ErrorLogLevel.Information) == ErrorLogLevel.Information)
				{
					// Construct an information message.
					string message = CreateFormattedMessage(format, arguments);
					EventLog.eventLog.WriteEntry(message, EventLogEntryType.Information);
				}
			}
			catch { }

		}

		/// <summary>
		/// Write a formatted warning to the event log.
		/// </summary>
		/// <param name="message">The format string for the message.</param>
		/// <param name="arguments">An array of optional arguments for the format string.</param>
		public static void Warning(string format, params object[] arguments)
		{

			// Note that if we can't write to the log we should ignore the exception.  To do otherwise causes cascading exceptions telling us we can't write to
			// the log.
			try
			{
				// Construct a warning.
				if ((EventLog.logLevel & ErrorLogLevel.Warning) == ErrorLogLevel.Warning)
				{
					string message = CreateFormattedMessage(format, arguments);
					EventLog.eventLog.WriteEntry(message, EventLogEntryType.Warning);
				}
			}
			catch { }

		}

		/// <summary>
		/// Combines the format string and the input parameters.
		/// </summary>
		/// <param name="format">The format for the event log message.</param>
		/// <param name="arguments">Ordered arguments for the message.</param>
		/// <returns>A message formatted with the arguments, the users domain and name.</returns>
		private static string CreateFormattedMessage(string format, params object[] arguments)
		{

			// The message is assembled with the user domain and name for a multiuser log. 
			string message = EventLog.isMultiuser ? String.Format(@"{0}\\{1}: {2}", System.Environment.UserDomainName,
				System.Environment.UserName, string.Format(format, arguments)) :
				String.Format(format, arguments);

			// The formatted message.
			return message;

		}

		/// <summary>
		/// return the eventLog level
		/// </summary>
		public static ErrorLogLevel LogLevel
		{
			get
			{
				return EventLog.logLevel;
			}
		}

		/// <summary>
		/// is the loging enabled for a level
		/// </summary>
		/// <param name="isLogingOnFor"></param>
		/// <returns>true if logging enabled</returns>
		public static Boolean IsLoggingEnabledFor(ErrorLogLevel isLogingOnFor)
		{
			return (EventLog.logLevel & isLogingOnFor) == isLogingOnFor;
		}

	}

}
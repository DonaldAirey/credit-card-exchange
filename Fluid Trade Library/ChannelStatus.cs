namespace FluidTrade.Core
{

    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Threading;

	/// <summary>
	/// Provides common data for the communication channels.
	/// </summary>
	public class ChannelStatus
	{

		// Private Static Fields
		private static System.Threading.ManualResetEvent tryToLoginEvent;
		private static System.Threading.ManualResetEvent logggedInEvent;
		private static System.Boolean isPrompted;
		private static System.Object secret;

		/// <summary>
		/// Initializes the static data for the common communication channel properties.
		/// </summary>
		static ChannelStatus()
		{

			// Initialize the static fields.
			ChannelStatus.IsPrompted = false;
			ChannelStatus.tryToLoginEvent = new ManualResetEvent(false);
			ChannelStatus.logggedInEvent = new ManualResetEvent(false);
			ChannelStatus.secret = null;

		}

		/// <summary>
		/// Gets or sets an indication of whether the user should be prompted for credentials.
		/// </summary>
		public static bool IsPrompted
		{
			get { return ChannelStatus.isPrompted; }
			set { ChannelStatus.isPrompted = value; }
		}

		/// <summary>
		/// Sets the password/thumbprint/etc. used for the next login.
		/// </summary>
		public static object Secret
		{
			internal get { return ChannelStatus.secret; }
			set { ChannelStatus.secret = value; }
		}

		/// <summary>
		/// Gets a signal used to suspect all communication to a service.
		/// </summary>
		public static ManualResetEvent LoginEvent
		{
			get { return ChannelStatus.tryToLoginEvent; }
		}

		/// <summary>
		/// Gets a signal used to suspect all communication to a service.
		/// </summary>
		public static ManualResetEvent LogggedInEvent
		{
			get { return ChannelStatus.logggedInEvent; }
		}

		/// <summary>
		/// Common exception handler for all channels.
		/// </summary>
		/// <param name="exception">A communication or service exception.</param>
		public static void HandleException(Exception exception)
		{

			// When the endpoint isn't available, the client will sleep for a few milliseconds and try again.
			if (exception is EndpointNotFoundException)
			{
				Thread.Sleep(1000);
			}

			// This will allow the user to try another identity.  There doesn't appear to be any way to just tell a channel to
			// prompt the client again for the credentials, so a new channel needs to be constructed.
			if (exception is SecurityAccessDeniedException || exception is MessageSecurityException)
			{
				ChannelStatus.IsPrompted = true;
			}

			// This exception comes from the user aborting out of a set of user credentials.  In this situation, it wouldn't be
			// useful to prompt the user again: they must have had a good reason for cancelling instead of just re-entering the
			// credentials.  This will prevent any further activity on any of the channels until the user decides to try again.
			// An external thread will need to reset this signal for the communications to start up again.
			if (exception is CommunicationObjectAbortedException)
			{
				ChannelStatus.LoginEvent.Reset();
			}

			// Write the error and stack trace out to the debug listener.
			EventLog.Error("{0}, {1}", exception.Message, exception.StackTrace);

		}

	}

}

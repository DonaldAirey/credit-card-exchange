namespace FluidTrade.Core
{

    using System;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

	/// <summary>
	/// Enables an application to display a user interface to collect information prior to creating a channel.
	/// </summary>
	class CertificateChannelInitializer : IInteractiveChannelInitializer
	{

		// Private Constants
		private const String myStoreName = "My";

		// Private Static Fields
		private static Oid oidClientAuthentication;
		private static X509Certificate2 x509Certificate2;

		// Private Instance Fields
		private PromptedClientCredentials promptedClientCredentials;

		// Private Delegates
		private delegate void PromptDelegate(object state);

		/// <summary>
		/// Used to communicate data to and from a thread that displays the prompt for a user certificate.
		/// </summary>
		private class PromptData
		{

			// Public Instance Fields
			public System.Boolean IsCanceled;
			public System.Security.Cryptography.X509Certificates.X509Certificate2 X509Certificate2;

		}

		/// <summary>
		/// Initializes the static fields.
		/// </summary>
		static CertificateChannelInitializer()
		{

			// This OID is used to find on Client Authentication certificates to present to the user from the certificate store.
			CertificateChannelInitializer.oidClientAuthentication = new Oid("1.3.6.1.5.5.7.3.2", "Client Authentication");

			// The thumbprint is used to store the last selected certificate in the user configuration file.  If that thumbprint is
			// available it is used to search for a certificate that matches the thumbprint from the certificate store.  If a 
			// matching certificate is found, it is used as the credential for all channels requiring a certificate.
			string thumbprint = Properties.Settings.Default.Thumbprint;
			if (thumbprint != string.Empty)
			{
				X509Store store = new X509Store(CertificateChannelInitializer.myStoreName, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
				foreach (X509Certificate2 x509Certificate2 in store.Certificates.Find(X509FindType.FindByThumbprint,
					thumbprint, true))
					CertificateChannelInitializer.x509Certificate2 = x509Certificate2;
				store.Close();
			}

			// If a complete set of credentials has been stored, then the user isn't prompted for this information.
			if (!ChannelStatus.IsPrompted)
				ChannelStatus.IsPrompted = CertificateChannelInitializer.x509Certificate2 == null;

		}

		/// <summary>
		/// Create an initializer for a channel requiring Basic authentication.
		/// </summary>
		/// <param name="promptedClientCredentials">The credentials that are to be initialized.</param>
		public CertificateChannelInitializer(PromptedClientCredentials promptedClientCredentials)
		{

			// Initialize the object.
			this.promptedClientCredentials = promptedClientCredentials;

		}

		/// <summary>
		/// Starts the user interface that prompts a user for a certificate.
		/// </summary>
		/// <param name="iClientChannel">The client channel.</param>
		/// <param name="callback">The callback object.</param>
		/// <param name="state">Any state data.</param>
		/// <returns>The System.IAsyncResult to use to call back when processing has completed.</returns>
		public IAsyncResult BeginDisplayInitializationUI(IClientChannel iClientChannel, AsyncCallback callback, object state)
		{

			// There are three ways of coordinating asynchronous activity.  The WCF channels appear to ignore all of them.  This
			// asynchronous interface appears to be here for future deveopment because it currently doesn't work.  A dummy
			// IAsyncResult is created here that makes it appear that the User Interface was handled immediately. The real work is
			// done when the synchronous 'End' method is called.
			ClientChannelResult clientChannelResult = new ClientChannelResult();
			clientChannelResult.IClientChannel = iClientChannel;
			return clientChannelResult;

		}

		/// <summary>
		/// Completes the user interface that prompts a user for a certificate.
		/// </summary>
		/// <param name="iAsyncResult">Represents the status of an asynchrous operation.</param>
		public void EndDisplayInitializationUI(IAsyncResult iAsyncResult)
		{

			// Any given thread can try to use a channel.  Most will be on a Multi Threaded Apartment (MTA).  To call up a user 
			// interface, a Single Threaded Apartment (STA) thread must be created.
			ClientChannelResult clientChannelResult = iAsyncResult as ClientChannelResult;

			// Only one thread at a time is allowed to prompt the user for credentials.  The credentials chosen here will be shared
			// by all other threads that try to access this type of channel.  Clearing the 'IsPrompted' flag will cause the prompt
			// to be displayed again allowing the user to change their identity.
			lock (typeof(CertificateChannelInitializer))
			{

				// If the user cancels out of the login screen, there's no sense in trying to initialize any of the other channels
				// with what we know are bad credentials.  The channel initializer will patiently sit here until a manual event 
				// tells it to try prompting the user again.
				ChannelStatus.LoginEvent.WaitOne();

				// Only prompt the user once for their credentials.  After a set of credentials is chosen, they'll be used by all
				// other threads that use this same type of channel.
				if (ChannelStatus.IsPrompted)
				{

					// Any worker thread can request a channel, so the user prompt needs to be invoked on a Single Threaded 
					// Apartment (STA) thread.  This will create that thread and communicate to it the current state of the 
					// channel.
					PromptData promptData = new PromptData();
					promptData.X509Certificate2 = CertificateChannelInitializer.x509Certificate2;

					// When running as a Windows application, the dialog box that prompts the user for credentials is run modally, 
					// preventing the user from accessing the main application until the credential dialog box is dismissed.  A
					// console application doesn't have the same luxury and must spawn a dedicated thread to handle the user
					// interface.
					if (Application.Current == null || Application.Current.Dispatcher == null)
					{

						// When running in a console environment, this will prompt the user from a dedicated thread.
						Thread thread = new Thread(PromptCertificate);
						thread.SetApartmentState(ApartmentState.STA);
						thread.Name = "Basic Prompt Thread";
						thread.Start(promptData);
						thread.Join();

					}
					else
					{

						// This will create the dialog box on the main application's message loop.
						Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new PromptDelegate(PromptCertificate),
							promptData);

					}

					// The user has the option to cancel the login operation.  If they cancel, the current operation will be 
					// aborted and further attempts to initialize a channel will be suspended.  If not, an attempt is made to use
					// the credentials to authorize communication with the web service.
					if (promptData.IsCanceled == true)
					{

						// If no credentials are given, this channel and all others like it are suspended until an external thread
						// resets the event.
						clientChannelResult.IClientChannel.Abort();
						ChannelStatus.LoginEvent.Reset();
						return;

					}
					else
					{

						// Copy the information out of the dialog box after it has been dismissed.  This information is shared
						// between all channels using Basic authentication.
						CertificateChannelInitializer.x509Certificate2 = promptData.X509Certificate2;
						Properties.Settings.Default.Thumbprint = CertificateChannelInitializer.x509Certificate2.Thumbprint;

						// This will prevent the user from being promtped again.
						ChannelStatus.IsPrompted = false;

					}

				}

				// This certificate is now part of the credentials available to this channel.
				this.promptedClientCredentials.Credential = CertificateChannelInitializer.x509Certificate2;

			}

		}

		/// <summary>
		/// Prompts the user for a certificate to authenticate that user on the channel.
		/// </summary>
		/// <param name="state">A generic parameter used to initialize the thread.</param>
		private void PromptCertificate(object state)
		{

			// Extract the thread parameters.
			PromptData promptData = state as PromptData;

			// This will select a list of valid certificates from the store that can be used for client authentication.
			X509Store store = new X509Store(CertificateChannelInitializer.myStoreName, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
			X509Certificate2Collection clientCertificates = store.Certificates.Find(X509FindType.FindByApplicationPolicy,
				CertificateChannelInitializer.oidClientAuthentication.Value, true);

			// The user is prompted here to select one of the certificates.
			WindowCertificate windowCertificate = new WindowCertificate();
			windowCertificate.X509Certificate2Collection = clientCertificates;
			windowCertificate.X509Certificate2 = promptData.X509Certificate2;
			if (windowCertificate.ShowDialog() == true)
			{
				promptData.IsCanceled = false;
				promptData.X509Certificate2 = windowCertificate.X509Certificate2;
			}
			else
			{
				promptData.IsCanceled = true;
			}

		}

	}

}


namespace FluidTrade.Core
{    
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
	/// Initalizes a channel that requires Basic Credentials from the user.
	/// </summary>
	class BasicChannelInitializer : IInteractiveChannelInitializer
	{

		// Private Static Fields
		private static System.Boolean areCredentialsPersistent;
		private static System.Net.NetworkCredential networkCredentials;

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
			public System.Boolean AreCredentialsPersistent;
			public System.Net.NetworkCredential NetworkCredentials;
			public System.ServiceModel.EndpointAddress EndpointAddress;

			/// <summary>
			/// Creates an object used to coordinate a thread that displays a prompt for a user certificate.
			/// </summary>
			public PromptData()
			{

				// Initialize the object
				this.AreCredentialsPersistent = false;

			}

		}

		/// <summary>
		/// Create the static resources for the channel initializer.
		/// </summary>
		static BasicChannelInitializer()
		{

			// This set of credentials is shared by all channels that use basic authentication.
			BasicChannelInitializer.networkCredentials = new NetworkCredential();
			BasicChannelInitializer.networkCredentials.Domain = Environment.UserDomainName;
			BasicChannelInitializer.networkCredentials.UserName = Environment.UserName;

			// If the user has requested the credentials to be stored, they are read from the user settings and applied to the
			// network credentials.
			BasicChannelInitializer.areCredentialsPersistent = Properties.Settings.Default.AreCredentialsPersistent;
			if (BasicChannelInitializer.areCredentialsPersistent)
			{
				BasicChannelInitializer.networkCredentials.UserName = Properties.Settings.Default.UserName;
				BasicChannelInitializer.networkCredentials.Password = Properties.Settings.Default.Password;
				BasicChannelInitializer.networkCredentials.Domain = Properties.Settings.Default.Domain;
			}

			// If a complete set of credentials has been stored, then the user isn't prompted for this information.
			if (!ChannelStatus.IsPrompted)
				ChannelStatus.IsPrompted = BasicChannelInitializer.networkCredentials.UserName == string.Empty ||
				BasicChannelInitializer.networkCredentials.Domain == string.Empty ||
				BasicChannelInitializer.networkCredentials.Password == string.Empty;

		}

		/// <summary>
		/// Create an initializer for a channel requiring Basic authentication.
		/// </summary>
		/// <param name="promptedClientCredentials">The credentials that are to be initialized.</param>
		public BasicChannelInitializer(PromptedClientCredentials promptedClientCredentials)
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
			lock (typeof(BasicChannelInitializer))
			{

				// If the user cancels out of the login screen, there's no sense in trying to initialize any of the other channels
				// with what we know are bad credentials.  The channel initializer will patiently sit here until a manual event 
				// tells it to try prompting the user again.
				ChannelStatus.LoginEvent.WaitOne();

				// Only prompt the user once for their credentials.  After a set of credentials is chosen, those credentials will
				// be used by all other threads that use a Basic authentication.
				if (ChannelStatus.IsPrompted)
				{

					// Any worker thread can request a channel, so the user prompt needs to be invoked on a Single Threaded
					// Apartment (STA) thread.  This structure is used to pass data to and from the thread where the dialog box is
					// invoked.
					PromptData promptData = new PromptData();
					promptData.AreCredentialsPersistent = BasicChannelInitializer.areCredentialsPersistent;
					promptData.NetworkCredentials = BasicChannelInitializer.networkCredentials;
					promptData.EndpointAddress = clientChannelResult.IClientChannel.RemoteAddress;

					// When running as a Windows application, the dialog box that prompts the user for credentials is run modally, 
					// preventing the user from accessing the main application until the credential dialog box is dismissed.  A
					// console application doesn't have the same luxury and must spawn a dedicated thread to handle the user
					// interface.
					if (Application.Current.Dispatcher == null)
					{

						// When running in a console environment, this will prompt the user from a dedicated thread.
						Thread thread = new Thread(PromptBasic);
						thread.SetApartmentState(ApartmentState.STA);
						thread.Name = "Basic Prompt Thread";
						thread.Start(promptData);
						thread.Join();

					}
					else
					{

						// This will create the dialog box on the main application's message loop.
						Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new PromptDelegate(PromptBasic),
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
						BasicChannelInitializer.areCredentialsPersistent = promptData.AreCredentialsPersistent;
						BasicChannelInitializer.networkCredentials = promptData.NetworkCredentials;

						// The information from the dialog box is copied to a persistent store when the user selects the 'Save
						// Credentials' check box.
						Properties.Settings.Default.AreCredentialsPersistent = BasicChannelInitializer.areCredentialsPersistent;
						if (BasicChannelInitializer.areCredentialsPersistent)
						{
							Properties.Settings.Default.UserName = BasicChannelInitializer.networkCredentials.UserName;
							Properties.Settings.Default.Domain = BasicChannelInitializer.networkCredentials.Domain;
							Properties.Settings.Default.Password = BasicChannelInitializer.networkCredentials.Password;
						}

						// This will prevent the user from being promtped again.
						ChannelStatus.IsPrompted = false;

					}

				}

				// This set of network credentials is now available to this channel for authentication.
				this.promptedClientCredentials.Credential = BasicChannelInitializer.networkCredentials;

			}

		}

		/// <summary>
		/// Prompts the user for a certificate to authenticate that user on the channel.
		/// </summary>
		/// <param name="state">A generic parameter used to initialize the thread.</param>
		private void PromptBasic(object state)
		{

			// Extract the thread parameters.
			PromptData promptData = state as PromptData;

			// This creates the Basic credentials prompt and initializes it with the current user name and password.
			WindowBasic windowBasic = new WindowBasic();
			windowBasic.AreCredentialsPersistent = promptData.AreCredentialsPersistent;
			windowBasic.NetworkCredentials = promptData.NetworkCredentials;
			windowBasic.EndpointAddress = promptData.EndpointAddress;

			// This presents the credentials to the user and allows them to be modified.
			if (windowBasic.ShowDialog() == true)
			{
				promptData.IsCanceled = false;
				promptData.NetworkCredentials = windowBasic.NetworkCredentials;
				promptData.AreCredentialsPersistent = windowBasic.AreCredentialsPersistent;
			}
			else
			{
				promptData.IsCanceled = true;
			}

		}

	}

}

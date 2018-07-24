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
	class UserNameChannelInitializer : IInteractiveChannelInitializer
	{

		// Private Static Fields		
		private static System.Net.NetworkCredential networkCredentials;

		// Private Instance Fields
		private PromptedClientCredentials promptedClientCredentials;

		// Private Delegates
		private delegate void PromptDelegate(object state);

		private object syncObject = new object();

		/// <summary>
		/// Used to communicate data to and from a thread that displays the prompt for a user certificate.
		/// </summary>
		private class PromptData
		{

			// Public Instance Fields
			public System.Boolean IsCanceled;			
			public System.Net.NetworkCredential NetworkCredentials;
			public System.ServiceModel.EndpointAddress EndpointAddress;

			/// <summary>
			/// Creates an object used to coordinate a thread that displays a prompt for a user certificate.
			/// </summary>
			public PromptData()
			{
			}

		}

		/// <summary>
		/// Create the static resources for the channel initializer.
		/// </summary>
		static UserNameChannelInitializer()
		{
			// This set of credentials is shared by all channels that use basic authentication.
			UserNameChannelInitializer.networkCredentials = new NetworkCredential();
			//UserNameChannelInitializer.networkCredentials.Domain = Environment.UserDomainName;
			UserNameChannelInitializer.networkCredentials.UserName = Properties.Settings.Default.UserName;			
		}

		/// <summary>
		/// Create an initializer for a channel requiring Basic authentication.
		/// </summary>
		/// <param name="promptedClientCredentials">The credentials that are to be initialized.</param>
		public UserNameChannelInitializer(PromptedClientCredentials promptedClientCredentials)
		{

			// Initialize the object.
			this.promptedClientCredentials = promptedClientCredentials;

			//Overwrite with usercredentials if specified
			if( String.IsNullOrEmpty(promptedClientCredentials.UserName.UserName) == false)
			{
				UserNameChannelInitializer.networkCredentials.UserName = promptedClientCredentials.UserName.UserName;
			}

			if (String.IsNullOrEmpty(promptedClientCredentials.UserName.Password) == false)
			{
				UserNameChannelInitializer.networkCredentials.Password = promptedClientCredentials.UserName.Password;
			}

			// If a complete set of credentials has been stored, then the user isn't prompted for this information.
			if (!ChannelStatus.IsPrompted)
				ChannelStatus.IsPrompted = UserNameChannelInitializer.networkCredentials.UserName == string.Empty ||
				UserNameChannelInitializer.networkCredentials.Password == string.Empty;

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
			lock (syncObject)
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
					promptData.NetworkCredentials = new NetworkCredential();
					promptData.NetworkCredentials.UserName = Properties.Settings.Default.UserName;
					promptData.NetworkCredentials.Password = ChannelStatus.Secret as String;
					promptData.EndpointAddress = clientChannelResult.IClientChannel.RemoteAddress;

					// When running as a Windows application, the dialog box that prompts the user for credentials is run modally, 
					// preventing the user from accessing the main application until the credential dialog box is dismissed.  A
					// console application doesn't have the same luxury and must spawn a dedicated thread to handle the user
					// interface.
					if (Application.Current == null || Application.Current.Dispatcher == null)
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
						UserNameChannelInitializer.networkCredentials = promptData.NetworkCredentials;

						// The information from the dialog box is copied to a persistent store when the user selects the 'Save
						// Credentials' check box.						
						Properties.Settings.Default.UserName = UserNameChannelInitializer.networkCredentials.UserName;

						// This will prevent the user from being promtped again.
						ChannelStatus.IsPrompted = false;

					}

				}
				// the password was reset internally, modify the network credentials accordingly.
				else if (ChannelStatus.Secret != null)
				{

					UserNameChannelInitializer.networkCredentials.Password = ChannelStatus.Secret as String;
					ChannelStatus.Secret = null;

				}

				// This set of network credentials is now available to this channel for authentication.
				this.promptedClientCredentials.Credential = UserNameChannelInitializer.networkCredentials;

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
			WindowUserName windowUsername = new WindowUserName();
			windowUsername.NetworkCredentials = promptData.NetworkCredentials;
			windowUsername.EndpointAddress = promptData.EndpointAddress;
			try
			{
				// This presents the credentials to the user and allows them to be modified.
				if (windowUsername.ShowDialog() == true)
				{
					promptData.IsCanceled = false;
					promptData.NetworkCredentials = windowUsername.NetworkCredentials;
				}
				else
				{
					promptData.IsCanceled = true;
				}
			}
			catch (InvalidOperationException ioe)
			{
				// TODO: Re-visit this later for now we are going to silently log that we got this exception.
				//  Some case we are getting duplicate login dialog boxes.
				EventLog.Error(string.Format("{0} {1}\n {2}", ioe.Message, ioe.ToString(), ioe.StackTrace));
			}
		}

	}

}

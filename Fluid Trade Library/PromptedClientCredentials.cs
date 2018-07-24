namespace FluidTrade.Core
{

    using System.IdentityModel.Selectors;
	using System.ServiceModel;
    using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;
	using System.ServiceModel.Channels;

	/// <summary>
	/// Enables the user to select client credentials for a session.
	/// </summary>
	public class PromptedClientCredentials : ClientCredentials
	{

		// Public Instance Fields
		public object Credential;

		/// <summary>
		/// Creates an object that allows the user to select credentials for a session.
		/// </summary>
		public PromptedClientCredentials() : base() { }

		/// <summary>
		/// Creates an object that allows the user to select credentials for a session.
		/// </summary>
		/// <param name="customClientCredentials">The original PromptedClientCredentials object.</param>
		public PromptedClientCredentials(PromptedClientCredentials customClientCredentials) : base(customClientCredentials) { }

		/// <summary>
		/// Creates a manager for the security tokens used by this set of credentials.
		/// </summary>
		/// <returns>An object that manages security tokens for the client.</returns>
		public override SecurityTokenManager CreateSecurityTokenManager()
		{

			// Create an object that manages security tokens for this set of client credentials.
			return new CustomSecurityTokenManager(this);

		}

		/// <summary>
		/// Applies the specified client behavoir to the endpoint.
		/// </summary>
		/// <param name="serviceEndpoint">The endpoint to which the specified client behavior is applied.</param>
		/// <param name="clientRuntime">The client behavior that is applied.</param>
		public override void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
		{

			// This will create specialized user interfaces for each of the security mechanism associated with an HTTP endpoint.
			if (serviceEndpoint.Binding is BasicHttpBinding)
			{

				// Extract the HTTP bindings from the generic object.
				BasicHttpBinding basicHttpBinding = serviceEndpoint.Binding as BasicHttpBinding;

				// Create a specialized initializer that will prompt the user for the appropriate credentials.
				switch (basicHttpBinding.Security.Transport.ClientCredentialType)
				{

				case HttpClientCredentialType.Basic:

					// Prompts for user name & Password.
					clientRuntime.InteractiveChannelInitializers.Add(new BasicChannelInitializer(this));
					break;

				case HttpClientCredentialType.Certificate:

					// Selects a certificate from the local store.
					clientRuntime.InteractiveChannelInitializers.Add(new CertificateChannelInitializer(this));
					break;

				}

			}

			// This will create specialized user interfaces for each of the security mechanism associated with an NET.TCP endpoint.
			if (serviceEndpoint.Binding is NetTcpBinding)
			{

				// Extract the HTTP bindings from the generic object.
				NetTcpBinding netTcpBinding = serviceEndpoint.Binding as NetTcpBinding;

				// Create a specialized initializer that will prompt the user for the appropriate credentials.
				switch (netTcpBinding.Security.Transport.ClientCredentialType)
				{				
				case TcpClientCredentialType.Certificate:

					// Selects a certificate from the local store.
					clientRuntime.InteractiveChannelInitializers.Add(new CertificateChannelInitializer(this));
					break;

				case TcpClientCredentialType.Windows:
					clientRuntime.InteractiveChannelInitializers.Add(new UserNameChannelInitializer(this));
					break;
				}

			}

			if (serviceEndpoint.Binding is CustomBinding)
			{
				CustomBinding customBinding = serviceEndpoint.Binding as CustomBinding;
				// Prompts for user name & Password.
				clientRuntime.InteractiveChannelInitializers.Add(new UserNameChannelInitializer(this));
			}

			// Allow the base class to apply the remaining behavoirs.
			base.ApplyClientBehavior(serviceEndpoint, clientRuntime);

		}

		/// <summary>
		/// Creates a copy of the client credentials.
		/// </summary>
		/// <returns>A copy of the client credentials</returns>
		protected override ClientCredentials CloneCore()
		{
			return new PromptedClientCredentials(this);
		}

	}

}

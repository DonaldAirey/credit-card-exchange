namespace FluidTrade.Core
{

    using System.IdentityModel.Selectors;
	using System.IdentityModel.Tokens;
	using System.Net;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel;
    using System.ServiceModel.Description;

    /// <summary>
	/// Manages security tokens for the client.
	/// </summary>
	class CustomSecurityTokenManager : ClientCredentialsSecurityTokenManager
	{

		/// <summary>
		/// Create an object that manages security tokens for the client.
		/// </summary>
		/// <param name="parent">The original ClientCredential manager.</param>
		public CustomSecurityTokenManager(ClientCredentials clientCredentials) : base(clientCredentials) { }

		/// <summary>
		/// Creates a security token provider.
		/// </summary>
		/// <param name="securityTokenRequirement">The System.IdentityModel.Selectors.SecurityTokenRequirement</param>
		/// <returns>A Security Token Provider.</returns>
		public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement securityTokenRequirement)
		{

			// The actual credentials are held by the PromptedClientCredentials object.  When the channels are initialized, the
			// resulting credentials obtained from the user are stored in a simple collection in this class.  They're used here to
			// create token providers based on the authentication scheme selected by the binding.
			if (this.ClientCredentials is PromptedClientCredentials)
			{

				// A set of credentials is maintained by the PromptedClientCredentials and populated by the channel initializers 
				// with input from the user.
				PromptedClientCredentials promptedClientCredentials = this.ClientCredentials as PromptedClientCredentials;

				// This will create the appropriate security token provider based on the key usage required.
				switch (securityTokenRequirement.KeyUsage)
				{

				case SecurityKeyUsage.Signature:

					// This creates a UserName token provider.
						if (securityTokenRequirement.TokenType == SecurityTokenTypes.UserName)
						{
							if (promptedClientCredentials.Credential is NetworkCredential)
							{

								// Note that the domain name is concatenated with the user name.  At this time, there's no
								// provision for authentication across domain boundaries, but it doesn't seem to hurt and it may be
								// useful for future features.
								NetworkCredential networkCredential = promptedClientCredentials.Credential as NetworkCredential;
								if (string.IsNullOrEmpty(networkCredential.Domain))
									return new UserNameSecurityTokenProvider(networkCredential.UserName, networkCredential.Password);
								else
									return new UserNameSecurityTokenProvider(string.Format("{0}\\{1}", networkCredential.Domain,
									networkCredential.UserName), networkCredential.Password);

							}
							else
							{
								return new UserNameSecurityTokenProvider(this.ClientCredentials.UserName.UserName, this.ClientCredentials.UserName.Password);
							}

						}
					// This creates a Certificate token provider.
					if (securityTokenRequirement.TokenType == SecurityTokenTypes.X509Certificate)
						if (promptedClientCredentials.Credential is X509Certificate2)
						{
							X509Certificate2 x509Certificate2 = promptedClientCredentials.Credential as X509Certificate2;
							return new X509SecurityTokenProvider(x509Certificate2);
						}

					break;

				case SecurityKeyUsage.Exchange:

					// There is no special handling when keys are exchanged to provide security.
					return base.CreateSecurityTokenProvider(securityTokenRequirement);

				}

			}

			// Any configuration not handled by the logic above is given a generic token handler.
			return base.CreateSecurityTokenProvider(securityTokenRequirement);

		}

		public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
		{
			return base.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
		}

	}

}

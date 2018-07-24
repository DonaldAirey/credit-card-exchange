using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.IdentityModel.Selectors;

namespace FluidTrade.Core
{
	class CustomServiceSecurityTokenManager : ServiceCredentialsSecurityTokenManager
	{
		/// <summary>
		/// Create an object that manages security tokens for the client.
		/// </summary>
		/// <param name="parent">The original ClientCredential manager.</param>
		public CustomServiceSecurityTokenManager(ServiceCredentials serviceCredentials)
			: base(serviceCredentials) { }

		public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
		{
			// Return your implementation of SecurityTokenProvider, if required.
			// This implementation delegates to the base class.
			return base.CreateSecurityTokenProvider(tokenRequirement);
		}

		public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
		{
			outOfBandTokenResolver = null;
			return base.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);

		}



	}
}

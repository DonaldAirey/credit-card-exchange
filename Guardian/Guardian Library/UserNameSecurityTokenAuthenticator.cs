using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// Username credentials are validated against domain credentials. We need to override this behavior and do
	/// our own validaton
	/// </summary>
	internal class UserNameSecurityTokenAuthenticator : SecurityTokenAuthenticator 
	{
		/// <summary>
		/// We will only authenticate userNameTokensecurity
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		protected override bool CanValidateTokenCore(System.IdentityModel.Tokens.SecurityToken token)
		{
			return (token is UserNameSecurityToken);	
		}


		protected override System.Collections.ObjectModel.ReadOnlyCollection<System.IdentityModel.Policy.IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
		{
			UserNameSecurityToken userNameToken = token as UserNameSecurityToken;

			// Validate the information contained in the username token. For demonstration 
			// purposes, this code just checks that the user name matches the password.
			if (userNameToken.UserName != userNameToken.Password)
			{
				throw new SecurityTokenValidationException("Invalid user name or password");
			}

			// Create just one Claim instance for the username token - the name of the user.			
			List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
			policies.Add(new AuthorizationPolicy());
			return policies.AsReadOnly();

		}
	}
}

namespace FluidTrade.Guardian
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.Security.Principal;
    using FluidTrade.Core;
	using System.IdentityModel.Policy;    

	/// <summary>
	/// Defines a set of rules for authorizing a user given a set of claims.
	/// </summary>
    public class AuthorizationPolicy : ClaimsAuthorizationPolicy
    {

		/// <summary>
		/// The URL of the issuer of this Authorization Policy.
		/// </summary>
		public const string IssuerUri = "http://schemas.fluidtrade.com//issuer";

		/// <summary>
		/// The name of the issuer.
		/// </summary>
		public const string IssuerName = "Guardian";

		/// <summary>
		/// Create the static resources used to authorize a user given a set of claims.
		/// </summary>
		///
		public AuthorizationPolicy()
		{

			// This ClaimSet uniquely represents the issuer of this policy and will qualify any set of claims.
			SetIssuerClaim(Claim.CreateUriClaim(new Uri(AuthorizationPolicy.IssuerUri)),
				Claim.CreateNameClaim(AuthorizationPolicy.IssuerName));

		}

        #region IAuthorizationPolicy Members

		/// <summary>
		/// Construct a set of claims for a user based on their role.
		/// </summary>
		/// <param name="identity">The identity of the current operation context.</param>
		/// <returns>A set of claims associated with the user's role.</returns>
		protected override ClaimSet MapClaims(IIdentity identity)
		{

			// The claims based on the users role will be collected here.
			List<Claim> listClaims = new List<Claim>();

			// This will prevent non-authenticated users from getting very far.
			if (!identity.IsAuthenticated)
				throw new NotSupportedException("User not authenticated.");

			// A user may participate in more than one role.  Each set of claims for each role will be aggregated into a single 
			// set of claims.
			foreach (string roleName in AuthorizationPolicy.webServiceRoleProvider.GetRolesForUser(identity.Name))
				switch (roleName)
				{

				case "Administrators":

					// Claims of Administrators
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Create, Resources.Application, Rights.PossessProperty));
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Destroy, Resources.Application, Rights.PossessProperty));
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Update, Resources.Application, Rights.PossessProperty));
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Read, Resources.Application, Rights.PossessProperty));
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Execute, Resources.Application, Rights.PossessProperty));
					break;

				case "Traders":

					// Claims of Traders
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Create, Resources.Application, Rights.PossessProperty));
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Update, Resources.Application, Rights.PossessProperty));
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Read, Resources.Application, Rights.PossessProperty));
					break;

				case "Compliance Officers":

					// Claims of Users
					listClaims.Add(new Claim(FluidTrade.Core.ClaimTypes.Read, Resources.Application, Rights.PossessProperty));
					break;

				}

			// This creates a claim set for the given identity and qualifying it with the issuers claims.
			return new DefaultClaimSet(ClaimsAuthorizationPolicy.IssuerClaimSet, listClaims);

		}

		//public bool Evaluate(EvaluationContext evaluationContext, ref object state)
		//{
		//    object identities;
		//    List<IIdentity> identitiesList = null;
		//    IIdentity authorisedIdentity;
		//    if (evaluationContext.Properties.TryGetValue("Identities", out identities)) 
		//    {
		//      identitiesList = identities as List<IIdentity>;
		//    }
			
		//    if (identitiesList == null || identitiesList.Count == 0) 
		//    {
		//      return false;
		//    }
		//    authorisedIdentity = identitiesList.Find(
		//      delegate(IIdentity match) {
		//        return String.Equals(match.AuthenticationType,
		//          "MyUserNamePasswordValidator",
		//          StringComparison.Ordinal);
		//    });
			
		//    // We also allow Windows-authenticated users...
		//    if (authorisedIdentity == null) 
		//    {
		//      authorisedIdentity = identitiesList.Find(
		//        delegate(IIdentity match) 
		//        {
		//          return match is WindowsIdentity;
		//        });
		//    }
			
		//    if (authorisedIdentity == null) 
		//    {
		//      return false;
		//    }
			
		//    //AppUserPrincipal appPrincipal = null;
		//    //try 
		//    //{
		//    //  // Construct custom Principal object...
		//    //  appPrincipal = AuthenticationProvider.Current.CreateAppPrincipal(
		//    //    authorisedIdentity);
		//    //}
		//    //catch (SqlException sqlError) 
		//    //    { return false; }
		//    //catch (SecurityException securityError) 
		//    //{ return false; }
			
			
		//    //evaluationContext.Properties["Principal"] = appPrincipal;
		//    //evaluationContext.AddToTarget(this, new DefaultClaimSet(
		//    //  ClaimSet.System,
		//    //  new Claim("MyNamespace", "AppUser", Rights.PossessProperty)));

		//    return true;
		//}
	
        #endregion

    }

}

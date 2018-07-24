namespace FluidTrade.Core
{

    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Principal;

	/// <summary>
	/// Defines a set of rules for authorizing a user given a set of claims.
	/// </summary>
    public abstract class ClaimsAuthorizationPolicy : IAuthorizationPolicy
    {

		// Private Constant Fields
		private const String roleProviderName = "Fluid Trade Role Provider";

		// Private Static Fields
		protected static ClaimSet issuerClaimSet;
		protected static WebServiceRoleProvider webServiceRoleProvider;

		// Private Instance Fields
		private System.Guid guid;

		/// <summary>
		/// Create the static resources used to authorize a user given a set of claims.
		/// </summary>
		static ClaimsAuthorizationPolicy()
		{

			// The standard interface for a Role Provider -- System.Security.Role -- has a problem with the commas that appear in 
			// the IIdentity.Name property. To get around the problem, the role provider is loaded up and called explicitly rather
			// than through the standard interface.  This emulates the important features of the Role Provider.  Perhaps someday in
			// the future when the problem is fixed with either the way certificates are encoded or the way the Role provider
			// handles commas, the System.Security.Role class can be used and these settings can be maintained in the Web.Config
			// section of the settings.
			ClaimsAuthorizationPolicy.webServiceRoleProvider = new WebServiceRoleProvider();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection.Add("connectionStringName", FluidTrade.Core.Properties.Settings.Default.ConnectionStringName);
			nameValueCollection.Add("applicationName", FluidTrade.Core.Properties.Settings.Default.ApplicationName);
			ClaimsAuthorizationPolicy.webServiceRoleProvider.Initialize(roleProviderName, nameValueCollection);

		}

		/// <summary>
		/// Create a set of rules for authorizing a user given a set of claims.
		/// </summary>
		public ClaimsAuthorizationPolicy()
        {

			// This provides a unique identifier for the policy.
            this.guid = Guid.NewGuid();

        }

		/// <summary>
		/// Sets the issuer claims.
		/// </summary>
		/// <param name="claims">A set of claims that uniquely identifies the issuer of this policy.</param>
		protected void SetIssuerClaim(params Claim[] claims)
		{

			// This set of claims is used to qualify a permission as belonging exclusively to the issuer of this policy.  It is 
			// similar to a namespace FluidTrade.Core that it prevents confusion with similar Claim types Resource names.
			ClaimsAuthorizationPolicy.issuerClaimSet = new DefaultClaimSet(claims);

		}

		/// <summary>
		/// Gets the set of claims that uniquely identify this web service's Issuer.
		/// </summary>
		/// <remarks>
		/// This provides the static information about the issuer and can be used in places that don't have access to an instance
		/// of this object.
		/// </remarks>
		public static ClaimSet IssuerClaimSet
		{
			get { return ClaimsAuthorizationPolicy.issuerClaimSet; }
		}

        #region IAuthorizationPolicy Members

		/// <summary>
		/// Evaluate the set of incoming claims.
		/// </summary>
		/// <param name="evaluationContext">The results of the authorization policies that have been evaluated.</param>
		/// <param name="state">The current state of the authorization evaluation.</param>
		/// <returns>true indicates evaluation of the authorization can continue, false will reject the authorization.</returns>
		public bool Evaluate(EvaluationContext evaluationContext, ref object state)
		{

			// A new principal with its associated claims is created for the first identity to have been evaluated by the transport
			// layer.  This principal will be applied to the thread before the operations are called.
			object property = null;
			if (evaluationContext.Properties.TryGetValue("Identities", out property))
			{
				List<IIdentity> identities = property as List<IIdentity>;
				if (identities != null)
					foreach (IIdentity iIdentity in identities)
					{
						///HACK - we are only interested custom username
						if (iIdentity.AuthenticationType != "Kerberos")
						{
							ClaimSet claimSet = MapClaims(iIdentity);
							ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(iIdentity, claimSet);
							evaluationContext.Properties["Principal"] = claimsPrincipal;
							evaluationContext.AddClaimSet(this, claimSet);
						}
					}
			}
			else
			{

				// If there was no identity provided by the transport, then a generic identity with no claims is used.  This user
				// will only be able to access 'Unrestricted' methods of an interface.
				ClaimSet emptyClaims = new DefaultClaimSet(ClaimsAuthorizationPolicy.IssuerClaimSet, new Claim[] { });
				evaluationContext.Properties["Principal"] = new ClaimsPrincipal(new GenericIdentity("Default User"), emptyClaims);

			}

			// There is no need to call the evaluation again, everything here is computed in a single pass.
			return true;

		}

		/// <summary>
		/// Construct a set of claims for a user based on their role.
		/// </summary>
		/// <param name="identity">The identity of the current operation context.</param>
		/// <returns>A set of claims associated with the user's role.</returns>
		protected abstract ClaimSet MapClaims(IIdentity identity);

		/// <summary>
		/// Gets a claim set that represents the issuer of the authorization policy.
		/// </summary>
		public ClaimSet Issuer
		{
			get { return ClaimsAuthorizationPolicy.issuerClaimSet; }
		}

        #endregion

        #region IAuthorizationComponent Members

		/// <summary>
		/// The unique identifier of the Authorization instance.
		/// </summary>
		public string Id
        {
			get { return this.guid.ToString(); }
        }

        #endregion

    }

}

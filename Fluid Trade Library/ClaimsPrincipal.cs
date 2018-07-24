namespace FluidTrade.Core
{

    using System;
    using System.IdentityModel.Claims;
    using System.Security.Principal;

	/// <summary>
	/// A Security Principal with Claims that are used to access application resources.
	/// </summary>
	public class ClaimsPrincipal : IClaimsPrincipal, IPrincipal
    {

		// Private Instance Fields
        private ClaimSet claimSet;
        private IIdentity iIdentity;

		/// <summary>
		/// Create a Security Principal and give it a set of Claims.
		/// </summary>
		/// <param name="identity">The identity of the Security Principal.</param>
		/// <param name="claims">The claims to associate with the Security Principal.</param>
        public ClaimsPrincipal(IIdentity iIdentity, ClaimSet claimSet)
        {

			// Initialize the object
			this.iIdentity = iIdentity;
            this.claimSet = claimSet;

        }

        #region IClaimsPrincipal Members

		/// <summary>
		/// Gets the claims associated with the Security Principal.
		/// </summary>
		ClaimSet IClaimsPrincipal.Claims
		{
			get { return this.claimSet; }
		}

		/// <summary>
		/// Gets the issuer of the claims associated with the Security Principal.
		/// </summary>
		ClaimSet IClaimsPrincipal.Issuer
        {
            get { return this.claimSet.Issuer; }
        }

		/// <summary>
		/// Determines whether the Principal has the requested claims.
		/// </summary>
		/// <param name="requiredClaims">A set of claims that are required to access a resource.</param>
		/// <returns>true if the Principal has the required claims, false otherwise.</returns>
		bool IClaimsPrincipal.HasRequiredClaims(ClaimSet requiredClaims)
        {

            // The issuer of the required claims must match the issuer of the Principal's claims.
            foreach (Claim claim in requiredClaims.Issuer)
                if (!this.claimSet.Issuer.ContainsClaim(claim))
					return false;

			// The Principal must have a claim that matches each of the resource's required claims.
			foreach (Claim claim in requiredClaims)
				if (!this.claimSet.ContainsClaim(claim))
					return false;

			// At this point, the issuer of the resource's required claims is a match for the issuer of the Principal's claims and
			// the Principal has a claim for each of the resource's required claims.
			return true;

        }

        #endregion

        #region IPrincipal Members

		/// <summary>
		/// Gets the IIdentity of the Principal.
		/// </summary>
		IIdentity IPrincipal.Identity
        {
			get { return this.iIdentity; }
        }

		/// <summary>
		/// Determines if the Security Principal is a member of the given role.
		/// </summary>
		/// <param name="role">The name of the role.</param>
		/// <returns>true if the Principal is a member of the given role.</returns>
		bool IPrincipal.IsInRole(string role)
        {
            throw new NotSupportedException("ClaimsPrincipal does not implement role-based security checks.");
        }

        #endregion

    }

}

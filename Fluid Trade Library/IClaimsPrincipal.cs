namespace FluidTrade.Core
{

    using System.IdentityModel.Claims;
    using System.Security.Principal;

	/// <summary>
	/// Describes the claims that can be associated with a Security Principal.
	/// </summary>
	public interface IClaimsPrincipal : IPrincipal
	{

		/// <summary>
		/// Gets the claims associated with the Security Principal.
		/// </summary>
		ClaimSet Claims { get; }

		/// <summary>
		/// Gets the issuer of the claims associated with the Security Principal.
		/// </summary>
		ClaimSet Issuer { get; }

		/// <summary>
		/// Determines if the Security Principal has the required claims.
		/// </summary>
		/// <param name="requiredClaims">A set of claims that describes a Principal's access to resources.</param>
		/// <returns>true if the Principal has all of the required claims, false if it doesn't.</returns>
		bool HasRequiredClaims(ClaimSet claimSet);

	}

}

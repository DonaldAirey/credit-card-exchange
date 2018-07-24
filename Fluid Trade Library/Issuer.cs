namespace FluidTrade.Core
{

	using System;
	using System.IdentityModel.Claims;

	class Issuer
	{

		// Public Constant Fields
		private const string IssuerUri = "http://schemas.fluidtrade.com//issuer";
		private const string IssuerName = "Common";

		// Private Static Fields
		private static System.IdentityModel.Claims.ClaimSet claimSet;

		static Issuer()
		{

			// This ClaimSet represents the issuer of this policy and will qualify any set of claims.
			Issuer.claimSet = new DefaultClaimSet(Claim.CreateUriClaim(new Uri(Issuer.IssuerUri)),
				Claim.CreateNameClaim(Issuer.IssuerName));

		}

		public static ClaimSet ClaimSet
		{
			get { return Issuer.ClaimSet; }
		}

	}

}

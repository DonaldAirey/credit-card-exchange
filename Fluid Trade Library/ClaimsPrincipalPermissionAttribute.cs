namespace FluidTrade.Core
{

	using System;
	using System.IdentityModel.Claims;
	using System.Security;
	using System.Security.Permissions;

	/// <summary>
	/// Specifies the code access for a Claims-Based security model.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ClaimsPrincipalPermissionAttribute : CodeAccessSecurityAttribute
    {

		// Public Instanc Fields
		public System.Boolean IsAuthenticated;
		public String ClaimType;
		public String Resource;

		/// <summary>
		/// Create a specification for the access to code for a Claims-Based security model.
		/// </summary>
		/// <param name="action"></param>
		public ClaimsPrincipalPermissionAttribute(SecurityAction action)
            : base(action)
        {

			// Initialize the object
            this.IsAuthenticated = true;

        }

		/// <summary>
		/// Create the permissions needed to access code.
		/// </summary>
		/// <returns>The permission required of a thread before it can execute code.</returns>
		public override IPermission CreatePermission()
        {

			// Unrestricted access allows any Principal to execute the code that follows.
			if (this.Unrestricted)
                return new ClaimsPrincipalPermission(PermissionState.Unrestricted);

			// This constructs an explicit permission that is needed by the thread's Principal in order for execution to continue.
			// If the thread doesn't posses this set of claims and is not authenticated, then an exception will be thrown.
			ClaimSet claimSet = new DefaultClaimSet(ClaimsAuthorizationPolicy.IssuerClaimSet,
				new Claim(this.ClaimType, this.Resource, Rights.PossessProperty));
            return new ClaimsPrincipalPermission(this.IsAuthenticated, claimSet);

        }
        
    }

}

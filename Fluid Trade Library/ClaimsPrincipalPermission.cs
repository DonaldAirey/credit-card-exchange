namespace FluidTrade.Core
{

    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
	/// Manages access to a resource based on a set of claims.
	/// </summary>
	public class ClaimsPrincipalPermission : IPermission, IUnrestrictedPermission
    {

		// Private Instance Fields
        private System.Boolean isAuthenticated;
		private System.Boolean isUnrestricted;
		private System.IdentityModel.Claims.ClaimSet requiredClaims;

		/// <summary>
		/// Create an object to manage the resources based on a set of claims.
		/// </summary>
		/// <param name="state">The allowed permission state (only 'Unrestricted' is allowed).</param>
		public ClaimsPrincipalPermission(PermissionState state)
		{

			// This prevents sloppy programmers from creating an unstable state.  While it may seem superfluous to allow any
			// parameter at all when only the PermissionState.Unrestricted is allowed, it makes the declaration of the permission
			// easier to read.
			if (state != PermissionState.Unrestricted)
				throw new ArgumentException("Only PermissionState.Unrestricted is legal for this constructor");

			// Initialize the object
			this.isUnrestricted = true;
			this.isAuthenticated = false;
			this.requiredClaims = new DefaultClaimSet();

		}

		/// <summary>
		/// Create an object to manage the resources based on a set of claims.
		/// </summary>
		/// <param name="isAuthenticated">Indicates whether the Principal needs to be authenticated.</param>
		/// <param name="requiredClaims">A Set of Claims the Principal must have to use a resource.</param>
		public ClaimsPrincipalPermission(bool isAuthenticated, ClaimSet requiredClaims)
		{

			// Initialize the object
			this.isUnrestricted = false;
			this.isAuthenticated = isAuthenticated;
			this.requiredClaims = requiredClaims;

		}

		/// <summary>
		/// Indicates whether the Permission requires the Principal to be authenticated.
		/// </summary>
		public bool IsAuthenticated
        {
            get { return this.isAuthenticated; }
        }

		/// <summary>
		/// A set of claims that uniquely identify the issuer of rights and resources.
		/// </summary>
        public ClaimSet Issuer
        {
			get { return this.requiredClaims.Issuer; }
        }

		/// <summary>
		/// A set of claims that must be satisfied in order to use a resource.
		/// </summary>
		public ClaimSet RequiredClaims
        {
            get { return this.requiredClaims; }
        }

		/// <summary>
		/// Indicates whether a ClaimsPrincipalPermission is equivalent to another.
		/// </summary>
		/// <param name="obj">The other object to be compared.</param>
		/// <returns>true if the two objects are equivalent.</returns>
		public override bool Equals(object obj)
        {

			// If both objects are subsets of each other, then they are equivalent.
			if (obj is IPermission)
			{
				IPermission iPermission = obj as IPermission;
				return this.IsSubsetOf(iPermission) && iPermission.IsSubsetOf(this);
			}

			// At this point, the two objects can not be compared.
			return false;

        }

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A value that can be used as an index into a hash table.ReaderWriterLock./returns>
		public override int GetHashCode()
        {

			// Combine all the fields into a composite hash code.
            int hashCode = this.isAuthenticated.GetHashCode();
            hashCode += this.requiredClaims.GetHashCode();
            hashCode += this.isUnrestricted.GetHashCode();
            return hashCode;

        }

		/// <summary>
		/// Determines whether the issuer of a ClaimSet is the same as the one associated with this permission scheme.
		/// </summary>
		/// <param name="target">The target ClaimSet containing an Issuer.</param>
		/// <returns>True if the issuer of the claim set is equivalent to the issuer of the required claims.</returns>
		protected bool IsExactIssuerMatch(ClaimSet claimSet)
		{

			// For an exact match, the two sets must at least have the same number of claims in the issuer's ClaimSet.
			if (claimSet.Issuer.Count != this.requiredClaims.Issuer.Count)
				return false;

			// If any of the claims in the target issuer fail to match the claims of the required set of claim's issuer, then the
			// two issuers are not equivalent.
			foreach (Claim claim in claimSet.Issuer)
				if (!this.requiredClaims.Issuer.ContainsClaim(claim))
					return false;

			// At this point, the source and target issuer have the same number of claims and each set has the same claims.
			return true;

		}

        #region IPermission Members

		/// <summary>
		/// Makes a copy of this ClaimsPrincipalPermission.
		/// </summary>
		/// <returns>A copy of the ClaimsPrincipalPermission object.</returns>
		public IPermission Copy()
        {

			// Unrestricted access does not require the user to have a role in order to access a resource.
            if (this.isUnrestricted)
                return new ClaimsPrincipalPermission(PermissionState.Unrestricted);

			// Most access will require a user to be authenticated and qualified with a set of claims that describe the rights of
			// the thread owning the Principal.
			return new ClaimsPrincipalPermission(this.isAuthenticated, this.requiredClaims);

        }

		/// <summary>
		/// Throws a System.Security.SecurityException at run time if the security requirement is not met.
		/// </summary>
		public void Demand()
        {

			// The operation is rejected if the Principal hasn't been set by the customer Authentication Manager.
            IClaimsPrincipal iClaimsPrincipal = Thread.CurrentPrincipal as IClaimsPrincipal;
            if (iClaimsPrincipal == null)
                throw new SecurityException("Access is denied. Security principal should be a IClaimSetPrincipal type.");

			// If the permission requires the user to be authenticated and they aren't, then throw the exception back at the 
			// caller.
			if (this.isAuthenticated && !iClaimsPrincipal.Identity.IsAuthenticated)
				throw new SecurityException("Access is denied. Security principal is not authenticated.");

			// If claims are required for access to some resource and the principal doesn't supply them, then throw the operation
			// back at the caller.
			if (this.requiredClaims != null && !iClaimsPrincipal.HasRequiredClaims(this.requiredClaims))
				throw new SecurityException("Access is denied. Security principal does not satisfy required claims.");

        }

		/// <summary>
		/// Creates and returns a permission that is the intersection of the current permission and the specified permission.
		/// </summary>
		/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current
		/// permission.</param>
		/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This
		/// new permission is null if the intersection is empty.</returns>
		public IPermission Intersect(IPermission target)
        {

			// An intersection operation isn't possible unless both operands employ the same permissioning scheme.
			if (target is ClaimsPrincipalPermission)
			{

				// This is the source Permission scheme for the operation.
				ClaimsPrincipalPermission targetPermission = target as ClaimsPrincipalPermission;

				// If this permission scheme allows unrestricted access, then it has nothing it can add to the intersection of the
				// two schemes.
				if (this.IsUnrestricted())
					return target.Copy();

				// If the other permission scheme is unrestricted, it has nothing to add to the intersection of the two schemes.
				if (targetPermission.IsUnrestricted())
					return this.Copy();

				// Both permission schemes must share the requirement that the user is authenticated and both issuers must be the
				// same for the claims to be merged.
				if (this.isAuthenticated == targetPermission.IsAuthenticated && IsExactIssuerMatch(targetPermission.Issuer))
				{

					// This creates a list that is the intersection of the claims in the two permission schemes.
					List<Claim> claims = new List<Claim>();
					foreach (Claim claim in this.requiredClaims)
						if (targetPermission.RequiredClaims.ContainsClaim(claim))
							claims.Add(claim);

					// Once the set of interesecting claims is created, the permission scheme can be created.  It is known at this 
					// point that the authenticated state of the user and the issuer are equivalent.
					return new ClaimsPrincipalPermission(this.isAuthenticated, new DefaultClaimSet(this.requiredClaims.Issuer,
						claims));

				}

			}

			// An intersection of the two permission schemes could not be constructed at this point.
			return null;

        }

		/// <summary>
		/// Determines whether the current permission is a subset of the specified permission.
		/// </summary>
		/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same
		/// type as the current permission.</param>
		/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
		public bool IsSubsetOf(IPermission target)
        {

			// A subset operation isn't possible unless both operands employ the same permissioning scheme.
			if (target is ClaimsPrincipalPermission)
			{

				// This is the source Permission scheme for the operation.
				ClaimsPrincipalPermission targetPermission = target as ClaimsPrincipalPermission;

				// Unrestricted permission sets are basically the set of every claim, so this permission set, no matter what is in
				// it, is a subset of everything.
				if (targetPermission.IsUnrestricted())
					return true;

				// At this point we know that the target set contains a finite number of claims.  If this permission set contains 
				// an inifinite number of claims, it can't be a subset of the target.
				if (this.IsUnrestricted())
					return false;

				// The two sets of permissions can only be compared if they require the same authentication state for the two
				// Principals and the issuers of both sets are exactly the same.
				if (this.isAuthenticated == targetPermission.IsAuthenticated && IsExactIssuerMatch(targetPermission.Issuer))
				{

					// Any claim in the required set that doesn't belong to the target set of permissions will disqualify the
					// required ClaimSet as a subset of the target.
					foreach (Claim claim in this.requiredClaims)
						if (!targetPermission.RequiredClaims.ContainsClaim(claim))
							return false;

					// At this point, the required ClaimSet is a subset of the target set of permissions.
					return true;

				}

			}

			// The required ClaimSet is not a subset of the target at this point.
			return false;

        }

		/// <summary>
		/// Creates a permission that is the union of the current permission and the specified permission.
		/// </summary>
		/// <param name="target">A permission to combine with the current permission. It must be of the same type as the
		/// current permission.</param>
		/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
		public IPermission Union(IPermission target)
        {

			// A union operation isn't possible unless both operands employ the same permissioning scheme.
			if (target is ClaimsPrincipalPermission)
			{

				// This is the source Permission scheme for the operation.
				ClaimsPrincipalPermission targetPermission = target as ClaimsPrincipalPermission;

				// If either premission contains an infinite set of claims, the union is an infinite (unrestricted) permission set.
				if (targetPermission.IsUnrestricted() || this.IsUnrestricted())
					return new ClaimsPrincipalPermission(PermissionState.Unrestricted);

				// The two sets of permissions can only be merged if they require the same authentication state for the two
				// Principals and the issuers of both sets are exactly the same.
				if (this.isAuthenticated == targetPermission.IsAuthenticated && IsExactIssuerMatch(targetPermission.Issuer))
				{

					// This creates a list of claims that is the union between the two sets of permissions.
					List<Claim> claims = new List<Claim>();
					foreach (Claim claim in this.requiredClaims)
						claims.Add(claim);
					foreach (Claim claim in targetPermission.RequiredClaims)
						if (!claims.Contains(claim))
							claims.Add(claim);

					// The state of authentication and the issuer are the same at this point.  This will create and return a
					// permission that is the union of the claims of the two permissions.
					return new ClaimsPrincipalPermission(this.isAuthenticated, new DefaultClaimSet(this.requiredClaims.Issuer,
						claims));

				}

			}

			// At this point, the two premissions couldn't be merged.
			return null;

        }

        #endregion

        #region ISecurityEncodable Members

		/// <summary>
		/// Converts XML into a permission.
		/// </summary>
		/// <param name="e">An XML description of the permission.</param>
		public void FromXml(SecurityElement e)
        {
            throw new NotSupportedException("ClaimsPrincipalPermission cannot be loaded from XML.");
        }

		/// <summary>
		/// Convert the permissions to XML.
		/// </summary>
		/// <returns>A Security Element</returns>
		public SecurityElement ToXml()
        {
            throw new NotSupportedException("ClaimsPrincipalPermission cannot be saved to XML.");
        }

        #endregion

        #region IUnrestrictedPermission Members

		/// <summary>
		/// Gets an indication of the ability of the permission to allow any Principal to access the resources.
		/// </summary>
		/// <returns></returns>
		public bool IsUnrestricted()
        {
            return this.isUnrestricted;
        }

        #endregion

    }
}

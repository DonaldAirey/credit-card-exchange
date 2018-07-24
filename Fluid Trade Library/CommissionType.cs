namespace FluidTrade.Core
{

	/// <summary>
	/// Identifies the different ways commissions can be calculated.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum CommissionType
	{
		/// <summary>Undefined commission type.</summary>
		Empty,
		/// <summary>The commission is applied as a flat fee.</summary>
		Fee,
		/// <summary>The commission is applied as a percent.</summary>
		Percent,
		/// <summary>The commission is applied in basis points.</summary>
		BasisPoint
	}

}

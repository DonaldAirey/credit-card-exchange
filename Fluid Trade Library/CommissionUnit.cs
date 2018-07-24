namespace FluidTrade.Core
{

	/// <summary>
	/// Identifies the different units in which the range of a commission tranche is specified.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum CommissionUnit
	{
		/// <summary>The range isn't specified.</summary>
		Empty,
		/// <summary>The range is specified in share units.</summary>
		Shares,
		/// <summary>The range is specified in face value.</summary>
		Face,
		/// <summary>The range is specified in market value.</summary>
		MarketValue
	}

}

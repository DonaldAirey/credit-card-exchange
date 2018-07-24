namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// The different units a period of time can be represented in.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum SettlementUnit
	{
		/// <summary>Undefined settlement type.</summary>
		Empty,
		/// <summary>The settlement is specified in basis points.</summary>
		BasisPoint,
		/// <summary>The settlement is specified in market value.</summary>
		MarketValue,
		/// <summary>The settlement is specified as a percent.</summary>
		Percent
	}

}

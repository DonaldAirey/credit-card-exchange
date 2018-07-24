namespace FluidTrade.Core
{
	/// <summary>
	/// Describes the various way an order for a security can specify the price at which a transaction can take place.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum OrderType
	{
		Market,
		Limit,
		Stop,
		StopLimit,
		MarketOnClose,
		WithOrWithout,
		LimitOrBetter,
		LimitWithOrWithout,
		OnBasis,
		OnClose,
		LimitOnClose,
		PreviouslyQuoted,
		PreviouslyIndicated,
		ForexLimit,
		ForexSwap,
		ForexPreviouslyIndicated,
		Funarii,
		MarketIfTouched,
		MarketWithLeftoverAsLimit,
		PreviousFundValuationPoint,
		NextFundValuationPoint,
		Pegged
	}
}

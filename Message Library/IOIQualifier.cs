using System;

namespace FluidTrade.Core
{

	/// <summary>
	/// FIX IOIQualifier.
	/// </summary>
	[Serializable()]
	public enum IOIQualifier
	{
		AllOrNone = 0,
		AtTheClose = 1,
		InTouchWith = 2,
		Limit = 3,
		MoreBehind = 4,
		AtTheOpen = 5,
		TakingPosition = 6,
		AtTheMarket = 7,
		ReadyToTrade = 8,
		PortfolioShown = 9,
		ThroughTheDay = 10,
		Versus = 11,
		WorkingAway = 12,
		CrossingOpportunity = 13,
		AtTheMidpoint = 14,
		PreOpen  = 15
	}
}

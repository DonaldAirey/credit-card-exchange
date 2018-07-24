namespace FluidTrade.Core 
{

	using System;

	/// <summary>Don't Know (Trade) Reason</summary>
	[Serializable()]
	public enum DKReason
	{
		UnknownSymbol = 0,
		WrongSide = 1,
		QuantityExceedsOrder = 2,
		NoMatchingOrder = 3,
		PriceExceedsLimit = 4,
		Other = 5
	}

}

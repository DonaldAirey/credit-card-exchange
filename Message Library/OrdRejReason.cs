namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Summary description for OrdRejReason.
	/// </summary>
	[Serializable()]
	public enum OrdRejReason
	{
		BrokerExchangeOption = 0,
		UnknownSymbol = 1,
		ExchangeClosed = 2,
		OrderExceedsLimit = 3,
		TooLateToEnter = 4,
		UnknownOrder = 5,
		DuplicateOrder = 6,
		DuplicateOfVerballyCommunicatedOrder = 7,
		StaleOrder = 8,
		TradeAlongRequired = 9,
		InvalidInvestorID = 10,
		UnsupportedOrderCharacteristic = 11,
		SurveillenceOption = 12,
		IncorrectQuantity = 13,
		IncorrectAllocatedQuantity = 14,
		UnknownAccount = 15,
		Other = 16
	}

}

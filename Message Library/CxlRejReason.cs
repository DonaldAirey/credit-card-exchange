namespace FluidTrade.Core 
{

	using System;

	/// <summary>Cancel Reject Reason</summary>
	[Serializable()]
	public enum CxlRejReason
	{
		TooLateToCancel = 0,
		UnknownOrder = 1,
		BrokerOption = 2,
		AlreadyPending = 3
	}

}

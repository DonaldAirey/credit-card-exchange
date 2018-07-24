namespace FluidTrade.Core
{

	using System;

	/// <summary>Execution Types for a FIX Message</summary>
	[Serializable()]
	public enum ExecType
	{
		New = 0,
		PartialFill = 1,
		Fill = 2,
		DoneForDay = 3,
		Canceled = 4,
		Replace = 5,
		PendingCancel = 6,
		Stopped = 7,
		Rejected = 8,
		Suspended = 9,
		PendingNew = 10,
		Calculated = 11,
		Expired = 12,
		Restated = 13,
		PendingReplace = 14,
		Trade = 15,
		TradeCorrect = 16,
		TradeCancel = 17,
		OrderStatus = 18
	}

}

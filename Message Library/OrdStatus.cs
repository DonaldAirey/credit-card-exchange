namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Summary description for OrdStatus.
	/// </summary>
	[Serializable()]
	public enum OrdStatus
	{
		New = 0,
		PartiallyFilled = 1,
		Filled = 2,
		DoneForDay = 3,
		Canceled = 4,
		Replaced = 5,
		PendingCancel = 6,
		Stopped = 7,
		Rejected = 8,
		Suspended = 9,
		PendingNew = 10,
		Calculated = 11,
		Expired = 12,
		AcceptedForBidding = 13,
		PendingReplace = 14
	}

}

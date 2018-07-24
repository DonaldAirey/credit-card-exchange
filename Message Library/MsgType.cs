namespace FluidTrade.Core
{

	using System;

	/// <summary>Field: MsgType</summary>
	[Serializable()]
	public enum MsgType
	{
		Heartbeat = 0,
		TestRequest = 1,
		ResendRequest = 2,
		Reject = 3,
		SequenceReset = 4,
		Logout = 5,
		IndicationOfInterest = 6,
		Advertisement = 7,
		ExecutionReport = 8,
		OrderCancelReject = 9,
		Login = 10,
		News = 11,
		Email = 12,
		Order = 13,
		OrderCancelRequest = 14,
		OrderCancelReplaceRequest = 15,
		OrderStatusRequest = 16,
		Allocation = 17,
		AllocationAck = 18,
		DontKnowTrade = 19,
		BusinessMessageReject = 20,
		NullMsgType = 9999
	}

}

namespace FluidTrade.Core 
{

	using System;

	/// <summary>Cancel Reject Reason</summary>
	[Serializable()]
	public enum CxlRejResponseTo
	{
		CancelRequest = 1,
		CancelReplaceRequest  = 2
	}

}

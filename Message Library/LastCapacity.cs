namespace FluidTrade.Core 
{

	using System;

	/// <summary>Last Capacity: Broker capacity in order execution.</summary>
	[Serializable()]
	public enum LastCapacity
	{
		Agent = 1,
		CrossAsAgent = 2,
		CrossAsPrincipal = 3,
		Principal = 4
	}

}

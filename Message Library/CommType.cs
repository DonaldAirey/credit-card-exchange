namespace FluidTrade.Core 
{

	using System;

	/// <summary>Commission Type</summary>
	[Serializable()]
	public enum CommType
	{
		PerShare = 1,
		Percentage = 2,
		Absolute = 3
	}

}

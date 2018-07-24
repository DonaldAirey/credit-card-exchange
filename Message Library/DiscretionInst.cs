namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// The Code to identify the price a DiscretionOffset is related to 
	/// and should be mathematically added to.
	/// </summary>
	[Serializable()]
	public enum DiscretionInst
	{
		/*Related To: */ Displayed = 0,
		/*Related To: */ Market = 1,
		/*Related To: */ Primary = 2,
		/*Related To: */ LocalPrimary = 3,
		/*Related To: */ Midpoint = 4,
		/*Related To: */ LastTrade = 5
	}

}

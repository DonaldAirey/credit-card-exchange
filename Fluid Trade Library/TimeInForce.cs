namespace FluidTrade.Core
{
	/// <summary>
	/// The time for which an order is valid.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum TimeInForce
	{
		Day,
		GoodTillCancel,
		AtTheOpening,
		ImmediateOrCancel,
		FillOrKill,
		GoodTillCrossing,
		GoodTillDate,
		AtTheClose
	}
}

namespace FluidTrade.Core
{
	/// <summary>
	/// The conditions for executing an order.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum Condition
	{
		/// <summary>Execute all the order or none of it.</summary>
		AllOrNone,
		/// <summary>Do Not Reduce</summary>
		DoNotReduce,
		/// <summary>All or None and Do Not Reduce</summary>
		AllOrNoneDoNotReduce
	}
}

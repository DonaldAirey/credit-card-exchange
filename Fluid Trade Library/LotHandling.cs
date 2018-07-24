namespace FluidTrade.Core
{
	/// <summary>
	/// Defines the duration of the order
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum LotHandling
	{
		/// <summary>Last In First Out</summary>
		LIFO,
		/// <summary>First In First Out</summary>
		FIFO,
		/// <summary>Minimize Tax Impact</summary>
		MINTAX
	}
}

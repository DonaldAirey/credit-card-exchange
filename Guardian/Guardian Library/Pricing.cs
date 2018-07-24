namespace FluidTrade.Guardian
{
	/// <summary>
	/// Used to specify which price to use in a calculation or display.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum Pricing
	{
		/// <summary>Use the real-time price.</summary>
		Last,
		/// <summary>Use last night's closing price.</summary>
		Close,
		/// <summary>Use the price at the start of the month.</summary>
		StartOfMonth,
		/// <summary>Use the price at the start of the year.</summary>
		StartOfYear
	}
}

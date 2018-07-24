namespace FluidTrade.Guardian
{
	/// <summary>
	/// Identifies blocking styles used by blotter to group order into blocks.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum BlockingStyle
	{
		/// <summary>No blocking</summary>
		None,
		/// <summary>Group trades by the security.</summary>
		Security,
		/// <summary>Group trades by security and settlement currency.</summary>
		SecurityCurrency,
		/// <summary>Group trades by account, security and settlement currency.</summary>
		AccountSecurityCurrency
	}
}

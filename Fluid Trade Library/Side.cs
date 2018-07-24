namespace FluidTrade.Core
{
	/// <summary>
	/// The types of transactions allowed by this order management system.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum Side
	{
		/// <summary>Buy a security.</summary>
		Buy,
		/// <summary>Sell a security.</summary>
		Sell,
		/// <summary>Buy on the next minus tick.</summary>
		BuyMinus,
		/// <summary>Covers a previous short sale.</summary>
		BuyCover,
		/// <summary>Sell on the next plus tick.</summary>
		SellPlus,
		/// <summary>Temporarily borrowing a security.</summary>
		SellShort,
		/// <summary>Sell Short Exempt from the Up-Tick Rule.</summary>
		SellShortExempt,
		/// <summary>The side is not disclosed.  Valid for IOI and List Order messages only.</summary>
		Undisclosed,
		/// <summary>Cross the order through an alternative trading source.</summary>
		Cross,
		/// <summary>Sell short on a crossing system.</summary>
		CrossShort,
		/// <summary>Sell short exempt from the Up-Tick Rule on a crossing system.</summary>
		CrossShortExempt,
		/// <summary>For use with a multileg instrument.</summary>
		AsDefined,
		/// <summary>For use with a multileg instrument.</summary>
		Opposite,
		/// <summary>Subscribe to a liquidity pool for the given security.</summary>
		Subscribe,
		/// <summary>Redeem a coupon.</summary>
		Redeem,
		/// <summary>Lend a security to another party.</summary>
		Lend,
		/// <summary>Borrow a security from another party.</summary>
		Borrow,
		/// <summary>A Deposit of currency into an account.</summary>
		Deposit,
		/// <summary>Withdraw of currency from an account.</summary>
		Withdraw,
	}
}

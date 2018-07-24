namespace FluidTrade.Core
{

	/// <summary>
	/// Types of groups possible.
	/// </summary>
	public enum GroupType
	{

		/// <summary>
		/// A global administrator group.
		/// </summary>
		FluidTradeAdmin = 0,
		/// <summary>
		/// An administrator group for a specific exchange.
		/// </summary>
		ExchangeAdmin = 1,
		/// <summary>
		/// An administrator group for a specific organization that uses an exchange.
		/// </summary>
		SiteAdmin = 2,
		/// <summary>
		/// A low privileges group specific to an organization.
		/// </summary>
		User,
		/// <summary>
		/// A custom, low privileges group specific to an organization.
		/// </summary>
		Custom

	}

}

namespace FluidTrade.Guardian
{

	using System;

	/// <summary>
	/// Indicates how an order can be crossed with another.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	[Flags]
	public enum Crossing
	{
		/// <summary>No choice has been made for matching.</summary>
		None = 0x00000000,
		/// <summary>Always submit for matching.</summary>
		AlwaysMatch = 0x00000001,
		/// <summary>User the trader preferences to decide whether the order is submitted for matching.</summary>
		UsePreferences = 0x00000002,
		/// <summary>Never submit the order for matching.</summary>
		NeverMatch = 0x00000004,
		/// <summary>The trader is away.  No matching should take place, but the original submission type should be preserved.</summary>
		Away = 0x00000008,
		/// <summary>Automatically route an order to its destination while matching.</summary>
		RouteToDestination = 0x00000010
	}

}

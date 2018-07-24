namespace FluidTrade.Core
{
	/// <summary>
	/// <summary>Indicates what kind of financial entity is trading a security.</summary>
	/// <summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum PartyType
	{
		/// <summary>A broker that acts only as an agent.</summary>
		Agency,
		/// <summary>A Broker that acts as either principal or agent.</summary>
		Broker,
        /// <summary>A container for consumer debt.</summary>
        DebtClass,
        /// <summary>A consumer debt negotiator.</summary>
        DebtNegotiator,
        /// <summary>A holder of consumer debt.</summary>
        DebtHolder,
		/// <summary>A Hedge Fund.</summary>
		Hedge,
		/// <summary>An Intitution</summary>
		Instutition,
		/// <summary>Use the parent setting in a hierarchy of financial entities.</summary>
		UseParent,
		/// <summary>This party can't participate in a cross.</summary>
		NotValid
	}
}

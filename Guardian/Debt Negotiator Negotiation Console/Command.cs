namespace FluidTrade.Guardian
{

	using System;

	/// <summary>
	/// Enumerations of the various states associated with the GUI update of the negotiation session.
	/// </summary>
	public enum Command
	{
		/// <summary>
		/// Uninitialized state.
		/// </summary>
		None,
		/// <summary>
		/// Indicates that the GUI is initialized for the first time.
		/// </summary>
		Initialize,
		/// <summary>
		/// Indicates that the chat portion of the negotiation has changed.
		/// </summary>
		ChatChanged,
		/// <summary>
		/// Indicates that the settlement information of the negotiation has changed.
		/// </summary>
		SettlementChanged
	};

}

namespace FluidTrade.Guardian
{
	/// <summary>
	/// Status Codes for Working Order.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum Status
	{
		/// <summary>
		/// The object is active in the crossing network.
		/// </summary>
		Active,
		/// <summary>
		/// The object has been successfully negotiated by both counter parties.
		/// </summary>
		Accepted,
		/// <summary>
		/// The object has been accepted for dark pool matching.
		/// </summary>
		AcceptedForBidding,
		/// <summary>
		/// The object was canceled.
		/// </summary>
		Canceled,
		/// <summary>
		/// The object is closed and ready for archiving.
		/// </summary>
		Closed,
		/// <summary>
		/// The object has been confirmed by both sides of a transaction.
		/// </summary>
		Confirmed,
		/// <summary>
		/// An attempt to trade the object with a counter party was declined.
		/// </summary>
		Declined,
		/// <summary>
		/// The object has been deleted.
		/// </summary>
		Deleted,
		/// <summary>
		/// The object is done for the day.
		/// </summary>
		DoneForDay,
		/// <summary>
		/// The object has one or more errors.
		/// </summary>
		Error,
		/// <summary>
		/// The object is uninitialized.
		/// </summary>
		Empty,
		/// <summary>
		/// The time period for an object has expired.
		/// </summary>
		Expired,
		/// <summary>
		/// The object has been completely executed.
		/// </summary>
		Filled,
		/// <summary>
		/// The object is locked until some action releases it.
		/// </summary>
		Locked,
		/// <summary>
		/// The object is negotiating with a contra party.
		/// </summary>
		Negotiating,
		/// <summary>
		/// The object is newly created.
		/// </summary>
		New,
		/// <summary>
		/// The offer has been accepted by the counter party.
		/// </summary>
		OfferAccepted,
		/// <summary>
		/// The object is awaiting instructions.
		/// </summary>
		Open,
		/// <summary>
		/// The object is waiting for a response.
		/// </summary>
		Pending,
		/// <summary>
		/// The object is waiting for a cancelation confirmation.
		/// </summary>
		PendingCancel,
		/// <summary>
		/// The object is waiting for confirmation of a new object.
		/// </summary>
		PendingNew,
		/// <summary>
		/// The object is waiting for confirmation that it was was replaced.
		/// </summary>
		PendingReplace,
		/// <summary>
		/// The object has been partially completed.
		/// </summary>
		PartiallyFilled,
		/// <summary>
		/// The object represents a partial match against a counter party.
		/// </summary>
		PartialMatch,
		/// <summary>
		/// The object was rejected by the destination.
		/// </summary>
		Rejected,
		/// <summary>
		/// The object was replaced by another object.
		/// </summary>
		Replaced,
		/// <summary>
		/// The object was prevented from executing.
		/// </summary>
		Stopped,
		/// <summary>
		/// The object has been submitted to a crossing network.
		/// </summary>
		Submitted,
		/// <summary>
		/// The object has been suspended.
		/// </summary>
		Suspended,
		/// <summary>
		/// The object exactly matches another from a counter party.
		/// </summary>
		ValidMatch,
		/// <summary>
		/// The object exactly matches another from a counter party and the client has sufficient to cover the settlement.
		/// </summary>
		ValidMatchFunds

	}

}

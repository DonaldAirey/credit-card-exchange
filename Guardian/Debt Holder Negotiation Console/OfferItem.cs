namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Used to communicate an offer to the background thread.
	/// </summary>
	public class OfferItem
	{

		/// <summary>
		/// The matchId that identifies this negotiation session.
		/// </summary>
		public Guid MatchId;

		/// <summary>
		/// The number of payments offered by the counter party.
		/// </summary>
		public Decimal PaymentLength;

		/// <summary>
		/// The duration of the payment offered by the counter party.
		/// </summary>
		public Decimal PaymentStartDateLength;

		/// <summary>
		/// The unit associated with the duration of the payments offered by self.
		/// </summary>
		public Guid PaymentStartDateUnitId;

		/// <summary>
		/// The offer units of the settlement value.
		/// </summary>
		public Guid SettlementUnitId;

		/// <summary>
		/// The offer party settlement value.
		/// </summary>
		public Decimal SettlementValue;

		/// <summary>
		/// The list of available payment methods offered by self.
		/// </summary>
		public List<Guid> PaymentMethods;

		/// <summary>
		/// State of the negotiaion
		/// </summary>
		public Guid StatusId;

		/// <summary>
		/// Creates a structure used to communicate an offer.
		/// </summary>
		public OfferItem()
		{

			// Initialize the object.
			this.PaymentMethods = new List<Guid>();

		}

	}

}
namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Used to communicate Settlement information to the foreground thread.
	/// </summary>
	public class NegotiationItem
	{

		/// <summary>
		/// The account balance of the debt negotiator.
		/// </summary>
		public Decimal AccountBalance;

		/// <summary>
		/// The number of days until the debtor begins paying.
		/// </summary>
		public Decimal CounterDays;

		/// <summary>
		/// The number of payments offered by the counter party.
		/// </summary>
		public Decimal CounterPaymentLength;

		/// <summary>
		/// The duration of the payment offered by the counter party.
		/// </summary>
		public Decimal CounterPaymentStartDateLength;

		/// <summary>
		/// The unit associated with the duration of the payments offered by the counter party.
		/// </summary>
		public Guid CounterPaymentStartDateUnitId;

		/// <summary>
		/// The counter party units of the settlement value.
		/// </summary>
		public Guid CounterSettlementUnitId;

		/// <summary>
		/// The counter party value of the settlement.
		/// </summary>
		public Decimal CounterSettlementValue;

		/// <summary>
		/// The matchId that identifies this negotiation session.
		/// </summary>
		public Guid MatchId;

		/// <summary>
		/// The number of days until the debtor begins paying.
		/// </summary>
		public Decimal OfferDays;

		/// <summary>
		/// The number of payments offered by the counter party.
		/// </summary>
		public Decimal OfferPaymentLength;

		/// <summary>
		/// The duration of the payment offered by the counter party.
		/// </summary>
		public Decimal OfferPaymentStartDateLength;

		/// <summary>
		/// The unit associated with the duration of the payments offered by self.
		/// </summary>
		public Guid OfferPaymentStartDateUnitId;

		/// <summary>
		/// The offer units of the settlement value.
		/// </summary>
		public Guid OfferSettlementUnitId;

		/// <summary>
		/// The offer party settlement value.
		/// </summary>
		public Decimal OfferSettlementValue;

		/// <summary>
		/// The offer party settlement value.
		/// </summary>
		public Status StatusCode;

		/// <summary>
		/// Copies information from the ConsumerTrustNegotiation record.
		/// </summary>
		/// <param name="consumerDebtNegotiationRow">The source row.</param>
		public NegotiationItem(ConsumerTrustNegotiationRow consumerTrustNegotiationRow)
		{

			// This extracts the negotiation information from the source record.
			this.AccountBalance = consumerTrustNegotiationRow.AccountBalance;
			this.CounterPaymentLength = consumerTrustNegotiationRow.CounterPaymentLength;
			this.CounterPaymentStartDateLength = consumerTrustNegotiationRow.CounterPaymentStartDateLength;
			this.CounterPaymentStartDateUnitId = consumerTrustNegotiationRow.CounterPaymentStartDateUnitId;
			this.CounterSettlementUnitId = consumerTrustNegotiationRow.CounterSettlementUnitId;
			this.CounterSettlementValue = consumerTrustNegotiationRow.CounterSettlementValue;
			this.MatchId = consumerTrustNegotiationRow.MatchId;
			this.OfferPaymentLength = consumerTrustNegotiationRow.OfferPaymentLength;
			this.OfferPaymentStartDateLength = consumerTrustNegotiationRow.OfferPaymentStartDateLength;
			this.OfferPaymentStartDateUnitId = consumerTrustNegotiationRow.OfferPaymentStartDateUnitId;
			this.OfferSettlementUnitId = consumerTrustNegotiationRow.OfferSettlementUnitId;
			this.OfferSettlementValue = consumerTrustNegotiationRow.OfferSettlementValue;			

			// One side may be quoting in days and the other side may be quoting in months.  In order to compare the two quotes to see if there is a match, the
			// time units must be decoded for each side.
			TimeUnit counterTimeUnit = default(TimeUnit);
			TimeUnit offerTimeUnit = default(TimeUnit);
			lock (DataModel.SyncRoot)
			{

				// Find the time units quoted by the debt negotiator.
				TimeUnitRow counterTimeUnitRow = DataModel.TimeUnit.TimeUnitKey.Find(this.CounterPaymentStartDateUnitId);
				counterTimeUnit = counterTimeUnitRow.TimeUnitCode;

				// Find the time units quoted by the debt holder.
				TimeUnitRow offerTimeUnitRow = DataModel.TimeUnit.TimeUnitKey.Find(this.OfferPaymentStartDateUnitId);
				offerTimeUnit = offerTimeUnitRow.TimeUnitCode;

				StatusRow statusRow = DataModel.Status.StatusKey.Find(consumerTrustNegotiationRow.StatusId);
				this.StatusCode = statusRow.StatusCode;

			}

			// Convert the time unit qoted by the debt negotiator into a standard number of days.
			switch (counterTimeUnit)
			{
			case TimeUnit.Days:
				this.CounterDays = this.CounterPaymentStartDateLength;
				break;
			case TimeUnit.Months:
				this.CounterDays = this.CounterPaymentStartDateLength * 30;
				break;
			case TimeUnit.Weeks:
				this.CounterDays = this.CounterPaymentStartDateLength * 7;
				break;
			}

			// Convert the time unit quoted by the debt holder into a standard number of days.
			switch (offerTimeUnit)
			{
			case TimeUnit.Days:
				this.OfferDays = this.OfferPaymentStartDateLength;
				break;
			case TimeUnit.Months:
				this.OfferDays = this.OfferPaymentStartDateLength * 30;
				break;
			case TimeUnit.Weeks:
				this.OfferDays = this.OfferPaymentStartDateLength * 7;
				break;
			}

		}

	}

}
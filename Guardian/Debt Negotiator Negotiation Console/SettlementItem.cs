namespace FluidTrade.Guardian
{

	using FluidTrade.Core;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Used to communicate Settlement information to the foreground thread.
	/// </summary>
	public class SettlementItem
	{

		/// <summary>
		/// The negotiation parameters.
		/// </summary>
		public NegotiationItem NegotiationItem;

		/// <summary>
		/// All the conversation items associated with a negotiation.
		/// </summary>
		public List<ChatItem> ChatItemList;

		/// <summary>
		/// The list of available payment methods offered by the counter party.
		/// </summary>
		public List<Guid> CounterPaymentMethods;

		/// <summary>
		/// The list of available payment methods offered by this party.
		/// </summary>
		public List<Guid> OfferPaymentMethods;

		/// <summary>
		/// The status of the negotiation.
		/// </summary>
		public Status Status;

		/// <summary>
		/// An empty, deleted settlment.
		/// </summary>
		public SettlementItem()
		{

			// Initialize the object.
			this.ChatItemList = new List<ChatItem>();
			this.CounterPaymentMethods = new List<Guid>();
			this.OfferPaymentMethods = new List<Guid>();
			this.Status = Status.Deleted;

		}

		/// <summary>
		/// Creates a new SettlementInfo instance.
		/// </summary>
		public SettlementItem(Guid matchId)
		{

			// Initialize the object.
			this.ChatItemList = new List<ChatItem>();
			this.CounterPaymentMethods = new List<Guid>();
			this.OfferPaymentMethods = new List<Guid>();

			// The data model must be locked in order to navigate the data.  The main idea here is to find the chat items and the negotiation elements
			// associated with this match and send that data to the foreground where it can be displayed.
			lock (DataModel.SyncRoot)
			{

				// This dialog will only handle matches that have this identifier.
				MatchRow matchRow = DataModel.Match.MatchKey.Find(matchId);

				if (matchRow == null)
				{

					this.Status = Status.Deleted;

				}
				else
				{

					// The status code drives the state of many of the controls in the console.
					this.Status = matchRow.StatusRow.StatusCode;

					// The negotiation table has a historical component. Ever time a change is made to the negotiation on either side a completely new record
					// is created to record the change.  While the earlier versions are useful for a historical context and for reports, this console is only
					// interested in the current version of the negotiations.
					Int64 maxVersion = Int64.MinValue;
					ConsumerTrustNegotiationRow consumerTrustNegotiationRow = null;
					foreach (ConsumerTrustNegotiationRow versionRow in matchRow.GetConsumerTrustNegotiationRows())
					{
						if (versionRow.Version > maxVersion)
						{
							maxVersion = versionRow.Version;
							consumerTrustNegotiationRow = versionRow;
						}
					}

					// Extract the scalar items used in the negotiation of a settlement.
					this.NegotiationItem = new NegotiationItem(consumerTrustNegotiationRow);

					// The Offer Payment Methods are a vector and need to be copied iteratively.
					foreach (ConsumerTrustNegotiationOfferPaymentMethodRow paymentMethod in
						consumerTrustNegotiationRow.GetConsumerTrustNegotiationOfferPaymentMethodRows())
					{
						this.OfferPaymentMethods.Add(paymentMethod.PaymentMethodTypeId);
					}

					// The Conter Offer Payment methods are also a vector.
					foreach (ConsumerTrustNegotiationCounterPaymentMethodRow paymentMethod in
						consumerTrustNegotiationRow.GetConsumerTrustNegotiationCounterPaymentMethodRows())
					{
						this.CounterPaymentMethods.Add(paymentMethod.PaymentMethodTypeId);
					}

					// When the console is initialized it is populated with every dialog item that has occurred for this match.
					foreach (ChatRow chatRow in matchRow.GetChatRows())
					{
						ChatItem chatItem = new ChatItem();
						chatItem.CreatedTime = chatRow.CreatedTime;
						chatItem.IsReply = chatRow.IsReply;
						chatItem.MatchId = matchId;
						chatItem.Message = chatRow.Message;
						this.ChatItemList.Add(chatItem);
					}

				}

			}

		}

	}

}
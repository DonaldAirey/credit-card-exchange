namespace FluidTrade.Guardian.Records
{

	using System.Runtime.Serialization;
	using FluidTrade.Core;
	using System;

	/// <summary>
	/// A description of an offer made by a Debt Negotiator.
	/// </summary>
	[DataContract]
	public class ConsumerDebtNegotiationInfo
	{

		/// <summary>
		/// The unique identifier of the negotiation.
		/// </summary>
		[DataMember]
		public Guid ConsumerDebtNegotiationId { get; set; }

		/// <summary>
		/// The offered length of the payments.
		/// </summary>
		[DataMember]
		public Decimal PaymentLength { get; set; }

		/// <summary>
		/// The offered starting date for the settlement.
		/// </summary>
		[DataMember]
		public Decimal PaymentStartDateLength { get; set; }

		/// <summary>
		/// The offerend units for the starting date of the settlement.
		/// </summary>
		[DataMember]
		public Guid PaymentStartDateUnitId { get; set; }

		/// <summary>
		/// A list of acceptable payment methods for the offer.
		/// </summary>
		[DataMember]
		public Guid[] PaymentMethodTypes { get; set; }

		/// <summary>
		/// The offered settlement value.
		/// </summary>
		[DataMember]
		public Decimal SettlementValue { get; set; }

		/// <summary>
		/// The units of the offered settlement value.
		/// </summary>
		[DataMember]
		public Guid SettlementUnitId { get; set; }

		/// <summary>
		/// The Row Version of this record.
		/// </summary>
		[DataMember]
		public Int64 RowVersion;
		
		/// <summary>
		/// The Row Version of this record.
		/// </summary>
		[DataMember (IsRequired=true)]
		public Guid StatusId;

	}

}

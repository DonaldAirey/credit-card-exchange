namespace FluidTrade.Guardian.Records
{

	using System.Runtime.Serialization;
	using FluidTrade.Core;
	using System;

	/// <summary>
	/// A description of an offer made by a Debt Negotiator.
	/// </summary>
	[DataContract]
	public class ConsumerDebtNegotiationIsReadInfo
	{

		/// <summary>
		/// The unique identifier of the negotiation.
		/// </summary>
		[DataMember]
		public Guid ConsumerDebtNegotiationId { get; set; }

		/// <summary>
		/// Whether the counter offer has been read or not.
		/// </summary>
		[DataMember]
		public Object IsRead { get; set; }

		/// <summary>
		/// The Row Version of this record.
		/// </summary>
		[DataMember]
		public Int64 RowVersion;

	}

}

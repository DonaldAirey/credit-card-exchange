namespace FluidTrade.Guardian.Records
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;
	using System.Runtime.Serialization;

	/// <summary>
	/// Record for debt rules.
	/// </summary>
	[DataContract]
	public class DebtRule : BaseRecord
	{

		/// <summary>
		/// IsAutoSettled flag.
		/// </summary>
		[DataMember]
		public bool? IsAutoSettled { get; set; }

		/// <summary>
		/// Name of the rule.
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// The debt class that owns the rule.
		/// </summary>
		[DataMember]
		public Guid? Owner { get; set; }

		/// <summary>
		/// The payment length.
		/// </summary>
		[DataMember]
		public decimal? PaymentLength { get; set; }

		/// <summary>
		/// The payment methods.
		/// </summary>
		[DataMember]
		public Guid[] PaymentMethod { get; set; }

		/// <summary>
		/// The scalar portion of the payment start offset.
		/// </summary>
		[DataMember]
		public decimal? PaymentStartDateLength { get; set; }

		/// <summary>
		/// The units portion of the payment start offset.
		/// </summary>
		[DataMember]
		public Guid? PaymentStartDateUnitId { get; set; }

		/// <summary>
		/// The units the settlement value is in.
		/// </summary>
		[DataMember]
		public Guid? SettlementUnitId { get; set; }

		/// <summary>
		/// Percentage of face value to settle for.
		/// </summary>
		[DataMember]
		public decimal? SettlementValue { get; set; }

	}

}

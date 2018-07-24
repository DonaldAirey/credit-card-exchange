namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents a Debt Holder. 
	/// </summary>
	[DataContract]
	public class ConsumerDebt : BaseRecord
	{
		/// <summary>
		/// Collection Date
		/// </summary>
		[DataMember]
		public Boolean? CollectionDate { get; set; }
		/// <summary>
		/// Consumer debt Id
		/// </summary>
		[DataMember]
		public object ConsumerDebtId { get; set; }
		/// <summary>
		/// Consumer foriegn key
		/// </summary>
		[DataMember]
		public Guid? ConsumerId { get; set; }
		/// <summary>
		/// Credit card Id.
		/// </summary>
		[DataMember]
		public Guid? CreditCardId { get; set; }
		/// <summary>
		/// Date of Delinquency
		/// </summary>
		[DataMember]
		public object DateOfDelinquency { get; set; }
		/// <summary>
		/// Debt Rule associated with this Debt Holder.
		/// </summary>
		[DataMember]
		public object DebtRuleId { get; set; }
		/// <summary>
		/// External Id.
		/// </summary>
		[DataMember]
		public object ExternalId0 { get; set; }
		/// <summary>
		/// Representative
		/// </summary>
		[DataMember]
		public object Representative { get; set; }
		/// <summary>
		/// Tag
		/// </summary>
		[DataMember]
		public object Tag { get; set; }
		/// <summary>
		/// The fluidtrade tenant id for the vendor that owns this record.
		/// </summary>
		[DataMember]
		public Guid TenantId { get; set; }
		/// <summary>
		/// The fluidtrade vendor code for the vendor that owns this record.
		/// </summary>
		[DataMember]
		public String VendorCode;

	}
}

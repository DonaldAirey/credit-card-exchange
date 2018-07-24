namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents a ConsumerTrustRecord
	/// </summary>
	[DataContract]
	public class ConsumerTrust : BaseRecord
	{				
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid? ConsumerId;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public object ConsumerTrustId;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public object DebtRuleId;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public object ExternalId0;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public object SavingsAccount;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public object SavingsBalance;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public object Tag;
		/// <summary>
		/// The fluidtrade tenant id for the vendor that owns this record.
		/// </summary>
		[DataMember]
		public Guid TenantId;
		/// <summary>
		/// The fluidtrade vendor code for the vendor that owns this record.
		/// </summary>
		[DataMember]
		public String VendorCode;
		
	}
}

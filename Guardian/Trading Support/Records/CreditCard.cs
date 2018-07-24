namespace FluidTrade.Guardian.Records
{
	using System.Runtime.Serialization;
	using System;

	/// <summary>
	/// Credit Card.
	/// </summary>
	[DataContract]
	public class CreditCard : BaseRecord
	{
		/// <summary>
		/// Balance on the credit card.
		/// </summary>
		[DataMember]
		public object AccountBalance { get; set; }
		/// <summary>
		/// Balance on the credit card.
		/// </summary>
		[DataMember]
		public String AccountNumber { get; set; }

		/// <summary>
		/// Balance on the credit card.
		/// </summary>
		[DataMember]
		public Guid? ConsumerId { get; set; }		
		/// <summary>
		/// Balance on the credit card.
		/// </summary>
		[DataMember]
		public String DebtHolder { get; set; }
		/// <summary>
		/// Debt Rule associated with this
		/// </summary>
		[DataMember]
		public object DebtRuleId { get; set; } 					
		/// <summary>
		/// Balance on the credit card.
		/// </summary>
		[DataMember]
		public String OriginalAccountNumber { get; set; }

	}
}

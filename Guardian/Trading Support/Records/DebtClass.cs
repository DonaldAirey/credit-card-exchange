namespace FluidTrade.Guardian.Records
{

	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents a Debt Holder. 
	/// </summary>
	[DataContract]
	public class DebtClass : Blotter
	{

		/// <summary>
		/// First line of address.
		/// </summary>
		[DataMember]
		public String Address1 { get; set; }
		/// <summary>
		/// Second line of address.
		/// </summary>
		[DataMember]
		public object Address2 { get; set; }
		/// <summary>
		/// Bank account number.
		/// </summary>
		[DataMember]
		public object BankAccountNumber { get; set; }
		/// <summary>
		/// Bank routing number.
		/// </summary>
		[DataMember]
		public object BankRoutingNumber { get; set; }
		/// <summary>
		/// City.
		/// </summary>
		[DataMember]
		public String City { get; set; }
		/// <summary>
		/// Company name.
		/// </summary>
		[DataMember]
		public String CompanyName { get; set; }
		/// <summary>
		/// Contact name.
		/// </summary>
		[DataMember]
		public object ContactName { get; set; }
		/// <summary>
		/// Debt Rule associated with this Consumer Debt account.  If this is null, then it will
		/// use the parent's the debt rule.
		/// </summary>
		[DataMember]
		public object DebtRuleId { get; set; }
		/// <summary>
		/// Department.
		/// </summary>
		[DataMember]
		public object Department { get; set; }
		/// <summary>
		/// Email.
		/// </summary>
		[DataMember]
		public object Email { get; set; }
		/// <summary>
		/// Fax number.
		/// </summary>
		[DataMember]
		public object Fax { get; set; }
		/// <summary>
		/// For benefit of.
		/// </summary>
		[DataMember]
		public object ForBenefitOf { get; set; }
		/// <summary>
		/// Phone number.
		/// </summary>
		[DataMember]
		public object Phone { get; set; }
		/// <summary>
		/// Province.
		/// </summary>
		[DataMember]
		public Guid? Province { get; set; }
		/// <summary>
		/// Postal code.
		/// </summary>
		[DataMember]
		public String PostalCode { get; set; }
		/// <summary>
		/// Settlement letter template.
		/// </summary>
		[DataMember]
		public object SettlementTemplate { get; set; }

	}
}

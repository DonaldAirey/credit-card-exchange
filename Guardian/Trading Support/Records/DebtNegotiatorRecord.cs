namespace FluidTrade.Guardian.Records
{

	using System.Runtime.Serialization;
	using System;

	/// <summary>
	/// The information required to create a credit card for a Debt Holder. 
	/// </summary>
	[DataContract]
	public class DebtNegotiatorRecord
	{

		/// <summary>
		/// The configuration used for external id lookups.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string ConfigurationId { get; set; }

		/// <summary>
		/// Card account balance.
		/// </summary>
		[DataMember(IsRequired = true)]
		public decimal AccountBalance { get; set; }

		/// <summary>
		/// Card account number.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string AccountCode { get; set; }

		/// <summary>
		/// First address line.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String Address1 { get; set; }

		/// <summary>
		/// Second address line.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String Address2 { get; set; }

		/// <summary>
		/// Bank account number.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String BankAccountNumber { get; set; }

		/// <summary>
		/// Bank routing number.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String BankRoutingNumber { get; set; }

		/// <summary>
		/// The blotter to add this record to.
		/// </summary>
		[DataMember(IsRequired = true)]
		public Guid Blotter { get; set; }

		/// <summary>
		/// City.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String City { get; set; }

		/// <summary>
		/// Country.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string CountryCode { get; set; }

		/// <summary>
		/// Currency.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string Currency { get; set; }

		/// <summary>
		/// An external id of the consumer (trust).
		/// </summary>
		[DataMember(IsRequired = true)]
		public string CustomerCode { get; set; }

		/// <summary>
		/// Date of birth.
		/// </summary>
		[DataMember(IsRequired = false)]
		public DateTime? DateOfBirth { get; set; }

		/// <summary>
		/// Credit card owner.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string DebtHolder { get; set; }

		/// <summary>
		/// First name.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String FirstName { get; set; }

		/// <summary>
		/// Is employed.
		/// </summary>
		[DataMember(IsRequired = false)]
		public bool? IsEmployed { get; set; }

		/// <summary>
		/// Last name.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string LastName { get; set; }

		/// <summary>
		/// Middle name.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string MiddleName { get; set; }

		/// <summary>
		/// Original card account number.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string OriginalAccountNumber { get; set; }

		/// <summary>
		/// Phone number.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Postal code.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string PostalCode { get; set; }

		/// <summary>
		/// Province.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The balance of savings account.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string SavingsAccount { get; set; }

		/// <summary>
		/// The balance of savings account.
		/// </summary>
		[DataMember(IsRequired = true)]
		public decimal SavingsBalance { get; set; }

		/// <summary>
		/// The entity holding the savings account
		/// </summary>
		[DataMember(IsRequired = true)]
		public string SavingsEntityCode { get; set; }

		/// <summary>
		/// Social security number.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string SocialSecurityNumber { get; set; }

		/// <summary>
		/// Name suffix.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string Suffix { get; set; }

		/// <summary>
		/// Salutation, Mister, Misses, etc.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String Salutation { get; set; }

		/// <summary>
		/// Account tag.
		/// </summary>
		[DataMember(IsRequired = false)]
		public string Tag { get; set; }

		/// <summary>
		/// Vendor identifier unique to the debt holder organization.
		/// </summary>
		[DataMember(IsRequired = true)]
		public string VendorCode { get; set; }

	}

}

namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents a DebtHolderImportTranslation. 
	/// </summary>
	[DataContract]
	public class DebtHolderImportTranslation : BaseRecord
	{

		/// <summary>
		/// Account Balance
		/// </summary>
		[DataMember]
		public String AccountBalance { get; set; }
					
		/// <summary>
		/// Account Balance
		/// </summary>
		[DataMember]
		public String AccountCode { get; set; }
 
		/// <summary>
		/// Address 
		/// </summary>
		[DataMember]
		public String Address1 { get; set; }

		/// <summary>
		/// Address Line 2
		/// </summary>
		[DataMember]
		public String Address2 { get; set; }

		/// <summary>
		/// City - i.e. Casablanca
		/// </summary>
		[DataMember]
		public String City { get; set; }

		/// <summary>
		/// Date you were born.  
		/// </summary>
		[DataMember]
		public String DateOfBirth { get; set; }

		/// <summary>
		/// DateOfDelinquency
		/// </summary>
		[DataMember]
		public String DateOfDelinquency { get; set; }

		/// <summary>
		/// DebtHolder
		/// </summary>
		[DataMember]
		public String DebtHolder { get; set; }

		/// <summary>
		/// ExternalId
		/// </summary>
		[DataMember]
		public String ExternalId { get; set; }

		/// <summary>
		/// FirstName
		/// </summary>
		[DataMember]
		public String FirstName { get; set; }
		
		/// <summary>
		/// LastName
		/// </summary>
		[DataMember]
		public String LastName { get; set; }

		/// <summary>
		/// middleName
		/// </summary>
		[DataMember]
		public String MiddleName { get; set; }
		
		/// <summary>
		/// Original Account Number
		/// </summary>
		[DataMember]
		public String OriginalAccountNumber { get; set; }
		
		/// <summary>
		/// Phone Number
		/// </summary>
		[DataMember]
		public String PhoneNumber { get; set; }
		
		/// <summary>
		/// Postal Code
		/// </summary>
		[DataMember]
		public String PostalCode { get; set; }

		/// <summary>
		/// Province Code
		/// </summary>
		[DataMember]
		public String ProvinceCode { get; set; }
		
		/// <summary>
		/// SocialSecurityNumber
		/// </summary>
		[DataMember]
		public String SocialSecurityNumber { get; set; }
	
		/// <summary>
		/// Suffix
		/// </summary>
		[DataMember]
		public String Suffix { get; set; }
		
		/// <summary>
		/// Vendor Code
		/// </summary>
		[DataMember]
		public String VendorCode { get; set; }
		
	}
}

namespace FluidTrade.Guardian.Records
{

	using System.Runtime.Serialization;
	using System;

	/// <summary>
	/// Consumer record.
	/// </summary>
	[DataContract]
	public class Consumer : BaseRecord
	{				
		/// <summary>
		/// Address1 field
		/// </summary>		
		[DataMember]
		public System.Object Address1 { get; set; }
		/// <summary>
		/// Address2 field
		/// </summary>
		[DataMember]
		public System.Object Address2 { get; set; }
		/// <summary>
		/// City field
		/// </summary>
		[DataMember]
		public System.Object City { get; set; }		
		/// <summary>
		/// Date of Birth field
		/// </summary>
		[DataMember]
		public System.Object DateOfBirth { get; set; }		
		/// <summary>
		/// Address2 field
		/// </summary>
		[DataMember]
		public System.Object ExternalId0 { get; set; }		
		/// <summary>
		/// Address2 field
		/// </summary>
		[DataMember]
		public System.Object ExternalId1 { get; set; }				 
		/// <summary>
		/// Address2 field
		/// </summary>
		[DataMember]
		public System.Object IsEmployed { get; set; }		 		 		
		/// <summary>
		/// First Name field
		/// </summary>
		[DataMember]
		public System.Object FirstName { get; set; }
		/// <summary>
		/// Middle Name field
		/// </summary>
		[DataMember]
		public System.Object MiddleName { get; set; }
		/// <summary>
		/// Last Name Field
		/// </summary>
		[DataMember]
		public System.Object LastName { get; set; }		
		/// <summary>
		/// If specified then matchId Modified time is updated
		/// </summary>
		[DataMember]
		public Guid? MatchId { get; set; }
		/// <summary>
		/// Postal code.
		/// </summary>
		[DataMember]
		public String PostalCode { get; set; }
		/// <summary>
		/// Phone Number
		/// </summary>
		[DataMember]
		public System.Object PhoneNumber { get; set; }
		/// <summary>
		/// Province Id
		/// </summary>
		[DataMember]
		public Guid? ProvinceId { get; set; }		
		/// <summary>
		/// Social Security Number
		/// </summary>
		[DataMember]
		public System.String SocialSecurityNumber { get; set; }
		/// <summary>
		/// Salutation
		/// </summary>
		[DataMember]
		public System.Object Salutation { get; set; }
		/// <summary>
		/// Suffix
		/// </summary>
		[DataMember]
		public System.Object Suffix { get; set; }
		/// <summary>
		/// If specified then workingorder Modified time is updated
		/// </summary>
		[DataMember]
		public Guid? WorkingOrderId { get; set; }	

	}

}

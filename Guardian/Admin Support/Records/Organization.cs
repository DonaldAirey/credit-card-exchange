namespace FluidTrade.Guardian
{

	using System.Runtime.Serialization;
	using System;

	/// <summary>
	/// Represents a User. 
	/// </summary>
	[DataContract]
	public class Organization 
	{

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public string ContactName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid OrganizationId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid TenantId { get; set; }

		
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public string TenantIdExternalId0 { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public string TenantName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public OrganizationTypeEnum Type { get; set; }

	}


	/// <summary>
	/// 
	/// </summary>
	[DataContract(Name = "OrganizationType")]
	public enum OrganizationTypeEnum
	{
		/// <summary>
		/// 
		/// </summary>
		[EnumMember]
		DebtHolder,
		/// <summary>
		/// 
		/// </summary>
		[EnumMember]
		DebtNegotiator		
	}
}

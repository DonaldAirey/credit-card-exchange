namespace FluidTrade.Guardian
{

	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents a User. 
	/// </summary>
	[DataContract]
	public class User 
	{

		/// <summary>
		/// Email address
		/// </summary>
		[DataMember(IsRequired = false)]
		public String Description { get; set; }

		/// <summary>
		/// GroupId of the user
		/// </summary>
		[DataMember(IsRequired = false)]
		public Guid GroupId { get; set; }

		/// <summary>
		/// Email address
		/// </summary>
		[DataMember(IsRequired = false)]
		public String EmailAddress { get; set; }

		/// <summary>
		/// The "identity" (thumbprint, password, etc.) of the user.
		/// </summary>
		[DataMember(IsRequired = false)]
		public String FullName { get; set; }

		/// <summary>
		/// Email address
		/// </summary>
		[DataMember(IsRequired = false)]
		public String LookupId { get; set; }
			
		
		/// <summary>
		/// The EntityId of the organization the user belongs to.
		/// </summary>
		[DataMember(IsRequired = false)]
		public Guid Organization { get; set; }

		
		/// <summary>
		/// The UserId of the organization the user belongs to [optional].
		/// </summary>
		[DataMember(IsRequired = false)]
		public Guid UserId { get; set; }

	}
}

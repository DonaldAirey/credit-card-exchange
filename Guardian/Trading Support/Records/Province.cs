namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Credit Card.
	/// </summary>
	[DataContract]
	public class Province : BaseRecord
	{
		/// <summary>
		/// Description of the province.
		/// </summary>
		[DataMember]
		public object Abbreviation { get; set; }
		/// <summary>
		/// CountryId.
		/// </summary>
		[DataMember]
		public Guid? CountryId { get; set; } 
		/// <summary>
		/// External Id0.
		/// </summary>
		[DataMember]
		public object ExternalId0 { get; set; }
		/// <summary>
		/// External Id1.
		/// </summary>
		[DataMember]
		public object ExternalId1   { get; set; } 
		/// <summary>
		/// Name.
		/// </summary>
		[DataMember]
		public object Name { get; set; }
		/// <summary>
		/// Primary key.
		/// </summary>
		[DataMember]
		public object ProvinceId { get; set; }
	}
}

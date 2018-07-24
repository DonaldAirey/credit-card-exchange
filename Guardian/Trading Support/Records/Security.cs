namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Credit Card.
	/// </summary>
	[DataContract]
	public class Security : BaseRecord
	{       
		/// <summary>
		/// Average Daily Volume.
		/// </summary>
		[DataMember]
		public Object AverageDailyVolume { get; set; }
		/// <summary>
		/// Country Id.
		/// </summary>
		[DataMember]
		public Guid? CountryId { get; set; } 
		/// <summary>
		/// External Logo.
		/// </summary>
		[DataMember]
		public Object Logo { get; set; }
		/// <summary>
		/// External Market Capitalization.
		/// </summary>
		[DataMember]
		public Object MarketCapitalization { get; set; }
        /// <summary>
		/// Minimum Quantity.
		/// </summary>
		[DataMember]
		public Object MinimumQuantity { get; set; }
        /// <summary>
		/// Price Factor.
		/// </summary>
		[DataMember]
		public Object PriceFactor { get; set; }
		/// <summary>
		/// Quantity Factor.
		/// </summary>
		[DataMember]
		public Object QuantityFactor { get; set; }
        /// <summary>
		/// Security Id
		/// </summary>
		[DataMember]
		public Object SecurityId { get; set; }
		/// <summary>
		/// Symbol.
		/// </summary>
		[DataMember]
		public Object Symbol { get; set; }
		/// <summary>
		/// The fluidtrade tenant id for the vendor that owns this record.
		/// </summary>
		[DataMember]
		public Guid TenantId;
        /// <summary>
		/// Volume Category Code.
		/// </summary>
		[DataMember]
		public Guid VolumeCategoryId { get; set; }

	}
}

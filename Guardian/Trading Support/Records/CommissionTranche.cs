namespace FluidTrade.Guardian.Records
{

	using System.Runtime.Serialization;
	using System;

	/// <summary>
	/// CommissionTranche record.
	/// </summary>
	[DataContract]
	public class CommissionTranche : BaseRecord
	{

		/// <summary>
		/// CommissionType field.
		/// </summary>
		[DataMember]
		public System.Guid CommissionType { get; set; }
		/// <summary>
		/// CommissionUnit field.
		/// </summary>
		[DataMember]
		public System.Guid CommissionUnit { get; set; }
		/// <summary>
		/// EndRange field.
		/// </summary>
		[DataMember]
		public System.Decimal? EndRange { get; set; }
		/// <summary>
		/// StartRange field.
		/// </summary>
		[DataMember]
		public System.Decimal StartRange { get; set; }
		/// <summary>
		/// Value field.
		/// </summary>
		[DataMember]
		public System.Decimal Value { get; set; }

	}

}

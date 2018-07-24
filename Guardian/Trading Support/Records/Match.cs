
using System;
using System.Runtime.Serialization;
namespace FluidTrade.Guardian.Records
{
	/// <summary>
	/// Match.
	/// </summary>
	[DataContract]
	public class Match : BaseRecord
	{
		/// <summary>
		/// Blotter Id
		/// </summary>
		[DataMember]
		public Guid? BlotterId{ get; set; }

		/// <summary>
		/// ContraMatchId
		/// </summary>
		[DataMember]
		public Guid? ContraMatchId { get; set; }

		/// <summary>
		/// ContraOrderId
		/// </summary>
		[DataMember]
		public Guid? ContraOrderId { get; set; }

		/// <summary>
		/// HeatIndex
		/// </summary>
		[DataMember]
		public object HeatIndex{ get; set; }

		/// <summary>
		/// Match details, could be values that go into calculation of heat index
		/// </summary>
		[DataMember]
		public string HeatIndexDetails { get; set; }

		/// <summary>
		/// MatchedTime
		/// </summary>
		[DataMember]
		public object MatchedTime{ get; set; }
		
		/// <summary>
		/// StatusCode
		/// </summary>
		[DataMember]
		public Guid? StatusCodeId{ get; set; }

		/// <summary>
		/// WorkingOrderId
		/// </summary>
		[DataMember]
		public Guid? WorkingOrderId{ get; set; }
	}
}

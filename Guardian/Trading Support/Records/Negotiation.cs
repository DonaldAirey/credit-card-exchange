using System;
using System.Runtime.Serialization;

namespace FluidTrade.Guardian.Records
{
	/// <summary>
	/// Negotiation.
	/// </summary>
	[DataContract]
	public class Negotiation : BaseRecord
	{
		/// <summary>
		/// Blotter Id
		/// </summary>
		[DataMember]
		public Guid? BlotterId { get; set; }
				
		/// <summary>
		/// ExecutionId
		/// </summary>
		[DataMember]
		public Guid? ExecutionId { get; set; }
		
		/// <summary>
		/// IsRead
		/// </summary>
		[DataMember]
		public object IsRead { get; set; }
		
		/// <summary>
		/// ExecutionId
		/// </summary>
		[DataMember]
		public Guid? MatchId { get; set; }
						
		/// <summary>
		/// Quantity
		/// </summary>
		[DataMember]
		public object Quantity { get; set; }
		
		/// <summary>
		/// StatusCode guid
		/// </summary>
		[DataMember]
		public Guid? StatusCodeId { get; set; }
	}
}

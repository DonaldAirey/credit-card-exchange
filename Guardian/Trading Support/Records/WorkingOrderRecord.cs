namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;
	using FluidTrade.Core;
	
	/// <summary>
	/// Datacontract for WorkingOrder records
	/// </summary>
	[DataContract]
	[KnownType(typeof(Side))]
	[KnownType(typeof(Crossing))]
	[KnownType(typeof(TimeInForce))]
	public class WorkingOrderRecord : BaseRecord
	{					
		/// <summary>
		/// AutomaticQuantity associated with this order
		/// </summary>
		[DataMember]
		public object AutomaticQuantity { get; set; }   
		/// <summary>
		/// Blotter Id of this order
		/// </summary>
		[DataMember]
		public Guid? BlotterId { get; set; }   
		/// <summary>
		/// Crossing code associated with this order
		/// </summary>
		[DataMember]
		public Guid? CrossingCode { get; set; }
		/// <summary>
		/// Destination Id associated with this order
		/// </summary>
		[DataMember]
		public object DestinationId { get; set; }    
		/// <summary>
		/// External Id0 associated with this order
		/// </summary>
		[DataMember]
		public object ExternalId0 { get; set; }    
		/// <summary>
		/// Is Automatic associated with this order
		/// </summary>
		[DataMember]
		public object IsAutomatic { get; set; }    
		/// <summary>
		/// Is Awake associated with this order
		/// </summary>
		[DataMember]
		public object IsAwake { get; set; }   
		/// <summary>
		/// Is Broker Match to determine if the broker match.
		/// </summary>
		[DataMember]
		public Boolean? IsBrokerMatch { get; set; }						
		/// <summary>
		///Flag to determine if the hedge fund match.
		/// </summary>
		[DataMember]
		public Boolean? IsHedgeFundMatch { get; set; }
		/// <summary>
		/// Is Intitution Match Flag
		/// </summary>
		[DataMember]
		public Boolean? IsInstitutionMatch { get; set; }				
		/// <summary>
		/// Limit Price associated with this order
		/// </summary>
		[DataMember]
		public object LimitPrice { get; set; }   		
		/// <summary>
		/// Order Type Code associated with this order
		/// </summary>
		[DataMember]
		public Guid? OrderTypeCode { get; set; }    
		/// <summary>
		/// Security Id associated with this order
		/// </summary>
		[DataMember]
		public Guid? SecurityId { get; set; }    
		/// <summary>
		/// Settlement Date associated with this order
		/// </summary>
		[DataMember]
		public DateTime? SettlementDate { get; set; }    
		/// <summary>
		/// Settlement Id associated with this order
		/// </summary>
		[DataMember]
		public object SettlementId { get; set; }    
		/// <summary>
		/// Side code associated with this order
		/// </summary>
		[DataMember]
		public Guid? SideId { get; set; }
		/// <summary>
		/// Start Time associated with this order
		/// </summary>
		[DataMember]
		public object StartUTCTime { get; set; }    
		/// <summary>
		/// Status code associated with this order
		/// </summary>
		[DataMember]
		public Guid? StatusCodeId { get; set; }    
		/// <summary>
		/// Stop Price associated with this order
		/// </summary>
		[DataMember]
		public object StopPrice { get; set; }    
		/// <summary>
		/// Stop Time associated with this order
		/// </summary>
		[DataMember]
		public object StopUTCTime { get; set; }    
		/// <summary>
		/// Submitted Quantity associated with this order
		/// </summary>
		[DataMember]
		public object SubmittedQuantity { get; set; }   
		/// <summary>
		/// Submitted Time 
		/// </summary>
		[DataMember]
		public object SubmittedUTCTime { get; set; }	
		/// <summary>
		/// Time in force associated with this order.
		/// </summary>
		[DataMember]
		public Guid? TimeInForceId { get; set; }
		/// <summary>
		/// Trade Date associated with this order
		/// </summary>
		[DataMember]
		public DateTime? TradeDate { get; set; }    
		/// <summary>
		/// Uploaded Time associated with this order
		/// </summary>
		[DataMember]
		public object UploadedUTCTime { get; set; }    
		/// <summary>
		/// WorkingOrder Id associated with this order
		/// </summary>
		[DataMember]
		public object WorkingOrderId { get; set; }   
	}
}

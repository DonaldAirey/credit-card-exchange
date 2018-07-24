namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Credit Card.
	/// </summary>
	[DataContract]
	public class Entity : BaseRecord
	{
		/// <summary>
		/// Description of the entity.
		/// </summary>
		[DataMember]
		public Guid? BlotterId { get; set; }

		/// <summary>
		/// Description of the entity.
		/// </summary>
		[DataMember]
		public object Description { get; set; }
		/// <summary>
		/// External Id0.
		/// </summary>
		[DataMember]
		public object ExternalId0   { get; set; } 
		/// <summary>
		/// External Id1.
		/// </summary>
		[DataMember]
		public object ExternalId1   { get; set; }
		/// <summary>
		/// External Id 2.
		/// </summary>
		[DataMember]
		public object ExternalId2   { get; set; } 
		/// <summary>
		/// External Id 3.
		/// </summary>
		[DataMember]
		public object ExternalId3   { get; set; }
		/// <summary>
		/// External Id 4.
		/// </summary>
		[DataMember]
		public object ExternalId4   { get; set; }
		/// <summary>
		/// External Id 5.
		/// </summary>
		[DataMember]
		public object ExternalId5   { get; set; }
		/// <summary>
		/// External Id 6.
		/// </summary>
		[DataMember]
		public object ExternalId6   { get; set; }
		/// <summary>
		/// External Id 7.
		/// </summary>
		[DataMember]
		public object ExternalId7   { get; set; }
		/// <summary>
		/// Image Id foriegn key.
		/// </summary>
		[DataMember]
		public Guid? ImageId   { get; set; }
		/// <summary>
		/// IsHidden Flag.
		/// </summary>
		[DataMember]
		public object IsHidden   { get; set; }
		/// <summary>
		/// Is ReadOnly Flag.
		/// </summary>
		[DataMember]
		public object IsReadOnly    { get; set; }
		/// <summary>
		/// Name of the Entity.
		/// </summary>
		[DataMember]
		public object Name   { get; set; }
		/// <summary>
		/// The parent Entity (if this record describes a new entity).
		/// </summary>
		[DataMember]
		public Guid? ParentId { get; set; }
		/// <summary>
		/// Tenant Id of the entity.  This is use for filtering the data.  This can be null.
		/// </summary>
		[DataMember]
		public Guid? TenantId { get; set; }
		/// <summary>
		/// Type of Entity.
		/// </summary>
		[DataMember]
		public Guid? TypeId { get; set; }
		
	}
}

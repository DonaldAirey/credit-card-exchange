namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Base entity common to all objects in the Web Service.  
	/// </summary>
	[DataContract]
	[KnownType(typeof(DBNull))]
	[KnownType(typeof(Entity))]
	[KnownType(typeof(EntityTree))]
	public class BaseRecord
	{

		/// <summary>
		/// The configurationId for use in resolving external IDs.
		/// </summary>
		[DataMember]
		public String ConfigurationId { get; set; }

		/// <summary>
		/// Unique timestamp identifier.  This is a required field for updates and deletes.
		/// </summary>
		[DataMember]
		public Int64 RowVersion { get; set; }

		/// <summary>
		/// Unique record identifier.  This is a required field for updates and deletes. This field
		/// will generated for inserts.
		/// </summary>
		[DataMember]
		public  Guid RowId { get; set; }		

	}
}

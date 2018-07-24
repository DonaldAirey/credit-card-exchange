namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Credit Card.
	/// </summary>
	[DataContract]
	public class Report : BaseRecord
	{
		/// <summary>
		/// External Id0.
		/// </summary>
		[DataMember]
		public String ExternalId0 { get; set; }
		/// <summary>
		/// Is Archived Flag.
		/// </summary>
		[DataMember]
		public Boolean? IsArchived { get; set; }
		/// <summary>
		/// Is Deleted Flag.
		/// </summary>
		[DataMember]
		public Boolean? IsDeleted { get; set; }
		/// <summary>
		/// Name of the Report.
		/// </summary>
		[DataMember]
		public String Name { get; set; }
		/// <summary>
		/// Report Id.
		/// </summary>
		[DataMember]
		public Guid? ReportId { get; set; }

		/// <summary>
		/// Report Type Id.
		/// </summary>
		[DataMember]
		public Guid? ReportTypeId { get; set; }

		/// <summary>
		/// Report XAML or blob.
		/// </summary>
		[DataMember]
		public object Xaml { get; set; }
	}
}

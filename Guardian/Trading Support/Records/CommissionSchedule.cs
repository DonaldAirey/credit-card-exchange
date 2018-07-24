namespace FluidTrade.Guardian.Records
{

	using System.Runtime.Serialization;
	using System;

	/// <summary>
	/// CommissionSchedule record.
	/// </summary>
	[DataContract]
	public class CommissionSchedule : BaseRecord
	{

		/// <summary>
		/// Name field.
		/// </summary>
		[DataMember]
		public System.String Name { get; set; }
		/// <summary>
		/// CommissionTranches field.
		/// </summary>
		[DataMember]
		public CommissionTranche[] CommissionTranches { get; set; }

	}

}

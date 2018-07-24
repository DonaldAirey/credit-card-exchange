namespace FluidTrade.Guardian.Records
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Credit Card.
	/// </summary>
	[DataContract]
	public class Blotter : Entity
	{

		/// <summary>
		/// Description of the entity.
		/// </summary>
		[DataMember]
		public CommissionSchedule CommissionSchedule { get; set; }

	}
}

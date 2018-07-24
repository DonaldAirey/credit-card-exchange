namespace FluidTrade.Guardian.Records
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Runtime.Serialization;

	/// <summary>
	/// A relationship between two entities.
	/// </summary>
	[DataContract]
	public class EntityTree : BaseRecord
	{

		/// <summary>
		/// The child entity in the relationship.
		/// </summary>
		[DataMember]
		public Guid ChildId { get; set; }

		/// <summary>
		/// The parent entity in the relsationship.
		/// </summary>
		[DataMember]
		public Guid ParentId { get; set; }

	}
}

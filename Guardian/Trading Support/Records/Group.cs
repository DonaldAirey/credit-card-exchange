namespace FluidTrade.Guardian.Records
{

	using System;
	using System.Runtime.Serialization;
	using FluidTrade.Core;

	/// <summary>
	/// An object representing an ACL group.
	/// </summary>
	[DataContract]
	public class Group : RightsHolder
	{

		/// <summary>
		/// The id of the type of the group.
		/// </summary>
		[DataMember]
		public GroupType GroupType { get; set; }

	}

}

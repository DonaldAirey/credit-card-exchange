namespace FluidTrade.Guardian.Records
{

	using System.Runtime.Serialization;
	using System;

	/// <summary>
	/// ChatInfo
	/// </summary>
	[DataContract]
	public class ChatInfo
	{
		/// <summary>
		/// The match to which this chat item belongs.
		/// </summary>
		[DataMember]
		public Guid MatchId { get; set; }

		/// <summary>
		/// The unique identifier of the chat item.
		/// </summary>
		[DataMember]
		public Guid ChatId { get; set; }

		/// <summary>
		/// The content of this chat item.
		/// </summary>
		[DataMember]
		public String Message { get; set; }

	}
}

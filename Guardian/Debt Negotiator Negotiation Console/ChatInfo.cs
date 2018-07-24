namespace FluidTrade.Guardian
{
	using System;
	
	/// <summary>
	/// Used to communicate Chat information to the foreground thread.
	/// </summary>
	public class ChatItem
	{

		/// <summary>
		/// The UTC creation time of this ChatItem object.
		/// </summary>
		public DateTime CreatedTime;

		/// <summary>
		/// A flag to indicate if the ChatItem is associated with self or the counter party.
		/// </summary>
		public Boolean IsReply;

		/// <summary>
		/// The unique identifier of the match to which this chat is associated.
		/// </summary>
		public Guid MatchId;

		/// <summary>
		/// The string containing the chat message.
		/// </summary>
		public String Message;

	}

}
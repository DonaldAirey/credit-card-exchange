namespace FluidTrade.Core
{


    /// <summary>
	/// The state of messages.
	/// </summary>
	public class State
	{
		public const int Created = 0;
		public const int Sent = 1;
		public const int Acknowledged = 2;
		public const int Rejected = 3;
		public const int CancelPending = 4;
		public const int CancelAcknowledged = 5;
		public const int CancelRejected = 6;
		public const int Cancelled = 7;
		public const int ReplacePending = 8;
		public const int ReplaceAcknowledged = 9;
		public const int ReplaceRejected = 10;
		public const int Replaced = 11;
		public const int DoneForDay = 12;
		public const int Stopped = 13;
		public const int Error = 14;
	}

}

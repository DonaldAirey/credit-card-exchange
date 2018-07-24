namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Arguments for a Socket Event.
	/// </summary>
	public class MessageGapEventArgs : EventArgs
	{

		private bool isHandled;
		private int beginSeqNo;
		private int endSeqNo;
		private string sessionId;

		/// <summary>A flag indicating whether the fix message has been handled.</summary>
		public bool IsHandled {get {return this.isHandled;} set {this.isHandled = value;}}

		/// <summary>The start of the message gap.</summary>
		public int BeginSeqNo {get {return this.beginSeqNo;}}

		/// <summary>The start of the message gap.</summary>
		public int EndSeqNo {get {return this.endSeqNo;}}

		/// <summary>Identifies a session between counterparties.</summary>
		public string SessionId {get {return this.sessionId;}}

		/// <summary>
		/// SocketEventArg Constructor
		/// </summary>
		/// <param name="fixMessage">The reject fixMessage</param>
		public MessageGapEventArgs(string sessionId, int beginSeqNo, int endSeqNo)
		{

			// Initialize the object.
			this.isHandled = false;
			this.sessionId = sessionId;
			this.beginSeqNo = beginSeqNo;
			this.endSeqNo = endSeqNo;

		}

	}
	
}

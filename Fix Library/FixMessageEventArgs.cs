namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Arguments for a FixMessageEvent.
	/// </summary>
	public class FixMessageEventArgs : EventArgs
	{

		private bool isHandled;
		private string sessionId;
		private FixMessage fixMessage;

		/// <summary>A flag indicating whether the fix message has been handled.</summary>
		public bool IsHandled {get {return this.isHandled;} set {this.isHandled = value;}}

		/// <summary>Identifies a session between counterparties.</summary>
		public string SessionId {get {return this.sessionId;}}

		/// <summary>The FIX FixMessage.</summary>
		public FixMessage FixMessage{get {return this.fixMessage;}}

		/// <summary>
		/// FixMessageEventArgs Constructor
		/// </summary>
		/// <param name="fixMessage">The fixMessage</param>
		public FixMessageEventArgs(string sessionId, FixMessage fixMessage)
		{

			// Initialize the object.
			this.isHandled = false;
			this.sessionId = sessionId;
			this.fixMessage = fixMessage;

		}

	}
	
}

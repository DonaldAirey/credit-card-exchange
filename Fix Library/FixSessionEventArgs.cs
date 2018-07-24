namespace FluidTrade.Core
{

	using System;

	/// <summary>
	/// Arguments for a FixSessionEvent.
	/// </summary>
	public class FixSessionEventArgs : EventArgs
	{

		private string sessionId;
		private string message;

		/// <summary>Identifies a session between counterparties.</summary>
		public string SessionId {get {return this.sessionId;}}

		/// <summary>Message.</summary>
		public string Message{get {return this.message;}}

		/// <summary>
		/// FixSessionEventArgs Constructor
		/// </summary>
		/// <param name="sessionId"></param>
		/// <param name="message"></param>
		public FixSessionEventArgs(string sessionId, string message)
		{

			// Initialize the object.
			this.sessionId = sessionId;
			this.message = message;

		}

	}
	
}

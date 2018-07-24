namespace FluidTrade.Guardian
{
	/// <summary>
	/// States are internal values for the condition of a process.
	/// </summary>
	/// <copyright>Copyright © 2007 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public enum State 
	{
		/// <summary>This object hasn't been altered since it was created.</summary>
		Initial,
		/// <summary>This object been sent to some destination.</summary>
		Sent,
		/// <summary>This object was received by the destination.</summary>
		Acknowledged,
		/// <summary>This object was rejected by the destination.</summary>
		Rejected,
		/// <summary>This object has been canceled, but no confirmation yet from the destination.</summary>
		CancelPending,
		/// <summary>The destination has confirmed the cancel instruction but not yet canceled the object.</summary>
		CancelAcknowledged,
		/// <summary>This object could not be canceled by the destination.</summary>
		CancelRejected,
		/// <summary>This object canceled by the destination.</summary>
		Cancelled,
		/// <summary>This object has requested a replacement with the destination.</summary>
		ReplacePending,
		/// <summary>The destination has received the request to change the object.</summary>
		ReplaceAcknowledged,
		/// <summary>The destination has rejected the request to change the object.</summary>
		ReplaceRejected,
		/// <summary>The object has been replaced an earlier object.</summary>
		Replaced,
		/// <summary>No more activity on this object is expected today.</summary>
		DoneForDay,
		/// <summary>The object has beeen manually stopped from being processed.</summary>
		Stopped,
		/// <summary>The object has some error that prevents any further activity.</summary>
		Error
	}
}

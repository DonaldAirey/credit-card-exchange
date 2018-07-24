namespace FluidTrade.Core
{

	using System;
	using System.Runtime.Serialization;

	[DataContractAttribute]
	public class DeadlockFault : FaultBase
	{
		/// <summary>
		/// Create information about a deadlock.
		/// </summary>
		/// <param name="message">A message about the failure.</param>
		public DeadlockFault(String message)
			: base(message, System.Diagnostics.TraceEventType.Error)
		{
		}
	}

}

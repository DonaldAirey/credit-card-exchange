namespace FluidTrade.Core
{

    using System;
    using System.Runtime.Serialization;

	/// <summary>
	/// Contains information about a failure to have the proper row version when accessing a record.
	/// </summary>
	[DataContract]
	public class FormatFault : FaultBase
	{
		/// <summary>
		/// Create information about a failure to have the proper row version when accessing a record.
		/// </summary>
		/// <param name="format">The format for the error message.</param>
		/// <param name="args">Variable list of parameters for the failure message.</param>
		public FormatFault(string message)
			:base(message, System.Diagnostics.TraceEventType.Error)
		{
		}
	}

}

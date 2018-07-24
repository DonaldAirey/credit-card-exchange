namespace FluidTrade.Core
{

    using System;
    using System.Runtime.Serialization;

	/// <summary>
	/// Contains information about a failure to have the proper row version when accessing a record.
	/// </summary>
	[DataContract]
	public class ArgumentFault : FaultBase
	{
		/// <summary>
		/// Create information about a failure to have the proper row version when accessing a record.
		/// </summary>
		/// <param name="format">The format for the error message.</param>
		public ArgumentFault(string message)
			:base(message, System.Diagnostics.TraceEventType.Error)
		{

		}
	}

}

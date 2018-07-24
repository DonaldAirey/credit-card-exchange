namespace FluidTrade.Core
{

    using System;
    using System.Runtime.Serialization;

	/// <summary>
	/// Contains information about a security failure.
	/// </summary>
	[DataContract]
	public class SecurityFault : FaultBase
	{

		/// <summary>
		/// Create information about a failure to have the proper row version when accessing a record.
		/// </summary>
		/// <param name="format">The format for the error message.</param>
		/// <param name="args">Variable list of parameters for the failure message.</param>
		public SecurityFault(string message)
			: base(message, System.Diagnostics.TraceEventType.Error)
		{
		}

		
		/// <summary>
		/// Gets or sets the message describing the fault.
		/// </summary>
		[DataMember]
		public ErrorCode FaultCode
		{
			get;
			set;
		}


	}

}

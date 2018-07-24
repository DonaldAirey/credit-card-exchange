namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Runtime.Serialization;

	/// <summary>
	/// Contains information about a failure to have the proper row version when accessing a record.
	/// </summary>
	[DataContract]
	public class FieldRequiredFault : FaultBase
	{
		/// <summary>
		/// Create information about a failure to have all required fields.
		/// </summary>
		/// <param name="format">The format for the error message.</param>
		public FieldRequiredFault(string message)
			: base(message, System.Diagnostics.TraceEventType.Error)
		{

		}
	}
}

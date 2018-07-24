using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Core
{
	using System;
	using System.Runtime.Serialization;
using System.Diagnostics;

	/// <summary>
	/// base class for faults, conditionally will log error to event log on fault creation
	/// </summary>
	[DataContract]
	public abstract class FaultBase
	{
		/// <summary>
		/// default ctor
		/// </summary>
		/// <param name="message">fault message</param>
		protected FaultBase(string message)
			: this(message, TraceEventType.Stop)
		{
		}
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="message">fault message</param>
		/// <param name="eventType">Logging level for this fault TraceEventType.Stop == No logging</param>
		protected FaultBase(string message, TraceEventType eventType)
		{
			this.Message = message;

			switch(eventType)
			{
				case TraceEventType.Error:
				case TraceEventType.Information:
				case TraceEventType.Warning:
					break;
				default:
					return;
			}

			string stackTrace = UnhandledExceptionHelper.GetStackString();

			string logString = string.Format("{0}\r\n{1}\r\n{2}", this.GetType().Name, message, stackTrace);
			switch(eventType)
			{
				case TraceEventType.Error:
					EventLog.Error(logString);
					break;
				case TraceEventType.Information:
					EventLog.Information(logString);
					break;
				case TraceEventType.Warning:
					EventLog.Warning(logString);
					break;
			}
		}

		/// <summary>
		/// get or set the fault message
		/// </summary>
		[DataMember]
		public string Message { get; private set; }
	}
}

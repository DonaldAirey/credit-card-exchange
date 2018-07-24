using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using FluidTrade.Core;
using System.IO;

namespace FluidTrade.Core
{
	
	/// <summary>
	/// 
	/// </summary>
	[DataContract]
	public class AsyncMethodResponse
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public AsyncMethodResponse(Guid clientId, string asyncTicket) 
		{
			this.ClientId = clientId;
			this.AsyncTicket = asyncTicket;
			IsSuccessful = true;
		}

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public object AsyncResponseData { get; set; }

		/// <summary>
		/// General status of the whole operation.
		/// </summary>
		[DataMember]
		public String AsyncTicket { get; set; }

		/// <summary>
		/// General status of the whole operation.
		/// </summary>
		[DataMember]
		public Guid ClientId { get; set; }

		/// <summary>
		/// ErrorString.
		/// </summary>
		[DataMember]
		public string ErrorText
		{
			get;
			set;
		}

		/// <summary>
		/// General status of the whole operation.
		/// </summary>
		[DataMember]
		public AsyncMethodResponse InnerAsyncMethodResponse { get; set; }


		/// <summary>
		/// General status of the whole operation.
		/// </summary>
		[DataMember]
		[DefaultValue(true)]
		public Boolean IsSuccessful { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public string StreamDataId { get; set; }

		/// <summary>
		/// Return type.
		/// </summary>
		[DataMember]
		public ErrorCode Result
		{
			get;
			set;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="errorText"></param>
		/// <param name="errorCode"></param>
		public void AddError(string errorText, ErrorCode errorCode)
		{
			this.IsSuccessful = false;
			this.ErrorText = errorText;
			this.Result = errorCode;
		}
	}
}

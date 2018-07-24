using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidTrade.Core;
using System.IO;

namespace FluidTrade.Guardian.ClientCallback
{

	/// <summary>
	/// container for response data
	/// </summary>
	public class QueuedResponsePayload
	{
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="data"></param>
		/// <param name="asyncTicket"></param>
		/// <param name="clientId"></param>
		/// <param name="stream"></param>
		public QueuedResponsePayload(object data, string asyncTicket, Guid clientId, Stream stream)
		{
			this.ClientId = clientId;

			this.PayloadTicketGuid = Guid.NewGuid();
			this.PayloadTicket = this.PayloadTicketGuid.ToString();
			this.AsyncTicket = asyncTicket;
			this.PayloadTime = DateTime.UtcNow;

			this.PayloadStream = stream;
			this.PayloadData = data;
		}

		/// <summary>
		/// identifier of async request
		/// </summary>
		public string AsyncTicket { get; protected set; }

		/// <summary>
		/// 
		/// </summary>
		public Guid ClientId { get; private set; }


		/// <summary>
		/// object data
		/// </summary>
		public object PayloadData { get; protected set; }

		/// <summary>
		/// stream data
		/// </summary>
		public Stream PayloadStream { get; protected set; }

		/// <summary>
		/// id of payload
		/// </summary>
		public String PayloadTicket { get; protected set; }

		/// <summary>
		/// id of payload
		/// </summary>
		public Guid PayloadTicketGuid { get; protected set; }


		/// <summary>
		/// time paylod was created
		/// </summary>
		public DateTime PayloadTime { get; protected set; }
	}


	/// <summary>
	/// ServerAdminCallbackManagerImpl.Publish uses PublishEventHandler to call
	/// work to be done so that the Publish() is abstract
	/// </summary>
	/// <param name="clientProxy">object that contains the web callback delegate</param>
	/// <param name="dpea">eventArg that is the parameters for the web call. most actions
	/// will have a subclass of the PublishEventArg</param>
	public delegate void PublishEventHandler(ClientProxy clientProxy, PublishEventArgs dpea);

	/// <summary>
	/// contains the parameters for the web call that ServerAdminCallbackManagerImpl.Publish will call. most actions
	/// will have a subclass of the PublishEventArg
	/// </summary>
	public class PublishEventArgs
	{
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="messageTimeUtc"></param>
		/// <param name="response"></param>
		public PublishEventArgs(DateTime messageTimeUtc, AsyncMethodResponse response)
		{
			//this.TargetClientProxyMap = targetClientProxyMap;
			this.MessageTimeUtc = messageTimeUtc;
			this.Response = response;
		}

		/// <summary>
		/// get clientId
		/// </summary>
		public Dictionary<Guid, ClientProxy> TargetClientProxyMap { get; private set; }

		/// <summary>
		/// get messageTime 
		/// </summary>
		public DateTime MessageTimeUtc { get; private set; }

		/// <summary>
		/// get response object
		/// </summary>
		public AsyncMethodResponse Response { get; private set; }
	}

	/// <summary>
	/// subclass of PublishEventArg that is use for the PublishMessage(). Contains the messageTxt that is being published
	/// </summary>
	public class PublishMessageEventArg : PublishEventArgs
	{
		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="messageTimeUtc"></param>
		/// <param name="response"></param>
		/// <param name="message"></param>
		public PublishMessageEventArg(DateTime messageTimeUtc, AsyncMethodResponse response, string message)
			: base(messageTimeUtc, response)
		{
			this.Message = message;
		}

		/// <summary>
		/// get message text
		/// </summary>
		public string Message { get; private set; }
	}

	/// <summary>
	/// subclass of PublishEventArg that is use for the PublishSendResponseTicket(). Contains the payload which contains the
	/// data or data stream which is being published
	/// </summary>
	public class PublishSendResponseTicketEventArg : PublishMessageEventArg
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="messageTimeUtc"></param>
		/// <param name="response"></param>
		/// <param name="message"></param>
		/// <param name="payload"></param>
		public PublishSendResponseTicketEventArg(DateTime messageTimeUtc, AsyncMethodResponse response,
			string message, QueuedResponsePayload payload)
			: base(messageTimeUtc, response, message)
		{
			this.Payload = payload;

		}

		/// <summary>
		/// get the payload data
		/// </summary>
		public QueuedResponsePayload Payload { get; private set; }
	}
}
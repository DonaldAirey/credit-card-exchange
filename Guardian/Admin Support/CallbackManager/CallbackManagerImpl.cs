using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidTrade.Core;
using System.ServiceModel;
using System.IO;

namespace FluidTrade.Guardian.ClientCallback
{
	/// <summary>
	/// implementation of the IServerAdminCallbackManager and IServerAdminStreamManager
	/// handles subscriptions/publication/ getting async response and stream data
	/// </summary>
	public partial class CallbackManagerImpl //: IServerAdminCallbackManager, IServerAdminStreamManager
	{
		private PublishEventHandler publishMessageHandler;
		private PublishEventHandler publishSendResponseTicketHandler;
		private PublishThreadPool publishThreadPool;
		private Random random;
		private int randomRequestCount;

		private const int MaxRandomRequestsWithSameRandom = 1000;


		private static CallbackManagerImpl instance;

		/// <summary>
		/// the one CallbackManagerImpl singleton
		/// </summary>
		public static CallbackManagerImpl Instance
		{
			get
			{
				if(instance == null)
					lock(typeof(CallbackManagerImpl))
						if(instance == null)
							instance = new CallbackManagerImpl();

				return instance;
			}
		}

		/// <summary>
		/// private constructor should only acces through Instance
		/// </summary>
		private CallbackManagerImpl()
		{
			this.publishMessageHandler = new PublishEventHandler(this.PublishMessageHandlerProc);
			this.publishSendResponseTicketHandler = new PublishEventHandler(PublishSendResponseTicketHandlerProc);
			publishThreadPool = new PublishThreadPool();
			this.random = new Random();
		}

		/// <summary>
		/// list of subscribers managed by ServerAdmin.Subscribe() and ServerAdmin.Unsubscribe()
		/// </summary>
		private readonly Dictionary<Guid, ClientProxy> subscribers = new Dictionary<Guid, ClientProxy>();

		/// <summary>
		/// map of pending payloads waiting for pickup
		/// </summary>
		private readonly Dictionary<Guid, QueuedResponsePayload> payloadTicketToResponseMap = new Dictionary<Guid, QueuedResponsePayload>();

		/// <summary>
		/// map of pending payloads waiting for pickup
		/// </summary>
		private readonly Dictionary<string, QueuedResponsePayload> streamIdToPayloadMap = new Dictionary<string, QueuedResponsePayload>();

		/// <summary>
		/// Add a client to subscription list
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public AsyncMethodResponse Subscribe(Guid clientId)
		{
			try
			{
				Guid newClientId = clientId;
				if(newClientId == Guid.Empty)
					newClientId = Guid.NewGuid();

				IServerAdminCallback callback = OperationContext.Current.GetCallbackChannel<IServerAdminCallback>();
				ClientProxy clientProxy = new ClientProxy(newClientId, callback);
				lock(subscribers)
				{
					ClientProxy existingClientProxy;
					if(clientId != Guid.Empty)
						subscribers.TryGetValue(clientId, out existingClientProxy);
					else
						existingClientProxy = null;

					if(existingClientProxy == null)
					{
						subscribers[newClientId] = clientProxy;
					}
					else
					{
						existingClientProxy.AssignNewCallback(callback);
						List<KeyValuePair<PublishEventArgs, PublishEventHandler>> pendingList = existingClientProxy.PopPendingMessageList();
						if(pendingList != null)
						{
							Dictionary<Guid, ClientProxy> targetClientProxyMap = new Dictionary<Guid, ClientProxy>();
							targetClientProxyMap[existingClientProxy.ClientId] = existingClientProxy;
							
							foreach(KeyValuePair<PublishEventArgs, PublishEventHandler> pair in pendingList)
								Publish(targetClientProxyMap, true, pair.Key, pair.Value);
						}
					}
				}
				AsyncMethodResponse response = new AsyncMethodResponse(newClientId, null);

				return response;
			}
			catch(Exception ex)
			{
				AsyncMethodResponse errorResponse = new AsyncMethodResponse(clientId, null);
				errorResponse.AddError(ex.Message, ErrorCode.Fatal);
				return errorResponse;
			}
		}

		/// <summary>
		/// remove client from subscription list
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public AsyncMethodResponse Unsubscribe(Guid clientId)
		{
			try
			{
				ClientProxy clientProxy = null;
				lock(this.subscribers)
				{
					if(subscribers.TryGetValue(clientId, out clientProxy))
					{
						clientProxy.Unsubscribe();
						subscribers.Remove(clientId);
					}
				}
				return null;
			}
			catch(Exception ex)
			{
				AsyncMethodResponse response = new AsyncMethodResponse(clientId, null);
				response.AddError(ex.Message, ErrorCode.Fatal);
				return response;
			}
		}

		/// <summary>
		/// called by client to get the async call response
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="payloadTicket"></param>
		/// <returns></returns>
		public AsyncMethodResponse GetAsyncResponseData(Guid clientId, Guid payloadTicket)
		{
			QueuedResponsePayload payload = null;
			lock(payloadTicketToResponseMap)
			{
				payloadTicketToResponseMap.TryGetValue(payloadTicket, out payload);
			}

			if(payload != null &&
				payload.ClientId == clientId)
			{
				AsyncMethodResponse response = new AsyncMethodResponse(clientId, payload.AsyncTicket);
				response.AsyncResponseData = payload.PayloadData;

				lock(payloadTicketToResponseMap)
					payloadTicketToResponseMap.Remove(payloadTicket);

				return response;
			}

			AsyncMethodResponse error = new AsyncMethodResponse(clientId, null);
			error.AddError("Not found", ErrorCode.RecordNotFound);
			return error;
		}

		/// <summary>
		/// Get the stream data
		/// </summary>
		/// <param name="streamDataId"></param>
		/// <returns></returns>
		public System.IO.Stream GetAsyncResponseStreamData(string streamDataId)
		{
			if(streamDataId == null)
				return null;
			string[] streamDataParts = streamDataId.Split('.');

			Guid clientId = new Guid(streamDataParts[0]);

			QueuedResponsePayload payload = null;
			lock(streamIdToPayloadMap)
			{
				streamIdToPayloadMap.TryGetValue(streamDataId, out payload);
			}

			if(payload != null &&
				payload.ClientId == clientId)
			{
				lock(streamIdToPayloadMap)
					streamIdToPayloadMap.Remove(streamDataId);

				return payload.PayloadStream;
			}

			return null;
		}


		/// <summary>
		/// publish message to subscribers
		/// </summary>
		/// <param name="targetClientProxyMap">client to publish to, Guid.Empty to publish to all</param>
		/// <param name="message">text to publish</param>
		/// <param name="response"></param>
		public void PublishMessage(Dictionary<Guid, ClientProxy> targetClientProxyMap, string message, AsyncMethodResponse response)
		{
			PublishMessageEventArg pmea = new PublishMessageEventArg(DateTime.UtcNow, response, message);
			this.Publish(targetClientProxyMap, false, pmea, this.publishMessageHandler);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public ClientProxy LookupClientProxy(Guid clientId)
		{
			ClientProxy clientProxy = null;
			lock(this.subscribers)
				subscribers.TryGetValue(clientId, out clientProxy);

			return clientProxy;
		}

		/// <summary>
		/// /// push the result ticket out to the client. The client then needs to turn around and call GetAsyncResponseData
		/// or GetAsyncResponseStreamData to get the data
		/// </summary>
		/// <param name="targetClientProxy"></param>
		/// <param name="message"></param>
		/// <param name="dataAttachment"></param>
		/// <param name="streamAttachment"></param>
		/// <param name="response"></param>
		public void PublishSendResponseTicket(ClientProxy targetClientProxy, string message, object dataAttachment, Stream streamAttachment, AsyncMethodResponse response)
		{
			QueuedResponsePayload payload = null;

			if(dataAttachment != null ||
				streamAttachment != null)
			{
				payload = new QueuedResponsePayload(dataAttachment, response.AsyncTicket, response.ClientId, streamAttachment);
				if(payload.PayloadStream != null)
				{
					string payloadStreamId = string.Format("{0}.{1}", response.ClientId, payload.PayloadTicket);
					response.StreamDataId = payloadStreamId;
					lock(streamIdToPayloadMap)
						streamIdToPayloadMap[payloadStreamId] = payload;
				}
				else if(payload.PayloadData != null)
				{
					lock(payloadTicketToResponseMap)
						payloadTicketToResponseMap[payload.PayloadTicketGuid] = payload;
				}
			}

			Dictionary<Guid, ClientProxy> targetClientProxyMap = new Dictionary<Guid, ClientProxy>();
			targetClientProxyMap.Add(targetClientProxy.ClientId, targetClientProxy);
			PublishSendResponseTicketEventArg psrtea = new PublishSendResponseTicketEventArg(DateTime.UtcNow, response, message, payload);
			this.Publish(targetClientProxyMap, true, psrtea, this.publishSendResponseTicketHandler);
		}

		private void PublishMessageHandlerProc(ClientProxy clientProxy, PublishEventArgs dpea)
		{
			PublishMessageEventArg dpmea = (PublishMessageEventArg)dpea;
			clientProxy.CallBack.OnServerMessage(dpmea.Message, dpmea.MessageTimeUtc, dpmea.Response);
		}

		// Summary:
		//     Returns a nonnegative random number.
		//
		// Returns:
		//     A 32-bit signed integer greater than or equal to zero and less than System.Int32.MaxValue.
		private int GetRandomNumber()
		{
			int retVal;
			lock(this.random)
			{
				this.randomRequestCount++;
				retVal = this.random.Next();
				if(this.randomRequestCount > MaxRandomRequestsWithSameRandom)
				{
					this.random = new Random();
					this.randomRequestCount = 0;
				}
			}
			return retVal;
		}

		private void Publish(Dictionary<Guid, ClientProxy> targetClientProxyMap, bool isMapPrivateCopy, PublishEventArgs dpea, PublishEventHandler publishHandler)
		{
			DateTime messageTime = DateTime.UtcNow;
			List<ClientProxy> tmpSubscribersList;
			if(isMapPrivateCopy == false)
			{
				lock(targetClientProxyMap)
					tmpSubscribersList = new List<ClientProxy>(targetClientProxyMap.Values);
			}
			else
			{
				tmpSubscribersList = new List<ClientProxy>(targetClientProxyMap.Values);
			}

			//not going to call the publish in the same order
			//each time so that one subscriber does not get precedence
			//with network and other things between the server and 
			//the client this might not matter, but we will do our best
			//to make sure we are fair

			int randomNum = this.GetRandomNumber();

			int startIndex = randomNum % tmpSubscribersList.Count;
			int incrament = (randomNum % 2 == 0) ? 1 : -1;

			int randomIndex = startIndex;
			for(int subscriberIndex = 0; subscriberIndex < tmpSubscribersList.Count; subscriberIndex++)
			{
				this.publishThreadPool.AddPublish(tmpSubscribersList[randomIndex], dpea, publishHandler);

				randomIndex += incrament;
				if(randomIndex < 0)
					randomIndex = tmpSubscribersList.Count - 1;
				else if(randomIndex >= tmpSubscribersList.Count)
					randomIndex = 0;
			}
		}

		private void PublishSendResponseTicketHandlerProc(ClientProxy clientProxy, PublishEventArgs pea)
		{
			PublishSendResponseTicketEventArg psrtea = (PublishSendResponseTicketEventArg)pea;
			clientProxy.CallBack.OnPendingResponseObject(psrtea.Message, psrtea.Payload.PayloadTicket, psrtea.MessageTimeUtc, psrtea.Response);
		}
	}
}

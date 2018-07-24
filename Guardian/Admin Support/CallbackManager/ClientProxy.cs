using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Guardian.ClientCallback
{
	/// <summary>
		/// class that keeps clientId and web callback delegate together
		/// </summary>
		public class ClientProxy
		{
			//message sequenceId
			/// <summary>
			/// 
			/// </summary>
			/// <param name="clientId"></param>
			/// <param name="callBack"></param>
			public ClientProxy(Guid clientId, IServerAdminCallback callBack)
			{
				this.ClientId = clientId;
				this.CallBack = callBack;
				this.pendingMessageList = new List<KeyValuePair<PublishEventArgs, PublishEventHandler>>();
			}

			/// <summary>
			/// 
			/// </summary>
			public Guid ClientId { get; private set; }

			/// <summary>
			/// 
			/// </summary>
			public void Unsubscribe()
			{
				this.CallBack = null;
				this.IsUnsubscribed = true;
			}

			/// <summary>
			/// 
			/// </summary>
			public bool IsUnsubscribed { get; private set; }

			private int sendEndCount;
			private int lastSendTickCount;
			private int averageSendTickCount;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="sendTickCount"></param>
			public void MarkEndSend(int sendTickCount)
			{
				this.lastSendTickCount = sendTickCount;
				sendEndCount++; 
				if(sendEndCount == 1)
					this.averageSendTickCount = sendTickCount;
				else
					this.averageSendTickCount = ((this.averageSendTickCount * sendEndCount) + sendTickCount) / this.sendEndCount;
			}

			
			private IServerAdminCallback callBack;
    		/// <summary>
			/// 
			/// </summary>
			public IServerAdminCallback CallBack
			{ 
				get
				{
					return this.callBack;
				} 
				private set
				{
					this.PublishingThread = null;
					this.callBack = value;
				} 
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="callback"></param>
			public void AssignNewCallback(IServerAdminCallback callback)
			{
				this.CallBack = callback;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="dpea"></param>
			/// <param name="publishHandler"></param>
			public void AddPendingMessage(PublishEventArgs dpea, PublishEventHandler publishHandler)
			{
				lock(this.pendingMessageList)
					this.pendingMessageList.Add(new KeyValuePair<PublishEventArgs, PublishEventHandler>(dpea, publishHandler));
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public List<KeyValuePair<PublishEventArgs, PublishEventHandler>> PopPendingMessageList()
			{
				List<KeyValuePair<PublishEventArgs, PublishEventHandler>> returnMessageList;
				lock(this.pendingMessageList)
				{
					if(this.pendingMessageList.Count == 0)
						return null;

					returnMessageList = new List<KeyValuePair<PublishEventArgs, PublishEventHandler>>(this.pendingMessageList);
					this.pendingMessageList.Clear();
				}

				return returnMessageList;
			}

			private List<KeyValuePair<PublishEventArgs, PublishEventHandler>> pendingMessageList;

			private PublishThread publishingThread;
			
			/// <summary>
			/// 
			/// </summary>
			public PublishThread PublishingThread 
			{ 
				get 
				{ 
					return this.publishingThread; 
				} 
				set 
				{
					if(this.publishingThread != null)
						this.publishingThread.RemoveClientProxy(this);

					this.publishingThread = value;
					if(this.publishingThread != null)
						this.publishingThread.AddClientProxy(this);
				} 
			}
		}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace FluidTrade.Guardian.ClientCallback
{
	/// <summary>
	/// 
	/// </summary>
	public class PublishThread
	{
		private int threadSleepTime = 200;
		private int threadWaitTime = 77;

		private HashSet<ClientProxy> clientProxySet;
		private FluidTrade.Core.WaitQueue<PublishParameters> waitQueue;
		private Thread innerThread;
		private int poolIndex;

		/// <summary>
		///
		/// </summary>
		/// <param name="poolIndex"></param>
		public PublishThread(int poolIndex)
		{
			this.poolIndex = poolIndex;
			this.clientProxySet = new HashSet<ClientProxy>();
			this.waitQueue = new FluidTrade.Core.WaitQueue<PublishParameters>();
			innerThread = new Thread(new ThreadStart(this.ThreadProc));
			innerThread.IsBackground = true;
			innerThread.Name = string.Concat("Publish_", this.poolIndex);
			innerThread.Start();
		}

		/// <summary>
		/// 
		/// </summary>
		public int ClientCount
		{
			get
			{
				lock(this.clientProxySet)
					return this.clientProxySet.Count;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientProxy"></param>
		public void AddClientProxy(ClientProxy clientProxy)
		{
			lock(this.clientProxySet)
				this.clientProxySet.Add(clientProxy);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientProxy"></param>
		public void RemoveClientProxy(ClientProxy clientProxy)
		{
			lock(this.clientProxySet)
				this.clientProxySet.Remove(clientProxy);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientProxy"></param>
		/// <param name="dpea"></param>
		/// <param name="publishHandler"></param>
		public void AddPublish(ClientProxy clientProxy, PublishEventArgs dpea, PublishEventHandler publishHandler)
		{
			this.waitQueue.Enqueue(new PublishParameters(clientProxy, dpea, publishHandler));
		}

		/// <summary>
		/// 
		/// </summary>
		protected class PublishParameters
		{
			/// <summary>
			/// 
			/// </summary>
			/// <param name="clientProxy"></param>
			/// <param name="dpea"></param>
			/// <param name="publishHandler"></param>
			public PublishParameters(ClientProxy clientProxy, PublishEventArgs dpea, PublishEventHandler publishHandler)
			{
				this.ClientProxy = clientProxy;
				this.PublishEventArg = dpea;
				this.PublishHandler = publishHandler;
			}

			/// <summary>
			/// 
			/// </summary>
			public ClientProxy ClientProxy { get; private set; }
			
			/// <summary>
			/// 
			/// </summary>
			public PublishEventArgs PublishEventArg{ get; private set; }
			
			/// <summary>
			/// 
			/// </summary>
			public PublishEventHandler PublishHandler{ get; private set; }
		}

		private void ThreadProc()
		{
			// The event handlers for the data model can't wait on locks and resources outside the data model.  There would simply be too many resources that 
			// could deadlock.  This code will pull requests off of a generic queue of actions and parameters and execute them using the authentication created
			// above.
			while(true)
			{
				if(this.waitQueue.Count == 0)
					Thread.Sleep(threadSleepTime);
				try
				{
					bool timedOut;
					// The thread will wait here until an action has been placed in the queue to be processed in this thread context.
					PublishParameters parameters = this.waitQueue.Dequeue(threadWaitTime, out timedOut);
					if(parameters == null)
						continue;

					this.PublishProc(parameters);
				}
				catch(ThreadAbortException)
				{
                    return;
				}
				catch(Exception exception)
				{

					// This will catch any exceptions thrown during the processing of the generic actions.
					FluidTrade.Core.EventLog.Error("{0} {1}: {2}\r\n{3}", Thread.CurrentThread.Name, exception.Message, exception.ToString(), exception.StackTrace);

				}
			}
		}

		private void PublishProc(PublishParameters publishParameters)
		{
			if(publishParameters.ClientProxy.IsUnsubscribed == true)
				return;

			try
			{
				if(((ICommunicationObject)publishParameters.ClientProxy.CallBack).State == CommunicationState.Opened)
				{
					try
					{
						int start = Environment.TickCount;
						publishParameters.PublishHandler(publishParameters.ClientProxy, publishParameters.PublishEventArg);
						publishParameters.ClientProxy.MarkEndSend(Environment.TickCount - start);
					}
					catch(Exception ex)
					{
						publishParameters.ClientProxy.AddPendingMessage(publishParameters.PublishEventArg, publishParameters.PublishHandler);
						FluidTrade.Core.EventLog.Error(ex);
					}
				}
				else
				{
					publishParameters.ClientProxy.AddPendingMessage(publishParameters.PublishEventArg, publishParameters.PublishHandler);
				}
			}
			catch(Exception ex)
			{
				FluidTrade.Core.EventLog.Error(ex);
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class PublishThreadPool
	{
		private List<PublishThread> publishThreadList;
		private static readonly int MaxThreads;

		static PublishThreadPool()
		{
			MaxThreads = System.Environment.ProcessorCount * 200;
		}

		/// <summary>
		/// 
		/// </summary>
		public PublishThreadPool()
		{
			this.publishThreadList = new List<PublishThread>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clientProxy"></param>
		/// <param name="dpea"></param>
		/// <param name="publishHandler"></param>
		public void AddPublish(ClientProxy clientProxy, PublishEventArgs dpea, PublishEventHandler publishHandler)
		{
			if(clientProxy.PublishingThread == null)
			{
				lock(clientProxy)
				{
					if(clientProxy.PublishingThread == null)
					{
						lock(this.publishThreadList)
						{
							foreach(PublishThread curPublishThread in this.publishThreadList)
							{
								if(curPublishThread.ClientCount == 0)
								{
									clientProxy.PublishingThread = curPublishThread;
									break;
								}
							}

							if(clientProxy.PublishingThread == null)
							{
								PublishThread publishThread = null;
								if(this.publishThreadList.Count < MaxThreads)
								{
									publishThread = new PublishThread(this.publishThreadList.Count);
									this.publishThreadList.Add(publishThread);
								}
								else
								{
									int minClientCount = int.MaxValue;
									foreach(PublishThread curPublishThread in this.publishThreadList)
									{
										if(curPublishThread.ClientCount < minClientCount)
										{
											publishThread = curPublishThread;
											minClientCount = curPublishThread.ClientCount;
										}
									}
								}
								clientProxy.PublishingThread = publishThread;
							}
						}
					}
				}
			}

			clientProxy.PublishingThread.AddPublish(clientProxy, dpea, publishHandler);
		}
	}
}

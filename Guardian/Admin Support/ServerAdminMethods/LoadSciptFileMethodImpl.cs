using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidTrade.Core;
using FluidTrade.Guardian.Records;
using System.IO;
using System.Transactions;
using FluidTrade.Guardian.ClientCallback;

namespace FluidTrade.Guardian
{
	public partial class ServerAdmin
	{
		/// <summary>
		/// Class that encapsulates the work for LoadSciptFileMethodImpl.
		/// This is done as a class so it is easy to store information about the method parameters/state
		/// between the calling thread and the worker thread.
		/// LoadSciptFileMethodImpl is a nested class of ServerAdmin so that
		/// it has access to the ServerAdmin.PublishMessage(). 
		/// </summary>
		protected class LoadSciptFileMethodImpl
		{
			/// <summary>
			/// script file to load
			/// </summary>
			private string scriptFilePath;

			/// <summary>
			/// ticket string to pass back to caller though callback so caller can match message to 
			/// the original request
			/// </summary>
			private string asyncTicket;


			private ServerAdmin serverAdmin;

			private ClientProxy clientProxy;

			private bool streamConents;

			/// <summary>
			/// ctor
			/// </summary>
			/// <param name="serverAdmin"></param>
			/// <param name="clientProxy"></param>
			/// <param name="scriptFilePath">script file to load (path to file on the server)</param>
			/// <param name="asyncTicket">ticket string to pass back to caller though callback so caller can match message to the original request</param>
			/// <param name="streamConents">should the contents of the file be streamed back to the client</param>
			public LoadSciptFileMethodImpl(ServerAdmin serverAdmin, ClientProxy clientProxy, string scriptFilePath, string asyncTicket, bool streamConents)
			{
				this.streamConents = streamConents;
				this.clientProxy = clientProxy;
				this.serverAdmin = serverAdmin;
				this.scriptFilePath = scriptFilePath.Trim().ToLowerInvariant();
				this.asyncTicket = asyncTicket;

				//start the thread to do the work
				ThreadPoolHelper.QueueUserWorkItem(new System.Threading.WaitCallback(this.LoadScriptFileProc));
			}

			/// <summary>
			/// Thread Method to do the work of Loading the file in a worker thread
			/// </summary>
			/// <param name="state">not used, but needed to match the WaitCallback signature</param>
			private void LoadScriptFileProc(object state)
			{
				AsyncMethodResponse response = new AsyncMethodResponse(this.clientProxy.ClientId, this.asyncTicket);
				FileStream responseFileStream = null;
				try
				{
					string directoryPath = System.IO.Path.GetDirectoryName(this.scriptFilePath);
					
					//figure out what file(s) need to be loaded. if xml then assume it is just one file
					//if not xml assume that the files contains a list of paths to xml files
					List<string> filePathList = new List<string>();
					if(this.scriptFilePath.EndsWith(".xml") == false)
					{
						using(System.IO.FileStream fileStream = new System.IO.FileStream(this.scriptFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
						{
							using(StreamReader sr = new StreamReader(fileStream))
							{
								string curFileLine = null;
								while((curFileLine = sr.ReadLine()) != null)
								{
									curFileLine = curFileLine.Trim();
									if(string.IsNullOrEmpty(curFileLine))
										continue;

									curFileLine = Path.IsPathRooted(curFileLine) ? curFileLine :
																	Path.Combine(directoryPath, curFileLine);

									//make sure to use the absolute path
									filePathList.Add(curFileLine);
								}
							}
						}
					}
					else
					{
						filePathList.Add(this.scriptFilePath);
					}

					try
					{
						foreach(string scriptFile in filePathList)
						{
							if(scriptFile == null)
								continue;

							

								//create a scriptloader to load the file
								ScriptLoader scriptLoader = new ScriptLoader();

								//subscribe to scriptloader messages so can publish them back to the client
								scriptLoader.NotifyMessage += new ScriptLoader.MessageEventHander(scriptLoader_NotifyMessage);
								//set scriptloader in local mode (meaning it is running inside the server)
								scriptLoader.LocalMode = true;
								scriptLoader.FileName = scriptFile;

								//load the file into the MT/DB
								scriptLoader.Load();
								
						}

						if(this.streamConents == true)
							responseFileStream = File.OpenRead(this.scriptFilePath);
					
						//it all worked
						response.Result = ErrorCode.Success;
					}
					catch(Exception ex)
					{
						//response.AddError(ex.Message);
						//catch exception and add it to the return
						EventLog.Warning(String.Format("{0}: {1}", ex.GetType(), ex.Message));
						response.AddError(ex.Message, ErrorCode.Fatal);
					}
				}
				catch(Exception ex)
				{
					//response.AddError(ex.Message);
					//catch exception and add it to the return
					//response.AddError(ex);
					EventLog.Warning(String.Format("{0}: {1}", ex.GetType(), ex.Message));
					response.AddError(ex.Message, ErrorCode.Fatal);
				}

				if(responseFileStream != null)
				{
					CallbackManagerImpl.Instance.PublishSendResponseTicket(this.clientProxy, null, null, responseFileStream, response);
				}
				else
				{
					//notify client that we are all done
					CallbackManagerImpl.Instance.PublishSendResponseTicket(this.clientProxy, "PublishSendResponseTicketResp", string.Format("Completed: {0}\r\n", this.scriptFilePath), null, response);
				}
			}

			/// <summary>
			/// handler for scriptloader progress messages
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="mea"></param>
			private void scriptLoader_NotifyMessage(ScriptLoader sender, ScriptLoader.MessageEventArgs mea)
			{
				AsyncMethodResponse response = new AsyncMethodResponse(this.clientProxy.ClientId, this.asyncTicket);

				Dictionary<Guid, ClientProxy> targetProxyMap = new Dictionary<Guid, ClientProxy>();
				targetProxyMap.Add(clientProxy.ClientId, clientProxy);

				//send null message for tick
				if(mea.IsProgressTick == true)
					CallbackManagerImpl.Instance.PublishMessage(targetProxyMap, null, response);
				else
					CallbackManagerImpl.Instance.PublishMessage(targetProxyMap, mea.Message, response);
			}
		}
	}
}
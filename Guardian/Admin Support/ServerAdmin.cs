using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using FluidTrade.Core;
using FluidTrade.Guardian.Records;
using System.IO;
using System.Threading;
using FluidTrade.Guardian.ClientCallback;
using System.Transactions;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// Server Admin impl to be used for FluidTrade internal 
	/// administration. This interface should not be published for general consumption
	/// </summary>
	public partial class ServerAdmin : IServerAdmin
	{
		/// <summary>
		/// IServerAdmin.LoadScriptFile impl
		///  load script file. File needs to be on the server and in an accessible directory
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="scriptFilePath">path to file to load. If not an xml file will assume that the file contains a list of xml files</param>
		/// <param name="streamFileContentsBackToClient">should the server stream the files back to the client</param>
		public AsyncMethodResponse LoadScriptFile(Guid clientId, string scriptFilePath, bool streamFileContentsBackToClient)
		{
			AsyncMethodResponse response = new AsyncMethodResponse(clientId, null);
			//check for empty path
			if(string.IsNullOrEmpty(scriptFilePath))
			{
				response.AddError("Missing filePath", ErrorCode.Fatal);
				return response;
			}

			//this call will return right away and
			//pass back an async ticket. the processing
			//will be done in a worker
			//and updates/information about the progress will
			//be sent back to the caller via a callback.
			response.AsyncTicket = Guid.NewGuid().ToString();

			ClientProxy clientProxy = CallbackManagerImpl.Instance.LookupClientProxy(clientId);

			//contructing the Impl will start a thread to do the work
			new LoadSciptFileMethodImpl(this, clientProxy, scriptFilePath, response.AsyncTicket, streamFileContentsBackToClient);

			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		public void OutputLocksToLog()
		{
			EventLog.Information("LOGGING LOCKS  tickCount:{0}", Environment.TickCount);
			List<string> transactionList = DataModelTransaction.GetTransactionStacks();

			if(transactionList != null)
			{
				foreach(string txnStack in transactionList)
				{
					EventLog.Information("<<<<**>>>>Txn Stack\r\n{0}\r\n", txnStack);
				}
			}
			DataModel.DataLock.EnterReadLock();
			try
			{
				System.Data.DataSet ds = DataModel.Match.DataSet;
				foreach(System.Data.DataTable table in ds.Tables)
				{
					foreach(DataRowBase row in table.Rows)
					{
						string tmp = row.GetCurrentLockStacks(true);
						if(tmp != null)
						{
							EventLog.Information("<<<<**>>>>{0}\r\n{1}\r\n", row.Table.TableName, tmp);
						}
					}
				}
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}
		}

		/// <summary>
		/// find any single matches and get rid of them
		/// </summary>
		public void CleanUnpairedMatches()
		{
			MatchDataTable.CleanUnpairedMatches();
		}

		/// <summary>
		/// find any single matches and get rid of them
		/// </summary>
		public void RematchAllWorkingOrders()
		{
			MatchDataTable.RematchAllWorkingOrders();
		}

		/// <summary>
		/// reload the server configs
		/// </summary>
		public void ResetServerConfigs()
		{
			System.Reflection.FieldInfo dataModelDataSetFI = typeof(DataModel).GetField("dataModelDataSet", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			System.Reflection.FieldInfo identifierFI = dataModelDataSetFI.FieldType.GetField("identifier", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			FluidTrade.Core.Properties.Settings.Default.Reload();
			identifierFI.SetValue(dataModelDataSetFI.GetValue(null), global::FluidTrade.Guardian.Properties.Settings.Default.DataModelInstanceId);
		}
	}
	
	/// <summary>
	/// Service that manages Subscriptions publication of messages to clients
	/// </summary>
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class ServerAdminCallbackManager : IServerAdminCallbackManager
	{
		/// <summary>
		/// subscribe for updates
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public AsyncMethodResponse Subscribe(Guid clientId)
		{
			return CallbackManagerImpl.Instance.Subscribe(clientId);
		}

		/// <summary>
		/// unsubscribe for updates
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public AsyncMethodResponse Unsubscribe(Guid clientId)
		{
			return CallbackManagerImpl.Instance.Unsubscribe(clientId);
		}

		/// <summary>
		/// Get the data for the update
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="payloadTicket"></param>
		/// <returns></returns>
		public AsyncMethodResponse GetAsyncResponseData(Guid clientId, Guid payloadTicket)
		{
			return CallbackManagerImpl.Instance.GetAsyncResponseData(clientId, payloadTicket);
		}
	}

	/// <summary>
	/// service that can return Sreamed data. It would be nice it 
	/// this was include in the ServerAdmin or ServerAdminCallbackManager
	/// but there are restrictions on using streams and callback. so this has been 
	/// pulled out into another service that work with ServerAdmin/ServerAdminCallbackManager
	/// </summary>
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class ServerAdminStreamManager : IServerAdminStreamManager
	{
		/// <summary>
		/// Get the stream data
		/// </summary>
		/// <param name="streamDataId"></param>
		/// <returns></returns>
		public System.IO.Stream GetAsyncResponseStreamData(string streamDataId)
		{
			return CallbackManagerImpl.Instance.GetAsyncResponseStreamData(streamDataId);
		}
	}
}

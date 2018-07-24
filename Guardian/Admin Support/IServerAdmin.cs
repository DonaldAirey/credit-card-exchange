using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using FluidTrade.Guardian.Records;
using FluidTrade.Core;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// Server Admin interface to be used for FluidTrade internal 
	/// administration. This interface should not be published for general consumption
	/// </summary>
	[System.ServiceModel.ServiceContractAttribute(ConfigurationName = "IServerAdmin")]
	public interface IServerAdmin
	{
		/// <summary>
		/// load script file. File needs to be on the server and in an accessible directory
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="scriptFilePath">path to file to load. If not an xml file will assume that the file contains a list of xml files</param>
		/// <param name="streamFileContentsBackToClient">true if should sream file contents back to client</param>
		[OperationContract]
		[ServiceKnownType(typeof(AsyncMethodResponse))]
		AsyncMethodResponse LoadScriptFile(Guid clientId, string scriptFilePath, bool streamFileContentsBackToClient);

		/// <summary>
		/// Go through all the the rows in all the tables and if there is a lock
		/// put it into the event log as info
		/// </summary>
		[OperationContract]
		void OutputLocksToLog();

		/// <summary>
		/// find any single matches and get rid of them
		/// </summary>
		[OperationContract]
		void CleanUnpairedMatches();

		/// <summary>
		/// find any single matches and get rid of them
		/// </summary>
		[OperationContract]
		void RematchAllWorkingOrders();

		/// <summary>
		/// find any single matches and get rid of them
		/// </summary>
		[OperationContract]
		void ResetServerConfigs();
	}

	/// <summary>
	/// Server Admin interface to be used for FluidTrade internal 
	/// administration. This interface should not be published for general consumption
	/// </summary>
	[System.ServiceModel.ServiceContractAttribute(ConfigurationName = "IServerAdminStreamManager")]
	public interface IServerAdminStreamManager
	{
		/// <summary>
		/// get the streaming data
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(AsyncMethodResponse))]
		System.IO.Stream GetAsyncResponseStreamData(string streamDataId);
	}

	/// <summary>
	/// Server Admin interface to be used for FluidTrade internal 
	/// administration. This interface should not be published for general consumption
	/// </summary>
	[System.ServiceModel.ServiceContractAttribute(ConfigurationName = "IServerAdminCallbackManager", CallbackContract = typeof(IServerAdminCallback))]
	public interface IServerAdminCallbackManager
	{

		/// <summary>
		/// subscribe for updates from server
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(AsyncMethodResponse))]
		AsyncMethodResponse Subscribe(Guid clientId);

		/// <summary>
		/// unsubscribe for updates
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(AsyncMethodResponse))]
		AsyncMethodResponse Unsubscribe(Guid clientId);


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(AsyncMethodResponse))]
		AsyncMethodResponse GetAsyncResponseData(Guid clientId, Guid payloadTicket);
	}

	/// <summary>
	/// Callback interface for IServerAdmin
	/// </summary>
	public interface IServerAdminCallback
	{
		/// <summary>
		/// the callback method for server messages
		/// </summary>
		/// <param name="message">message text from server</param>
		/// <param name="timestamp"></param>
		/// <param name="response"></param>
		[OperationContract]
		void OnServerMessage(string message, DateTime timestamp, AsyncMethodResponse response);

		/// <summary>
		/// callback for a pending response object that is an object type vs stream
		/// </summary>
		/// <param name="message"></param>
		/// <param name="asyncPayloadTicket"></param>
		/// <param name="timestamp"></param>
		/// <param name="response"></param>
		[OperationContract]
		void OnPendingResponseObject(string message, string asyncPayloadTicket, DateTime timestamp, AsyncMethodResponse response);

	}	
}

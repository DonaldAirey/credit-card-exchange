namespace FluidTrade.Core
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;

	/// <summary>
	/// A catch-all handler for WCF exceptions. We can hook this up in the cofig 
	/// </summary>
	public class ServerErrorHandler : IErrorHandler, IServiceBehavior 
	{
		/// <summary>
		/// Log the exception
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool HandleError(Exception error)
		{
			EventLog.Error(error);
			return true;
		}

		/// <summary>
		/// ProvideFault
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="version"></param>
		/// <param name="fault"></param>
		public void ProvideFault(Exception exception, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
		{	
			//Propogate the exception back to the client.			
			//if (exception is FaultException)
			//	return;			
			//else
			//{
			//    FaultException faultException = new FaultException(exception.Message);
			//    MessageFault messageFault = faultException.CreateMessageFault();
			//    fault = Message.CreateMessage(version, messageFault, faultException.Action);
			//}
		}


		/// <summary>
		/// AddBindingParameters
		/// </summary>
		/// <param name="serviceDescription"></param>
		/// <param name="serviceHostBase"></param>
		/// <param name="endpoints"></param>
		/// <param name="bindingParameters"></param>
		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		/// <summary>
		/// ApplyDispatchBehavior.  Insert our Errorhandler.
		/// </summary>
		/// <param name="serviceDescription"></param>
		/// <param name="serviceHostBase"></param>
		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			IErrorHandler errorHandler = new ServerErrorHandler();

			foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
			{
				ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
				channelDispatcher.ErrorHandlers.Add(errorHandler);
			}                   

		}

		/// <summary>
		/// Validate  - we do not do anything here.  It is here just because the interface ditates it
		/// </summary>
		/// <param name="serviceDescription"></param>
		/// <param name="serviceHostBase"></param>
		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}

	}
}

﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4016
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FluidTrade.Guardian.CallbackManagerWebRef {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="CallbackManagerWebRef.IServerAdminCallbackManager", CallbackContract=typeof(FluidTrade.Guardian.CallbackManagerWebRef.IServerAdminCallbackManagerCallback))]
    public interface IServerAdminCallbackManager {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerAdminCallbackManager/Subscribe", ReplyAction="http://tempuri.org/IServerAdminCallbackManager/SubscribeResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(FluidTrade.Core.ErrorCode))]
        FluidTrade.Core.AsyncMethodResponse Subscribe(System.Guid clientId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerAdminCallbackManager/Unsubscribe", ReplyAction="http://tempuri.org/IServerAdminCallbackManager/UnsubscribeResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(FluidTrade.Core.ErrorCode))]
        FluidTrade.Core.AsyncMethodResponse Unsubscribe(System.Guid clientId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerAdminCallbackManager/GetAsyncResponseData", ReplyAction="http://tempuri.org/IServerAdminCallbackManager/GetAsyncResponseDataResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(FluidTrade.Core.ErrorCode))]
        FluidTrade.Core.AsyncMethodResponse GetAsyncResponseData(System.Guid clientId, System.Guid payloadTicket);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface IServerAdminCallbackManagerCallback {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerAdminCallbackManager/OnServerMessage", ReplyAction="http://tempuri.org/IServerAdminCallbackManager/OnServerMessageResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(FluidTrade.Core.ErrorCode))]
        void OnServerMessage(string message, System.DateTime timestamp, FluidTrade.Core.AsyncMethodResponse response);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServerAdminCallbackManager/OnPendingResponseObject", ReplyAction="http://tempuri.org/IServerAdminCallbackManager/OnPendingResponseObjectResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(FluidTrade.Core.ErrorCode))]
        void OnPendingResponseObject(System.Guid payloadTicket, System.DateTime timestamp, FluidTrade.Core.AsyncMethodResponse response);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface IServerAdminCallbackManagerChannel : FluidTrade.Guardian.CallbackManagerWebRef.IServerAdminCallbackManager, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class ServerAdminCallbackManagerClient : System.ServiceModel.DuplexClientBase<FluidTrade.Guardian.CallbackManagerWebRef.IServerAdminCallbackManager>, FluidTrade.Guardian.CallbackManagerWebRef.IServerAdminCallbackManager {
        
        public ServerAdminCallbackManagerClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public ServerAdminCallbackManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public ServerAdminCallbackManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ServerAdminCallbackManagerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ServerAdminCallbackManagerClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public FluidTrade.Core.AsyncMethodResponse Subscribe(System.Guid clientId) {
            return base.Channel.Subscribe(clientId);
        }
        
        public FluidTrade.Core.AsyncMethodResponse Unsubscribe(System.Guid clientId) {
            return base.Channel.Unsubscribe(clientId);
        }
        
        public FluidTrade.Core.AsyncMethodResponse GetAsyncResponseData(System.Guid clientId, System.Guid payloadTicket) {
            return base.Channel.GetAsyncResponseData(clientId, payloadTicket);
        }
    }
}
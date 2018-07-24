namespace FluidTrade.Guardian
{

	using System;
	using System.Runtime.Serialization;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// Configuration Parameters.
	/// </summary>
	[DataContract]
	public class OperationParameters
	{
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Boolean AreBusinessRulesActive;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Boolean IsCrossingActive;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Boolean IsChatActive;
	}

	/// <summary>
	/// Simulation parameters 
	/// </summary>
	[DataContract]
	public class SimulationParameters
	{
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Boolean IsBrokerSimulationRunning;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Boolean IsPriceSimulationRunning;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Double Frequency;

	}

	/// <summary>
	/// Destination Order entity.
	/// </summary>
	[DataContract]
	public class DestinationOrderInfo
	{

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid BlotterId;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid DestinationId;
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Decimal OrderedQuantity;

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid OrderTypeId;

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid SecurityId;

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid SettlementId;

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid SideCodeId;

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid TimeInForceCodeId;

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid WorkingOrderId;

	}

	/// <summary>
	/// Destination order identification
	/// </summary>
	[DataContract]
	public class DestinationOrderReference
	{

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Guid DestinationId;

		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public Int64 RowVersion;

	}

	/// <summary>
	/// Web service constract.
	/// </summary>
	[ServiceContractAttribute(ConfigurationName = "ITradingSupport")]
	public interface ITradingSupport
	{
		/// <summary>
		/// Accept a match.
		/// </summary>
		/// <param name="matchId">Match id to accept</param>
		/// <param name="rowVersion">Unique identifier of the match row.</param>
		[OperationContract]
		void AcceptMatch(Guid matchId, Int64 rowVersion);

		/// <summary>
		/// Clears the destination orders
		/// </summary>
		[OperationContract]
		void ClearDestinationOrders();

		/// <summary>
		/// Create Consumer record(s).		
		/// <param name="objects"></param>
		/// <returns></returns>
		/// /// </summary>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Consumer))]
		MethodResponse<Guid[]> CreateConsumer(Consumer[] objects);


		/// <summary>
		/// Create Debt Holder translation record(s). These are used by the import to map
		/// external data to internal fields/.
		/// <param name="objects"></param>
		/// <returns></returns>
		/// /// </summary>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(DebtHolderImportTranslation))]
		MethodResponse<Guid[]> CreateDebtHolderImportTranslation(DebtHolderImportTranslation[] objects);


		/// <summary>
		/// Update Consumer record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Consumer))]
		MethodResponse<ErrorCode> DeleteConsumer(Consumer[] objects);

		/// <summary>
		/// Update Consumer record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Consumer))]
		MethodResponse<ErrorCode> UpdateConsumer(Consumer[] objects);


		/// <summary>
		/// Get Consumer record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Consumer))]
		MethodResponse<Consumer[]> GetConsumer(Guid[] ids);

		/// <summary>
		/// Update ConsumerDebt record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(ConsumerDebt))]
		MethodResponse<Guid[]> CreateConsumerDebt(ConsumerDebt[] objects);

		/// <summary>
		/// Update ConsumerDebt record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(ConsumerDebt))]
		MethodResponse<ErrorCode> UpdateConsumerDebt(ConsumerDebt[] objects);

		/// <summary>
		/// Update ConsumerDebt record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(ConsumerDebt))]
		MethodResponse<ErrorCode> DeleteConsumerDebt(ConsumerDebt[] objects);

		/// <summary>
		/// Get ConsumerDebt record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Consumer))]
		MethodResponse<Consumer[]> GetConsumerDebt(Guid[] ids);

		/// <summary>
		/// Move ConsumerTrust to new blotter.
		/// </summary>
		/// <param name="orders"></param>
		/// <param name="blotterId"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		MethodResponse<ErrorCode> MoveConsumerDebtToBlotter(Guid blotterId, BaseRecord[] orders);

		/// <summary>
		/// Update ConsumerDebtNegotiation record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Records.ConsumerDebtNegotiationInfo))]
		MethodResponse<ErrorCode> DeleteConsumerDebtNegotiation(Records.ConsumerDebtNegotiationInfo[] objects);

		/// <summary>
		/// Get ConsumerDebtNegotiation record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Records.ConsumerDebtNegotiationInfo))]
		MethodResponse<Records.ConsumerDebtNegotiationInfo[]> GetConsumerDebtNegotiation(Guid[] ids);

		/// <summary>
		/// Import debt holder records.
		/// </summary>
		/// <param name="objects">The records to import.</param>
		/// <returns>The results of importing the records.</returns>
		[System.ServiceModel.OperationContract]
		[System.ServiceModel.FaultContract(typeof(RecordNotFoundFault))]
		[System.ServiceModel.ServiceKnownType(typeof(DebtHolderRecord))]
		MethodResponse<Guid[]> ImportDebtHolderRecords(DebtHolderRecord[] objects);

		/// <summary>
		/// Import debt negotiator records.
		/// </summary>
		/// <param name="objects">The records to import.</param>
		/// <returns>The results of importing the records.</returns>
		[System.ServiceModel.OperationContract]
		[System.ServiceModel.FaultContract(typeof(RecordNotFoundFault))]
		[System.ServiceModel.ServiceKnownType(typeof(DebtHolderRecord))]
		MethodResponse<Guid[]> ImportDebtNegotiatorRecords(DebtNegotiatorRecord[] objects);

		/// <summary>
		/// 
		/// </summary>
		[OperationContract]
		[System.ServiceModel.ServiceKnownType(typeof(DebtHolder))]
		MethodResponse<Guid[]> CreateDebtHolder(DebtHolder[] objects);

		/// <summary>
		/// Create a Chat Item.
		/// </summary>
		/// <param name="chatInfos">An array of items used in a chat session.</param>
		[OperationContract]
		[System.ServiceModel.FaultContract(typeof(RecordNotFoundFault))]
		[System.ServiceModel.ServiceKnownType(typeof(ChatInfo))]
		void CreateChat(ChatInfo[] chatInfos);

		/// <summary>
		/// Update Debt Holder record(s). If the debt holder does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(DebtHolder))]
		MethodResponse<ErrorCode> UpdateDebtHolder(DebtHolder[] objects);

		/// <summary>
		/// 
		/// </summary>
		[OperationContract]
		[System.ServiceModel.ServiceKnownType(typeof(DebtHolder))]
		MethodResponse<ErrorCode> DeleteDebtHolder(DebtHolder[] objects);

		/// <summary>
		/// 
		/// </summary>
		[OperationContract]
		[System.ServiceModel.ServiceKnownType(typeof(DebtNegotiator))]
		MethodResponse<Guid[]> CreateDebtNegotiator(DebtNegotiator[] objects);

		/// <summary>
		/// Update Debt Negotiator record(s). If the debt negotiator does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(DebtNegotiator))]
		MethodResponse<ErrorCode> UpdateDebtNegotiator(DebtNegotiator[] objects);

		/// <summary>
		/// 
		/// </summary>
		[OperationContract]
		[System.ServiceModel.ServiceKnownType(typeof(DebtNegotiator))]
		MethodResponse<ErrorCode> DeleteDebtNegotiator(DebtNegotiator[] objects);

		/// <summary>
		/// Accept the Consumer Debt Settlement.
		/// </summary>
		/// <param name="consumerDebtSettlementAcceptInfos"></param>
		[OperationContract]
		[ServiceKnownType(typeof(Records.ConsumerTrustNegotiationInfo))]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		[FaultContract(typeof(ArgumentFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		void AcceptConsumerDebtSettlement(ConsumerDebtSettlementAcceptInfo[] consumerDebtSettlementAcceptInfos);

		/// <summary>
		/// Create a consumer debt settlement.
		/// </summary>
		/// <param name="consumerDebtSettlementId"></param>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		[FaultContract(typeof(ArgumentFault))]
		[FaultContract(typeof(PaymentMethodFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		void CreateConsumerDebtSettlement(Guid consumerDebtSettlementId);
		
		/// <summary>
		/// Rejects a collection of Consumer Debt Negotiations
		/// </summary>
		/// <param name="consumerDebtNegotiations">An array of Consumer Trust negotiation records.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerDebtNegotiationInfo))]
		void RejectConsumerDebtNegotiation(ConsumerDebtNegotiationInfo[] consumerDebtNegotiations);

		/// <summary>
		/// Reset the Consumer Debt Settlement status bit.
		/// </summary>
		/// <param name="consumerDebtSettlements"></param>
		[OperationContract]				
		[FaultContract(typeof(ArgumentFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		MethodResponse<ErrorCode> ResetConsumerDebtSettlement(BaseRecord[] consumerDebtSettlements);


		/// <summary>
		/// Move ConsumerTrust to new blotter.
		/// </summary>
		/// <param name="orders"></param>
		/// <param name="blotterId"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		MethodResponse<ErrorCode> MoveConsumerTrustToBlotter(Guid blotterId, BaseRecord[] orders);

		/// <summary>
		/// Update ConsumerTrust record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(ConsumerTrust))]
		MethodResponse<ErrorCode> UpdateConsumerTrust(ConsumerTrust[] objects);


		/// <summary>
		/// Get ConsumerTrust record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Consumer))]
		MethodResponse<Consumer[]> GetConsumerTrust(Guid[] ids);

		/// <summary>
		/// Updates Consumer Trust negotiation record(s).
		/// </summary>
		/// <param name="consumerDebtNegotiations">An array of Consumer Trust negotiation records.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerTrustNegotiationInfo))]
		void UpdateConsumerDebtNegotiation(Records.ConsumerDebtNegotiationInfo[] consumerDebtNegotiations);

		/// <summary>
		/// Set the IsRead flag on a collection of Consumer Debt negotiations.
		/// </summary>
		/// <param name="consumerDebtNegotiationIsReads">An array of records that indicate the new state of the negotiation.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerDebtNegotiationInfo))]
		void UpdateConsumerDebtNegotiationIsRead(Records.ConsumerDebtNegotiationIsReadInfo[] consumerDebtNegotiationIsReads);

		/// <summary>
		/// Reverts negotiators to their original state.
		/// </summary>
		/// <param name="consumerTrustNegotiations">An array of Consumer Trust negotiation records.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerTrustNegotiationInfo))]
		void RejectConsumerTrustNegotiation(ConsumerTrustNegotiationInfo[] consumerTrustNegotiations);


		/// <summary>
		/// Updates Consumer Trust negotiation record(s).
		/// </summary>
		/// <param name="consumerTrustNegotiations">An array of Consumer Trust negotiation records.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerTrustNegotiationInfo))]
		void UpdateConsumerTrustNegotiation(Records.ConsumerTrustNegotiationInfo[] consumerTrustNegotiations);

		/// <summary>
		/// Set the IsRead flag on a collection of Consumer Trust negotiations.
		/// </summary>
		/// <param name="consumerTrustNegotiationIsReads">An array of records that indicate the new state of the negotiation.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerTrustNegotiationInfo))]
		void UpdateConsumerTrustNegotiationIsRead(Records.ConsumerTrustNegotiationIsReadInfo[] consumerTrustNegotiationIsReads);

		/// <summary>
		/// Create consumer trust settlement.
		/// </summary>
		/// <param name="consumerTrustSettlementId"></param>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		[FaultContract(typeof(ArgumentFault))]
		[FaultContract(typeof(PaymentMethodFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		void CreateConsumerTrustSettlement(Guid consumerTrustSettlementId);

		/// <summary>
		/// Update CreditCard record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(CreditCard))]
		MethodResponse<Guid[]> CreateCreditCard(CreditCard[] objects);

		/// <summary>
		/// Update CreditCard record(s). If the CreditCard does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(CreditCard))]
		MethodResponse<ErrorCode> UpdateCreditCard(CreditCard[] objects);

		/// <summary>
		/// Update CreditCard record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(CreditCard))]
		MethodResponse<ErrorCode> DeleteCreditCard(CreditCard[] objects);

		/// <summary>
		/// Get CreditCard record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(CreditCard))]
		MethodResponse<CreditCard[]> GetCreditCard(Guid[] ids);

		/// <summary>
		/// Updates Consumer Trust payment record(s).
		/// </summary>
		/// <param name="consumerDebtPayments">An array of Consumer Trust payment records.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerDebtPayment))]
		MethodResponse<ErrorCode> UpdateConsumerDebtPayment(Records.ConsumerDebtPayment[] consumerDebtPayments);

		/// <summary>
		/// Updates Consumer Trust payment record(s).
		/// </summary>
		/// <param name="consumerTrustPayments">An array of Consumer Trust payment records.</param>
		[OperationContract]
		[FaultContract(typeof(OptimisticConcurrencyFault))]
		[FaultContract(typeof(RecordNotFoundFault))]
		[FaultContract(typeof(SecurityFault))]
		[ServiceKnownType(typeof(Records.ConsumerTrustPayment))]
		MethodResponse<ErrorCode> UpdateConsumerTrustPayment(Records.ConsumerTrustPayment[] consumerTrustPayments);

		/// <summary>
		/// Creates destinatinOrders
		/// </summary>
		/// <param name="destinationOrders"></param>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		[ServiceKnownType(typeof(OrderType))]
		[ServiceKnownType(typeof(Side))]
		[ServiceKnownType(typeof(TimeInForce))]
		void CreateDestinationOrders(FluidTrade.Guardian.DestinationOrderInfo[] destinationOrders);

		/// <summary>
		/// Deletes destination oders
		/// </summary>
		/// <param name="destinationOrderReferences"></param>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		void DestroyDestinationOrders(FluidTrade.Guardian.DestinationOrderReference[] destinationOrderReferences);

		/// <summary>
		/// Declines a match.
		/// </summary>
		/// <param name="matchId">Match id to decline</param>
		/// <param name="rowVersion">Unique identifier of the match row.</param>
		[OperationContract]
		void DeclineMatch(Guid matchId, Int64 rowVersion);

		/// <summary>
		/// Update Negotiation record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Records.Negotiation))]
		MethodResponse<Guid[]> CreateNegotiation(Records.Negotiation[] objects);

		/// <summary>
		/// Update Negotiation record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Records.Negotiation))]
		MethodResponse<ErrorCode> UpdateNegotiation(Records.Negotiation[] objects);

		/// <summary>
		/// Decline a negotiation.
		/// </summary>
		/// <param name="negotiationId">NegUnique identifier of the negotiation</param>
		[OperationContract]
		void DeclineNegotiation(Guid negotiationId);

		/// <summary>
		/// Offer Negotiaion to the counter Party.
		/// </summary>
		/// <param name="matchId"></param>
		/// <param name="quantity"></param>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		void OfferNegotiation(Guid matchId, Decimal quantity);


		/// <summary>
		/// Update Province record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Province))]
		MethodResponse<Guid[]> CreateProvince(Province[] objects);


		/// <summary>
		/// Update Province record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Province))]
		MethodResponse<ErrorCode> UpdateProvince(Province[] objects);


		/// <summary>
		/// Update Report record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Report))]
		MethodResponse<Guid[]> CreateReport(Report[] objects);

		/// <summary>
		/// Update Report record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Report))]
		MethodResponse<ErrorCode> UpdateReport(Report[] objects);

		/// <summary>
		/// Update Security record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Security))]
		MethodResponse<Guid[]> CreateSecurity(Security[] objects);

		/// <summary>
		/// Update Security record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Security))]
		MethodResponse<ErrorCode> DeleteSecurity(Security[] objects);

		/// <summary>
		/// Get Security record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Security))]
		MethodResponse<Security[]> GetSecurity(Guid[] ids);

		/// <summary>
		/// Update Security record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Security))]
		MethodResponse<ErrorCode> UpdateSecurity(Security[] objects);

		/// <summary>
		/// Rrturns the wsdl.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		System.Collections.Generic.Dictionary<System.String, System.String> GetServerMetadata();

		/// <summary>
		/// Get the simulation parameters
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		FluidTrade.Guardian.SimulationParameters GetSimulationParameters();


		/// <summary>
		/// Set the simulation parameters.
		/// </summary>
		/// <param name="simulationParameters"></param>
		[OperationContract]
		void SetSimulationParameters(FluidTrade.Guardian.SimulationParameters simulationParameters);

		/// <summary>
		/// Get user associated with the user Id.
		/// </summary>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		[TransactionFlowAttribute(TransactionFlowOption.Allowed)]
		[FaultContract(typeof(RecordNotFoundFault))]
		Guid GetUserId();

		/// <summary>
		/// Update Consumer record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(WorkingOrderRecord))]
		MethodResponse<Guid[]> CreateWorkingOrder(WorkingOrderRecord[] objects);

		/// <summary>
		/// Update Consumer record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(WorkingOrderRecord))]
		MethodResponse<ErrorCode> UpdateWorkingOrder(WorkingOrderRecord[] objects);

		/// <summary>
		/// Update Consumer record(s).
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(WorkingOrderRecord))]
		MethodResponse<ErrorCode> DeleteWorkingOrder(WorkingOrderRecord[] objects);

		/// <summary>
		/// Get Consumer record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(WorkingOrderRecord))]
		MethodResponse<WorkingOrderRecord[]> GetWorkingOrder(Guid[] ids);

		#region Commission Schedules
		/// <summary>
		/// Create commission schedules.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns>The CommissionIds of the created objects.</returns>
		[OperationContract]
		[ServiceKnownType(typeof(CommissionSchedule))]
		MethodResponse<Guid[]> CreateCommissionSchedule(CommissionSchedule[] objects);
		#endregion Commission Schedules

		/// <summary>
		/// 
		/// </summary>
		[OperationContract]
		[System.ServiceModel.ServiceKnownType(typeof(DebtRule))]
		MethodResponse<Guid[]> CreateDebtRule(DebtRule[] objects);
	
		/// <summary>
		/// 
		/// </summary>
		[OperationContract]
		[System.ServiceModel.ServiceKnownType(typeof(DebtRule))]
		MethodResponse<ErrorCode> DeleteDebtRule(DebtRule[] objects);
		/// <summary>
		/// 
		/// </summary>
		[OperationContract]
		[System.ServiceModel.ServiceKnownType(typeof(DebtRule))]
		MethodResponse<ErrorCode> UpdateDebtRule(DebtRule[] debtRules);

		/// <summary>
		/// Get blotter record(s).
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		[OperationContract]
		[FaultContract(typeof(RecordNotFoundFault))]
		[ServiceKnownType(typeof(Blotter))]
		MethodResponse<Blotter[]> GetBlotter(Guid[] ids);

		/// <summary>
		/// Update Consumer record(s). If the consumer does not exist, it will create a new one.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		[OperationContract]
		[ServiceKnownType(typeof(Entity))]
		MethodResponse<ErrorCode> UpdateEntity(Entity[] objects);

		/// <summary>
		/// Create a new EntityTree row.
		/// </summary>
		/// <param name="objects">The records describing the rows.</param>
		/// <returns>The EntityTreeId of each new row.</returns>
		[OperationContract]
		[ServiceKnownType(typeof(EntityTree))]
		MethodResponse<Guid[]> CreateEntityTree(EntityTree[] objects);
		/// <summary>
		/// Update an EntityTree row.
		/// </summary>
		/// <param name="objects">The records describing the rows.</param>
		/// <returns>The error or success code.</returns>
		[OperationContract]
		[ServiceKnownType(typeof(EntityTree))]
		MethodResponse<ErrorCode> UpdateEntityTree(EntityTree[] objects);
		/// <summary>
		/// Destroy an EntityTree row.
		/// </summary>
		/// <param name="objects">The records describing the rows.</param>
		/// <returns>The error or success code.</returns>
		[OperationContract]
		[ServiceKnownType(typeof(EntityTree))]
		MethodResponse<ErrorCode> DeleteEntityTree(EntityTree[] objects);

		/// <summary>
		/// Grant a particular rights holder specific rights to an entity.
		/// </summary>
		/// <param name="rightsHolderId">The rights holder's id.</param>
		/// <param name="entityId">The entity's id.</param>
		/// <param name="rightId">The specific right's id.</param>
		/// <returns>An error code.</returns>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		MethodResponse<Guid> GrantAccess(Guid rightsHolderId, Guid entityId, Guid rightId);
		/// <summary>
		/// Revoke any and all access a rights holder has to an entity.
		/// </summary>
		/// <param name="rightsHolderId">The rights holder's id.</param>
		/// <param name="entityId">The entity's id.</param>
		/// <returns>The error code.</returns>
		[OperationContract]
		[ServiceKnownType(typeof(Guid))]
		MethodResponse<ErrorCode> RevokeAccess(Guid rightsHolderId, Guid entityId);

	}

}

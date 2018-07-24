namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Reflection;
	using System.Security.Permissions;
	using System.Security.Principal;
	using System.ServiceModel;
	using System.Threading;
	using System.Transactions;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;
    
	/// <summary>
	/// Fluid Trade Trading Support Web Service.
	/// </summary>
	public class TradingSupport : ITradingSupport
	{
		private static OperationManager operationManager;
		/// <summary>
		/// Create the static resources required for trading support.
		/// </summary>
		static TradingSupport()
		{
			// This creates an automatic instance of the operations manager which is used to enforce the
			// business rules on the data model.
			TradingSupport.operationManager = new OperationManager();
		}

		/// <summary>
		/// Determine whether the current user has access to an entity.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction the query takes place in.</param>
		/// <param name="entity">The entityId of the entity the user might have access to.</param>
		/// <param name="access">The level of access to check for.</param>
		/// <returns></returns>
		internal static bool HasAccess(DataModelTransaction dataModelTransaction, Guid entity, AccessRight access)
		{
			return DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, entity, access);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateConsumer(Consumer[] objects)
		{
			return objects.Create<Consumer, ConsumerPersistence>();
		}

		/// <summary>
		/// Create Debt Holder translation record(s). These are used by the import to map
		/// external data to internal fields/.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateDebtHolderImportTranslation(DebtHolderImportTranslation[] objects)
		{
			return objects.Create<DebtHolderImportTranslation, DebtHolderImportTranslationPersistence>();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateConsumer(Consumer[] objects)
		{
			return objects.Update<Consumer, ConsumerPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteConsumer(Consumer[] objects)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<Consumer[]> GetConsumer(Guid[] ids)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateConsumerDebt(ConsumerDebt[] objects)
		{
			return objects.Create<ConsumerDebt, ConsumerDebtPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateConsumerDebt(ConsumerDebt[] objects)
		{
			return objects.Update<ConsumerDebt, ConsumerDebtPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteConsumerDebt(ConsumerDebt[] objects)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<Consumer[]> GetConsumerDebt(Guid[] ids)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Move orders to new blotter.
		/// </summary>
		/// <param name="orders"></param>
		/// <param name="blotterId"></param>
		/// <returns></returns>		
		public MethodResponse<ErrorCode> MoveConsumerDebtToBlotter(Guid blotterId, BaseRecord[] orders)
		{			

			MethodResponse<ErrorCode> returnCodes = new MethodResponse<ErrorCode>() { Result = ErrorCode.Success };
			Int32 bulkIndex = 0;
			foreach (BaseRecord record in orders)
			{
				
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						ConsumerDebtPersistence consumerPersistance = new ConsumerDebtPersistence();
						consumerPersistance.MoveToBlotter(blotterId, record);
						transactionScope.Complete();
					}
				}
				catch (Exception exception)
				{
					EventLog.Error(exception);
					returnCodes.AddError(exception, bulkIndex);
				}
				finally
				{
					bulkIndex++;
				}

				

			}
			if (returnCodes.HasErrors())
				returnCodes.Result = ErrorCode.NoJoy;
			
			return returnCodes;
		}
		
		/// <summary>
		/// Updates an array of Consumer Debt negotiation records.
		/// </summary>
		/// <param name="consumerDebtNegotiations">An array of ConsumerDebtNegotiationInfo records.</param>\		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void UpdateConsumerDebtNegotiation(ConsumerDebtNegotiationInfo[] consumerDebtNegotiations)
		{
			ConsumerDebtNegotiationHelper.Update(consumerDebtNegotiations);
		}

		/// <summary>
		/// Updates the 'IsRead' status of a collection of Consumer Debt negotiation records.
		/// </summary>
		/// <param name="consumerDebtNegotiationIsReads">A collection of records describing the update.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void UpdateConsumerDebtNegotiationIsRead(ConsumerDebtNegotiationIsReadInfo[] consumerDebtNegotiationIsReads)
		{
			ConsumerDebtNegotiationHelper.Update(consumerDebtNegotiationIsReads);
		}

		/// <summary>
		/// Creates a settlement for a Consumer Trust Negotiation.
		/// </summary>
		/// <param name="consumerTrustSettlementId">The identifier of the negotiation.</param>
		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void CreateConsumerTrustSettlement(Guid consumerTrustSettlementId)
		{
			try
			{
				// The internal helpers take care of the bulk of the work.
				ConsumerTrustHelper.CreateConsumerTrustSettlement(consumerTrustSettlementId);
			}
			catch (FaultException<ArgumentFault>)
			{
				throw;
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteConsumerDebtNegotiation(ConsumerDebtNegotiationInfo[] objects)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<ConsumerDebtNegotiationInfo[]> GetConsumerDebtNegotiation(Guid[] ids)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Accepts settlements from the Consumer Debt user.
		/// </summary>
		/// <param name="consumerDebtSettlementAcceptInfos">A list of settlements to be accepted.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void AcceptConsumerDebtSettlement(ConsumerDebtSettlementAcceptInfo[] consumerDebtSettlementAcceptInfos)
		{
			// The internal helpers take care of the bulk of the work.
			ConsumerDebtHelper.AcceptConsumerDebtSettlement(consumerDebtSettlementAcceptInfos);
		}

		/// <summary>
		/// Creates elements used for a generic conversation between two parties.
		/// </summary>
		/// <param name="chatInfos">An array of conversation items.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void CreateChat(ChatInfo[] chatInfos)
		{

			// Call out to the Chat Manager to create this item.
			ChatManager.CreateChat(chatInfos);

		}

		/// <summary>
		/// Creates a settlement for a Consumer Debt Negotiation.
		/// </summary>
		/// <param name="consumerDebtSettlementId">The identifier of the negotiation.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void CreateConsumerDebtSettlement(Guid consumerDebtSettlementId)
		{
			try
			{
				// The internal helpers take care of the bulk of the work.
				ConsumerDebtHelper.CreateConsumerDebtSettlement(consumerDebtSettlementId);
			}
			catch (FaultException<ArgumentFault>)
			{
				throw;
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateConsumerTrust(ConsumerTrust[] objects)
		{
			return objects.Create<ConsumerTrust, ConsumerTrustPersistence>();
		}

		/// <summary>
		/// Rejects a collection of Consumer Debt negotiation records.
		/// </summary>
		/// <param name="consumerDebtNegotiations">A collection of records describing the update.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void RejectConsumerDebtNegotiation(ConsumerDebtNegotiationInfo[] consumerDebtNegotiations)
		{
			ConsumerDebtNegotiationHelper.Reject(consumerDebtNegotiations);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateConsumerTrust(ConsumerTrust[] objects)
		{
			return objects.Update<ConsumerTrust, ConsumerTrustPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteConsumerTrust(ConsumerTrust[] objects)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<Consumer[]> GetConsumerTrust(Guid[] ids)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Move orders to new blotter.
		/// </summary>
		/// <param name="orders"></param>
		/// <param name="blotterId"></param>
		/// <returns></returns>		
		public MethodResponse<ErrorCode> MoveConsumerTrustToBlotter(Guid blotterId, BaseRecord[] orders)
		{						
			Int32 bulkIndex = 0;
			MethodResponse<ErrorCode> returnCodes = new MethodResponse<ErrorCode>() { Result = ErrorCode.Success };
			foreach (BaseRecord record in orders)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						ConsumerTrustPersistence consumerPersistance = new ConsumerTrustPersistence();
						consumerPersistance.MoveToBlotter(blotterId, record);
						transactionScope.Complete();
					}
				}
				catch (Exception exception)
				{
					EventLog.Error(exception);
					returnCodes.AddError(exception, bulkIndex);
				}
				finally
				{
					bulkIndex++;
				}
			}

			if (returnCodes.HasErrors())
				returnCodes.Result = ErrorCode.NoJoy;


			return returnCodes;
		}

		/// <summary>
		/// Updates a collection of Consumer Trust negotiation records.
		/// </summary>
		/// <param name="consumerTrustNegotiations">A collection of records describing the update.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void RejectConsumerTrustNegotiation(ConsumerTrustNegotiationInfo[] consumerTrustNegotiations)
		{
			ConsumerTrustNegotiationHelper.Reject(consumerTrustNegotiations);
		}


		/// <summary>
		/// Reset the Consumer Debt Settlement status bit.
		/// </summary>
		/// <param name="consumerDebtSettlements"></param>		
		public MethodResponse<ErrorCode> ResetConsumerDebtSettlement(BaseRecord[] consumerDebtSettlements)
		{
			Int32 bulkIndex = 0;
			MethodResponse<ErrorCode> returnCodes = new MethodResponse<ErrorCode>() { Result = ErrorCode.Success };
			foreach (BaseRecord record in consumerDebtSettlements)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{						
						ConsumerDebtHelper.ResetSettlement(record);
						transactionScope.Complete();
					}
				}				
				catch (Exception exception)
				{
					EventLog.Error(exception);
					returnCodes.AddError(exception, bulkIndex);
				}
				finally
				{
					bulkIndex++;
				}
			}

			if (returnCodes.HasErrors())
				returnCodes.Result = ErrorCode.NoJoy;

			return returnCodes;
		}

		/// <summary>
		/// Updates a collection of Consumer Trust negotiation records.
		/// </summary>
		/// <param name="consumerTrustNegotiations">A collection of records describing the update.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void UpdateConsumerTrustNegotiation(ConsumerTrustNegotiationInfo[] consumerTrustNegotiations)
		{
			ConsumerTrustNegotiationHelper.Update(consumerTrustNegotiations);
		}

		/// <summary>
		/// Updates the 'IsRead' status of a collection of Consumer Trust negotiation records.
		/// </summary>
		/// <param name="consumerTrustNegotiationIsReads">A collection of records describing the update.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void UpdateConsumerTrustNegotiationIsRead(ConsumerTrustNegotiationIsReadInfo[] consumerTrustNegotiationIsReads)
		{
			ConsumerTrustNegotiationHelper.Update(consumerTrustNegotiationIsReads);
		}

		/// <summary>
		/// Create credit cards
		/// </summary>
		/// <param name="creditCards"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateCreditCard(CreditCard[] creditCards)
		{
			return creditCards.Create<CreditCard, CreditCardPersistence>();
		}
		/// <summary>
		/// Update credit card methods
		/// </summary>
		/// <param name="creditCards"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateCreditCard(CreditCard[] creditCards)
		{
			return creditCards.Update<CreditCard, CreditCardPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="creditCards"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteCreditCard(CreditCard[] creditCards)
		{
			return creditCards.Delete<CreditCard, CreditCardPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<CreditCard[]> GetCreditCard(Guid[] ids)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Updates Consumer Trust payment record(s).
		/// </summary>
		/// <param name="consumerDebtPayments">An array of Consumer Trust payment records.</param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateConsumerDebtPayment(Records.ConsumerDebtPayment[] consumerDebtPayments)
		{
			return consumerDebtPayments.Update<ConsumerDebtPayment, ConsumerDebtPaymentPersistence>();
		}
		/// <summary>
		/// Updates Consumer Trust payment record(s).
		/// </summary>
		/// <param name="consumerTrustPayments">An array of Consumer Trust payment records.</param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateConsumerTrustPayment(Records.ConsumerTrustPayment[] consumerTrustPayments)
		{
			return consumerTrustPayments.Update<ConsumerTrustPayment, ConsumerTrustPaymentPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>		
		public MethodResponse<Guid[]> CreateDebtHolder(DebtHolder[] debtHolders)
		{
			return debtHolders.Create<DebtHolder, DebtHolderPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateDebtHolder(Records.DebtHolder[] objects)
		{
			return objects.Update<Records.DebtHolder, DebtHolderPersistence>();
		}
	
		/// <summary>
		/// 
		/// </summary>		
		public MethodResponse<ErrorCode> DeleteDebtHolder(DebtHolder[] debtHolders)
		{
			return debtHolders.Delete<DebtHolder, DebtHolderPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>		
		public MethodResponse<Guid[]> CreateDebtNegotiator(DebtNegotiator[] debtNegotiators)
		{
			return debtNegotiators.Create<DebtNegotiator, DebtNegotiatorPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateDebtNegotiator(Records.DebtNegotiator[] objects)
		{
			return objects.Update<Records.DebtNegotiator, DebtNegotiatorPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>		
		public MethodResponse<ErrorCode> DeleteDebtNegotiator(DebtNegotiator[] debtNegotiators)
		{
			return debtNegotiators.Delete<DebtNegotiator, DebtNegotiatorPersistence>();
		}
		/// <summary>
		/// Create debt rules.
		/// </summary>
		/// <param name="debtRules">The debt rule records.</param>
		/// <returns>The guids of created for the new records.</returns>
		public MethodResponse<Guid[]> CreateDebtRule(DebtRule[] debtRules)
		{
			return debtRules.Create<DebtRule, DebtRulePersistence>();
		}
		/// <summary>
		/// Delete debt rules.
		/// </summary>		
		/// <param name="debtRules">The debt rule records.</param>
		/// <returns>The error (if any) generated by the request.</returns>
		public MethodResponse<ErrorCode> DeleteDebtRule(DebtRule[] debtRules)
		{
			return debtRules.Delete<DebtRule, DebtRulePersistence>();
		}
		/// <summary>
		/// Update debt rules.
		/// </summary>
		/// <param name="debtRules">The debt rule records.</param>
		/// <returns>The error (if any) generated by the request.</returns>
		public MethodResponse<ErrorCode> UpdateDebtRule(DebtRule[] debtRules)
		{
			return debtRules.Update<DebtRule, DebtRulePersistence>();
		}

		/// <summary>
		/// Create objects in the data store.    
		/// </summary>
		/// <param name="objects">Array of objects to install.</param>
		/// <returns>Array of ids of the created item.  In case of failure the id == Guid.Empty</returns>		
		public MethodResponse<System.Guid[]> ImportDebtHolderRecords(DebtHolderRecord[] objects)
		{
			List<Guid> guids = new List<Guid>();
			MethodResponse<Guid[]> returnCodes = new MethodResponse<Guid[]>();
			Int32 bulkIndex = 0;
			foreach (DebtHolderRecord record in objects)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						Guid tenantId = PersistenceHelper.GetTenantForEntity(DataModelTransaction.Current, record.Blotter);
						DebtHolderRecordPersistence datamodelPersistance = new DebtHolderRecordPersistence(record);						
						EntityRow existingEntity =
							TradingSupport.FindEntityByKey("Import", "Entity", new object[] { record.AccountCode, tenantId }, false);
						if (existingEntity == null)
							guids.Add(datamodelPersistance.Create());
						else
							guids.Add(datamodelPersistance.Update(existingEntity));
						transactionScope.Complete();
					}
				}
				catch (Exception exception)
				{
					EventLog.Information("{0}:{1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
					returnCodes.AddError(exception, bulkIndex);
					guids.Add(Guid.Empty);
				}
				bulkIndex += 1;
			}
			returnCodes.Result = guids.ToArray();
			return returnCodes;
		}

		/// <summary>
		/// Create objects in the data store.    
		/// </summary>
		/// <param name="objects">Array of objects to install.</param>
		/// <returns>Array of ids of the created item.  In case of failure the id == Guid.Empty</returns>		
		public MethodResponse<System.Guid[]> ImportDebtNegotiatorRecords(DebtNegotiatorRecord[] objects)
		{
			List<Guid> guids = new List<Guid>();
			MethodResponse<Guid[]> returnCodes = new MethodResponse<Guid[]>();
			Int32 bulkIndex = 0;
			foreach (DebtNegotiatorRecord record in objects)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						Guid tenantId = PersistenceHelper.GetTenantForEntity(DataModelTransaction.Current, record.Blotter);
						DebtNegotiatorRecordPersistence datamodelPersistance = new DebtNegotiatorRecordPersistence(record);
						EntityRow existingEntity = null;
						CreditCardRow existingCard = FindCreditCardByKey("Import", "CreditCard", new object[] { record.AccountCode, tenantId }, false);
						if (existingCard != null)
							existingCard.ReleaseReaderLock(DataModelTransaction.Current.TransactionId);
						existingEntity = FindEntityByKey("Import", "Entity", new object[] { record.CustomerCode, tenantId }, false);
						guids.Add(datamodelPersistance.Create(existingEntity, existingCard));
						transactionScope.Complete();
					}
				}
				catch (Exception exception)
				{
					EventLog.Information("{0}:{1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
					returnCodes.AddError(exception, bulkIndex);
					guids.Add(Guid.Empty);
				}
				bulkIndex += 1;
			}
			returnCodes.Result = guids.ToArray();
			return returnCodes;
		}
	
		/// <summary>
		/// Clear working orders.
		/// </summary>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void ClearDestinationOrders()
		{
			DestinationOrderHelper.ClearDestinationOrders();
		}
		/// <summary>
		/// Create working orders
		/// </summary>
		/// <seealso cref="DestinationOrderInfo"/>
		/// <param name="destinationOrders">DestinationOrderInfo</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void CreateDestinationOrders(DestinationOrderInfo[] destinationOrders)
		{
			DestinationOrderHelper.CreateDestinationOrders(destinationOrders);
		}
		/// <summary>
		/// Destroy destination orders.
		/// </summary>
		/// <param name="destinationOrderReferences">Array of DestinationOrderReference</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void DestroyDestinationOrders(DestinationOrderReference[] destinationOrderReferences)
		{
			DestinationOrderHelper.DestroyDestinationOrders(destinationOrderReferences);
		}

		/// <summary>
		/// Accept Match implementation
		/// </summary>
		/// <param name="matchId">Id of the macth.</param>
		/// <param name="rowVersion">Unique identifier of the row.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void AcceptMatch(Guid matchId, Int64 rowVersion)
		{
			Match.Accept(matchId, rowVersion);
		}
		/// <summary>
		/// Decline match.
		/// </summary>
		/// <param name="matchId">Id of the match.</param>
		/// <param name="rowVersion">Unique id of the row.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void DeclineMatch(Guid matchId, Int64 rowVersion)
		{
			Match.Decline(matchId, rowVersion);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateNegotiation(Records.Negotiation[] objects)
		{
			return objects.Create<Records.Negotiation, NegotiationPersistence>();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateNegotiation(Records.Negotiation[] objects)
		{
			return objects.Update<Records.Negotiation, NegotiationPersistence>();
		}
		/// <summary>
		/// Decline Negotiation
		/// </summary>
		/// <seealso cref="System.Guid"/>
		/// <param name="matchId">Id of the match.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void DeclineNegotiation(Guid matchId)
		{
			Negotiation.Decline(matchId);
		}
		/// <summary>
		/// Offer Negotiation.
		/// </summary>
		/// <param name="matchId">Id of the Match.</param>
		/// <param name="quantity">Quantity to offer.</param>		
		[OperationBehavior(TransactionScopeRequired = true)]
		public void OfferNegotiation(Guid matchId, Decimal quantity)
		{
			Negotiation.Offer(matchId, quantity);
		}
	
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateProvince(Province[] objects)
		{
			return objects.Create<Province, ProvincePersistence>();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateProvince(Province[] objects)
		{
			return objects.Update<Province, ProvincePersistence>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateReport(Report[] objects)
		{
			return objects.Create<Report, ReportPersistence>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateReport(Report[] objects)
		{
			return objects.Update<Report, ReportPersistence>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateSecurity(Security[] objects)
		{
			return objects.Create<Security, SecurityPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteSecurity(Security[] objects)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// User Id associated with the Web Service
		/// </summary>
		public static Guid DaemonUserId
		{
			get
			{
				//HACK - we need a DaemonUser that everyone has access to.
				// This operation requires a middle tier context to access the data model.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;								
				string userName = WindowsIdentity.GetCurrent().Name;
				userName = "NT AUTHORITY\\NETWORK SERVICE";

				UserRow userRow = DataModel.User.UserKeyIdentityName.Find(userName.ToLower());
				
				if (userRow == null)
				{					
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("User", new object[] { "DaemonUser" }));
				}
				// If a record is found in the User table that matches the thread's identity, then lock it for the duration of the transaction and insure that
				// it wasn't deleted between the time it was found and the time it was locked.
				userRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				dataModelTransaction.AddLock(userRow);
				if ((userRow.RowState == DataRowState.Detached))
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("User", new object[] { "DaemonUser" }));
				
				// This is the user's internal identity to the data model.
				return userRow.UserId;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<Security[]> GetSecurity(Guid[] ids)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateSecurity(Security[] objects)
		{
			return objects.Update<Security, SecurityPersistence>();
		}

		/// <summary>
		/// Gets a list of server attributes.
		/// </summary>
		/// <returns>A table of server attributes.</returns>		
		public Dictionary<string, string> GetServerMetadata()
		{
			// The metadata is expressed as a metadata of key-value pairs.
			Dictionary<String, String> metadata = new Dictionary<string, string>();
			// The assembly name and version are useful for versioning.
			AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
			metadata.Add("AssemblyName", assemblyName.Name);
			metadata.Add("AssemblyVersion", assemblyName.Version.ToString());
			// Fill the metadata with publically available information about the server.
			foreach (object attributeObject in Assembly.GetExecutingAssembly().GetCustomAttributes(true))
			{
				// The company name
				if (attributeObject is AssemblyCompanyAttribute)
				{
					AssemblyCompanyAttribute assemblyCompanyAttribute = attributeObject as AssemblyCompanyAttribute;
					metadata.Add("Company", assemblyCompanyAttribute.Company);
				}
				// The copyright Notice
				if (attributeObject is AssemblyCopyrightAttribute)
				{
					AssemblyCopyrightAttribute assemblyCopyrightAttribute = attributeObject as AssemblyCopyrightAttribute;
					metadata.Add("Copyright", assemblyCopyrightAttribute.Copyright);
				}
				// The description of the module.
				if (attributeObject is AssemblyDescriptionAttribute)
				{
					AssemblyDescriptionAttribute assemblyDescriptionAttribute = attributeObject as AssemblyDescriptionAttribute;
					metadata.Add("Description", assemblyDescriptionAttribute.Description);
				}
				// The name of the product.
				if (attributeObject is AssemblyProductAttribute)
				{
					AssemblyProductAttribute assemblyProductAttribute = attributeObject as AssemblyProductAttribute;
					metadata.Add("Product", assemblyProductAttribute.Product);
				}
			}
			// The client can use this information to insure that the right server has been found.
			return metadata;
		}
		/// <summary>
		/// Get simulation parameters.
		/// </summary>
		/// <seealso cref="SimulationParameters"/>
		/// <returns>SimulationParameters</returns>		
		[OperationBehavior(TransactionScopeRequired = false)]
		public SimulationParameters GetSimulationParameters()
		{
            return null;
		}

		/// <summary>
		/// Gets a list of server attributes.
		/// </summary>
		/// <returns>A table of server attributes.</returns>		
		[OperationBehaviorAttribute(TransactionScopeRequired = true)]
		public Guid GetUserId()
		{
			return TradingSupport.UserId;
		}

		/// <summary>
		/// Determine the tenantId of the Tenant that owns a User.
		/// </summary>
		/// <param name="dataModelTransaction">The curren transaction.</param>
		/// <param name="userId">The EntityId of the user.</param>
		/// <returns>The tenantId of the tenant the user is in.</returns>
		public static Guid GetTenantIdForUser(DataModelTransaction dataModelTransaction, Guid userId)
		{

			RightsHolderRow userRow = DataModel.RightsHolder.RightsHolderKey.Find(userId);

			if (userRow == null)
			{
				EventLog.Error(string.Format("Invalid User Request in TradingSupport.GetTenantIdForUser: {0}", userId));
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("RightsHolder", new object[] { userId }));
			}

			userRow.AcquireReaderLock(dataModelTransaction);
			return userRow.TenantId;

		}

		/// <summary>
		/// User Id associated with the Web Service
		/// </summary>
		public static Guid UserId
		{
			get
			{
				// This operation requires a middle tier context to access the data model.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
				// The user's identity has been set by the communication layers that handle the channel.
				IClaimsPrincipal iClaimsPrincipal = Thread.CurrentPrincipal as IClaimsPrincipal;
				// Once the identity of the user is known, it can be cross referenced to the data model to find out if there is a user that has been associated
				// with this identity.
				UserRow userRow = DataModel.User.UserKeyIdentityName.Find(iClaimsPrincipal.Identity.Name.ToLower());
				if(userRow == null)
				{
					EventLog.Error(string.Format("Invalid User Request in TradingSupport.get_UserId: {0}", iClaimsPrincipal.Identity.Name));
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("User", new object[] { iClaimsPrincipal.Identity.Name }));
				}
				// If a record is found in the User table that matches the thread's identity, then lock it for the duration of the transaction and insure that
				// it wasn't deleted between the time it was found and the time it was locked.
				userRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				dataModelTransaction.AddLock(userRow);
				if ((userRow.RowState == DataRowState.Detached))
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("User", new object[] { iClaimsPrincipal.Identity.Name }));
				// This is the user's internal identity to the data model.
				return userRow.UserId;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentUserId"></param>
		/// <param name="currentOrganizationId"></param>
		public static void GetCurrentUserContext(out Guid currentUserId, out Guid currentOrganizationId)
		{
			currentUserId = UserId;
			currentOrganizationId = Guid.Empty;

			if (currentUserId == Guid.Empty)
				return;
			
			// This operation requires a middle tier context to access the data model.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			RightsHolderRow rightsHolderRow = DataModel.RightsHolder.RightsHolderKey.Find(currentUserId);
			if (rightsHolderRow == null)
			{
				if (rightsHolderRow == null)
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("User", new object[] { currentUserId }));
			}

			rightsHolderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			currentOrganizationId = rightsHolderRow.TenantId;
			rightsHolderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

		}


		/// <summary>
		/// Sets the operationg heuristics of the simulation.
		/// </summary>
		/// <param name="simulationParameters">The heuristics of the simulation.</param>
		[ClaimsPrincipalPermission(SecurityAction.Demand, ClaimType = ClaimTypes.Update, Resource = Resources.Application)]
		[OperationBehavior(TransactionScopeRequired = false)]
		public void SetSimulationParameters(SimulationParameters simulationParameters)
		{
		}
	
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<Guid[]> CreateWorkingOrder(WorkingOrderRecord[] objects)
		{
			return objects.Create<WorkingOrderRecord, WorkingOrderPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateWorkingOrder(WorkingOrderRecord[] objects)
		{
			return objects.Update<WorkingOrderRecord, WorkingOrderPersistence>();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> DeleteWorkingOrder(WorkingOrderRecord[] objects)
		{			

			return objects.Delete<WorkingOrderRecord, WorkingOrderPersistence>();
			

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<WorkingOrderRecord[]> GetWorkingOrder(Guid[] ids)
		{
			throw new NotImplementedException();
		}
	
		
		#region Commission Schedules
		/// <summary>
		/// Create commission schedules.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns>The CommissionIds of the created objects.</returns>
		public MethodResponse<Guid[]> CreateCommissionSchedule(CommissionSchedule[] objects)
		{
			return objects.Create<CommissionSchedule, CommissionSchedulePersistence>();
		}
		#endregion Commission Schedules

		#region Support Methods
		/// <summary>
		/// Find a Country row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <returns>The row corresponding to the key.</returns>
		internal static CountryRow FindCountryByKey(string configurationId, string relation, object []key)
		{
			DataModelTransaction transaction = DataModelTransaction.Current;
			object[] configKey = new object[] { configurationId, relation };
			ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configKey);
			if ((configurationRow == null))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Configuration", configKey),
					String.Format("The country identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			configurationRow.AcquireReaderLock(transaction);
			if ((configurationRow.RowState == DataRowState.Detached))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Configuration", configKey),
					String.Format("The country identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			DataIndex index = DataModel.Country.Indices[configurationRow.IndexName];
			if ((index == null))
				throw new FaultException<IndexNotFoundFault>(
					new IndexNotFoundFault(relation, configurationRow.IndexName),
					String.Format("The country identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			CountryRow row = index.Find(key) as CountryRow;
			if ((row == null))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault(relation, key),
					String.Format("The country identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			configurationRow.ReleaseReaderLock(transaction.TransactionId);
			row.AcquireReaderLock(transaction);
			if ((row.RowState == DataRowState.Detached))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault(relation, key),
					String.Format("The country identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			return row;
		}
		/// <summary>
		/// Find a Province row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <returns>The row corresponding to the key.</returns>
		internal static ProvinceRow FindProvinceByKey(string configurationId, string relation, object[] key)
		{
			DataModelTransaction transaction = DataModelTransaction.Current;
			object[] configKey = new object[] { configurationId, relation };
			ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configKey);
			if ((configurationRow == null))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Configuration", configKey),
					String.Format("The province identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			configurationRow.AcquireReaderLock(transaction);
			ProvinceRow row = null;
			try
			{
				if ((configurationRow.RowState == DataRowState.Detached))
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("Configuration", configKey),
						String.Format("The province identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				DataIndex index = DataModel.Province.Indices[configurationRow.IndexName];
				if ((index == null))
					throw new FaultException<IndexNotFoundFault>(
						new IndexNotFoundFault(relation, configurationRow.IndexName),
						String.Format("The province identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				row = index.Find(key) as ProvinceRow;
				if ((row == null))
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault(relation, key),
						String.Format("The province identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			}
			finally
			{
				configurationRow.ReleaseReaderLock(transaction.TransactionId);
			}
			row.AcquireReaderLock(transaction);
			if ((row.RowState == DataRowState.Detached))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault(relation, key),
					String.Format("The province identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			return row;
		}
		/// <summary>
		/// Find a Entity row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <returns>The row corresponding to the key.</returns>
		internal static EntityRow FindEntityByKey(string configurationId, string relation, object[] key)
		{
			return TradingSupport.FindEntityByKey(configurationId, relation, key, true);
		}
		/// <summary>
		/// Find a Entity row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <param name="throwOnError">Whether to throw an exception if the record can't be found.</param>
		/// <returns>The row corresponding to the key, if it is found. If throwOnError is false, returns null on error.</returns>
		internal static EntityRow FindEntityByKey(string configurationId, string relation, object[] key, bool throwOnError)
		{
			DataModelTransaction transaction = DataModelTransaction.Current;
			object[] configKey = new object[] { configurationId, relation };
			ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configKey);
			if ((configurationRow == null))
				if (throwOnError)
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("Configuration", configKey),
						String.Format("The entity identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				else
					return null;
			configurationRow.AcquireReaderLock(transaction);
			EntityRow row = null;
			try
			{
				if ((configurationRow.RowState == DataRowState.Detached))
					if (throwOnError)
						throw new FaultException<RecordNotFoundFault>(
							new RecordNotFoundFault("Configuration", configKey),
							String.Format("The entity identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
					else
						return null;
				DataIndex index = DataModel.Entity.Indices[configurationRow.IndexName];
				if ((index == null))
					if (throwOnError)
						throw new FaultException<IndexNotFoundFault>(
							new IndexNotFoundFault(relation, configurationRow.IndexName),
							String.Format("The entity identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
					else
						return null;
				row = index.Find(key) as EntityRow;
				if ((row == null))
					if (throwOnError)
						throw new FaultException<RecordNotFoundFault>(
							new RecordNotFoundFault(relation, new object[] { key }),
							String.Format("The entity identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
					else
						return null;
			}
			finally
			{
				configurationRow.ReleaseReaderLock(transaction.TransactionId);
			}
			row.AcquireReaderLock(transaction);
			if ((row.RowState == DataRowState.Detached))
			{				
				if (throwOnError)
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault(relation, new object[] { key }),
						String.Format("The entity identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				else
					return null;
			}
			return row;
		}
		/// <summary>
		/// Find a Consumer row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <returns>The row corresponding to the key.</returns>
		internal static ConsumerRow FindConsumerByKey(string configurationId, string relation, object[] key)
		{
			DataModelTransaction transaction = DataModelTransaction.Current;
			object[] configKey = new object[] { configurationId, relation };
			ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configKey);
			if ((configurationRow == null))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Configuration", configKey),
					String.Format("The consumer identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			configurationRow.AcquireReaderLock(transaction);
			ConsumerRow row = null;
			try
			{
				if ((configurationRow.RowState == DataRowState.Detached))
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("Configuration", configKey),
						String.Format("The consumer identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				DataIndex index = DataModel.Consumer.Indices[configurationRow.IndexName];
				if ((index == null))
					throw new FaultException<IndexNotFoundFault>(
						new IndexNotFoundFault(relation, configurationRow.IndexName),
						String.Format("The consumer identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				row = index.Find(key) as ConsumerRow;
				if ((row == null))
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault(relation, key),
						String.Format("The consumer identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			}
			finally
			{
				configurationRow.ReleaseReaderLock(transaction.TransactionId);
			}
			row.AcquireReaderLock(transaction);

			if ((row.RowState == DataRowState.Detached))
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault(relation, key),
					String.Format("The consumer identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
			return row;
		}
		/// <summary>
		/// Find a CreditCard row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <returns>The row corresponding to the key.</returns>
		internal static CreditCardRow FindCreditCardByKey(string configurationId, string relation, object[] key)
		{
			return TradingSupport.FindCreditCardByKey(configurationId, relation, key, true);
		}
		/// <summary>
		/// Find a CreditCard row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <param name="throwOnError">Whether to throw an exception if the record can't be found.</param>
		/// <returns>The row corresponding to the key, if it is found. If throwOnError is false, returns null on error.</returns>
		internal static CreditCardRow FindCreditCardByKey(string configurationId, string relation, object[] key, bool throwOnError)
		{
			DataModelTransaction transaction = DataModelTransaction.Current;
			object[] configKey = new object[] { configurationId, relation };
			ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configKey);
			if ((configurationRow == null))
				if (throwOnError)
					throw new FaultException<RecordNotFoundFault>(
						new RecordNotFoundFault("Configuration", configKey),
						String.Format("The credit card identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				else
					return null;
			configurationRow.AcquireReaderLock(transaction);
			CreditCardRow row = null;
			try
			{
				if ((configurationRow.RowState == DataRowState.Detached))
					if (throwOnError)
						throw new FaultException<RecordNotFoundFault>(
							new RecordNotFoundFault("Configuration", configKey),
							String.Format("The credit card identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
					else
						return null;
				DataIndex index = DataModel.CreditCard.Indices[configurationRow.IndexName];
				if ((index == null))
					if (throwOnError)
						throw new FaultException<IndexNotFoundFault>(
							new IndexNotFoundFault(relation, configurationRow.IndexName),
							String.Format("The credit card identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
					else
						return null;
				row = index.Find(key) as CreditCardRow;
				if ((row == null))
					if (throwOnError)
						throw new FaultException<RecordNotFoundFault>(
							new RecordNotFoundFault(relation, key),
							String.Format("The credit card identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
					else
						return null;
			}
			finally
			{
				configurationRow.ReleaseReaderLock(transaction.TransactionId);
			}
			row.AcquireReaderLock(transaction);
			if ((row.RowState == DataRowState.Detached))
				if (throwOnError)
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault(relation, key),
						String.Format("The credit card identified by \"{0}\" could not be found.", CommonConversion.FromArray(key)));
				else
					return null;
			return row;
		}
		/// <summary>
		/// Find a Type row by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId">The configuration to use for the lookup.</param>
		/// <param name="relation">The relation name to use for the lookup.</param>
		/// <param name="key">The key to lookup.</param>
		/// <returns>The row corresponding to the key.</returns>
		internal static TypeRow FindTypeByKey(string configurationId, string relation, object[] key)
		{
			DataModelTransaction transaction = DataModelTransaction.Current;
			object[] configKey = new object[] { configurationId, relation };
			ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configKey);
			if ((configurationRow == null))
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Configuration", configKey));
			configurationRow.AcquireReaderLock(transaction);
			TypeRow row = null;
			try
			{
				if ((configurationRow.RowState == DataRowState.Detached))
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Configuration", configKey));
				DataIndex index = DataModel.Type.Indices[configurationRow.IndexName];
				if ((index == null))
					throw new FaultException<IndexNotFoundFault>(new IndexNotFoundFault(relation, configurationRow.IndexName));
				row = index.Find(key) as TypeRow;
				if ((row == null))
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault(relation, key));
			}
			finally
			{
				configurationRow.ReleaseReaderLock(transaction.TransactionId);
			}
			row.AcquireReaderLock(transaction);
			if ((row.RowState == DataRowState.Detached))
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault(relation, key));
			return row;
		}
		/// <summary>
		/// Find an Image by the appropriate external id, as determined by the relation and the configuration.
		/// </summary>
		/// <param name="configurationId"></param>
		/// <param name="relation"></param>
		/// <param name="key"></param>
		/// <returns>The row corresponding to the key.</returns>
		internal static ImageRow FindImageByKey(string configurationId, string relation, object[] key)
		{
			DataModelTransaction transaction = DataModelTransaction.Current;
			object[] configKey = new object[] { configurationId, relation };
			ConfigurationRow configurationRow = DataModel.Configuration.ConfigurationKey.Find(configKey);
			if ((configurationRow == null))
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Configuration", configKey));
			configurationRow.AcquireReaderLock(transaction);
			ImageRow row = null;
			try
			{
				if ((configurationRow.RowState == DataRowState.Detached))
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Configuration", configKey));
				DataIndex index = DataModel.Image.Indices[configurationRow.IndexName];
				if ((index == null))
					throw new FaultException<IndexNotFoundFault>(new IndexNotFoundFault(relation, configurationRow.IndexName));
				row = index.Find(key) as ImageRow;
				if ((row == null))
					throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault(relation, key));
			}
			finally
			{
				configurationRow.ReleaseReaderLock(transaction.TransactionId);
			}
			row.AcquireReaderLock(transaction);
			if ((row.RowState == DataRowState.Detached))
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault(relation, key));
			return row;
		}
		internal static bool IsColumnOld(DataRow row, string columnName, object value)
		{
			return row.IsNull(columnName) && value != null || value == null && !row.IsNull(columnName) || value != null && !value.Equals(row[columnName]);
		}

		#endregion

		/// <summary>
		/// Get a set of Blotter records by their ids.
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public MethodResponse<Blotter[]> GetBlotter(Guid[] ids)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public MethodResponse<ErrorCode> UpdateEntity(Entity[] objects)
		{
			return objects.Update<Entity, EntityPersistence>();
		}

		/// <summary>
		/// Create a new EntityTree row.
		/// </summary>
		/// <param name="objects">The records describing the rows.</param>
		/// <returns>The EntityTreeId of each new row.</returns>
		public MethodResponse<Guid[]> CreateEntityTree(EntityTree[] objects)
		{
			return objects.Create<EntityTree, EntityTreePersistence>();
		}
		/// <summary>
		/// Update an EntityTree row.
		/// </summary>
		/// <param name="objects">The records describing the rows.</param>
		/// <returns>The error or success code.</returns>
		public MethodResponse<ErrorCode> UpdateEntityTree(EntityTree[] objects)
		{
			return objects.Update<EntityTree, EntityTreePersistence>();
		}
		/// <summary>
		/// Destroy an EntityTree row.
		/// </summary>
		/// <param name="objects">The records describing the rows.</param>
		/// <returns>The error or success code.</returns>
		public MethodResponse<ErrorCode> DeleteEntityTree(EntityTree[] objects)
		{
			return objects.Delete<EntityTree, EntityTreePersistence>();
		}

		/// <summary>
		/// Grant a particular rights holder specific rights to an entity.
		/// </summary>
		/// <param name="rightsHolderId">The rights holder's id.</param>
		/// <param name="entityId">The entity's id.</param>
		/// <param name="rightId">The specific right's id.</param>
		/// <returns>An error code.</returns>
		public MethodResponse<Guid> GrantAccess(Guid rightsHolderId, Guid entityId, Guid rightId)
		{

			MethodResponse<Guid> returnCodes = new MethodResponse<Guid>() { Result = Guid.Empty };

			try
			{
				using (TransactionScope transactionScope = new TransactionScope())
				{
					returnCodes.Result = AccessControlHelper.GrantAccess(rightsHolderId, entityId, rightId);
					transactionScope.Complete();
				}
			}
			catch (Exception exception)
			{
				EventLog.Error(exception);
				returnCodes.AddError(exception, 0);
			}

			return returnCodes;

		}
		/// <summary>
		/// Revoke any and all access a rights holder has to an entity.
		/// </summary>
		/// <param name="rightsHolderId">The rights holder's id.</param>
		/// <param name="entityId">The entity's id.</param>
		/// <returns>The error code.</returns>
		public MethodResponse<ErrorCode> RevokeAccess(Guid rightsHolderId, Guid entityId)
		{

			MethodResponse<ErrorCode> returnCodes = new MethodResponse<ErrorCode>() { Result = ErrorCode.Success };

			try
			{
				using (TransactionScope transactionScope = new TransactionScope())
				{
					returnCodes.Result = AccessControlHelper.RevokeAccess(rightsHolderId, entityId);

					if (returnCodes.Result == ErrorCode.Success)
						transactionScope.Complete();
					else
						returnCodes.AddError("Revoke access failed", returnCodes.Result);
				}
			}
			catch (Exception exception)
			{
				EventLog.Error(exception);
				returnCodes.AddError(exception, 0);
			}

			return returnCodes;

		}

	}

}

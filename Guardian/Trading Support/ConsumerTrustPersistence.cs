namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// 
	/// </summary>
	internal class ConsumerTrustPersistence : DataModelPersistence<ConsumerTrust>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public ConsumerTrustPersistence()
		{
		}
		/// <summary>
		/// Create a new Consumer Trust record.
		/// </summary>
		/// <returns></returns>
		public override Guid Create(ConsumerTrust record)
		{

			DataModel dataModel = new DataModel();
			Guid consumerTrustId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			////Create a entry in credit card
			dataModel.CreateConsumerTrust(
				record.ConsumerId.GetValueOrDefault(),
				consumerTrustId,
				record.DebtRuleId,
				record.ExternalId0,
				record.SavingsAccount,
				record.SavingsBalance,
				record.Tag,
				record.TenantId,
				record.VendorCode);

			return consumerTrustId;

		}

		/// <summary>
		/// Update a consumer trust record.
		/// </summary>
		/// <returns></returns>
		public override void Update(ConsumerTrust record)
		{
			DataModel dataModel = new DataModel();

			if (record.RowId == null || DataModel.ConsumerTrust.ConsumerTrustKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("ConsumerTrust", new object[] { record.RowId }));
			}

			dataModel.UpdateConsumerTrust(
				record.ConsumerId, //Get the boxed value
				null,
				new object[] { record.RowId },
				record.DebtRuleId,
				record.ExternalId0,
				record.RowVersion,
				record.SavingsAccount,
				record.SavingsBalance,
				record.Tag,
				null,
				record.VendorCode);

		}

		/// <summary>
		/// Delete a debt holder
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(ConsumerTrust record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;

			if (record.RowId == null || DataModel.ConsumerTrust.ConsumerTrustKey.Find(record.RowId) == null)
				return ErrorCode.RecordNotFound;

			ConsumerTrustRow consumerTrust = DataModel.ConsumerTrust.ConsumerTrustKey.Find(record.RowId);
			consumerTrust.AcquireReaderLock(transaction);
			Guid debtId = consumerTrust.ConsumerTrustId;
			ConsumerRow consumer = consumerTrust.ConsumerRow;
			DebtRuleRow debtRule = consumerTrust.DebtRuleRow;
			consumerTrust.ReleaseReaderLock(transaction.TransactionId);

#if false   // If we switch from explicitly deleting the working order to explicitly deleting the security, then we need this.
			if (!TradingSupport.HasAccess(transaction, debtId, AccessRight.Write))
				return ErrorCode.AccessDenied;
#endif

			consumerTrust.AcquireWriterLock(transaction);
			if (consumerTrust.RowState != DataRowState.Deleted && consumerTrust.RowState != DataRowState.Detached)
				dataModel.DestroyConsumerTrust(new object[] { consumerTrust.ConsumerTrustId }, record.RowVersion);
			consumerTrust.ReleaseWriterLock(transaction.TransactionId);

			consumer.AcquireWriterLock(transaction);
			if (consumer.RowState != DataRowState.Deleted && consumer.RowState != DataRowState.Detached)
				dataModel.DestroyConsumer(new object[] { consumer.ConsumerId }, consumer.RowVersion);
			consumer.ReleaseWriterLock(transaction.TransactionId);

			if (debtRule != null)
			{

				debtRule.AcquireReaderLock(transaction);
				if (debtRule.RowState != DataRowState.Deleted && debtRule.RowState != DataRowState.Detached && debtRule.GetDebtRuleMapRows().Length == 0)
				{

					DebtRulePersistence debtRulePersistence = new DebtRulePersistence();
					Guid debtRuleId = debtRule.DebtRuleId;
					long rowVersion = debtRule.RowVersion;
					debtRule.ReleaseReaderLock(transaction.TransactionId);
					debtRulePersistence.Delete(new Records.DebtRule { RowId = debtRuleId, RowVersion = rowVersion });

				}
				else
				{

					debtRule.ReleaseReaderLock(transaction.TransactionId);

				}

			}

			return ErrorCode.Success;
		}


		public override ConsumerTrust Get(Guid id)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Move this record to a new blotter
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="record"></param>
		/// <returns></returns>
		public void MoveToBlotter(Guid blotterId, BaseRecord record)
		{

			//All the operations need to be done in a transaction.
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
			DataModel dataModel = new DataModel();
			Guid modifiedByUser = TradingSupport.DaemonUserId;

			//Let the errors propogate back
			MoveToInfo moveToInfo = CollectData(record, blotterId, dataModelTransaction);

			//This will be null if there is nothing to do.
			if (moveToInfo == null)
				return;

			dataModel.UpdateWorkingOrder(
				null,
				blotterId,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				DateTime.UtcNow,
				modifiedByUser,
				null,
				record.RowVersion,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				new object[] { record.RowId });

			if (moveToInfo.matchRows != null)
			{
				//Move Match data
				MoveMatchRowsToBlotter(blotterId, modifiedByUser, dataModel, moveToInfo.matchRows);

				//Move Negotiatiaon Data
				MoveNegotiationRowsToBlotter(blotterId, modifiedByUser, dataModel, moveToInfo.consumerTrustNegotiationRows);
				MoveNegotiationCounterPaymentMethodRowsToBlotter(blotterId, dataModel, moveToInfo.consumerTrustNegotiationCounterPaymentMethodRows);
				MoveNegotiationOfferPaymentMethodRowsToBlotter(blotterId, dataModel, moveToInfo.consumerTrustNegotiationOfferPaymentMethodRows);
				MoveChatRowsToBlotter(blotterId, dataModel, moveToInfo.chatRows);

				//Move Settlement data				
				MoveSettlementRowsToBlotter(blotterId, dataModel, moveToInfo.settlementRows);
				MoveSettlementPaymentMethodRowsToBlotter(blotterId, dataModel, moveToInfo.consumerTrustSettlementPaymentMethodRows);
				MovePaymentRowsToBlotter(blotterId, dataModel, moveToInfo.consumerTrustPaymentRows);
			}
						
		}


		/// <summary>
		/// Lock rows and collect the data.
		/// </summary>
		/// <param name="record"></param>
		/// <param name="blotterId"></param>
		/// <param name="dataModelTransaction"></param>
		/// <returns></returns>
		private MoveToInfo CollectData(BaseRecord record,Guid blotterId, DataModelTransaction dataModelTransaction)
		{
			MoveToInfo moveToInfo = new MoveToInfo();
			WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(record.RowId);
			MatchRow[] workingOrderRowMatchRows = null;

			if (workingOrderRow == null || workingOrderRow.RowState == DataRowState.Detached)
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("WorkingOrder", new object[] { record }));

			workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{

				if(workingOrderRow.RowVersion != record.RowVersion)
					throw new FaultException<OptimisticConcurrencyFault>(new OptimisticConcurrencyFault("WorkingOrder", new object[] { record.RowId }),
						new FaultReason("OptimisticConcurrencyFault:  Someone updated this before you.  Please try again!"));

				//Nothing to do
				if (blotterId == workingOrderRow.BlotterId)
					return null;

				workingOrderRowMatchRows = workingOrderRow.GetMatchRows();
			}
			finally
			{
				workingOrderRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}
		
			//Get all matches.			
			if (workingOrderRowMatchRows != null)
			{
				moveToInfo.matchRows = new List<BaseRecord>();
				foreach (MatchRow matchRow in workingOrderRowMatchRows)
				{
					matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
					try
					{
						moveToInfo.matchRows.Add(new BaseRecord() { RowId = matchRow.MatchId, RowVersion = matchRow.RowVersion });

						//Grab all the chat rows associated with this match.
						foreach (ChatRow chatRow in matchRow.GetChatRows())
						{
							chatRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
							try
							{
								moveToInfo.chatRows.Add(new BaseRecord() { RowId = chatRow.ChatId, RowVersion = chatRow.RowVersion });
							}
							finally
							{
								chatRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
							}
						}

						//Grab all the negotiation rows associated with this match.
						CollectNegotiationData(dataModelTransaction, moveToInfo, matchRow);
					}
					finally
					{
						matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
					}

				}
			}

			return moveToInfo;
		}

		/// <summary>
		/// Collect negotiation and settlement data to move
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="moveToInfo"></param>
		/// <param name="matchRow"></param>
		private static void CollectNegotiationData(DataModelTransaction dataModelTransaction, MoveToInfo moveToInfo, MatchRow matchRow)
		{
			foreach (ConsumerTrustNegotiationRow consumerTrustNegotiationRow in matchRow.GetConsumerTrustNegotiationRows())
			{
				consumerTrustNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					moveToInfo.consumerTrustNegotiationRows.Add(new BaseRecord() { RowId = consumerTrustNegotiationRow.ConsumerTrustNegotiationId, RowVersion = consumerTrustNegotiationRow.RowVersion });


					//Get counter payment rows to move
					foreach (ConsumerTrustNegotiationCounterPaymentMethodRow counterPaymentMethodRow in consumerTrustNegotiationRow.GetConsumerTrustNegotiationCounterPaymentMethodRows())
					{
						counterPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerTrustNegotiationCounterPaymentMethodRows.Add(new BaseRecord() { RowId = counterPaymentMethodRow.ConsumerTrustNegotiationCounterPaymentMethodId, RowVersion = counterPaymentMethodRow.RowVersion });
						}
						finally
						{
							counterPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}

					//Get offer payment rows to move
					foreach (ConsumerTrustNegotiationOfferPaymentMethodRow offerPaymentMethodRow in consumerTrustNegotiationRow.GetConsumerTrustNegotiationOfferPaymentMethodRows())
					{
						offerPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerTrustNegotiationOfferPaymentMethodRows.Add(new BaseRecord() { RowId = offerPaymentMethodRow.ConsumerTrustNegotiationOfferPaymentMethodId, RowVersion = offerPaymentMethodRow.RowVersion });
						}
						finally
						{
							offerPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}

					//Get Settlement rows to move
					CollectSettlementData(dataModelTransaction, moveToInfo, consumerTrustNegotiationRow);
				}
				finally
				{
					consumerTrustNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="moveToInfo"></param>
		/// <param name="consumerTrustNegotiationRow"></param>
		private static void CollectSettlementData(DataModelTransaction dataModelTransaction, MoveToInfo moveToInfo, ConsumerTrustNegotiationRow consumerTrustNegotiationRow)
		{
			foreach (ConsumerTrustSettlementRow settlementRow in consumerTrustNegotiationRow.GetConsumerTrustSettlementRows())
			{
				settlementRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					moveToInfo.settlementRows.Add(new BaseRecord() { RowId = settlementRow.ConsumerTrustSettlementId, RowVersion = settlementRow.RowVersion });

					//Get  payment method rows to move
					foreach (ConsumerTrustSettlementPaymentMethodRow consumerTrustSettlementPaymentMethodRow in settlementRow.GetConsumerTrustSettlementPaymentMethodRows())
					{
						consumerTrustSettlementPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerTrustSettlementPaymentMethodRows.Add(new BaseRecord() { RowId = consumerTrustSettlementPaymentMethodRow.ConsumerTrustSettlementPaymentMethodId, RowVersion = consumerTrustSettlementPaymentMethodRow.RowVersion });
						}
						finally
						{
							consumerTrustSettlementPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}

					//Get payment rows to move
					foreach (ConsumerTrustPaymentRow settlementRowPaymentMethodRow in settlementRow.GetConsumerTrustPaymentRows())
					{
						settlementRowPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerTrustPaymentRows.Add(new BaseRecord() { RowId = settlementRowPaymentMethodRow.ConsumerTrustPaymentId, RowVersion = settlementRowPaymentMethodRow.RowVersion });
						}
						finally
						{
							settlementRowPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}


				}
				finally
				{
					settlementRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			}
		}

		/// <summary>
		/// Move MatchRows To Blotter 
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="modifiedByUser"></param>
		/// <param name="dataModel"></param>
		/// <param name="matchRows"></param>
		private void MoveMatchRowsToBlotter(Guid blotterId, Guid modifiedByUser, DataModel dataModel, List<BaseRecord> matchRows)
		{
			foreach (BaseRecord match in matchRows)
			{
				dataModel.UpdateMatch(
					blotterId,
					null,
					null,
					null,
					null,
					null,
					null,
					new object[] { match.RowId },
					match.RowVersion,
					null,
					null);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="chatRows"></param>
		private void MoveChatRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> chatRows)
		{
			foreach (BaseRecord chatRow in chatRows)
			{
				dataModel.UpdateChat(
					blotterId,
					null,
					new object[] { chatRow.RowId },
					null,
					null,
					null,
					null,
					chatRow.RowVersion);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="modifiedByUser"></param>
		/// <param name="dataModel"></param>
		/// <param name="consumerTrustNegotiationRows"></param>
		private void MoveNegotiationRowsToBlotter(Guid blotterId, Guid modifiedByUser, DataModel dataModel, List<BaseRecord> consumerTrustNegotiationRows)
		{

			// These variables are used for auditing the changes to this record.
			DateTime createdTime = DateTime.UtcNow;
			DateTime modifiedTime = createdTime;

			foreach (BaseRecord negotiationRow in consumerTrustNegotiationRows)
			{
				dataModel.UpdateConsumerTrustNegotiation(
					null,
					blotterId,
					null,
					new object[] { negotiationRow.RowId },
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					modifiedTime,
					modifiedByUser,
					null,
					null,
					null,
					null,
					null,
					negotiationRow.RowVersion,
					null,
					null);
			}
		}

		/// <summary>
		/// Move Negotiation Counter PaymentMethodRows to a different blotter
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="consumerTrustNegotiationCounterPaymentMethodRows"></param>
		private void MoveNegotiationCounterPaymentMethodRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> consumerTrustNegotiationCounterPaymentMethodRows)
		{
			foreach (var counterPaymentMethod in consumerTrustNegotiationCounterPaymentMethodRows)
			{
				dataModel.UpdateConsumerTrustNegotiationCounterPaymentMethod(
					blotterId,
					null,
					new object[] { counterPaymentMethod.RowId },
					null,
					null,
					counterPaymentMethod.RowVersion);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="consumerTrustNegotiationOfferPaymentMethodRows"></param>
		private void MoveNegotiationOfferPaymentMethodRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> consumerTrustNegotiationOfferPaymentMethodRows)
		{
			foreach (var offerPaymentMethod in consumerTrustNegotiationOfferPaymentMethodRows)
			{
				dataModel.UpdateConsumerTrustNegotiationOfferPaymentMethod(blotterId, null, null,
					new object[] { offerPaymentMethod.RowId },
					null, offerPaymentMethod.RowVersion);
			}
		}


		/// <summary>
		/// Update settlement rows
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="consumerTrustSettlementRows"></param>
		private void MoveSettlementRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> consumerTrustSettlementRows)
		{
			foreach (BaseRecord settlementRow in consumerTrustSettlementRows)
			{
				dataModel.UpdateConsumerTrustSettlement(
					null,
					blotterId,
					null,
					null,
					new object[] { settlementRow.RowId },
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					settlementRow.RowVersion,
					null,
					null,
					null);
			}
		}

		/// <summary>
		/// Update Payment Method
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="settlementRowPaymentMethodRows"></param>
		private void MovePaymentRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> settlementRowPaymentMethodRows)
		{

			foreach (var paymentMethodRow in settlementRowPaymentMethodRows)
			{
				dataModel.UpdateConsumerTrustPayment(
					null,
					null,
					blotterId,
					null,
					null,
					null,
					new object[] { paymentMethodRow.RowId },
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					paymentMethodRow.RowVersion,
					null,
					null
					);
			}
		}


		/// <summary>
		/// Update settlement Payment Method
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="consumerTrustSettlementPaymentMethodRows"></param>
		private void MoveSettlementPaymentMethodRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> consumerTrustSettlementPaymentMethodRows)
		{
			foreach (var paymentMethodRow in consumerTrustSettlementPaymentMethodRows)
			{
				dataModel.UpdateConsumerTrustSettlementPaymentMethod(
					blotterId,
					null,
					null,
					new object[] { paymentMethodRow.RowId },
					null,
					paymentMethodRow.RowVersion);
			}
		}

		/// <summary>
		/// Nested object to hold moveTo Data
		/// </summary>
		private class MoveToInfo
		{
			internal List<BaseRecord> matchRows = null;
			internal List<BaseRecord> chatRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerTrustNegotiationRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerTrustNegotiationCounterPaymentMethodRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerTrustNegotiationOfferPaymentMethodRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerTrustPaymentRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerTrustSettlementPaymentMethodRows = new List<BaseRecord>();
			internal List<BaseRecord> settlementRows = new List<BaseRecord>();
		}
	}
}

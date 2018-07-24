namespace FluidTrade.Guardian
{
	using System;


	using System.Data;
	using System.ServiceModel;
	using System.Linq;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;
	using System.Collections.Generic;


	/// <summary>
	/// 
	/// </summary>
	internal class ConsumerDebtPersistence : DataModelPersistence<ConsumerDebt>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public ConsumerDebtPersistence()
		{
		}
		/// <summary>
		/// Create a new Consumer Trust
		/// </summary>
		/// <returns></returns>
		public override Guid Create(ConsumerDebt record)
		{

			DataModel dataModel = new DataModel();
			Guid entityId = Guid.NewGuid();

			// Since DebtHolder object requires entries in Blotter, DebtClass and DebtHolder, 
			//a transaction is required to lock the records and change the data model.

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			////Create a entry in credit card
			dataModel.CreateConsumerDebt(
				record.CollectionDate,
				entityId,
				record.ConsumerId.GetValueOrDefault(),
				record.CreditCardId.GetValueOrDefault(),
				record.DateOfDelinquency,
				record.DebtRuleId,
				record.ExternalId0,
				record.Representative,
				record.Tag,
				record.TenantId,
				record.VendorCode);


			return entityId;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override void Update(ConsumerDebt record)
		{
			DataModel dataModel = new DataModel();

			if (record.RowId == null || DataModel.ConsumerDebt.ConsumerDebtKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("ConsumerDebt", new object[] { record.RowId }));
			}

			dataModel.UpdateConsumerDebt(
				record.CollectionDate,
				null,
				new object[] { record.RowId },
				record.ConsumerId,
				record.CreditCardId,
				record.DateOfDelinquency,
				record.DebtRuleId,
				record.ExternalId0,
				record.Representative,
				record.RowVersion,
				record.Tag,
				null,
				record.VendorCode);
		}

		/// <summary>
		/// Delete a debt holder
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(ConsumerDebt record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;

			if (record.RowId == null || DataModel.ConsumerDebt.ConsumerDebtKey.Find(record.RowId) == null)
				return ErrorCode.RecordNotFound;

			ConsumerDebtRow consumerDebt = DataModel.ConsumerDebt.ConsumerDebtKey.Find(record.RowId);
			consumerDebt.AcquireReaderLock(transaction);
			ConsumerRow consumer;
			CreditCardRow card;
			DebtRuleRow debtRule;

			try
			{
				Guid debtId = consumerDebt.ConsumerDebtId;
				consumer = consumerDebt.ConsumerRow;
				card = consumerDebt.CreditCardRow;
				debtRule = consumerDebt.DebtRuleRow;
			}
			finally
			{
				consumerDebt.ReleaseReaderLock(transaction.TransactionId);
			}

#if false   // If we switch from explicitly deleting the working order to explicitly deleting the security, then we need this.
			if (!TradingSupport.HasAccess(transaction, debtId, AccessRight.Write))
				return ErrorCode.AccessDenied;
#endif

			consumerDebt.AcquireWriterLock(transaction);
			if (consumerDebt.RowState != DataRowState.Deleted && consumerDebt.RowState != DataRowState.Detached)
				dataModel.DestroyConsumerDebt(new object[] { consumerDebt.ConsumerDebtId }, record.RowVersion);
			consumerDebt.ReleaseWriterLock(transaction.TransactionId);

			consumer.AcquireWriterLock(transaction);
			if (consumer.RowState != DataRowState.Deleted && consumer.RowState != DataRowState.Detached && consumer.GetConsumerDebtRows().Length == 0)
				dataModel.DestroyConsumer(new object[] { consumer.ConsumerId }, consumer.RowVersion);
			consumer.ReleaseWriterLock(transaction.TransactionId);

			card.AcquireWriterLock(transaction);
			if (card.RowState != DataRowState.Deleted && card.RowState != DataRowState.Detached)
				dataModel.DestroyCreditCard(new object[] { card.CreditCardId }, card.RowVersion);
			card.ReleaseWriterLock(transaction.TransactionId);

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


		public override ConsumerDebt Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <returns></returns>
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
				MoveMatchRowsToBlotter(blotterId, modifiedByUser, dataModel, moveToInfo.matchRows);

				//Move Negotiatiaon Data
				MoveNegotiationRowsToBlotter(blotterId, modifiedByUser, dataModel, moveToInfo.consumerDebtNegotiationRows);
				MoveNegotiationCounterPaymentMethodRowsToBlotter(blotterId, dataModel, moveToInfo.consumerDebtNegotiationCounterPaymentMethodRows);
				MoveNegotiationOfferPaymentMethodRowsToBlotter(blotterId, dataModel, moveToInfo.consumerDebtNegotiationOfferPaymentMethodRows);
				MoveChatRowsToBlotter(blotterId, dataModel, moveToInfo.chatRows);

				//Move Settlement data				
				MoveSettlementRowsToBlotter(blotterId, dataModel, moveToInfo.settlementRows);
				MoveSettlementPaymentMethodRowsToBlotter(blotterId, dataModel, moveToInfo.consumerDebtSettlementPaymentMethodRows);
				MovePaymentRowsToBlotter(blotterId, dataModel, moveToInfo.consumerDebtPaymentRows);
			}

		}


		/// <summary>
		/// Lock rows and collect the data.
		/// </summary>
		/// <param name="record"></param>
		/// <param name="blotterId"></param>
		/// <param name="dataModelTransaction"></param>
		/// <returns></returns>
		private MoveToInfo CollectData(BaseRecord record, Guid blotterId, DataModelTransaction dataModelTransaction)
		{
			MoveToInfo moveToInfo = new MoveToInfo();
			WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(record.RowId);
			MatchRow[] workingOrderRowMatchRows = null;

			if (workingOrderRow == null || workingOrderRow.RowState == DataRowState.Detached)
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("WorkingOrder", new object[] { record.RowId }));

			workingOrderRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				//Nothing to do
				if (blotterId == workingOrderRow.BlotterId)
					return null;

				if (workingOrderRow.RowVersion != record.RowVersion)
					throw new FaultException<OptimisticConcurrencyFault>(new OptimisticConcurrencyFault("WorkingOrder", new object[] { record.RowId }));

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
			foreach (ConsumerDebtNegotiationRow consumerDebtNegotiationRow in matchRow.GetConsumerDebtNegotiationRows())
			{
				consumerDebtNegotiationRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					moveToInfo.consumerDebtNegotiationRows.Add(new BaseRecord() { RowId = consumerDebtNegotiationRow.ConsumerDebtNegotiationId, RowVersion = consumerDebtNegotiationRow.RowVersion });


					//Get counter payment rows to move
					foreach (ConsumerDebtNegotiationCounterPaymentMethodRow counterPaymentMethodRow in consumerDebtNegotiationRow.GetConsumerDebtNegotiationCounterPaymentMethodRows())
					{
						counterPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerDebtNegotiationCounterPaymentMethodRows.Add(new BaseRecord() { RowId = counterPaymentMethodRow.ConsumerDebtNegotiationCounterPaymentMethodId, RowVersion = counterPaymentMethodRow.RowVersion });
						}
						finally
						{
							counterPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}

					//Get offer payment rows to move
					foreach (ConsumerDebtNegotiationOfferPaymentMethodRow offerPaymentMethodRow in consumerDebtNegotiationRow.GetConsumerDebtNegotiationOfferPaymentMethodRows())
					{
						offerPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerDebtNegotiationOfferPaymentMethodRows.Add(new BaseRecord() { RowId = offerPaymentMethodRow.ConsumerDebtNegotiationOfferPaymentMethodId, RowVersion = offerPaymentMethodRow.RowVersion });
						}
						finally
						{
							offerPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}

					//Get Settlement rows to move
					CollectSettlementData(dataModelTransaction, moveToInfo, consumerDebtNegotiationRow);
				}
				finally
				{
					consumerDebtNegotiationRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataModelTransaction"></param>
		/// <param name="moveToInfo"></param>
		/// <param name="consumerDebtNegotiationRow"></param>
		private static void CollectSettlementData(DataModelTransaction dataModelTransaction, MoveToInfo moveToInfo, ConsumerDebtNegotiationRow consumerDebtNegotiationRow)
		{
			foreach (ConsumerDebtSettlementRow settlementRow in consumerDebtNegotiationRow.GetConsumerDebtSettlementRows())
			{
				settlementRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					moveToInfo.settlementRows.Add(new BaseRecord() { RowId = settlementRow.ConsumerDebtSettlementId, RowVersion = settlementRow.RowVersion });

					//Get  payment method rows to move
					foreach (ConsumerDebtSettlementPaymentMethodRow consumerDebtSettlementPaymentMethodRow in settlementRow.GetConsumerDebtSettlementPaymentMethodRows())
					{
						consumerDebtSettlementPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerDebtSettlementPaymentMethodRows.Add(new BaseRecord() { RowId = consumerDebtSettlementPaymentMethodRow.ConsumerDebtSettlementPaymentMethodId, RowVersion = consumerDebtSettlementPaymentMethodRow.RowVersion });
						}
						finally
						{
							consumerDebtSettlementPaymentMethodRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
					}

					//Get payment rows to move
					foreach (ConsumerDebtPaymentRow settlementRowPaymentMethodRow in settlementRow.GetConsumerDebtPaymentRows())
					{
						settlementRowPaymentMethodRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							moveToInfo.consumerDebtPaymentRows.Add(new BaseRecord() { RowId = settlementRowPaymentMethodRow.ConsumerDebtPaymentId, RowVersion = settlementRowPaymentMethodRow.RowVersion });
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
		///  Move MatchRows To Blotter 
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
		/// Move NegotiationRows
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="modifiedByUser"></param>
		/// <param name="dataModel"></param>
		/// <param name="consumerDebtNegotiationRows"></param>
		private void MoveNegotiationRowsToBlotter(Guid blotterId, Guid modifiedByUser, DataModel dataModel, List<BaseRecord> consumerDebtNegotiationRows)
		{

			foreach (BaseRecord negotiationRow in consumerDebtNegotiationRows)
			{
				dataModel.UpdateConsumerDebtNegotiation(
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
					DateTime.UtcNow,
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
		/// 
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="negotiationCounterPaymentMethodRows"></param>
		private void MoveNegotiationCounterPaymentMethodRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> negotiationCounterPaymentMethodRows)
		{
			foreach (BaseRecord counterPaymentMethod in negotiationCounterPaymentMethodRows)
			{
				dataModel.UpdateConsumerDebtNegotiationCounterPaymentMethod(
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
		/// <param name="negotiationOfferPaymentMethodRows"></param>
		private void MoveNegotiationOfferPaymentMethodRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> negotiationOfferPaymentMethodRows)
		{
			foreach (BaseRecord offerPaymentMethod in negotiationOfferPaymentMethodRows)
			{
				dataModel.UpdateConsumerDebtNegotiationOfferPaymentMethod(blotterId,
					null,
					null,
					new object[] { offerPaymentMethod.RowId },
					null,
					offerPaymentMethod.RowVersion);
			}
		}


		/// <summary>
		///  Update settlement rows
		/// </summary>
		/// <param name="blotterId"></param>
		/// <param name="dataModel"></param>
		/// <param name="settlementRows"></param>
		private void MoveSettlementRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> settlementRows)
		{

			foreach (BaseRecord settlementRow in settlementRows)
			{

				dataModel.UpdateConsumerDebtSettlement(
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
			foreach (BaseRecord paymentMethodRow in settlementRowPaymentMethodRows)
			{
				dataModel.UpdateConsumerDebtPayment(
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
		/// <param name="settlementPaymentMethodRows"></param>
		private void MoveSettlementPaymentMethodRowsToBlotter(Guid blotterId, DataModel dataModel, List<BaseRecord> settlementPaymentMethodRows)
		{
			foreach (BaseRecord paymentMethodRow in settlementPaymentMethodRows)
			{
				dataModel.UpdateConsumerDebtSettlementPaymentMethod(blotterId,
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
			internal List<BaseRecord> consumerDebtNegotiationRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerDebtNegotiationCounterPaymentMethodRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerDebtNegotiationOfferPaymentMethodRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerDebtPaymentRows = new List<BaseRecord>();
			internal List<BaseRecord> consumerDebtSettlementPaymentMethodRows = new List<BaseRecord>();
			internal List<BaseRecord> settlementRows = new List<BaseRecord>();
		}

	}		
}


namespace FluidTrade.Guardian
{
	using System;
	using System.Data;
	using System.Security;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// 
	/// </summary>
	internal class WorkingOrderPersistence : DataModelPersistence<WorkingOrderRecord>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public  WorkingOrderPersistence()
		{
		}
		/// <summary>
		/// Create a new DebtHolder
		/// </summary>
		/// <returns></returns>
		public override Guid Create(WorkingOrderRecord record)
		{
			DataModel dataModel = new DataModel();
			Guid workingOrderId = Guid.NewGuid();

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			Guid blotter = record.BlotterId.GetValueOrDefault();
			if (blotter == Guid.Empty || !TradingSupport.HasAccess(dataModelTransaction, blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");

			Guid createdbyUserId = TradingSupport.UserId;
			DateTime currentUTCTime = DateTime.UtcNow;
			
			////Create a entry in credit card
			dataModel.CreateWorkingOrder(
				record.AutomaticQuantity,
				record.BlotterId.GetValueOrDefault(),
				currentUTCTime,
				createdbyUserId,
				record.CrossingCode.GetValueOrDefault(),
				record.DestinationId,
				record.ExternalId0,
				record.IsAutomatic,
				record.IsAwake,
				record.IsBrokerMatch,
				record.IsHedgeFundMatch,
				record.IsInstitutionMatch,
				record.LimitPrice,
				currentUTCTime,
				createdbyUserId,
				record.OrderTypeCode,
				record.SecurityId.GetValueOrDefault(),
				record.SettlementDate.GetValueOrDefault(currentUTCTime),
				record.SettlementId,
				record.SideId.GetValueOrDefault(),
				record.StartUTCTime,
				record.StatusCodeId.GetValueOrDefault(),
				record.StopPrice,
				record.StopUTCTime,
				record.SubmittedQuantity,
				record.SubmittedUTCTime,
				record.TimeInForceId.GetValueOrDefault(),
				record.TradeDate.GetValueOrDefault(currentUTCTime),
				record.UploadedUTCTime,
				workingOrderId);

			return workingOrderId;

		}

		/// <summary>
		/// Update WorkingOderRow
		/// </summary>
		public override void Update(WorkingOrderRecord record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			Guid blotter = record.BlotterId.GetValueOrDefault();
			if (blotter == Guid.Empty || !TradingSupport.HasAccess(dataModelTransaction, blotter, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected blotter");

			if (record.RowId == null || DataModel.WorkingOrder.WorkingOrderKey.Find(record.RowId) == null)
			{
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("Working Order", new object[] { record.RowId }));
			}

			// It is a good idea to have a central method for updating the Working Order in the event the parameter order changes.
			dataModel.UpdateWorkingOrder(
				record.AutomaticQuantity,
				record.BlotterId,
				null,
				null,
				record.CrossingCode,
				record.DestinationId,
				record.ExternalId0,
				record.IsAutomatic,
				record.IsAwake,
				record.IsBrokerMatch,
				record.IsHedgeFundMatch,
				record.IsInstitutionMatch,
				record.LimitPrice,
				DateTime.UtcNow,
				TradingSupport.UserId,
				record.OrderTypeCode,
				record.RowVersion,
				record.SecurityId,
				record.SettlementDate,
				record.SettlementId,
				record.SideId,
				record.StartUTCTime,
				record.StatusCodeId,
				record.StopPrice,
				record.StopUTCTime,
				record.SubmittedQuantity,
				record.SubmittedUTCTime,
				record.TimeInForceId,
				record.TradeDate,
				record.UploadedUTCTime,
				null,
				new object[] { record.RowId });
		}

		/// <summary>
		/// Delete a working order row.
		/// </summary>
		/// <param name="record">The working order record to delete.</param>
		/// <returns>Error code of any failure, or Success.</returns>
		public override ErrorCode Delete(WorkingOrderRecord record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find((Guid)record.RowId);

			if(workingOrderRow == null)
				return ErrorCode.RecordNotFound;

			return DeleteRow(dataModel, transaction, workingOrderRow);

		}

		/// <summary>
		/// Delete a WorkingOrderRow.
		/// </summary>
		/// <param name="dataModel">The data model.</param>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="workingOrderRow">The working order row to delete.</param>
		/// <returns>Error code of any failure, or Success.</returns>
		public ErrorCode DeleteRow(DataModel dataModel, DataModelTransaction transaction, WorkingOrderRow workingOrderRow)
		{

			SecurityRow securityRow = null;
			EntityRow securityEntityRow = null;
			MatchRow[] matchRows;
			ConsumerDebtRow[] consumerDebtRows;
			ConsumerTrustRow[] consumerTrustRows;
			CreditCardRow creditCardRow = null;
			ConsumerRow consumerRow = null;
			Guid blotterId;
			Guid securityEntityId = Guid.Empty;
			Int64 securityEntityRowVersion = 0;
			Guid consumerId = Guid.Empty;
			Int64 consumerRowVersion = 0;
			Guid creditCardId = Guid.Empty;
			Int64 creditCardRowVersion = 0;
			Boolean consumerStillInUse = false;

			workingOrderRow.AcquireWriterLock(transaction.TransactionId, DataModel.LockTimeout);
			if (workingOrderRow.RowState == DataRowState.Deleted ||
				workingOrderRow.RowState == DataRowState.Detached)
			{

				workingOrderRow.ReleaseLock(transaction.TransactionId);
				return ErrorCode.RecordNotFound;

			}
			else
			{
				transaction.AddLock(workingOrderRow);
			}
			blotterId = workingOrderRow.BlotterId;
			securityRow = workingOrderRow.SecurityRowByFK_Security_WorkingOrder_SecurityId;
			matchRows = workingOrderRow.GetMatchRows();

			if (matchRows != null)
				foreach (MatchRow matchRow in matchRows)
					if (IsSettled(transaction, matchRow))
						return ErrorCode.RecordExists;

			if(!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, blotterId, AccessRight.Write))
			{
				workingOrderRow.ReleaseLock(transaction.TransactionId);
				return ErrorCode.AccessDenied;
			}
			securityRow.AcquireWriterLock(transaction.TransactionId, DataModel.LockTimeout);
			if(securityRow.RowState == DataRowState.Deleted ||
				securityRow.RowState == DataRowState.Detached)
			{
				workingOrderRow.ReleaseLock(transaction.TransactionId);
				securityRow.ReleaseWriterLock(transaction.TransactionId);
				return ErrorCode.RecordNotFound;
			}
			securityEntityRow = securityRow.EntityRow;
			consumerDebtRows = securityRow.GetConsumerDebtRows();
			consumerTrustRows = securityRow.GetConsumerTrustRows();
			securityRow.ReleaseWriterLock(transaction.TransactionId);

			securityEntityRow.AcquireWriterLock(transaction);
			if(securityEntityRow.RowState == DataRowState.Deleted ||
				securityEntityRow.RowState == DataRowState.Detached)
			{
				workingOrderRow.ReleaseLock(transaction.TransactionId);
				securityEntityRow.ReleaseLock(transaction.TransactionId);
				return ErrorCode.RecordNotFound;
			}
			securityEntityId = securityEntityRow.EntityId;
			securityEntityRowVersion = securityEntityRow.RowVersion;
			securityEntityRow.ReleaseLock(transaction.TransactionId);

			if (consumerTrustRows.Length > 0 && consumerDebtRows.Length > 0)
				EventLog.Warning("Deleting a working order associated with both ConsumerDebt and ConsumerTrust rows");
			else if (consumerDebtRows.Length > 1)
				EventLog.Warning("Deleting a working order associated with more than one ConsumerDebt row");
			else if (consumerTrustRows.Length > 1)
				EventLog.Warning("Deleting a working order associated with more than one ConsumerTrust row");

			if (consumerDebtRows.Length == 1)
			{

				ConsumerDebtRow consumerDebtRow = consumerDebtRows[0];

				consumerDebtRow.AcquireWriterLock(transaction);
				if(consumerDebtRow.RowState == DataRowState.Deleted ||
					consumerDebtRow.RowState == DataRowState.Detached)
				{
				}
				else
				{
					creditCardRow = consumerDebtRow.CreditCardRow;
					consumerRow = consumerDebtRow.ConsumerRow;
				}
				consumerDebtRow.ReleaseLock(transaction.TransactionId);

			}
			else if (consumerTrustRows.Length == 1)
			{

				ConsumerTrustRow consumerTrustRow = consumerTrustRows[0];

				consumerTrustRow.AcquireWriterLock(transaction);
				if(consumerTrustRow.RowState == DataRowState.Deleted ||
					consumerTrustRow.RowState == DataRowState.Detached)
				{
				}
				else
				{
					consumerRow = consumerTrustRow.ConsumerRow;
				}
				consumerTrustRow.ReleaseLock(transaction.TransactionId);
			}

			if (consumerRow != null)
			{
				consumerRow.AcquireWriterLock(transaction);
				if(consumerRow.RowState == DataRowState.Deleted ||
					consumerRow.RowState == DataRowState.Detached)
				{
					consumerRow = null;
				}
				else
				{
					consumerStillInUse = consumerRow.GetConsumerDebtRows().Length > 1;
					consumerId = consumerRow.ConsumerId;
					consumerRowVersion = consumerRow.RowVersion;
				}
				consumerRow.ReleaseLock(transaction.TransactionId);
			}

			if (creditCardRow != null)
			{

				creditCardRow.AcquireWriterLock(transaction);
				if(creditCardRow.RowState == DataRowState.Deleted ||
					creditCardRow.RowState == DataRowState.Detached)
				{
					creditCardRow = null;
				}
				else
				{
					creditCardId = creditCardRow.ConsumerId;
					creditCardRowVersion = creditCardRow.RowVersion;
				}
				creditCardRow.ReleaseLock(transaction.TransactionId);

			}

			//gonna get the lock on the workingOrder and let the txn commit/rollback get rid of it
			//this will basically wrap the delete row
			//action in a critical section because the first
			//reader lock in the method is on the workingOrder row
			//workingOrderRow.AcquireWriterLock(transaction.TransactionId, DataModel.LockTimeout);
			if(workingOrderRow.RowState == DataRowState.Deleted ||
					workingOrderRow.RowState == DataRowState.Detached)
			{
				workingOrderRow.ReleaseLock(transaction.TransactionId);
				return ErrorCode.RecordNotFound;
			}
			//securityRow.AcquireWriterLock(transaction.TransactionId, DataModel.LockTimeout);
			//if(securityRow.RowState == DataRowState.Deleted ||
			//        securityRow.RowState == DataRowState.Detached)
			//{
			//    workingOrderRow.ReleaseLock(transaction.TransactionId);
			//    return ErrorCode.RecordNotFound;
			//}
			if(creditCardRow != null && consumerStillInUse)
			{
				dataModel.DestroyCreditCard(new object[] { creditCardId }, creditCardRowVersion);
			}
			if(consumerRow != null && !consumerStillInUse)
			{
				dataModel.DestroyConsumer(new object[] { consumerId }, consumerRowVersion);
			}

			dataModel.DestroyEntity(new object[] { securityEntityId }, securityEntityRowVersion);

			return ErrorCode.Success;

		}

		public override WorkingOrderRecord Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Update working order time.
		/// </summary>
		/// <param name="workingOrderId"></param>
		public void UpdateModifyTime(Guid workingOrderId)
		{
			WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(workingOrderId);
			if (workingOrderRow != null && workingOrderRow.RowState != DataRowState.Detached)
			{
				workingOrderRow.AcquireReaderLock(DataModelTransaction.Current);
				this.Update(new WorkingOrderRecord() 
				{ RowId = workingOrderRow.WorkingOrderId, RowVersion = workingOrderRow.RowVersion, BlotterId = workingOrderRow.BlotterId });
			}
		}
		
		/// <summary>
		/// Determine whether there is a settlement (indirectly) associated with a match.
		/// </summary>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="match">The match to check.</param>
		/// <returns>True if the account is settled.</returns>
		public static bool IsSettled(DataModelTransaction transaction, MatchRow match)
		{
			return MatchDataTable.IsSettled(transaction, match, true);
		}
	}

}

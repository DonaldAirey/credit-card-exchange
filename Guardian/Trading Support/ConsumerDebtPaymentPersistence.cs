namespace FluidTrade.Guardian
{

	using System;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	internal class ConsumerDebtPaymentPersistence : DataModelPersistence<ConsumerDebtPayment>
	{

		/// <summary>
		/// Placeholder.
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		public override Guid Create(ConsumerDebtPayment record)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Placeholder
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public override ConsumerDebtPayment Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Update Consumer Debt Payment Record
		/// </summary>
		/// <param name="record"></param>
		public override void Update(ConsumerDebtPayment record)
		{

			DataModel dataModel = new DataModel();
			//Use the current transaction
			DataModelTransaction transaction = DataModelTransaction.Current;
			Guid userId = TradingSupport.UserId;
			//Use the deamonUser so that Modified user can be resolved from the other side of the chinese firewall.
			Guid deamonUser = TradingSupport.DaemonUserId;			
			Guid blotterId;
			Int64 contraPaymentRowVersion;

			//Sanity Check
			ConsumerDebtPaymentRow consumerDebtPaymentRow = DataModel.ConsumerDebtPayment.ConsumerDebtPaymentKey.Find(record.RowId);
			if (consumerDebtPaymentRow == null)
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("ConsumerDebtPayment", new object[] { record.RowId }));

			consumerDebtPaymentRow.AcquireWriterLock(transaction.TransactionId, DataModel.LockTimeout);
			try
			{
				blotterId = consumerDebtPaymentRow.BlotterId;
			}
			finally
			{
				consumerDebtPaymentRow.ReleaseReaderLock(transaction.TransactionId);
			}

			if (!DataModelFilters.HasAccess(transaction, userId, blotterId, AccessRight.Write))
				throw new FaultException<SecurityFault>(new SecurityFault("Current user does not have write access to the payment summary"));


			ConsumerTrustPaymentRow consumerTrustPaymentRow = DataModel.ConsumerTrustPayment.ConsumerTrustPaymentKey.Find(record.RowId);
			consumerTrustPaymentRow.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);
			try
			{
				contraPaymentRowVersion = consumerTrustPaymentRow.RowVersion;
			}
			finally
			{
				consumerTrustPaymentRow.ReleaseLock(transaction.TransactionId);
			}

			dataModel.UpdateConsumerDebtPayment(
				record.ActualPaymentDate == null? DBNull.Value : (object)record.ActualPaymentDate.Value,
				record.ActualPaymentValue == null ? DBNull.Value : (object)record.ActualPaymentValue.Value,
				null,
				record.CheckId,
				record.ClearedDate,
				record.RowId,
				new object[] { record.RowId },
				null,
				null,
				null,
				null,
				null,
				null,
				record.Fee0 == null ? DBNull.Value : (object)record.Fee0,
				null,
				null,
				null,
				record.IsActive,
				record.IsCleared,
				record.Memo0,
				record.Memo1,
				record.Memo2,
				record.Memo3,
				DateTime.UtcNow,
				userId,
				record.RowVersion,
				record.StatusId,
				record.TrackingNumber);


			//Update Contra side
			dataModel.UpdateConsumerTrustPayment(
				record.ActualPaymentDate == null ? DBNull.Value : (object)record.ActualPaymentDate.Value,
				record.ActualPaymentValue == null ? DBNull.Value : (object)record.ActualPaymentValue.Value,
				null,
				record.CheckId,
				record.ClearedDate,
				null,
				new object[] { record.RowId },
				null,
				null,
				null,
				null,
				null,				
				record.Fee0 == null ? DBNull.Value : (object)record.Fee0,
				null,
				null,
				null,
				record.IsActive,
				record.IsCleared,
				record.Memo0,
				record.Memo1,
				record.Memo2,
				record.Memo3,
				DateTime.UtcNow,
				deamonUser,
				contraPaymentRowVersion,
				record.StatusId,
				record.TrackingNumber);



		}

		public override FluidTrade.Core.ErrorCode Delete(ConsumerDebtPayment record)
		{
			throw new NotImplementedException();
		}

	}

}

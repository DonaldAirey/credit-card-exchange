namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	class ConsumerTrustPaymentPersistence : DataModelPersistence<ConsumerTrustPayment>
	{

		public override Guid Create(ConsumerTrustPayment record)
		{
			throw new NotImplementedException();
		}

		public override ConsumerTrustPayment Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public override void Update(ConsumerTrustPayment record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			Guid userId = TradingSupport.UserId;
			Guid deamonUser = TradingSupport.DaemonUserId;
			ConsumerTrustPaymentRow consumerTrustPaymentRow = DataModel.ConsumerTrustPayment.ConsumerTrustPaymentKey.Find(record.RowId);
			Guid blotterId;
			Int64 contraPaymentRowVersion;

			if (consumerTrustPaymentRow == null)
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("ConsumerTrustPayment", new object[] { record.RowId }));

			consumerTrustPaymentRow.AcquireWriterLock(transaction);
			blotterId = consumerTrustPaymentRow.BlotterId;

			if (!DataModelFilters.HasAccess(transaction, userId, blotterId, AccessRight.Write))
				throw new FaultException<SecurityFault>(new SecurityFault("Current user does not have write access to the payment summary"));

			ConsumerDebtPaymentRow consumerDebtPaymentRow = DataModel.ConsumerDebtPayment.ConsumerDebtPaymentKey.Find(record.RowId);
			consumerDebtPaymentRow.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);
			try
			{
				contraPaymentRowVersion = consumerDebtPaymentRow.RowVersion;
			}
			finally
			{
				consumerDebtPaymentRow.ReleaseLock(transaction.TransactionId);
			}


			dataModel.UpdateConsumerTrustPayment(
				record.ActualPaymentDate == null ? DBNull.Value : (object)record.ActualPaymentDate.Value,
				record.ActualPaymentValue == null ? DBNull.Value : (object)record.ActualPaymentValue,
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
			dataModel.UpdateConsumerDebtPayment(
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

		public override FluidTrade.Core.ErrorCode Delete(ConsumerTrustPayment record)
		{
			throw new NotImplementedException();
		}

	}

}

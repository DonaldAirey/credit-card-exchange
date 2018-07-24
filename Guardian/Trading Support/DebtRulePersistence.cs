using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidTrade.Guardian.Records;
using FluidTrade.Core;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace FluidTrade.Guardian
{
	class DebtRulePersistence : DataModelPersistence<DebtRule>
	{
		/// <summary>
		/// Create a new DebtRule
		/// </summary>
		/// <returns></returns>
		public override Guid Create(DebtRule record)
		{

			DataModel dataModelClient = new DataModel();
			Guid debtRuleId = Guid.NewGuid();

			// These fields are required by this method, but not by the record itself, so bark if they're missing.
			if (record.IsAutoSettled == null)
				throw new FaultException("IsAutoSettled cannot be null");
			if (record.Owner == null)
				throw new FaultException("Owner cannot be null");
			if (record.PaymentLength == null)
				throw new FaultException("PaymentLength cannot be null");
			if (record.PaymentMethod == null)
				throw new FaultException("SettlementValue cannot be null");
			if (record.PaymentStartDateLength == null)
				throw new FaultException("PaymentStartDateLength cannot be null");
			if (record.PaymentStartDateUnitId == null)
				throw new FaultException("PaymentStartDateUnitId cannot be null");
			if (record.SettlementUnitId == null)
				throw new FaultException("SettlementUnitId cannot be null");
			if (record.SettlementValue == null)
				throw new FaultException("SettlementValue cannot be null");

			if (!DataModelFilters.HasAccess(DataModelTransaction.Current, TradingSupport.UserId, record.Owner.Value, AccessRight.Write))
				throw new SecurityAccessDeniedException("Current user does not have right access to the debt class that would own this debt rule");

			dataModelClient.CreateDebtRule(
				debtRuleId,
				null,
				record.IsAutoSettled,
				record.Name,
				record.PaymentLength.Value,
				record.PaymentStartDateLength.Value,
				record.PaymentStartDateUnitId.Value,
				record.SettlementUnitId.Value,
				record.SettlementValue.Value);
			dataModelClient.CreateDebtRuleMap(record.Owner.Value, debtRuleId, Guid.NewGuid(), null);

			foreach (Guid paymentMethod in record.PaymentMethod)
				dataModelClient.CreateDebtRulePaymentMethod(debtRuleId, Guid.NewGuid(), paymentMethod);

			return debtRuleId;

		}

		public override DebtRule Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public override void Update(DebtRule record)
		{

			DataModel dataModelClient = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			DebtRuleRow debtRuleRow = DataModel.DebtRule.DebtRuleKey.Find(record.RowId);
			DebtRuleMapRow[] debtRuleMapRows;
			List<Guid> paymentMethod = record.PaymentMethod.ToList();

			if (record.RowId == null || debtRuleRow == null)
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("DebtRule", new object[] { record.RowId }), "DebtRule could not be found");

			debtRuleRow.AcquireReaderLock(transaction);
			debtRuleMapRows = debtRuleRow.GetDebtRuleMapRows();

			// There really should be at most one of these, but there may be none.
			foreach (DebtRuleMapRow debtRuleMapRow in debtRuleMapRows)
			{

				debtRuleMapRow.AcquireReaderLock(transaction);
				if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, debtRuleMapRow.DebtClassId, AccessRight.Write))
					throw new SecurityAccessDeniedException("Current user does not have right access to the debt class that owns this debt rule");
				debtRuleMapRow.ReleaseLock(transaction.TransactionId);

			}

			foreach (DebtRulePaymentMethodRow methodRow in debtRuleRow.GetDebtRulePaymentMethodRows())
			{

				methodRow.AcquireReaderLock(transaction);
				if (paymentMethod.Contains(methodRow.PaymentMethodTypeId))
					paymentMethod.Remove(methodRow.PaymentMethodTypeId);
				else
					dataModelClient.DestroyDebtRulePaymentMethod(new object[] { methodRow.DebtRulePaymentMethodId }, methodRow.RowVersion);
				methodRow.ReleaseLock(transaction.TransactionId);

			}

			foreach (Guid method in paymentMethod)
				dataModelClient.CreateDebtRulePaymentMethod(record.RowId, Guid.NewGuid(), method);

			debtRuleRow.ReleaseLock(transaction.TransactionId);

			dataModelClient.UpdateDebtRule(
				null,
				new object[] { record.RowId },
				null,
				record.IsAutoSettled,
				record.Name,
				record.PaymentLength,
				record.PaymentStartDateLength,
				record.PaymentStartDateUnitId,
				record.RowVersion,
				record.SettlementUnitId,
				record.SettlementValue);
			debtRuleRow.ReleaseLock(transaction.TransactionId);



		}

		public override ErrorCode Delete(DebtRule record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;

			if (record.RowId == null || DataModel.DebtRule.DebtRuleKey.Find(record.RowId) == null)
				return ErrorCode.RecordNotFound;

			DebtRuleRow debtRuleRow = DataModel.DebtRule.DebtRuleKey.Find(record.RowId);

			return this.DeleteRow(dataModel, transaction, debtRuleRow);

		}

		public ErrorCode DeleteRow(DataModel dataModel, DataModelTransaction transaction, DebtRuleRow debtRuleRow)
		{

			debtRuleRow.AcquireReaderLock(transaction);
			DebtRuleMapRow[] debtRuleMaps = debtRuleRow.GetDebtRuleMapRows();

			if (debtRuleRow.GetDebtClassRows().Length != 0)
				return ErrorCode.AccessDenied;
			debtRuleRow.ReleaseReaderLock(transaction.TransactionId);

			foreach (DebtRuleMapRow debtRuleMapRow in debtRuleMaps)
			{

				debtRuleMapRow.AcquireReaderLock(transaction);
				if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, debtRuleMapRow.DebtClassId, AccessRight.Write))
					return ErrorCode.AccessDenied;
				debtRuleMapRow.ReleaseReaderLock(transaction.TransactionId);

			}

			debtRuleRow.AcquireWriterLock(transaction);
			dataModel.DestroyDebtRule(new object[] { debtRuleRow.DebtRuleId }, debtRuleRow.RowVersion);
			debtRuleRow.ReleaseLock(transaction.TransactionId);

			return ErrorCode.Success;

		}

	}

}

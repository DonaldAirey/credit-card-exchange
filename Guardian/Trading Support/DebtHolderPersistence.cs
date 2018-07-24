namespace FluidTrade.Guardian
{
	using System;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;
	using System.Data;
	using System.Security;


	/// <summary>
	/// 
	/// </summary>
	internal class DebtHolderPersistence : DataModelPersistence<DebtHolder>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public  DebtHolderPersistence()
		{
		}
		/// <summary>
		/// Create a new DebtHolder
		/// </summary>
		/// <returns></returns>
		public override Guid Create(DebtHolder record)
		{

			DataModel dataModel = new DataModel();
			Guid entityId = Guid.Empty;
			TypeRow typeRow = null;
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			if (record.TypeId != null && record.TypeId != Guid.Empty)
				typeRow = DataModel.Type.TypeKey.Find(record.TypeId.Value);
			else
				typeRow = DataModel.Type.TypeKeyExternalId0.Find("DEBT HOLDER");

			if (typeRow != null)
			{

				DebtClassPersistence debtClassPersistence = new DebtClassPersistence();
				typeRow.AcquireReaderLock(dataModelTransaction);
				record.TypeId = typeRow.TypeId;
				typeRow.ReleaseLock(dataModelTransaction.TransactionId);

				entityId = debtClassPersistence.Create(record);

				dataModel.CreateDebtHolder(entityId, null);

			}

			return entityId;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override void Update(DebtHolder record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			EntityPersistence entityPersistence = new EntityPersistence();
			DebtClassPersistence debtClassPersistence = new DebtClassPersistence();

			entityPersistence.Update(record);
			debtClassPersistence.Update(record);

		}

		/// <summary>
		/// Delete a debt holder
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(DebtHolder record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			WorkingOrderPersistence workingOrderPersistence = new WorkingOrderPersistence();
			DebtRulePersistence debtRulePersistence = new DebtRulePersistence();
			DebtHolderRow debtHolderRow = DataModel.DebtHolder.DebtHolderKey.Find(record.RowId);
			DebtHolderImportTranslationRow debtHolderImportTranslationRow;
			DebtClassRow debtClassRow;
			BlotterRow blotterRow;
			WorkingOrderRow[] workingOrderRows;
			DebtRuleMapRow[] debtRuleMapRows;
			EntityRow entityRow;
			EntityTreeRow[] children;
			Guid entityId;
			Int64 entityRowVersion;
			Guid debtHolderImportTranslationId = Guid.Empty;
			Int64 debtHolderImportTranslationRowVersion = 0;

			if (record.RowId == null || debtHolderRow == null)
				return ErrorCode.RecordNotFound;
			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, record.RowId, AccessRight.Write))
				return ErrorCode.AccessDenied;

			debtHolderRow.AcquireReaderLock(transaction);
			debtClassRow = debtHolderRow.DebtClassRow;
			debtHolderImportTranslationRow = debtHolderRow.DebtHolderImportTranslationRow;
			debtHolderRow.ReleaseReaderLock(transaction.TransactionId);

			if (debtHolderImportTranslationRow != null)
			{

				debtHolderImportTranslationRow.AcquireReaderLock(transaction);
				debtHolderImportTranslationId = debtHolderImportTranslationRow.DebtHolderImportTranslationId;
				debtHolderImportTranslationRowVersion = debtHolderImportTranslationRow.RowVersion;
				debtHolderImportTranslationRow.ReleaseReaderLock(transaction.TransactionId);

			}

			debtClassRow.AcquireReaderLock(transaction);
			blotterRow = debtClassRow.BlotterRow;
			debtRuleMapRows = debtClassRow.GetDebtRuleMapRows();
			debtClassRow.ReleaseReaderLock(transaction.TransactionId);

			blotterRow.AcquireReaderLock(transaction);
			entityRow = blotterRow.EntityRow;
			workingOrderRows = blotterRow.GetWorkingOrderRows();
			blotterRow.ReleaseLock(transaction.TransactionId);

			entityRow.AcquireReaderLock(transaction);
			children = entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId();
			entityId = entityRow.EntityId;
			entityRowVersion = entityRow.RowVersion;
			entityRow.ReleaseReaderLock(transaction.TransactionId);

			// Fail if the debt class has any children.
			if (children.Length != 0)
				return ErrorCode.AccessDenied;

			// Destroy the import translation.
			if (debtHolderImportTranslationRow != null)
				dataModel.DestroyDebtHolderImportTranslation(new object[] { debtHolderImportTranslationId }, debtHolderImportTranslationRowVersion);

			// Delete any rules this debt class may own.
			foreach (DebtRuleMapRow debtRuleMapRow in debtRuleMapRows)
			{

				DebtRuleRow debtRuleRow;

				debtRuleMapRow.AcquireReaderLock(transaction);
				debtRuleRow = debtRuleMapRow.DebtRuleRow;
				debtRuleMapRow.ReleaseReaderLock(transaction.TransactionId);

				debtRulePersistence.DeleteRow(dataModel, transaction, debtRuleRow);

			}

			// Delete any working orders this debt class may contain.
			foreach (WorkingOrderRow workingOrderRow in workingOrderRows)
				workingOrderPersistence.DeleteRow(dataModel, transaction, workingOrderRow);

			// Delete the entity itself.
			dataModel.DestroyEntity(new object[] { entityId }, entityRowVersion);

			return ErrorCode.Success;

		}


		public override DebtHolder Get(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}

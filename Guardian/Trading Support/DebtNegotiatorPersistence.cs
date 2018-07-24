namespace FluidTrade.Guardian
{
	using System;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;
	using System.Data;


	/// <summary>
	/// 
	/// </summary>
	internal class DebtNegotiatorPersistence : DataModelPersistence<DebtNegotiator>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public DebtNegotiatorPersistence()
		{
		}
		/// <summary>
		/// Create a new DebtNegotiator
		/// </summary>
		/// <returns></returns>
		public override Guid Create(DebtNegotiator record)
		{

			DataModel dataModel = new DataModel();
			Guid entityId = Guid.Empty;
			TypeRow typeRow = null;
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			if (record.TypeId != null && record.TypeId != Guid.Empty)
				typeRow = DataModel.Type.TypeKey.Find(record.TypeId.Value);
			else
				typeRow = DataModel.Type.TypeKeyExternalId0.Find("DEBT NEGOTIATOR");

			if (typeRow != null)
			{

				DebtClassPersistence debtClassPersistence = new DebtClassPersistence();
				typeRow.AcquireReaderLock(dataModelTransaction);
				record.TypeId = typeRow.TypeId;
				typeRow.ReleaseLock(dataModelTransaction.TransactionId);

				entityId = debtClassPersistence.Create(record);

				dataModel.CreateDebtNegotiator(entityId);

			}

			return entityId;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override void Update(DebtNegotiator record)
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
		public override ErrorCode Delete(DebtNegotiator record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			WorkingOrderPersistence workingOrderPersistence = new WorkingOrderPersistence();
			DebtRulePersistence debtRulePersistence = new DebtRulePersistence();
			DebtNegotiatorRow debtNegotiatorRow = DataModel.DebtNegotiator.DebtNegotiatorKey.Find(record.RowId);
			// If/when we get these translations, we'll need this stuff.
			//DebtNegotiatorImportTranslationRow debtNegotiatorImportTranslationRow;
			DebtClassRow debtClassRow;
			BlotterRow blotterRow;
			WorkingOrderRow[] workingOrderRows;
			DebtRuleMapRow[] debtRuleMapRows;
			EntityRow entityRow;
			EntityTreeRow[] children;
			Guid entityId;
			Int64 entityRowVersion;
			//Guid debtNegotiatorImportTranslationId = Guid.Empty;
			//Int64 debtNegotiatorImportTranslationRowVersion = 0;

			if (record.RowId == null || debtNegotiatorRow == null)
				return ErrorCode.RecordNotFound;
			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, record.RowId, AccessRight.Write))
				return ErrorCode.AccessDenied;

			debtNegotiatorRow.AcquireReaderLock(transaction);
			debtClassRow = debtNegotiatorRow.DebtClassRow;
			//debtNegotiatorImportTranslationRow = debtNegotiatorRow.DebtNegotiatorImportTranslationRow;
			debtNegotiatorRow.ReleaseReaderLock(transaction.TransactionId);

#if false   // If/when we get these translations, we'll need this stuff.
			if (debtHolderImportTranslationRow != null)
			{

				debtNegotiatorImportTranslationRow.AcquireReaderLock(transaction);
				debtNegotiatorImportTranslationId = debtNegotiatorImportTranslationRow.DebtNegotiatorImportTranslationId;
				debtNegotiatorImportTranslationRowVersion = debtNegotiatorImportTranslationRow.RowVersion;
				debtNegotiatorImportTranslationRow.ReleaseReaderLock(transaction.TransactionId);

			}
#endif

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
			//if (debtHolderImportTranslationRow != null)
			//	dataModel.DestroyDebtNegotiatorImportTranslation(new object[] { debtNegotiatorImportTranslationId }, debtNegotiatorImportTranslationRowVersion);

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


		public override DebtNegotiator Get(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}

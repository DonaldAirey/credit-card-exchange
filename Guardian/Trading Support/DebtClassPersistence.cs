namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	class DebtClassPersistence : DataModelPersistence<DebtClass>
	{

		public override DebtClass Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create most of a debt class. This should be kicked off by children of DebtClass.
		/// </summary>
		/// <param name="record">The record, complete with TypeId and ImageId.</param>
		/// <returns>The entityId of the new debt class.</returns>
		public override Guid Create(DebtClass record)
		{

			DataModel dataModel = new DataModel();
			TypeRow typeRow = DataModel.Type.TypeKey.Find(record.TypeId.Value);
			EntityRow parentEntity;
			AssetViewerTemplateRow[] templates;
			Guid entityId = Guid.Empty;
			Guid organizationId = Guid.Empty;
			DateTime createdTime = DateTime.UtcNow;

			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			Guid ruleId = Guid.NewGuid();
			entityId = Guid.NewGuid();

			if (record.ParentId == null)
				throw new FaultException<ArgumentFault>(new ArgumentFault("Parent EntityId not specified"), "Parent EntityId not specified");
			if (!DataModelFilters.HasAccess(dataModelTransaction, TradingSupport.UserId, record.ParentId.Value, AccessRight.Write))
				throw new FaultException<SecurityFault>(
					new SecurityFault("The current user doesn't have write permission to the specified parent"),
					"The current user doesn't have write permission to the specified parent");

			parentEntity = DataModel.Entity.EntityKey.Find(record.ParentId.Value);

			typeRow.AcquireReaderLock(dataModelTransaction);
			templates = typeRow.GetAssetViewerTemplateRows();
			if (record.ImageId == null && !typeRow.IsImageIdNull())
				record.ImageId = typeRow.ImageId;
			typeRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

			if (record.ImageId == null)
			{

				ImageRow imageRow = DataModel.Image[0];
				imageRow.AcquireReaderLock(dataModelTransaction);
				record.ImageId = imageRow.ImageId;
				imageRow.ReleaseReaderLock(dataModelTransaction.TransactionId);

			}

			parentEntity.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
			try
			{
				record.Name = this.GetUniqueName(dataModelTransaction, parentEntity, record.Name as String);
			}
			finally
			{
				parentEntity.ReleaseReaderLock(dataModelTransaction.TransactionId);
			}


			dataModel.CreateEntity(
				createdTime,
				record.Description,
				entityId,
				Guid.NewGuid().ToString(),
				null,
				null,
				null,
				null,
				null,
				null,
				Guid.NewGuid().ToString(),
				record.ImageId.Value,
				false,
				false,
				createdTime, 
				record.Name,
				record.TenantId.GetValueOrDefault(),
				record.TypeId.Value);

			dataModel.CreateEntityTree(entityId, Guid.NewGuid(), null, record.ParentId.Value);

			dataModel.CreateBlotter(entityId, PartyTypeMap.FromCode(PartyType.UseParent));

			foreach (AssetViewerTemplateRow template in templates)
			{
			
				template.AcquireReaderLock(dataModelTransaction);
				
				dataModel.CreateBlotterConfiguration(Guid.NewGuid(), entityId, null, template.ReportId, template.ReportTypeId);

				template.ReleaseReaderLock(dataModelTransaction.TransactionId);
	
			}

			dataModel.CreateDebtClass(
				record.Address1,
				record.Address2,
				record.BankAccountNumber,
				record.BankRoutingNumber,
				record.City,
				null,
				record.CompanyName,
				record.ContactName,
				entityId,
				record.DebtRuleId,
				record.Department,
				record.Email,
				record.Fax,
				record.ForBenefitOf,
				record.Phone,
				record.PostalCode,
				record.Province,
				null);

			organizationId = this.FindOrganization(dataModelTransaction);
			Guid tenantId = record.TenantId.HasValue ? record.TenantId.GetValueOrDefault() : organizationId;

			PersistenceHelper.AddGroupPermissions(dataModel, dataModelTransaction, organizationId, entityId, tenantId);
			dataModel.CreateAccessControl(
				Guid.NewGuid(), 
				AccessRightMap.FromCode(AccessRight.FullControl), 
				entityId, 
				TradingSupport.UserId,
				tenantId);

			return entityId;

		}

		/// <summary>
		/// Find the organization an entity belongs to.
		/// </summary>
		/// <param name="transaction">The current transaction.</param>
		/// <returns>The organization's entity Id.</returns>
		private Guid FindOrganization(DataModelTransaction transaction)
		{

			Guid userId = TradingSupport.UserId;
			RightsHolderRow rightsHolderRow = DataModel.RightsHolder.RightsHolderKey.Find(userId);
			Guid organization;

			if (rightsHolderRow == null)
				throw new FaultException<OptimisticConcurrencyFault>(
							new OptimisticConcurrencyFault("RightsHolder", new object[] { userId }),
							"The current user has been deleted");

			rightsHolderRow.AcquireReaderLock(transaction);
			organization = rightsHolderRow.TenantId;
			rightsHolderRow.ReleaseLock(transaction.TransactionId);

			return organization;

		}

		/// <summary>
		/// Calculate a unique entity name from a base name.
		/// </summary>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="parentEntityRow">The (writelocked) entity that would be the parent of the entity we intend to name.</param>
		/// <param name="name">The base name (eg. the name of the entity, provided there isn't already a sibling with that name).</param>
		/// <returns>A unique name.</returns>
		private String GetUniqueName(DataModelTransaction transaction, EntityRow parentEntityRow, String name)
		{

			Int64 nameNumber = 1;
			String uniqueName = name;
			List<String> existingNames = new List<String>();

			foreach (EntityTreeRow entityTree in parentEntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
			{

				EntityRow entityRow;

				entityTree.AcquireReaderLock(transaction);
				entityRow = entityTree.EntityRowByFK_Entity_EntityTree_ChildId;
				entityTree.ReleaseLock(transaction.TransactionId);

				entityRow.AcquireReaderLock(transaction);
				existingNames.Add(entityRow.Name);
				entityRow.ReleaseLock(transaction.TransactionId);

			}

			existingNames.Sort();

			while (existingNames.BinarySearch(uniqueName) >= 0)
			{

				uniqueName = String.Format("{0} ({1})", name, nameNumber);
				nameNumber += 1;

			}

			return uniqueName;

		}

		/// <summary>
		/// Determine whether this debt class has a parent of the same type.
		/// </summary>
		/// <param name="transaction">The curren transaction.</param>
		/// <param name="entityRow">The entity row of the debt class.</param>
		/// <returns></returns>
		public Boolean HasParent(DataModelTransaction transaction, EntityRow entityRow)
		{

			Boolean has = false;
			EntityTreeRow[] entityTreeRows;
			Guid typeId;

			entityRow.AcquireReaderLock(transaction);
			typeId = entityRow.TypeId;
			entityTreeRows = entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId();
			entityRow.ReleaseLock(transaction.TransactionId);

			foreach (EntityTreeRow entityTreeRow in entityTreeRows)
			{

				EntityRow parentRow;

				entityTreeRow.AcquireReaderLock(transaction);
				parentRow = entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId;
				entityTreeRow.ReleaseLock(transaction.TransactionId);

				parentRow.AcquireReaderLock(transaction);
				has = parentRow.TypeId == typeId;
				parentRow.ReleaseLock(transaction.TransactionId);

				if (has)
					break;

			}

			return has;

		}

		/// <summary>
		/// Update the debt class portion of a record.
		/// </summary>
		/// <param name="record"></param>
		public override void Update(DebtClass record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			DateTime modifiedTime = DateTime.UtcNow;
			CommissionScheduleRow oldCommissionScheduleRow;
			DebtClassRow debtClassRow = DataModel.DebtClass.DebtClassKey.Find(record.RowId);
			EntityRow entityRow;
			CommissionTrancheRow[] tranches = null;
			Guid oldCommissionScheduleId;
			Int64 oldCommissionScheduleRowVersion;
			Int64 debtClassRowVersion;
			Int64 entityRowVersion;
			bool newSchedule = false;

			debtClassRow.AcquireReaderLock(transaction);
			debtClassRowVersion = debtClassRow.RowVersion;
			entityRow = DataModel.Entity.EntityKey.Find(record.RowId);
			oldCommissionScheduleRow = debtClassRow.CommissionScheduleRow;
			if (record.CommissionSchedule != null &&
				(oldCommissionScheduleRow == null || debtClassRow.CommissionScheduleId != record.CommissionSchedule.RowId))
				newSchedule = true;
			if (record.DebtRuleId == null && !this.HasParent(transaction, entityRow))
				throw new FaultException<FieldRequiredFault>(
					new FieldRequiredFault("Top-level debt classes must have a debt rule."),
					"Top-level debt classes must have a debt rule.");
			debtClassRow.ReleaseLock(transaction.TransactionId);

			entityRow.AcquireReaderLock(transaction);
			entityRowVersion = entityRow.RowVersion;
			entityRow.ReleaseLock(transaction.TransactionId);

			if (record.CommissionSchedule != null)
			{

				oldCommissionScheduleRow.AcquireReaderLock(transaction);
				oldCommissionScheduleId = oldCommissionScheduleRow.CommissionScheduleId;
				oldCommissionScheduleRowVersion = oldCommissionScheduleRow.RowVersion;
				tranches = oldCommissionScheduleRow.GetCommissionTrancheRows();
				oldCommissionScheduleRow.ReleaseLock(transaction.TransactionId);

			}


			if (newSchedule)
			{

				if (oldCommissionScheduleRow != null)
					dataModel.DestroyCommissionSchedule(
						new object[] { oldCommissionScheduleRow.CommissionScheduleId },
						oldCommissionScheduleRow.RowVersion);

				record.CommissionSchedule.RowId = Guid.NewGuid();

				dataModel.CreateCommissionSchedule(record.CommissionSchedule.RowId, null, record.CommissionSchedule.Name);

				foreach (CommissionTranche tranche in record.CommissionSchedule.CommissionTranches)
					dataModel.CreateCommissionTranche(
						record.CommissionSchedule.RowId,
						Guid.NewGuid(),
						tranche.CommissionType,
						tranche.CommissionUnit,
						tranche.EndRange,
						null,
						tranche.StartRange,
						tranche.Value);

			}
			else if (record.CommissionSchedule != null)
			{

				foreach (CommissionTrancheRow tranche in tranches)
				{

					Guid trancheId;
					Int64 rowVersion;

					tranche.AcquireReaderLock(transaction);
					trancheId = tranche.CommissionTrancheId;
					rowVersion = tranche.RowVersion;
					tranche.ReleaseLock(transaction.TransactionId);

					if (record.CommissionSchedule.CommissionTranches.FirstOrDefault(t => t.RowId == trancheId) == null)
						dataModel.DestroyCommissionTranche(new object[] { trancheId }, rowVersion);

				}

				foreach (CommissionTranche tranche in record.CommissionSchedule.CommissionTranches)
					if (DataModel.CommissionTranche.CommissionTrancheKey.Find(tranche.RowId) == null)
						dataModel.CreateCommissionTranche(
							record.CommissionSchedule.RowId,
							Guid.NewGuid(),
							tranche.CommissionType,
							tranche.CommissionUnit,
							tranche.EndRange,
							null,
							tranche.StartRange,
							tranche.Value);
					else
						dataModel.UpdateCommissionTranche(
							record.CommissionSchedule.RowId,
							tranche.RowId,
							new object[] { tranche.RowId },
							tranche.CommissionType,
							tranche.CommissionUnit,
							tranche.EndRange != null ? (object)tranche.EndRange.Value : DBNull.Value,
							null,
							tranche.RowVersion,
							tranche.StartRange,
							tranche.Value);

				dataModel.UpdateCommissionSchedule(
					record.CommissionSchedule.RowId,
					new object[] { record.CommissionSchedule.RowId },
					null,
					record.CommissionSchedule.Name,
					record.CommissionSchedule.RowVersion);

			}

			dataModel.UpdateDebtClass(
				record.Address1,
				record.Address2 != null ? (object)record.Address2 : DBNull.Value,
				record.BankAccountNumber != null ? (object)record.BankAccountNumber : DBNull.Value,
				record.BankRoutingNumber != null ? (object)record.BankRoutingNumber : DBNull.Value,
				record.City,
				record.CommissionSchedule != null ? (object)record.CommissionSchedule.RowId : DBNull.Value,
				record.CompanyName != null ? (object)record.CompanyName : DBNull.Value,
				record.ContactName != null ? (object)record.ContactName : DBNull.Value,
				record.RowId,
				new object[] { record.RowId },
				record.DebtRuleId != null ? record.DebtRuleId : DBNull.Value,
				record.Department != null ? record.Department : DBNull.Value,
				record.Email != null ? record.Email : DBNull.Value,
				record.Fax != null ? record.Fax : DBNull.Value,
				record.ForBenefitOf != null ? record.ForBenefitOf : DBNull.Value,
				record.Phone != null ? record.Phone : DBNull.Value,
				record.PostalCode,
				record.Province,
				debtClassRowVersion,
				record.SettlementTemplate != null ? record.SettlementTemplate : DBNull.Value);

			dataModel.UpdateEntity(
				null,
				null,
				record.RowId,
				new object[] { record.RowId },
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
				null,
				entityRowVersion,
				null,
				null);

		}

		public override ErrorCode Delete(DebtClass record)
		{
			throw new NotImplementedException();
		}
	}
}

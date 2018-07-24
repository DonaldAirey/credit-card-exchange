namespace FluidTrade.Guardian
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;


	/// <summary>
	/// Entity maintaince methods.
	/// </summary>
	internal class EntityPersistence : DataModelPersistence<Entity>
	{
		/// <summary>
		/// Create a new DebtHolder
		/// </summary>
		/// <returns></returns>
		public override Guid Create(Entity record)
		{
			DataModel dataModel = new DataModel();
			Guid entityId = Guid.NewGuid();
			DateTime createdTime = DateTime.UtcNow;

			if (record.ExternalId7 == null)
				record.ExternalId7 = Guid.NewGuid().ToString();

			dataModel.CreateEntity(
				createdTime,
				record.Description,
				entityId,
				record.ExternalId0,
				record.ExternalId1,
				record.ExternalId2,
				record.ExternalId3,
				record.ExternalId4,
				record.ExternalId5,
				record.ExternalId6,
				record.ExternalId7,
				record.ImageId.GetValueOrDefault(),
				record.IsHidden,
				record.IsReadOnly,
				createdTime,
				record.Name,
				record.TenantId.GetValueOrDefault(),
				record.TypeId.GetValueOrDefault());


			return entityId;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override void Update(Entity record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			DateTime modifiedTime = DateTime.UtcNow;
			EntityRow entityRow = DataModel.Entity.EntityKey.Find(record.RowId);
			Dictionary<String, EntityRow> parents = new Dictionary<String, EntityRow>();
			List<String> parentNames = new List<String>();
			EntityTreeRow[] entityTreeRows;
			String oldName = null;
			Int64 rowVersion;

			if (!TradingSupport.HasAccess(transaction, record.RowId, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the selected Entity");
			if (record.Name != null && !EntityPersistence.IsNameValid(record.Name as String))
				throw new FaultException<ArgumentFault>(new ArgumentFault("Names cannot contain a backslash"), "Names cannot contain a backslash");

			entityRow.AcquireReaderLock(transaction);
			entityTreeRows = entityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId();
			oldName = entityRow.Name;
			rowVersion = entityRow.RowVersion;
			entityRow.ReleaseLock(transaction.TransactionId);

			if (!oldName.Equals(record.Name))
			{

				foreach (EntityTreeRow entityTreeRow in entityTreeRows)
				{

					EntityRow parentEntityRow;

					entityTreeRow.AcquireReaderLock(transaction);
					parentEntityRow = entityTreeRow.EntityRowByFK_Entity_EntityTree_ParentId;
					entityTreeRow.ReleaseLock(transaction.TransactionId);

					parentEntityRow.AcquireReaderLock(transaction);
					parents.Add(parentEntityRow.Name, parentEntityRow);
					parentEntityRow.ReleaseLock(transaction.TransactionId);

				}

				parentNames = parents.Keys.ToList();

				parentNames.Sort();

				foreach (String parentName in parentNames)
					parents[parentName].AcquireWriterLock(transaction);

				foreach (EntityRow parentEntityRow in parents.Values)
					if (!EntityPersistence.IsNameUnique(transaction, parentEntityRow, record.Name as String))
						throw new FaultException<RecordExistsFault>(
							new RecordExistsFault("Entity", new object[] { record.Name }),
							"An entity with this name already exists");

			}

			dataModel.UpdateEntity(
				null,
				record.Description,
				null,
				new object[] { record.RowId },
				record.ExternalId0,
				record.ExternalId1,
				record.ExternalId2,
				record.ExternalId3,
				record.ExternalId4,
				record.ExternalId5,
				record.ExternalId6,
				record.ExternalId7,
				record.ImageId,
				record.IsHidden,
				record.IsReadOnly,
				modifiedTime,
				record.Name,
				rowVersion,
				record.TenantId,
				null);

		}

		/// <summary>
		/// Delete a debt holder
		/// </summary>
		/// <returns>True for sucess</returns>
		public override ErrorCode Delete(Entity record)
		{
			DataModel dataModel = new DataModel();
			DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

			if (!TradingSupport.HasAccess(dataModelTransaction, record.RowId, AccessRight.FullControl))
				throw new SecurityException("Current user does not have write access to the selected Entity");

			if (record.RowId == null || DataModel.Entity.EntityKey.Find(record.RowId) == null)
			{
				return ErrorCode.RecordNotFound;
			}

			dataModel.DestroyEntity(
					new object[] { record.RowId },
					record.RowVersion);

			return ErrorCode.Success;
		}


		public override Entity Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Determine whether a name is unique within a parent entity.
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="parentEntityRow"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Boolean IsNameUnique(DataModelTransaction transaction, EntityRow parentEntityRow, String name)
		{

			Boolean isNameUnique = true;

			foreach (EntityTreeRow entityTree in parentEntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId())
			{

				EntityRow entityRow;
				String oldName;

				entityTree.AcquireReaderLock(transaction);
				entityRow = entityTree.EntityRowByFK_Entity_EntityTree_ChildId;
				entityTree.ReleaseLock(transaction.TransactionId);

				entityRow.AcquireReaderLock(transaction);
				oldName = entityRow.Name;
				isNameUnique = !entityRow.Name.Equals(name);
				entityRow.ReleaseLock(transaction.TransactionId);

				if (!isNameUnique)
					break;

			}

			return isNameUnique;

		}

		/// <summary>
		/// Determine whether a name is a valid entity name.
		/// </summary>
		/// <param name="name">The proposed name.</param>
		/// <returns>True if the name is acceptable.</returns>
		public static bool IsNameValid(String name)
		{

			return name.IndexOfAny(new Char[] { '\\', '/' }) == -1 && !String.IsNullOrEmpty(name.Trim());

		}
	}
}

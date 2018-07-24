namespace FluidTrade.Guardian
{

	using System;
	using System.Security;
	using System.Linq;
	using System.ServiceModel;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// Entity relationship maintainance.
	/// </summary>
	internal class EntityTreePersistence : DataModelPersistence<EntityTree>
	{

		/// <summary>
		/// Create a new relationship.
		/// </summary>
		/// <param name="record">The record describing the relationship.</param>
		/// <returns>The entity tree id of the new relationship.</returns>
		public override Guid Create(EntityTree record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			EntityRow child = DataModel.Entity.EntityKey.Find(record.ChildId);
			EntityRow parent = DataModel.Entity.EntityKey.Find(record.ParentId);
			String childName;
			Guid entityTreeId;
			
			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, record.ParentId, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the parent entity");

			child.AcquireReaderLock(transaction);
			try
			{
				childName = child.Name;
			}
			finally
			{
				child.ReleaseLock(transaction.TransactionId);
			}

			//Sanity Checks
			if(record.ChildId == record.ParentId)
				throw new FaultException<ArgumentFault>( new ArgumentFault("Create EntityTree"),
					new FaultReason(String.Format("Cannot add {0} as a child of this element because it wil create a circular relationship", childName)));
			
	
			if(IsParentEntity(transaction, parent, record.ChildId) == true)
				throw new FaultException<ArgumentFault>(new ArgumentFault("Create EntityTree"),
						new FaultReason(String.Format("Cannot add {0} as a child of this element because it wil create a circular relationship", childName)));

			parent.AcquireWriterLock(transaction);
			if (!EntityPersistence.IsNameUnique(transaction, parent, childName))
				throw new FaultException<RecordExistsFault>(
					new RecordExistsFault("Entity", new object[] { childName }),
					"An entity with this name already exists");

			entityTreeId = Guid.NewGuid();

			dataModel.CreateEntityTree(record.ChildId, entityTreeId, null, record.ParentId);

			return entityTreeId;

		}
		
		/// <summary>
		/// See if the ChildId is a ParentId to stop circular reference.
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="parent"></param>
		/// <param name="childId"></param>
		/// <returns></returns>
		private  bool IsParentEntity(DataModelTransaction transaction, EntityRow parent, Guid childId)
		{
			parent.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);
			try
			{
				//Make sure we are not adding a child element that is also a parent.
				foreach (EntityTreeRow entityRow in parent.GetEntityTreeRowsByFK_Entity_EntityTree_ChildId())
				{
					entityRow.AcquireWriterLock(transaction.TransactionId, DataModel.LockTimeout);
					try
					{
						if (entityRow.ParentId == childId)
							return true;

						if (IsParentEntity(transaction, entityRow.EntityRowByFK_Entity_EntityTree_ParentId, childId) == true)
							return true;

					}
					finally
					{
						entityRow.ReleaseLock(transaction.TransactionId);
					}
				}
			}
			finally
			{
				parent.ReleaseReaderLock(transaction.TransactionId);
			}
			return false;
		}

		/// <summary>
		/// Fill out and return a entity tree record for a given id.
		/// </summary>
		/// <param name="id">The entity tree id.</param>
		/// <returns>The populated record.</returns>
		public override EntityTree Get(Guid id)
		{

			DataModelTransaction transaction = DataModelTransaction.Current;
			EntityTree record = new EntityTree();
			EntityTreeRow entityTreeRow = DataModel.EntityTree.EntityTreeKey.Find(id);

			if (entityTreeRow == null)
				throw new FaultException<RecordNotFoundFault>(new RecordNotFoundFault("EntityTree", new object[] { id }));

			entityTreeRow.AcquireReaderLock(transaction);
			record.RowId = entityTreeRow.EntityTreeId;
			record.RowVersion = entityTreeRow.RowVersion;
			record.ChildId = entityTreeRow.ChildId;
			record.ParentId = entityTreeRow.ParentId;
			entityTreeRow.ReleaseLock(transaction.TransactionId);

			return record;

		}

		/// <summary>
		/// Placeholder
		/// </summary>
		/// <param name="record"></param>
		public override void Update(EntityTree record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			EntityTreeRow entityTreeRow = DataModel.EntityTree.EntityTreeKey.Find(record.RowId);
			EntityRow child = DataModel.Entity.EntityKey.Find(record.ChildId);
			EntityRow parent = DataModel.Entity.EntityKey.Find(record.ParentId);
			String childName;

			entityTreeRow.AcquireWriterLock(transaction);

			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, entityTreeRow.ParentId, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the old parent entity");

			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, record.ParentId, AccessRight.Write))
				throw new SecurityException("Current user does not have write access to the new parent entity");

			parent.AcquireWriterLock(transaction);
			child.AcquireReaderLock(transaction);
			childName = child.Name;
			child.ReleaseLock(transaction.TransactionId);

			if (record.ChildId == record.ParentId)
				throw new FaultException<ArgumentFault>(new ArgumentFault("Create EntityTree"),
					new FaultReason(String.Format("Cannot add {0} as a child of this element because it wil create a circular relationship", childName)));


			if (IsParentEntity(transaction, parent, record.ChildId) == true)
				throw new FaultException<ArgumentFault>(new ArgumentFault("Create EntityTree"),
						new FaultReason(String.Format("Cannot add {0} as a child of this element because it wil create a circular relationship", childName)));

			if (!EntityPersistence.IsNameUnique(transaction, parent, childName))
				throw new FaultException<RecordExistsFault>(
					new RecordExistsFault("Entity", new object[] { childName }),
					"An entity with this name already exists");

			dataModel.UpdateEntityTree(
				record.ChildId,
				entityTreeRow.EntityTreeId,
				new object[] { entityTreeRow.EntityTreeId },
				null,
				record.ParentId,
				entityTreeRow.RowVersion);

		}

		/// <summary>
		/// Remove an entity tree link between to entities.
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		public override FluidTrade.Core.ErrorCode Delete(EntityTree record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			EntityTreeRow entityTreeRow = DataModel.EntityTree.EntityTreeKey.Find(record.RowId);
			Guid parentEntityId;

			if (entityTreeRow == null)
				return ErrorCode.RecordNotFound;

			entityTreeRow.AcquireWriterLock(transaction);
			parentEntityId = entityTreeRow.ParentId;

			if (!DataModelFilters.HasAccess(transaction, TradingSupport.UserId, parentEntityId, AccessRight.Write))
				return ErrorCode.AccessDenied;

			dataModel.DestroyEntityTree(new object[] { record.RowId }, record.RowVersion);

			return ErrorCode.Success;

		}

	}

}

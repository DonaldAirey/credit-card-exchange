namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Guardian.Records;

	/// <summary>
	/// Group persistence methods.
	/// </summary>
	public class GroupPersistence : DataModelPersistence<Group>
	{

		/// <summary>
		/// Create a new group.
		/// </summary>
		/// <param name="record">The group object.</param>
		/// <returns>The GroupId of the new group.</returns>
		public override Guid Create(Group record)
		{

			DataModel dataModel = new DataModel();
			DataModelTransaction transaction = DataModelTransaction.Current;
			EntityPersistence entityPersistence = new EntityPersistence();
			Guid entityId;
			TypeRow typeRow = DataModel.Type.TypeKeyExternalId0.Find("GROUP");

			typeRow.AcquireReaderLock(transaction.TransactionId, DataModel.LockTimeout);

			try
			{

				record.TypeId = typeRow.TypeId;
				record.ImageId = typeRow.ImageId;

			}
			finally
			{

				typeRow.ReleaseReaderLock(transaction.TransactionId);

			}

			entityId = entityPersistence.Create(record);

			dataModel.CreateRightsHolder(
				entityId,
				record.TenantId.Value);
			dataModel.CreateGroup(
				entityId,
				GroupTypeMap.FromCode(record.GroupType),
				record.TenantId.Value);

			return entityId;

		}

		/// <summary>
		/// Retrieve a group object.
		/// </summary>
		/// <param name="id">The GroupId.</param>
		/// <returns>The group object.</returns>
		public override Group Get(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Update a group.
		/// </summary>
		/// <param name="record">The group object.</param>
		public override void Update(Group record)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Delete a group.
		/// </summary>
		/// <param name="record">The group object.</param>
		/// <returns>Any resulting error.</returns>
		public override FluidTrade.Core.ErrorCode Delete(Group record)
		{
			throw new NotImplementedException();
		}

	}

}

namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using FluidTrade.Core;
	using FluidTrade.Guardian.Records;
	using System.ServiceModel;

	/// <summary>
	/// Static methods for manipulating the ACLs.
	/// </summary>
	public class AccessControlHelper
	{

		/// <summary>
		/// Grant a particular rights holder specific rights to an entity.
		/// </summary>
		/// <param name="rightsHolderId">The rights holder's id.</param>
		/// <param name="entityId">The entity's id.</param>
		/// <param name="rightId">The specific right's id.</param>
		/// <returns>The id of the AccessControl row.</returns>
		internal static Guid GrantAccess(Guid rightsHolderId, Guid entityId, Guid rightId)
		{

			DataModelTransaction transaction = DataModelTransaction.Current;
			DataModel dataModel = new DataModel();
			Guid currentUserId = TradingSupport.UserId;
			UserRow currentUserRow = DataModel.User.UserKey.Find(currentUserId);
			RightsHolderRow rightsHolderRow = DataModel.RightsHolder.RightsHolderKey.Find(rightsHolderId);
			Guid rightsHolderTenantId;
			AccessControlRow accessControlRow = DataModel.AccessControl.AccessControlKeyRightsHolderIdEntityId.Find(rightsHolderId, entityId);
			Guid accessControlId = Guid.Empty;

			// Determine whether current user has write access to the entity.
			if (!DataModelFilters.HasAccess(transaction, currentUserId, entityId, AccessRight.Write))
				throw new FaultException<SecurityFault>(
					new SecurityFault(String.Format("{0} does not write permission to {1}", rightsHolderId, entityId)));

			rightsHolderRow.AcquireReaderLock(transaction);
			rightsHolderTenantId = rightsHolderRow.TenantId;
			rightsHolderRow.ReleaseReaderLock(transaction.TransactionId);

			// Determine whether current user's tenant is upstream from rights holder we're modifying.
			if (!DataModelFilters.HasTenantAccess(transaction, currentUserId, rightsHolderTenantId))
				throw new FaultException<SecurityFault>(
					new SecurityFault(String.Format("{0} does not control over tenant {1}", rightsHolderId, rightsHolderTenantId)));

			if (accessControlRow != null)
			{

				accessControlRow.AcquireWriterLock(transaction);
				accessControlId = accessControlRow.AccessControlId;
				dataModel.UpdateAccessControl(
					accessControlRow.AccessControlId,
					new object[] { accessControlRow.AccessControlId },
					rightId,
					entityId,
					rightsHolderId,
					accessControlRow.RowVersion,
					rightsHolderTenantId);

			}
			else
			{

				accessControlId = Guid.NewGuid();
				dataModel.CreateAccessControl(
					accessControlId,
					rightId,
					entityId,
					rightsHolderId,
					rightsHolderTenantId);

			}

			return accessControlId;

		}

		/// <summary>
		/// Revoke any and all access a rights holder has to an entity.
		/// </summary>
		/// <param name="rightsHolderId">The rights holder's id.</param>
		/// <param name="entityId">The entity's id.</param>
		/// <returns>The error code.</returns>
		internal static ErrorCode RevokeAccess(Guid rightsHolderId, Guid entityId)
		{

			DataModelTransaction transaction = DataModelTransaction.Current;
			DataModel dataModel = new DataModel();
			Guid currentUserId = TradingSupport.UserId;
			UserRow currentUserRow = DataModel.User.UserKey.Find(currentUserId);
			RightsHolderRow rightsHolderRow = DataModel.RightsHolder.RightsHolderKey.Find(rightsHolderId);
			Guid rightsHolderTenantId;
			AccessControlRow accessControlRow = DataModel.AccessControl.AccessControlKeyRightsHolderIdEntityId.Find(rightsHolderId, entityId);

			// Determine whether current user has write access to the entity.
			if (!DataModelFilters.HasAccess(transaction, currentUserId, entityId, AccessRight.Write))
				return ErrorCode.AccessDenied;

			rightsHolderRow.AcquireReaderLock(transaction);
			rightsHolderTenantId = rightsHolderRow.TenantId;
			rightsHolderRow.ReleaseReaderLock(transaction.TransactionId);

			// Determine whether current user's tenant is upstream from rights holder we're modifying.
			if (!DataModelFilters.HasTenantAccess(transaction, currentUserId, rightsHolderTenantId))
				return ErrorCode.AccessDenied;

			if (accessControlRow != null)
			{

				accessControlRow.AcquireWriterLock(transaction);
				dataModel.DestroyAccessControl(new object[] { accessControlRow.AccessControlId }, accessControlRow.RowVersion);

			}
			else
			{

				return ErrorCode.RecordNotFound;

			}

			return ErrorCode.Success;

		}

	}
}

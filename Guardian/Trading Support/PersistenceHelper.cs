using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Transactions;
using FluidTrade.Core;
using FluidTrade.Guardian.Records;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// Extension methods for persisting collections
	/// </summary>
	internal static class PersistenceHelper
	{

		internal static MethodResponse<Guid[]> Create<RecordType, PersistenceType>(this IEnumerable<RecordType> source)
			where RecordType : BaseRecord
			where PersistenceType : DataModelPersistence<RecordType>, new()
		{
			List<Guid> guids = new List<Guid>();
			MethodResponse<Guid[]> returnCodes = new MethodResponse<Guid[]>();
			PersistenceType datamodelPersistance = new PersistenceType();
			Int32 bulkIndex = 0;

			foreach (RecordType record in source)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						guids.Add(datamodelPersistance.Create(record));
						transactionScope.Complete();
					}

				}
				catch (Exception exception)
				{
					EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
					returnCodes.AddError(exception, bulkIndex);
				}

				bulkIndex += 1;

			}

			returnCodes.Result = guids.ToArray();
			return returnCodes;
		
		}

		internal static MethodResponse<ErrorCode> Update<RecordType, PersistenceType>(this IEnumerable<RecordType> source)
			where RecordType : BaseRecord
			where PersistenceType : DataModelPersistence<RecordType>, new()
		{
			MethodResponse<ErrorCode> returnCodes = new MethodResponse<ErrorCode>() { Result = ErrorCode.Success };
			PersistenceType datamodelPersistance = new PersistenceType();
			Int32 bulkIndex = 0;

			foreach (RecordType record in source)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						datamodelPersistance.Update(record);
						transactionScope.Complete();
					}
				}
				catch (Exception exception)
				{
					EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
					returnCodes.AddError(exception, bulkIndex);
				}

				bulkIndex += 1;

			}

			if (returnCodes.Errors.Length > 0)
				returnCodes.Result = ErrorCode.NoJoy;

			return returnCodes;
		}

		internal const Int32 deadlockRetiesMax = 3;

		/// <summary>
		/// Delete a set of records from the data model. Each record is deleted in order. The method returns when all the records are successfully
		/// deleted, or when an error occurs - whichever comes first.
		/// </summary>
		/// <typeparam name="RecordType">The specific type of the record.</typeparam>
		/// <typeparam name="PersistenceType">The type that handles deleting the record type.</typeparam>
		/// <param name="source">The record set to delete.</param>
		/// <returns>The errors that may have occurred attempted to delete the set.</returns>
		internal static MethodResponse<ErrorCode> Delete<RecordType, PersistenceType>(this IEnumerable<RecordType> source)
			where RecordType : BaseRecord
			where PersistenceType : DataModelPersistence<RecordType>, new()
		{
			MethodResponse<ErrorCode> returnCodes = new MethodResponse<ErrorCode>() { Result = ErrorCode.Success };
			PersistenceType datamodelPersistance = new PersistenceType();
			Int32 bulkIndex = 0;

			foreach (RecordType record in source)
			{
				try
				{

					Boolean finished = false;

					for (int deadlockRetry = 0; !finished && deadlockRetry < deadlockRetiesMax; deadlockRetry++)
					{
						try
						{
							using (TransactionScope transactionScope = new TransactionScope())
							{
								// This provides a context for any transactions.
								DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

								ErrorCode result = datamodelPersistance.Delete(record);

								if (result == ErrorCode.Success)
								{

									transactionScope.Complete();

								}
								else if (result != ErrorCode.RecordNotFound)
								{
									returnCodes.AddError(new ErrorInfo() { ErrorCode = result, BulkIndex = bulkIndex });
								}

								// Break out of the deadlock loop if the transaction completed expectedly (either failure or success).
								finished = true;
							}
						}
						catch (Exception ex)
						{
							if (FluidTrade.Core.Utilities.SqlErrorHelper.IsDeadlockException(ex))
							{
								if (deadlockRetry == deadlockRetiesMax - 1)
									throw;
								if (EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
									EventLog.Warning("Deadlock exception\r\n{0}: {1}", ex.Message, ex.StackTrace);
								System.Threading.Thread.Sleep(2000 * deadlockRetry + 1);
							}
							else
							{
								throw;
							}
						}
					}
				}
				catch (FaultException<RecordNotFoundFault> exception)
				{

					EventLog.Information("RecordNotFound ignored by delete operation: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

				}
				catch (Exception exception)
				{

					EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
					returnCodes.AddError(exception, bulkIndex);
					break;

				}

				if (returnCodes.HasErrors())
					break;

				bulkIndex += 1;
			}

			if (returnCodes.Errors.Length > 0)
				returnCodes.Result = ErrorCode.NoJoy;

			return returnCodes;
		}

		/// <summary>
		/// Add default permissions to an object.
		/// </summary>
		/// <param name="dataModel">The data model.</param>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="organizationId">The TenentId of the organization where the scan should start (eg. where the entity is).</param>
		/// <param name="entityId">The entityId of the object to modify.</param>
		///<param name="tenantId">The root TenantId for which all the records will be created for</param>
		public static void AddGroupPermissions(DataModel dataModel, DataModelTransaction transaction, Guid organizationId, Guid entityId, Guid tenantId)
		{

			TenantRow organizationRow = DataModel.Tenant.TenantKey.Find(organizationId);
			RightsHolderRow[] rightsHolders;
			TenantTreeRow[] tenantTreeRows;

			if (organizationRow == null)
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Organization", new object[] { organizationId }),
					"The organization has been deleted.");

			organizationRow.AcquireReaderLock(transaction);
			tenantTreeRows = organizationRow.GetTenantTreeRowsByFK_Tenant_TenantTree_ChildId();
			rightsHolders = organizationRow.GetRightsHolderRows();
			organizationRow.ReleaseLock(transaction.TransactionId);

			foreach (RightsHolderRow rightsHolderRow in rightsHolders)
			{

				GroupRow[] groupRows;

				rightsHolderRow.AcquireReaderLock(transaction);
				groupRows = rightsHolderRow.GetGroupRows();
				rightsHolderRow.ReleaseLock(transaction.TransactionId);

				if (groupRows.Length > 0)
				{

					Guid groupId;
					GroupRow group = groupRows[0];
					GroupTypeRow groupType;

					group.AcquireReaderLock(transaction);
					groupId = group.GroupId;
					groupType = group.GroupTypeRow;
					group.ReleaseReaderLock(transaction.TransactionId);

					groupType.AcquireReaderLock(transaction);
					if (groupType.GroupTypeCode == GroupType.ExchangeAdmin ||
						groupType.GroupTypeCode == GroupType.FluidTradeAdmin ||
						groupType.GroupTypeCode == GroupType.SiteAdmin)
						dataModel.CreateAccessControl(
							Guid.NewGuid(), 
							AccessRightMap.FromCode(AccessRight.FullControl), 
							entityId, 
							groupId,
							tenantId);

				}

			}

			foreach (TenantTreeRow tenantTreeRow in tenantTreeRows)
			{

				Guid parentId;

				tenantTreeRow.AcquireReaderLock(transaction);
				parentId = tenantTreeRow.ParentId;
				tenantTreeRow.ReleaseLock(transaction.TransactionId);

				AddGroupPermissions(dataModel, transaction, parentId, entityId, tenantId);

			}


		}

		/// <summary>
		/// Get the tenantId this entity is associated with.
		/// </summary>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="entityId">The entityId of the entity.</param>
		/// <returns>The tenantId of the tenant.</returns>
		public static Guid GetTenantForEntity(DataModelTransaction transaction, Guid entityId)
		{

			Guid tenantId;
			EntityRow entityRow = DataModel.Entity.EntityKey.Find(entityId);

			if (entityRow == null)
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Entity", new object[] { entityId }),
					"The entity has been deleted.");

			entityRow.AcquireReaderLock(transaction);
			tenantId = entityRow.TenantId;
			entityRow.ReleaseLock(transaction.TransactionId);

			return tenantId;

		}

		/// <summary>
		/// Get the securityId of the security this consumer is a part of.
		/// </summary>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="consumerId">The consumerId.</param>
		/// <returns>The securityId of the security.</returns>
		public static Guid GetBlotterForConsumer(DataModelTransaction transaction, Guid consumerId)
		{

			Guid blotterId = Guid.Empty;
			ConsumerRow consumerRow = DataModel.Consumer.ConsumerKey.Find(consumerId);
			ConsumerDebtRow[] consumerDebtRows;
			ConsumerTrustRow[] consumerTrustRows;
			SecurityRow securityRow;
			WorkingOrderRow[] workingOrderRows;

			if (consumerRow == null)
				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Consumer", new object[] { consumerId }),
					"The consumer has been deleted.");

			consumerRow.AcquireReaderLock(transaction);
			consumerDebtRows = consumerRow.GetConsumerDebtRows();
			consumerTrustRows = consumerRow.GetConsumerTrustRows();
			consumerRow.ReleaseLock(transaction.TransactionId);

			if (consumerDebtRows.Length > 0)
			{

				consumerDebtRows[0].AcquireReaderLock(transaction);
				securityRow = consumerDebtRows[0].SecurityRow;
				consumerDebtRows[0].ReleaseLock(transaction.TransactionId);

			}
			else if (consumerTrustRows.Length > 0)
			{

				consumerTrustRows[0].AcquireReaderLock(transaction);
				securityRow = consumerTrustRows[0].SecurityRow;
				consumerTrustRows[0].ReleaseLock(transaction.TransactionId);

			}
			else
			{

				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Consumer", new object[] { consumerId }),
					"This consumer record is an orphan - it is not related to any security.");

			}

			securityRow.AcquireReaderLock(transaction);
			workingOrderRows = securityRow.GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId();
			securityRow.ReleaseLock(transaction.TransactionId);

			if (workingOrderRows.Length > 0)
			{

				workingOrderRows[0].AcquireReaderLock(transaction);
				blotterId = workingOrderRows[0].BlotterId;
				workingOrderRows[0].ReleaseLock(transaction.TransactionId);

			}
			else
			{

				throw new FaultException<RecordNotFoundFault>(
					new RecordNotFoundFault("Consumer", new object[] { consumerId }),
					"This consumer record is an orphan - it is not related to any working order.");

			}

			return blotterId;

		}

	}

}

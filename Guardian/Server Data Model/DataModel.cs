using System.Diagnostics;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Transactions;
namespace FluidTrade.Guardian 
{
	/// <summary>
	/// hand coded impl for ConsumerDataTable
	/// 
	/// </summary>
	partial class ConsumerDataTable : FluidTrade.Core.Matching.IExtendedStorageTable
	{
		/// <summary>
		/// helper class that implemnts the IExtendedStorageTable
		/// </summary>
		FluidTrade.Core.Matching.ExtendedStorageTableHelper extendedStorageTableHelper = new FluidTrade.Core.Matching.ExtendedStorageTableHelper();
		
		/// <summary>
		/// get the next storage index for IExtendedStorage
		/// </summary>
		/// <returns></returns>
		public int GetNextExtendedStorageIndex()
		{
			return this.extendedStorageTableHelper.GetNextExtendedStorageIndex();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void AfterOnRowChanging(System.Data.DataRowChangeEventArgs e)
		{
			base.AfterOnRowChanging(e);
			if(e.Row.RowState == System.Data.DataRowState.Detached)
				return;

			//when the row changes clear the row storage.
			((FluidTrade.Core.Matching.IExtendedStorageRow)e.Row).ClearContents();
		}
	}
	partial class MatchDataTable
	{
		/// <summary>
		/// Determine whether there is a settlement (indirectly) associated with a match.
		/// </summary>
		/// <param name="transaction">The current transaction.</param>
		/// <param name="match">The match to check.</param>
		/// <param name="checkContra">If true, the contra match will also be checked.</param>
		/// <returns>True if the account is settled.</returns>
		public static bool IsSettled(DataModelTransaction transaction, MatchRow match, System.Boolean checkContra)
		{

			ConsumerDebtNegotiationRow[] consumerDebtNegotiationRows;
			ConsumerTrustNegotiationRow[] consumerTrustNegotiationRows;
			MatchRow contraMatch;

			if((match as System.Data.DataRow).RowState == System.Data.DataRowState.Added)
				return false;

			match.AcquireReaderLock(transaction);
			if(match.RowState == System.Data.DataRowState.Deleted || match.RowState == System.Data.DataRowState.Detached)
			{

				match.ReleaseReaderLock(transaction.TransactionId);
				return false;

			}
			consumerDebtNegotiationRows = match.GetConsumerDebtNegotiationRows();
			consumerTrustNegotiationRows = match.GetConsumerTrustNegotiationRows();
			contraMatch = DataModel.Match.MatchKey.Find(match.ContraMatchId);
			match.ReleaseReaderLock(transaction.TransactionId);

			if(contraMatch == null)
				return false;

			foreach(ConsumerDebtNegotiationRow row in consumerDebtNegotiationRows)
			{

				row.AcquireReaderLock(transaction);
				if(row.RowState == System.Data.DataRowState.Deleted || row.RowState == System.Data.DataRowState.Detached)
				{

					row.ReleaseReaderLock(transaction.TransactionId);
					continue;

				}

				int settlements = row.GetConsumerDebtSettlementRows().Length;
				row.ReleaseReaderLock(transaction.TransactionId);
				if(settlements != 0)
					return true;

			}

			foreach(ConsumerTrustNegotiationRow row in consumerTrustNegotiationRows)
			{

				row.AcquireReaderLock(transaction);
				if(row.RowState == System.Data.DataRowState.Deleted || row.RowState == System.Data.DataRowState.Detached)
				{

					row.ReleaseReaderLock(transaction.TransactionId);
					continue;

				}

				int settlements = row.GetConsumerTrustSettlementRows().Length;
				row.ReleaseReaderLock(transaction.TransactionId);
				if(settlements != 0)
					return true;

			}

			if(checkContra)
				return IsSettled(transaction, contraMatch, false);
			else
				return false;

		}


		/// <summary>
		/// 
		/// </summary>
		public static void CleanUnpairedMatches()
		{
			List<KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>>> singleMatchWorkingOrderIds = new List<KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>>>();
			try
			{
				foreach(MatchRow matchRow in DataModel.Match.Rows)
				{
					try
					{
						if(matchRow.RowState == System.Data.DataRowState.Deleted ||
							matchRow.RowState == System.Data.DataRowState.Detached)
							continue;

						Guid matchId = (Guid)matchRow[DataModel.Match.MatchIdColumn];
						Guid contraMatchId = (Guid)matchRow[DataModel.Match.ContraMatchIdColumn];
						MatchRow contraMatchRow = DataModel.Match.MatchKey.Find(contraMatchId);
						if(contraMatchRow == null ||
							contraMatchRow.RowState == System.Data.DataRowState.Deleted ||
							contraMatchRow.RowState == System.Data.DataRowState.Detached)
						{
							//make sure we get the matchid even if there is a problem with getting the workingorderIds

							KeyValuePair<Guid, Guid> orderPair;
							try
							{
								orderPair = new KeyValuePair<Guid, Guid>(
														(Guid)matchRow[DataModel.Match.WorkingOrderIdColumn],
														(Guid)matchRow[DataModel.Match.ContraOrderIdColumn]);
							}
							catch
							{
								orderPair = new KeyValuePair<Guid, Guid>();
							}

							singleMatchWorkingOrderIds.Add(new KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>>(matchRow, orderPair));
						}
					}
					catch
					{
					}
				}//end foreach

				//this point have collected the match rows to remove and the workingorder rows to rematch
				if(singleMatchWorkingOrderIds.Count > 0)
					FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new System.Threading.WaitCallback(CleanUnpairedMatchesProc), singleMatchWorkingOrderIds);
			}
			finally
			{
			}
		}

		/// <summary>
		/// request that all the working orders status is rematched
		/// </summary>
		public static void RematchAllWorkingOrders()
		{
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(new System.Threading.WaitCallback(RematchAllWorkingOrdersProc));
		}

		/// <summary>
		/// thread method that will mark all the working orders to rematch
		/// </summary>
		/// <param name="state"></param>
		private static void RematchAllWorkingOrdersProc(object state)
		{

			DataModel.DataLock.EnterReadLock();
			List<Guid> workingOrderIdList = new List<Guid>();
			try
			{
				foreach(WorkingOrderRow workingOrderRow in DataModel.WorkingOrder)
				{
					if(workingOrderRow.RowState == System.Data.DataRowState.Deleted ||
						workingOrderRow.RowState == System.Data.DataRowState.Detached)
						continue;

					workingOrderIdList.Add((Guid)workingOrderRow[DataModel.WorkingOrder.WorkingOrderIdColumn]);
				}
			}
			catch(Exception ex)
			{
				global::FluidTrade.Core.EventLog.Information("Error in RematchAllWorkingOrdersProc {0}\r\n{1}", ex.Message, ex.StackTrace);
						
				return;
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}

			DateTime utcModTime = DateTime.UtcNow.AddHours(12);
			foreach(Guid workingOrderId in workingOrderIdList)
			{
				try
				{
					UpdateOrderModifyTime(utcModTime, workingOrderId);
				}
				catch(Exception ex)
				{
					global::FluidTrade.Core.EventLog.Information("Error in RematchAllWorkingOrdersProc UpdateOrderModifyTime {0}\r\n{1}", ex.Message, ex.StackTrace);

					return;
				}
			}
		}

		private const int deadlockRetiesMax = 3;
		/// <summary>
		/// clean up the bad matches
		/// </summary>
		/// <param name="state"></param>
		private static void CleanUnpairedMatchesProc(object state)
		{
			List<KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>>> singleMatchWorkingOrderIds = (List<KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>>>)state;
			//first delete all the matches in their own txn
			foreach(KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>> pair in singleMatchWorkingOrderIds)
			{
				MatchRow matchRow = pair.Key;
				try
				{
					DeleteMatch(matchRow);
				}
				catch(Exception ex)
				{
					FluidTrade.Core.EventLog.Warning("DeleteMatch exception\r\n{0}: {1}\r\n", ex.Message, ex.ToString());
				}
			}

			//set to something in the future.. not being saved so does not really matter how
			//far but if matches run for a long time this is basiclly how long the process of update
			//will run because the check will no longer be > utcNow
			DateTime utcNowPlusHour = DateTime.UtcNow.AddHours(12);
					
			foreach(KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>> pair in singleMatchWorkingOrderIds)
			{
				Guid workingOrderId = pair.Value.Key;

				if(workingOrderId == Guid.Empty)
					continue;

				try
				{
					UpdateOrderModifyTime(utcNowPlusHour, workingOrderId);
				}
				catch(Exception ex)
				{
					FluidTrade.Core.EventLog.Warning("UpdateOrderModifyTime exception\r\n{0}: {1}\r\n", ex.Message, ex.ToString());
				}
			}

			foreach(KeyValuePair<MatchRow, KeyValuePair<Guid, Guid>> pair in singleMatchWorkingOrderIds)
			{
				Guid contraWorkingOrderId = pair.Value.Value;

				if(contraWorkingOrderId == Guid.Empty)
					continue;

				try
				{
					UpdateOrderModifyTime(utcNowPlusHour, contraWorkingOrderId);
				}
				catch(Exception ex)
				{
					FluidTrade.Core.EventLog.Warning("UpdateOrderModifyTime exception\r\n{0}: {1}\r\n", ex.Message, ex.ToString());
				}
			}
		}

		/// <summary>
		/// update the order modify time, but dont save it. this will be used as a message to
		/// re-process the order
		/// </summary>
		/// <param name="utcModTime"></param>
		/// <param name="workingOrderId"></param>
		private static void UpdateOrderModifyTime(DateTime utcModTime, Guid workingOrderId)
		{
			using(TransactionScope transactionScope = new TransactionScope())
			{
				// This provides a context for any transactions.
				DataModelTransaction dataModelTransaction = DataModelTransaction.Current;
				WorkingOrderRow workingOrderRow = DataModel.WorkingOrder.WorkingOrderKey.Find(workingOrderId);
				if(workingOrderRow == null)
					return;

				DataModel dataModel = new DataModel();
				workingOrderRow.AcquireWriterLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
				try
				{
					if(workingOrderRow.RowState == System.Data.DataRowState.Deleted ||
						workingOrderRow.RowState == System.Data.DataRowState.Detached)
						return;

					DataModel.DataLock.EnterWriteLock();
					try
					{
						workingOrderRow.BeginEdit();
						workingOrderRow[DataModel.WorkingOrder.ModifiedTimeColumn] = utcModTime;
					}
					finally
					{
						workingOrderRow.EndEdit();
						workingOrderRow.RejectChanges();
						DataModel.DataLock.ExitWriteLock();
					}
				}
				finally
				{
					workingOrderRow.ReleaseWriterLock(dataModelTransaction.TransactionId);
				}
			}
		}

		private static void DeleteMatch(MatchRow matchRow)
		{
			for(int deadlockRetry = 0; deadlockRetry < deadlockRetiesMax; deadlockRetry++)
			{
				try
				{
					using(TransactionScope transactionScope = new TransactionScope())
					{
						// This provides a context for any transactions.
						DataModelTransaction dataModelTransaction = DataModelTransaction.Current;

						long rowVersion;
						Guid matchId;
						matchRow.AcquireReaderLock(dataModelTransaction.TransactionId, DataModel.LockTimeout);
						try
						{
							if(matchRow.RowState == System.Data.DataRowState.Detached ||
								matchRow.RowState == System.Data.DataRowState.Deleted)
							continue;

							rowVersion = matchRow.RowVersion;
							matchId = matchRow.MatchId;
						}
						finally
						{
							matchRow.ReleaseReaderLock(dataModelTransaction.TransactionId);
						}
						DataModel dataModel = new DataModel();
						dataModel.DestroyMatch(new object[] { matchId }, rowVersion);

						transactionScope.Complete();
						return;
					}//end using
				}
				catch(System.Data.SqlClient.SqlException sqlEx)
				{
					if(FluidTrade.Core.Utilities.SqlErrorHelper.IsDeadlockException(sqlEx))
					{
						if(deadlockRetry == deadlockRetiesMax - 1)
							throw;

						FluidTrade.Core.EventLog.Warning("Deadlock exception\r\n{0}: {1}\r\n{2}", sqlEx.Message, sqlEx.ToString(), sqlEx.StackTrace);
					}
					else
					{
						throw;
					}
				}
			}//end deadlock retry
		}
	}

	/// <summary>
	/// hand coded impl for ConsumerDataRow
	/// 
	/// </summary>
	partial class ConsumerRow : FluidTrade.Core.Matching.IExtendedStorageRow
	{
		/// <summary>
		/// helper class that implemnts the IExtendedStorageRow
		/// </summary>
		FluidTrade.Core.Matching.ExtendedStorageRowHelper extendedStorageRowHelper = new FluidTrade.Core.Matching.ExtendedStorageRowHelper(4);

		/// <summary>
		/// set exteneded storage value by Index,  The index is the value
		/// that is returned from IExtendedStorageTable.GetNextExtendedStorageIndex()
		/// </summary>
		/// <param name="index">index of storage location in the IExtendedStorage. this should always be the 
		/// value that is returned by GetNextExtendedStorageIndex at init time</param>
		/// <param name="value">value to store in extended storage</param>
		public void SetExtendedStorage(int index, object value)
		{
			this.extendedStorageRowHelper.SetExtendedStorage(index, value);
		}

		/// <summary>
		/// get exteneded storage by Index,  The index is the value
		/// that is returned from IExtendedStorageTable.GetNextExtendedStorageIndex()
		/// </summary>
		/// <param name="index">index of storage location in the IExtendedStorage. this should always be the 
		/// value that is returned by GetNextExtendedStorageIndex at init time</param>
		/// <returns></returns>
		public object GetExtendedStorage(int index)
		{
			return this.extendedStorageRowHelper.GetExtendedStorage(index);
		}

		/// <summary>
		/// make the extended storage size bigger
		/// </summary>
		/// <param name="newSize"></param>
		public void GrowStorageSize(int newSize)
		{
			this.extendedStorageRowHelper.GrowStorageSize(newSize);
		}

		/// <summary>
		/// get the storage size
		/// </summary>
		public int StorageSize
		{
			get { return this.extendedStorageRowHelper.StorageSize; }
		}

		/// <summary>
		/// clear all the exteneded storage that is bound to the row
		/// </summary>
		public void ClearContents()
		{
			this.extendedStorageRowHelper.ClearContents();
		}

		/// <summary>
		/// Gets the children rows in the ConsumerTrust table. Without checking for locks
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ConsumerTrustRow[] GetConsumerTrustRows_NoLockCheck()
		{
			bool acquiredReader = false;
			if(DataModel.DataLock.IsWriteLockHeld == false &&
				DataModel.DataLock.IsReadLockHeld == false)
			{
				DataModel.DataLock.EnterReadLock();
				acquiredReader = true;
			}
			try
			{
				return ((ConsumerTrustRow[])(this.GetChildRows(this.tableConsumer.ConsumerConsumerTrustRelation)));	
			}
			finally
			{
				if(acquiredReader == true)
					DataModel.DataLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Gets the children rows in the ConsumerDebt table. Without checking for locks
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ConsumerDebtRow[] GetConsumerDebtRows_NoLockCheck()
		{
			bool acquiredReader = false;
			if(DataModel.DataLock.IsWriteLockHeld == false &&
				DataModel.DataLock.IsReadLockHeld == false)
			{
				DataModel.DataLock.EnterReadLock();
				acquiredReader = true;
			} 
			try
			{
				return ((ConsumerDebtRow[])(this.GetChildRows(this.tableConsumer.ConsumerConsumerDebtRelation)));
			}				
			finally
			{
				if(acquiredReader == true)
					DataModel.DataLock.ExitReadLock();
			} 
		}

		
		/// <summary>
		/// Gets the children rows in the CreditCard table. Without checking for locks
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public CreditCardRow[] GetCreditCardRows_NoLockCheck()
		{
			DataModel.DataLock.EnterReadLock();
			try
			{
				return ((CreditCardRow[])(this.GetChildRows(this.tableConsumer.ConsumerCreditCardRelation)));
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			} 
		}
	}


	/// <summary>
	/// hand coded impl for CreditCardDataTable
	/// 
	/// </summary>
	partial class CreditCardDataTable : FluidTrade.Core.Matching.IExtendedStorageTable
	{
		/// <summary>
		/// helper class that implemnts the IExtendedStorageTable
		/// </summary>
		FluidTrade.Core.Matching.ExtendedStorageTableHelper extendedStorageTableHelper = new FluidTrade.Core.Matching.ExtendedStorageTableHelper();

		/// <summary>
		/// get the next storage index for IExtendedStorage
		/// </summary>
		/// <returns></returns>
		public int GetNextExtendedStorageIndex()
		{
			return this.extendedStorageTableHelper.GetNextExtendedStorageIndex();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void AfterOnRowChanging(System.Data.DataRowChangeEventArgs e)
		{
			base.AfterOnRowChanging(e);
			if(e.Row.RowState == System.Data.DataRowState.Detached)
				return;

			//when the row changes clear the row storage.
			((FluidTrade.Core.Matching.IExtendedStorageRow)e.Row).ClearContents();
		}
	}

	/// <summary>
	/// hand coded impl for CreditCardDataRow
	/// 
	/// </summary>
	partial class CreditCardRow : FluidTrade.Core.Matching.IExtendedStorageRow
	{
		/// <summary>
		/// helper class that implemnts the IExtendedStorageRow
		/// </summary>
		FluidTrade.Core.Matching.ExtendedStorageRowHelper extendedStorageRowHelper = new FluidTrade.Core.Matching.ExtendedStorageRowHelper(4);

		/// <summary>
		/// set exteneded storage value by Index,  The index is the value
		/// that is returned from IExtendedStorageTable.GetNextExtendedStorageIndex()
		/// </summary>
		/// <param name="index">index of storage location in the IExtendedStorage. this should always be the 
		/// value that is returned by GetNextExtendedStorageIndex at init time</param>
		/// <param name="value">value to store in extended storage</param>
		public void SetExtendedStorage(int index, object value)
		{
			this.extendedStorageRowHelper.SetExtendedStorage(index, value);
		}

		/// <summary>
		/// get exteneded storage by Index,  The index is the value
		/// that is returned from IExtendedStorageTable.GetNextExtendedStorageIndex()
		/// </summary>
		/// <param name="index">index of storage location in the IExtendedStorage. this should always be the 
		/// value that is returned by GetNextExtendedStorageIndex at init time</param>
		/// <returns></returns>
		public object GetExtendedStorage(int index)
		{
			return this.extendedStorageRowHelper.GetExtendedStorage(index);
		}

		/// <summary>
		/// make the extended storage size bigger
		/// </summary>
		/// <param name="newSize"></param>
		public void GrowStorageSize(int newSize)
		{
			this.extendedStorageRowHelper.GrowStorageSize(newSize);
		}

		/// <summary>
		/// get the storage size
		/// </summary>
		public int StorageSize
		{
			get { return this.extendedStorageRowHelper.StorageSize; }
		}

		/// <summary>
		/// clear all the exteneded storage that is bound to the row
		/// </summary>
		public void ClearContents()
		{
			this.extendedStorageRowHelper.ClearContents();
		}

		/// <summary>
		/// Gets the children rows in the ConsumerDebt table.without checking lock
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ConsumerDebtRow[] GetConsumerDebtRows_NoLockCheck()
		{
			try
			{
				DataModel.DataLock.EnterReadLock();
				return ((ConsumerDebtRow[])(this.GetChildRows(this.tableCreditCard.CreditCardConsumerDebtRelation)));
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Gets the data in the CreditCardId column. without checking for lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public global::System.Guid CreditCardId_NoLockCheck
		{
			get
			{
				return ((global::System.Guid)(this[this.tableCreditCard.CreditCardIdColumn]));
			}
		}

	}

	partial class WorkingOrderRow
	{
		/// <summary>
		/// Gets the data in the SecurityId column. without checking for lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public global::System.Guid SecurityId_NoLockCheck
		{
			get
			{
				return ((global::System.Guid)(this[this.tableWorkingOrder.SecurityIdColumn]));
			}
		}


		/// <summary>
		/// Gets the data in the WorkingOrderId column. without checking for lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public global::System.Guid WorkingOrderId_NoLockCheck
		{
			get
			{
				return ((global::System.Guid)(this[this.tableWorkingOrder.WorkingOrderIdColumn]));
			}
		}

		/// <summary>
		/// Gets the parent row in the Blotter table.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public BlotterRow BlotterRow_NoLockCheck
		{
			get
			{
				try
				{
					DataModel.DataLock.EnterReadLock();
					return ((BlotterRow)(this.GetParentRow(this.tableWorkingOrder.BlotterWorkingOrderRelation)));
				}
				finally
				{
					DataModel.DataLock.ExitReadLock();
				}
			}
		}

		/// <summary>
		/// Gets the children rows in the Match table.without checking lock
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MatchRow[] GetMatchRows_NoLockCheck()
		{
			DataModel.DataLock.EnterReadLock();
			try
			{
				return ((MatchRow[])(this.GetChildRows(this.tableWorkingOrder.WorkingOrderMatchRelation)));
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Gets or sets the data in the StatusId column.without checking for lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public global::System.Guid StatusId_NoLockCheck
		{
			get
			{
				return ((global::System.Guid)(this[this.tableWorkingOrder.StatusIdColumn]));
			}
		}
	}

	partial class ConsumerDebtRow
	{
		/// <summary>
		/// Gets the parent row in the Consumer table. without checking lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public ConsumerRow ConsumerRow_NoLockCheck
		{
			get
			{
				DataModel.DataLock.EnterReadLock();
				try
				{
					return ((ConsumerRow)(this.GetParentRow(this.tableConsumerDebt.ConsumerConsumerDebtRelation)));
				}
				finally
				{
					DataModel.DataLock.ExitReadLock();
				} 
			}
		}

		/// <summary>
		/// Gets the parent row in the Security table.without checking lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public SecurityRow SecurityRow_NoLockCheck
		{
			get
			{
				DataModel.DataLock.EnterReadLock();
				try
				{
					return ((SecurityRow)(this.GetParentRow(this.tableConsumerDebt.SecurityConsumerDebtRelation)));
				}
				finally
				{
					DataModel.DataLock.ExitReadLock();
				} 
			}
		}

		/// <summary>
		/// Gets the parent row in the Security table.without checking lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public global::System.Guid TenantId_NoLockCheck
		{
			get
			{
				return ((global::System.Guid)(this[this.tableConsumerDebt.TenantIdColumn]));				
			}
		}

		/// <summary>
		/// Gets the parent row in the CreditCard table.without checking lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public CreditCardRow CreditCardRow_NoLockCheck
		{
			get
			{
				DataModel.DataLock.EnterReadLock();
				try
				{
					return ((CreditCardRow)(this.GetParentRow(this.tableConsumerDebt.CreditCardConsumerDebtRelation)));
				}
				finally
				{
					DataModel.DataLock.ExitReadLock();
				} 
			}
		}
	}

	partial class ConsumerTrustRow
	{
		/// <summary>
		/// Gets the parent row in the Consumer table. without checking lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public ConsumerRow ConsumerRow_NoLockCheck
		{
			get
			{
				DataModel.DataLock.EnterReadLock();
				try
				{
					return ((ConsumerRow)(this.GetParentRow(this.tableConsumerTrust.ConsumerConsumerTrustRelation)));
				}
				finally
				{
					DataModel.DataLock.ExitReadLock();
				}
			}
		}

		/// <summary>
		/// Gets the parent row in the Security table.without checking lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public SecurityRow SecurityRow_NoLockCheck
		{
			get
			{
				DataModel.DataLock.EnterReadLock();
				try
				{
					return ((SecurityRow)(this.GetParentRow(this.tableConsumerTrust.SecurityConsumerTrustRelation)));
				}
				finally
				{
					DataModel.DataLock.ExitReadLock();
				}
			}
		}

		/// <summary>
		/// Gets the parent row in the Security table.without checking lock
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never),
			EditorBrowsable(EditorBrowsableState.Never)]
		public global::System.Guid  TenantId_NoLockCheck
		{
			get
			{
				
				return ((global::System.Guid)(this[this.tableConsumerTrust.TenantIdColumn]));								
			}
		}
	}


	partial class SecurityRow
	{
		/// <summary>
		/// Gets the children rows in the WorkingOrder table.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public WorkingOrderRow[] GetWorkingOrderRowsByFK_Security_WorkingOrder_SecurityId_NoLockCheck()
		{
			DataModel.DataLock.EnterReadLock();
			try
			{
				return ((WorkingOrderRow[])(this.GetChildRows(this.tableSecurity.SecurityWorkingOrderByFK_Security_WorkingOrder_SecurityIdRelation)));
			}
			finally
			{
				DataModel.DataLock.ExitReadLock();
			}
		}
	}

	partial class DataModel
	{

		/// <summary>
		/// Enctypt the column using hash
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string EncryptString(string field)
		{
			return FluidTrade.Core.CryptoHelper.EncryptUsingHash(field);
		}
	}

}


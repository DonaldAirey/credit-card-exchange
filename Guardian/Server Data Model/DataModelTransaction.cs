using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Guardian
{
	/// <summary>
	/// A transaction to add or reject a group of changes to the DataModel as a single unit of work.
	/// </summary>
	[global::System.ComponentModel.DesignerCategoryAttribute("code")]
	public class DataModelTransaction : global::System.Transactions.IEnlistmentNotification, global::FluidTrade.Core.IDataModelTransaction
	{
		/// <summary>
		/// this is the dataModelTxn's txn stack this 
		/// is assigned from the static currentThreadDataModelTransactionStack when the
		/// datamodelTxn is created so that it will be married to the thread 
		/// that the dataModel txn was created on. This could be important
		/// if wanted to look at a txn from another thread (for debugging)
		/// the txn should not be used on a different thread.
		/// </summary>
		private Stack<DataModelTransaction> dataModelTransactionStack;

		/// <summary>
		/// thread static stack of current txns.  This should not be used directly use
		/// currentThreadDataModelTransactionStack (without the leading '_')
		/// </summary>
		[ThreadStatic]
		private static Stack<DataModelTransaction> _currentThreadDataModelTransactionStack;
		
		/// <summary>
		/// Logging helper method
		/// </summary>
		/// <returns></returns>
		public static global::System.Collections.Generic.List<global::System.String> GetTransactionStacks()
		{
			global::System.Threading.Monitor.Enter(DataModelTransaction.syncRoot);
			try
			{
				if(DataModelTransaction.transactionTable.Count == 0)
					return null;
				System.Collections.Generic.List<string> retList = new System.Collections.Generic.List<string>();
				if(FluidTrade.Core.EventLog.IsLoggingEnabledFor(FluidTrade.Core.EventLog.ErrorLogLevel.Verbose))
					foreach(DataModelTransaction dmt in DataModelTransaction.transactionTable.Values)
						retList.Add(dmt.stackTrce);

				return retList;
			}
			finally
			{
				global::System.Threading.Monitor.Exit(DataModelTransaction.syncRoot);
			}

		}

		private static string connectionString;

		private global::System.Collections.Generic.HashSet<global::FluidTrade.Core.IRow> lockList;

		private global::System.Collections.Generic.List<global::FluidTrade.Core.IRow> recordList;

		private global::System.Data.SqlClient.SqlConnection sqlConnection;

		private static object syncRoot;

		private global::System.Guid transactionId;

		private static global::System.Collections.Generic.Dictionary<string, DataModelTransaction> transactionTable;

		/// <summary>
		/// Creates the static resources required by the DataModelTransaction.
		/// </summary>
		static DataModelTransaction()
		{
			DataModelTransaction.connectionString = global::System.Configuration.ConfigurationManager.ConnectionStrings["DataModel"].ConnectionString;
			DataModelTransaction.transactionTable = new global::System.Collections.Generic.Dictionary<string, DataModelTransaction>();
			DataModelTransaction.syncRoot = new object();
		}
		string localIdentifier;
		string stackTrce;


		private global::System.Transactions.Transaction innerTransaction;
		/// <summary>
		/// Creates a DataModelTransaction.
		/// </summary>
		/// <param name="transaction">The transaction to which this context is bound.</param>
		private DataModelTransaction(global::System.Transactions.Transaction transaction)
		{
			if(FluidTrade.Core.EventLog.IsLoggingEnabledFor(FluidTrade.Core.EventLog.ErrorLogLevel.Verbose))
					this.stackTrce = FluidTrade.Core.UnhandledExceptionHelper.GetStackString();
			this.localIdentifier = transaction.TransactionInformation.LocalIdentifier;
			this.innerTransaction = transaction;

			this.lockList = new global::System.Collections.Generic.HashSet<global::FluidTrade.Core.IRow>();
			this.recordList = new global::System.Collections.Generic.List<global::FluidTrade.Core.IRow>();
			this.transactionId = global::System.Guid.NewGuid();
			transaction.EnlistVolatile(this, global::System.Transactions.EnlistmentOptions.None);

			//put this txn on the current thread's transaction stack
			this.dataModelTransactionStack = currentThreadDataModelTransactionStack;
			//in theory the stack should only be accessed from the same thread so dont need a lock
			//although might want to look at the stack through the DataModelTransaction
			//via another thread. so lock just to be safe
			lock(this.dataModelTransactionStack)
				this.dataModelTransactionStack.Push(this);
		}

		/// <summary>
		/// Gets the lock for the data model.
		/// </summary>
		[global::System.ComponentModel.BrowsableAttribute(false)]
		public static DataModelTransaction Current
		{
			get
			{
				try
				{
					global::System.Threading.Monitor.Enter(DataModelTransaction.syncRoot);
					global::System.Transactions.Transaction transaction = global::System.Transactions.Transaction.Current;
					string localIdentifier = transaction.TransactionInformation.LocalIdentifier;
					DataModelTransaction dataModelTransaction;
					if((DataModelTransaction.transactionTable.TryGetValue(localIdentifier, out dataModelTransaction) == false))
					{
						dataModelTransaction = new DataModelTransaction(transaction);
						transactionTable.Add(localIdentifier, dataModelTransaction);
						transaction.TransactionCompleted += new global::System.Transactions.TransactionCompletedEventHandler(dataModelTransaction.OnTransactionCompleted);
					}
					return dataModelTransaction;
				}
				finally
				{
					global::System.Threading.Monitor.Exit(DataModelTransaction.syncRoot);
				}
			}
		}

		/// <summary>
		/// Gets the SQL Connection used to access the persistent data store.
		/// </summary>
		[global::System.ComponentModel.BrowsableAttribute(false)]
		public global::System.Data.SqlClient.SqlConnection SqlConnection
		{
			get
			{
				if(this.sqlConnection == null)
					lock(this)
						if(this.sqlConnection == null)
						{
							this.sqlConnection = new global::System.Data.SqlClient.SqlConnection(DataModelTransaction.connectionString);
							this.sqlConnection.Open();
							this.sqlConnection.EnlistTransaction(this.innerTransaction);
						}
				return this.sqlConnection;
			}
		}

		/// <summary>
		/// Gets the unique identifier of this transaction.
		/// </summary>
		[global::System.ComponentModel.BrowsableAttribute(false)]
		public global::System.Guid TransactionId
		{
			get
			{
				return this.transactionId;
			}
		}

		/// <summary>
		/// Adds a row lock to the list of locks that must be released at the end of a transaction.
		/// </summary>
		/// <param name="iRow">The lock to be added to the transaction.</param>
		public void AddLock(global::FluidTrade.Core.IRow iRow)
		{
			lock(this.lockList)
				this.lockList.Add(iRow);
		}

		/// <summary>
		/// Adds a row lock to the list of locks that must be released at the end of a transaction.
		/// </summary>
		/// <param name="iRow">The record to be added to the transaction.</param>
		public void AddRecord(global::FluidTrade.Core.IRow iRow)
		{
			if((iRow.RowState != global::System.Data.DataRowState.Unchanged))
			{
				this.recordList.Remove(iRow);
			}
			this.recordList.Add(iRow);
		}

		/// <summary>
		/// Adds a row lock to the list of locks that must be released at the end of a transaction.
		/// </summary>
		/// <param name="enlistment">Facilitates communication bewtween an enlisted transaction participant and the transaction
		/// manager during the final phase of the transaction.</param>
		public void Commit(global::System.Transactions.Enlistment enlistment)
		{
			try
			{
				global::System.Collections.Generic.List<object> transactionLogItem = new global::System.Collections.Generic.List<object>();
				DataModel.TransactionLogLock.EnterWriteLock();
				DataModel.DataLock.EnterWriteLock();
				for(int recordIndex = 0; (recordIndex < this.recordList.Count); recordIndex = (recordIndex + 1))
				{
					global::FluidTrade.Core.IRow iRow = this.recordList[recordIndex];
					global::FluidTrade.Core.ITable iTable = ((global::FluidTrade.Core.ITable)(iRow.Table));
					if((iRow.RowState == global::System.Data.DataRowState.Modified))
					{
						transactionLogItem.Clear();
						transactionLogItem.Add(global::FluidTrade.Core.RecordState.Modified);
						transactionLogItem.Add(iTable.Ordinal);
						for(int keyIndex = 0; (keyIndex < iTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
						{
							transactionLogItem.Add(iRow[iTable.PrimaryKey[keyIndex]]);
						}
						for(int columnIndex = 0; (columnIndex < iTable.Columns.Count); columnIndex = (columnIndex + 1))
						{
							if((iRow[columnIndex].Equals(iRow[columnIndex, global::System.Data.DataRowVersion.Original]) == false))
							{
								transactionLogItem.Add(columnIndex);
								transactionLogItem.Add(iRow[columnIndex]);
							}
						}
						DataModel.AddTransaction(iRow, transactionLogItem.ToArray());
						iRow.AcceptChanges();
					}
					else
					{
						if((iRow.RowState == global::System.Data.DataRowState.Added))
						{
							transactionLogItem.Clear();
							transactionLogItem.Add(global::FluidTrade.Core.RecordState.Added);
							transactionLogItem.Add(iTable.Ordinal);
							for(int keyIndex = 0; (keyIndex < iTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
							{
								transactionLogItem.Add(iRow[iTable.PrimaryKey[keyIndex]]);
							}
							for(int columnIndex = 0; (columnIndex < iTable.Columns.Count); columnIndex = (columnIndex + 1))
							{
								if((iRow[columnIndex].Equals(iTable.Columns[columnIndex].DefaultValue) == false))
								{
									transactionLogItem.Add(columnIndex);
									transactionLogItem.Add(iRow[columnIndex]);
								}
							}
							DataModel.AddTransaction(iRow, transactionLogItem.ToArray());
							iRow.AcceptChanges();
						}
						else
						{
							transactionLogItem.Clear();
							transactionLogItem.Add(global::FluidTrade.Core.RecordState.Deleted);
							transactionLogItem.Add(iTable.Ordinal);
							for(int keyIndex = 0; (keyIndex < iTable.PrimaryKey.Length); keyIndex = (keyIndex + 1))
							{
								transactionLogItem.Add(iRow[iTable.PrimaryKey[keyIndex], global::System.Data.DataRowVersion.Original]);
							}
							DataModel.AddTransaction(iRow, transactionLogItem.ToArray());
							iRow.AcceptChanges();
						}
					}
				}
			}
			finally
			{
				ClearRowLockList();

				DataModel.TransactionLogLock.ExitWriteLock();
				DataModel.DataLock.ExitWriteLock();

				try
				{
					//txn is done remove this txn from the threads stack
					//in theory the stack should only be accessed from the same thread so dont need a lock
					//although might want to look at the stack through the DataModelTransaction
					//via another thread. so lock just to be safe
					lock(this.dataModelTransactionStack)
						this.dataModelTransactionStack.Pop();
				}
				catch(Exception ex)
				{
					try
					{
						global::FluidTrade.Core.EventLog.Information("Error in Commit dataModelTransactionStack.Pop {0}\r\n{1}", ex.Message, ex.StackTrace);
					}
					catch
					{
					}
				}
			}
			enlistment.Done();
		}

		/// <summary>
		/// clear and release the row lock for the rows that are in the lockList
		/// </summary>
		/// <returns>true if rows are cleared from the list</returns>
		private bool ClearRowLockList()
		{
			bool clearedRowsInList = false; 
			try
			{
				if(this.lockList != null)
				{
					lock(this.lockList)
					{
						if(this.lockList != null)
						{
							foreach(global::FluidTrade.Core.IRow lockedRow in this.lockList)
							{
								try
								{
									clearedRowsInList = true;
									lockedRow.ReleaseLock(this.transactionId);
								}
								catch(global::System.Exception ex)
								{
									try
									{
										global::FluidTrade.Core.EventLog.Information("Error in relaseLock {0}\r\n{1}", ex.Message, ex.StackTrace);
									}
									catch
									{
									}
								}
							}

							this.lockList = null;
						}
					}
				}
			}
			catch(Exception ex)
			{
				try
				{
					global::FluidTrade.Core.EventLog.Information("Error in ClearRowLockList {0}\r\n{1}", ex.Message, ex.StackTrace);
				}
				catch
				{
				}
			}

			return clearedRowsInList;
		}

		/// <summary>
		/// get the currecnt thread's stack that contains the open dataModelTxns
		/// </summary>
		private static Stack<DataModelTransaction> currentThreadDataModelTransactionStack
		{
			get
			{
				//since _currentThreadDataModelTransactionStack is threadStatic need to check 
				//for existance
				if(_currentThreadDataModelTransactionStack == null)
					_currentThreadDataModelTransactionStack = new Stack<DataModelTransaction>();

				return _currentThreadDataModelTransactionStack;
			}
		}

		/// <summary>
		/// Get the current DataModelTxn that for the Calling Thread
		/// </summary>
		public static DataModelTransaction CurrentThreadDataModelTransaction
		{
			get
			{
				lock(currentThreadDataModelTransactionStack)
					return currentThreadDataModelTransactionStack.Peek();
			}
		}
		/// <summary>
		/// Adds a row lock to the list of locks that must be released at the end of a transaction.
		/// </summary>
		/// <param name="enlistment">Facilitates communication bewtween an enlisted transaction participant and the transaction
		/// manager during the final phase of the transaction.</param>
		public void InDoubt(global::System.Transactions.Enlistment enlistment)
		{
			/*This method is called for volatile resources when the transaction manager has invoked a single phase commit 
			 * operation to a single durable resource,
			 * and then connection to the durable resource was lost prior to getting the transaction result. 
			 * At that point, the transaction outcome cannot be safely determined. 
			 *	As InDoubt is considered to be a final state for a transaction, you should not call Commit or Rollback after calling InDoubt.
			*/
			try
			{
				global::FluidTrade.Core.EventLog.Information("<<<***>>> DataModelTransaction InDoubt");
			}
			catch
			{
			}
			if(this.ClearRowLockList())
			{
				try
				{
					global::FluidTrade.Core.EventLog.Information("<<<***>>> DataModelTransaction InDoubt cleared rows in List");
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// Processes the completion of a transaction.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="transactionEventArgs">The event arguments.</param>
		private void OnTransactionCompleted(object sender, global::System.Transactions.TransactionEventArgs transactionEventArgs)
		{
			try
			{
				if(FluidTrade.Core.EventLog.IsLoggingEnabledFor(FluidTrade.Core.EventLog.ErrorLogLevel.Verbose))
					this.stackTrce += " closing";

				lock(this)
				{
					if(this.sqlConnection != null)
					{
						this.sqlConnection.Close();
						this.sqlConnection = null;
					}
				}
			}
			finally
			{
				if(this.ClearRowLockList())
				{
					try
					{
						global::FluidTrade.Core.EventLog.Information("<<<***>>> OnTransactionCompleted cleared rows in List");
					}
					catch
					{
					}
				}

				global::System.Threading.Monitor.Enter(DataModelTransaction.syncRoot);
				try
				{
					DataModelTransaction.transactionTable.Remove(this.localIdentifier);
					//global::System.Transactions.Transaction transaction = transactionEventArgs.Transaction;
					//string localIdentifier = transaction.TransactionInformation.LocalIdentifier;
					//DataModelTransaction dataModelTransaction;
					//if (DataModelTransaction.transactionTable.TryGetValue(localIdentifier, out dataModelTransaction))
					//{
					//    dataModelTransaction.SqlConnection.Close();
					//    DataModelTransaction.transactionTable.Remove(localIdentifier);
					//}
				}
				finally
				{
					global::System.Threading.Monitor.Exit(DataModelTransaction.syncRoot);
				}
			}
		}

		/// <summary>
		/// Indicates that the transaction can be committed.
		/// </summary>
		/// <param name="preparingEnlistment">Facilitates communication bewtween an enlisted transaction participant and the
		/// transaction manager during the final phase of the transaction.</param>
		public void Prepare(global::System.Transactions.PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		/// <summary>
		/// Adds a row lock to the list of locks that must be released at the end of a transaction.
		/// </summary>
		/// <param name="enlistment">Facilitates communication bewtween an enlisted transaction participant and the transaction
		/// manager during the final phase of the transaction.</param>
		public void Rollback(global::System.Transactions.Enlistment enlistment)
		{
			this.recordList.Reverse();
			try
			{
				DataModel.DataLock.EnterWriteLock();
				for(int recordIndex = 0; (recordIndex < this.recordList.Count); recordIndex = (recordIndex + 1))
				{
					global::FluidTrade.Core.IRow iRow = this.recordList[recordIndex];
					if(((iRow.RowState == global::System.Data.DataRowState.Added)
								|| ((iRow.RowState == global::System.Data.DataRowState.Deleted)
								|| (iRow.RowState == global::System.Data.DataRowState.Modified))))
					{
						iRow.RejectChanges();
					}
				}
			}
			finally
			{
				this.ClearRowLockList();

				DataModel.DataLock.ExitWriteLock();
				try
				{
					//txn is done remove this txn from the threads stack
					//in theory the stack should only be accessed from the same thread so dont need a lock
					//although might want to look at the stack through the DataModelTransaction
					//via another thread. so lock just to be safe
					lock(this.dataModelTransactionStack)
						this.dataModelTransactionStack.Pop();
				}
				catch(Exception ex)
				{
					try
					{
						global::FluidTrade.Core.EventLog.Information("Error in Commit dataModelTransactionStack.Pop {0}\r\n{1}", ex.Message, ex.StackTrace);
					}
					catch
					{
					}
				}
			}
			enlistment.Done();
		}
	}
}

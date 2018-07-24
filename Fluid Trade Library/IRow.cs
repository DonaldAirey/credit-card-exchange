namespace FluidTrade.Core
{
	using System;
	using System.Data;
	using System.Threading;

	/// <summary>
	/// Abstract interface to common features of a row.
	/// </summary>
	public interface IRow
	{

		/// <summary>
		/// Gets the object at the given index.
		/// </summary>
		/// <param name="index">The column index into the row data.</param>
		/// <param name="dataRowVersion">The version of the data.</param>
		object this[int index, global::System.Data.DataRowVersion dataRowVersion]
		{
			get;
		}

		/// <summary>
		/// Gets the object at the given index.
		/// </summary>
		/// <param name="index">The column index into the row data.</param>
		object this[global::System.Data.DataColumn index]
		{
			get;
		}

		/// <summary>
		/// Gets the object at the given index.
		/// </summary>
		/// <param name="index">The column index into the row data.</param>
		/// <param name="dataRowVersion">The version of the data.</param>
		object this[global::System.Data.DataColumn index, global::System.Data.DataRowVersion dataRowVersion]
		{
			get;
		}

		/// <summary>
		/// Gets the object at the given index.
		/// </summary>
		/// <param name="index">The column index into the row data.</param>
		object this[int index]
		{
			get;
		}

		/// <summary>
		/// Gets the table to which this row belongs.
		/// </summary>
		global::System.Data.DataRowState RowState
		{
			get;
		}

		/// <summary>
		/// Gets the table to which this row belongs.
		/// </summary>
		long RowVersion
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the table to which this row belongs.
		/// </summary>
		global::System.Data.DataTable Table
		{
			get;
		}

		/// <summary>
		/// Accepts any changes made to the row.
		/// </summary>
		void AcceptChanges();

		/// <summary>
		/// Acquires a reader lock for this record.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		/// <param name="timeSpan">The time that the thread will wait for the lock.</param>
		void AcquireReaderLock(global::System.Guid transactionId, global::System.TimeSpan timeSpan);

		/// <summary>
		/// Acquires a reader lock for this record.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for this operation.</param>
		void AcquireReaderLock(IDataModelTransaction dataModelTransaction);

		/// <summary>
		/// Acquires a writer lock for this record.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for this operation.</param>
		void AcquireWriterLock(IDataModelTransaction dataModelTransaction);

		/// <summary>
		/// Acquires a writer lock for this record.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		/// <param name="timeSpan">The time that the thread will wait for the lock.</param>
		void AcquireWriterLock(global::System.Guid transactionId, global::System.TimeSpan timeSpan);

		/// <summary>
		/// Gets the parent row using a relation.
		/// </summary>
		/// <param name="dataRelation">Represents a Parent/Child relationship between two System.Data.DataTable objects.</param>
		/// <returns>The parent row of the this row.</returns>
		global::System.Data.DataRow GetParentRow(global::System.Data.DataRelation dataRelation);

		/// <summary>
		/// Gets a value indicating whether the owner of a token holds a reader lock.
		/// </summary>
		/// <returns>true if the current token owner holds a reader lock.</returns>
		bool IsLockHeld(global::System.Guid transactionId);

		/// <summary>
		/// Gets the timeout value for locking operations.
		/// </summary>
		System.TimeSpan LockTimeout { get; }

		/// <summary>
		/// Rejects any changes made to the row.
		/// </summary>
		void RejectChanges();

		/// <summary>
		/// Releases every lock held by this record.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		void ReleaseLock(global::System.Guid transactionId);

		/// <summary>
		/// Releases the reader lock on this record.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		void ReleaseReaderLock(global::System.Guid transactionId);

		/// <summary>
		/// Releases the writer lock on this record.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		void ReleaseWriterLock(global::System.Guid transactionId);
	}

	public abstract class DataRowBase : DataRow, IRow
	{
		private global::System.Collections.Generic.List<System.Guid> _readers;
		private global::System.Collections.Generic.List<string> _readersStackTrace;

		private long readerWaiters;

		private object rowRoot;

		private long rowVersion;

		private global::System.Guid _writer;

		private string writerStackTrace;
		private global::System.Int32 writerThread;
		private global::System.Guid writer
		{
			get
			{
				return _writer;
			}
			set
			{
				if (value == Guid.Empty)
				{
					this.writerStackTrace = null;
					this.writerThread = 0;
				}
				else
				{
					if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
						this.writerStackTrace = UnhandledExceptionHelper.GetStackString();
					this.writerThread = Thread.CurrentThread.ManagedThreadId;
				}
				this._writer = value;
			}
		}

		private object writerRoot;

		private long writerWaiters;

		protected DataRowBase(global::System.Data.DataRowBuilder dataRowBuilder) :
			base(dataRowBuilder)
		{
			this._readers = new global::System.Collections.Generic.List<System.Guid>();
			if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
				this._readersStackTrace = new System.Collections.Generic.List<string>();
			this.rowRoot = new object();
			this.writerRoot = new object();
			this.writer = global::System.Guid.Empty;
			
		}

		/// <summary>
		/// returns LockTimeout for dataSet
		/// </summary>
		public abstract TimeSpan LockTimeout { get; }

		public long RowVersion
		{
			get
			{
				return this.rowVersion;
			}
			set
			{
				this.rowVersion = value;
			}
		}

		/// <summary>
		/// Acquires a reader lock for this record.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for this operation.</param>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public virtual void AcquireReaderLock(IDataModelTransaction dataModelTransaction)
		{
			global::System.Guid transactionId = dataModelTransaction.TransactionId;
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				this.readerWaiters = (this.readerWaiters + 1);
				if((this.writer != transactionId))
				{
					for(
					; (this.writer != global::System.Guid.Empty);
					)
					{
						if(false == global::System.Threading.Monitor.Wait(this.rowRoot, this.LockTimeout))
						{
							try
							{
								EventLog.Information("AcquireReaderLock DEGRADE from TIMEOUT\r\n {0} Current Stack: {1} \r\n{2}", this.GetRowDebugDescription(), UnhandledExceptionHelper.GetStackString(), this.GetCurrentLockStacks(false));
							}
							catch
							{
							}
							break;
						}
					}
					int index = this._readers.BinarySearch(transactionId);
					if((index < 0))
					{
						InsertToReaderList(transactionId, index);
					}
				}
			}
			finally
			{
				this.readerWaiters = (this.readerWaiters - 1);
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
			dataModelTransaction.AddLock(this);
			if((this.RowState == global::System.Data.DataRowState.Detached))
			{
				throw new global::System.Exception("The " + this.GetType().FullName + " record was deleted after it was locked");
			}
		}

		private string GetRowDebugDescription()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("RowType:");
			sb.Append(this.GetType().Name);
			sb.Append(" HashCode:");
			sb.Append(this.GetHashCode());
			sb.Append(" Key:");
			try
			{
				DataColumn[] keyCols = this.Table.PrimaryKey;

				foreach(DataColumn dc in keyCols)
				{
					sb.Append(this[dc]);
					sb.Append(":");
				}
			}
			catch
			{
			}

			return sb.ToString();
		}

		/// <summary>
		/// Acquires a reader lock.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		/// <param name="timeSpan">The time that the thread will wait for the lock.</param>
		///[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public virtual void AcquireReaderLock(global::System.Guid transactionId, global::System.TimeSpan timeSpan)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				this.readerWaiters = (this.readerWaiters + 1);
				if((this.writer != transactionId))//no-op if there is a writer lock
				{
					for(
					; (this.writer != global::System.Guid.Empty);
					)
					{
						if(false == global::System.Threading.Monitor.Wait(this.rowRoot, timeSpan))
						{
							try
							{
								EventLog.Information("AcquireReaderLock DEGRADE from TIMEOUT\r\n {0} Current Stack: {1} \r\n{2}", this.GetRowDebugDescription(), UnhandledExceptionHelper.GetStackString(), this.GetCurrentLockStacks(false));
							}
							catch
							{
							}

							break;
						}
					}
					int index = this._readers.BinarySearch(transactionId);
					if((index < 0))
					{
						InsertToReaderList(transactionId, index);
					}
				}
			}
			finally
			{
				this.readerWaiters = (this.readerWaiters - 1);
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		private void InsertToReaderList(global::System.Guid transactionId, int index)
		{
			this._readers.Insert(((0 - index)
											  - 1), transactionId);
			if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
			{
				this._readersStackTrace.Insert(((0 - index)
												  - 1), UnhandledExceptionHelper.GetStackString());
			}
		}

		/// <summary>
		/// Acquires a writer lock for this record.
		/// </summary>
		/// <param name="dataModelTransaction">The transaction context for this operation.</param>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public virtual void AcquireWriterLock(IDataModelTransaction dataModelTransaction)
		{
			global::System.Guid transactionId = dataModelTransaction.TransactionId;
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				this.writerWaiters = (this.writerWaiters + 1);
				if((this.writer != transactionId))
				{
					int index = this._readers.BinarySearch(transactionId);
					if((index >= 0))
					{
						this._readers.RemoveAt(index);
						if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
							this._readersStackTrace.RemoveAt(index);
					}
					for(
					; ((this.writer != global::System.Guid.Empty)
								|| (this._readers.Count != 0));
					)
					{
						try
						{
							try
							{
								global::System.Threading.Monitor.Enter(this.writerRoot);
								global::System.Threading.Monitor.Exit(this.rowRoot);
								if(false == global::System.Threading.Monitor.Wait(this.writerRoot, this.LockTimeout))
								{
									try
									{
										EventLog.Information("AcquireWriterLock TIMEOUT\r\n {0} Current Stack: {1} \r\n{2}", this.GetRowDebugDescription(), UnhandledExceptionHelper.GetStackString(), this.GetCurrentLockStacks(false));
									}
									catch
									{
									}

									throw new FluidTrade.Core.Utilities.DeadlockException("AcquireWriterLock TIMEOUT", null);
								}
							}
							finally
							{
								global::System.Threading.Monitor.Exit(this.writerRoot);
							}
						}
						finally
						{
							global::System.Threading.Monitor.Enter(this.rowRoot);
						}
					}
					this.writer = transactionId;
				}
			}
			finally
			{
				this.writerWaiters = (this.writerWaiters - 1);
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
			dataModelTransaction.AddLock(this);
			if((this.RowState == global::System.Data.DataRowState.Detached))
			{
				throw new global::System.Exception("The " + this.GetType().FullName + " record was deleted after it was locked");
			}
		}

		/// <summary>
		/// Acquires a writer lock.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		/// <param name="timeSpan">The time that the thread will wait for the lock.</param>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public virtual void AcquireWriterLock(global::System.Guid transactionId, global::System.TimeSpan timeSpan)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				this.writerWaiters = (this.writerWaiters + 1);
				if((this.writer != transactionId))
				{
					int index = this._readers.BinarySearch(transactionId);
					if((index >= 0))
					{
						this._readers.RemoveAt(index);
						if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
							this._readersStackTrace.RemoveAt(index);
					}
					for(
					; ((this.writer != global::System.Guid.Empty)
								|| (this._readers.Count != 0));
					)
					{
						try
						{
							try
							{
								global::System.Threading.Monitor.Enter(this.writerRoot);
								global::System.Threading.Monitor.Exit(this.rowRoot);
								if(false == global::System.Threading.Monitor.Wait(this.writerRoot, timeSpan))
								{
									try
									{
										EventLog.Information("AcquireWriterLock TIMEOUT\r\n {0} Current Stack: {1} \r\n{2}", this.GetRowDebugDescription(), UnhandledExceptionHelper.GetStackString(), this.GetCurrentLockStacks(false));
									}
									catch
									{
									}

									throw new FluidTrade.Core.Utilities.DeadlockException("AcquireWriterLock TIMEOUT", null);
								}
							}
							finally
							{
								global::System.Threading.Monitor.Exit(this.writerRoot);
							}
						}
						finally
						{
							global::System.Threading.Monitor.Enter(this.rowRoot);
						}
					}
					this.writer = transactionId;
				}
			}
			finally
			{
				this.writerWaiters = (this.writerWaiters - 1);
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the owner of a token holds a reader lock.
		/// </summary>
		/// <returns>true if the current token owner holds a reader lock.</returns>
		public bool IsLockHeld()
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				return ((this.writer != Guid.Empty)
							|| (this._readers.Count > 0));
			}
			finally
			{
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the owner of a token holds a reader lock.
		/// </summary>
		/// <returns>true if the current token owner holds a reader lock.</returns>
		public bool IsLockHeld(global::System.Guid transactionId)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				return ((this.writer == transactionId)
							|| (this._readers.BinarySearch(transactionId) >= 0));
			}
			finally
			{
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}


		/// <summary>
		/// Gets a value indicating whether the owner of a token holds a reader lock.
		/// </summary>
		/// <returns>true if the current token owner holds a reader lock.</returns>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public bool IsReaderLockHeld(global::System.Guid transactionId)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				return (this._readers.BinarySearch(transactionId) >= 0);
			}
			finally
			{
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the owner of a token holds a writer lock.
		/// </summary>
		/// <returns>true if the current token owner holds a writer lock.</returns>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public bool IsWriterLockHeld(global::System.Guid transactionId)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				return (this.writer == transactionId);
			}
			finally
			{
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		/// <summary>
		/// Releases every lock held by this record.
		/// </summary>
		/// <param name="transactionId">A token used to identify the holder of the lock.</param>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public virtual void ReleaseLock(global::System.Guid transactionId)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				int index = this._readers.BinarySearch(transactionId);
				if((index >= 0))
				{
					this._readers.RemoveAt(index);
					if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
						this._readersStackTrace.RemoveAt(index);
					if(((this._readers.Count == 0)
								&& (this.writerWaiters != 0)))
					{
						try
						{
							global::System.Threading.Monitor.Enter(this.writerRoot);
							global::System.Threading.Monitor.Pulse(this.writerRoot);
						}
						finally
						{
							global::System.Threading.Monitor.Exit(this.writerRoot);
						}
					}
				}
				if((this.writer == transactionId))
				{
					this.writer = global::System.Guid.Empty;
					if((this.readerWaiters > 0))
					{
						global::System.Threading.Monitor.PulseAll(this.rowRoot);
					}
					else
					{
						if((this.writerWaiters > 0))
						{
							try
							{
								global::System.Threading.Monitor.Enter(this.writerRoot);
								global::System.Threading.Monitor.Pulse(this.writerRoot);
							}
							finally
							{
								global::System.Threading.Monitor.Exit(this.writerRoot);
							}
						}
					}
				}
			}
			finally
			{
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		/// <summary>
		/// Releases the reader lock on this record.
		/// </summary>
		/// <param name="transactionId">A token used to hold locks.</param>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public virtual void ReleaseReaderLock(global::System.Guid transactionId)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				if(this.writer != transactionId) //no-op if there is a writer lock
				{
					int index = this._readers.BinarySearch(transactionId);
					if((index < 0))
					{
						throw new global::System.ServiceModel.FaultException<FluidTrade.Core.SynchronizationLockFault>(new global::FluidTrade.Core.SynchronizationLockFault(this.Table.TableName));
					}
					this._readers.RemoveAt(index);
					if(EventLog.IsLoggingEnabledFor(EventLog.ErrorLogLevel.Verbose))
						this._readersStackTrace.RemoveAt(index);

					if(((this._readers.Count == 0)
								&& (this.writerWaiters != 0)))
					{
						try
						{
							global::System.Threading.Monitor.Enter(this.writerRoot);
							global::System.Threading.Monitor.Pulse(this.writerRoot);
						}
						finally
						{
							global::System.Threading.Monitor.Exit(this.writerRoot);
						}
					}
				}
			}
			finally
			{
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		/// <summary>
		/// Releases the writer lock on this record.
		/// </summary>
		/// <param name="transactionId">A token used to hold locks.</param>
		//[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public virtual void ReleaseWriterLock(global::System.Guid transactionId)
		{
			try
			{
				global::System.Threading.Monitor.Enter(this.rowRoot);
				if((this.writer != transactionId))
				{
					throw new global::System.ServiceModel.FaultException<FluidTrade.Core.SynchronizationLockFault>(new global::FluidTrade.Core.SynchronizationLockFault("AccessControl"));
				}
				this.writer = global::System.Guid.Empty;
				if((this.readerWaiters > 0))
				{
					global::System.Threading.Monitor.PulseAll(this.rowRoot);
				}
				else
				{
					if((this.writerWaiters > 0))
					{
						try
						{
							global::System.Threading.Monitor.Enter(this.writerRoot);
							global::System.Threading.Monitor.Pulse(this.writerRoot);
						}
						finally
						{
							global::System.Threading.Monitor.Exit(this.writerRoot);
						}
					}
				}
			}
			finally
			{
				global::System.Threading.Monitor.Exit(this.rowRoot);
			}
		}

		public string GetCurrentLockStacks(bool includeRowDescription)
		{
			string retString = null;
			if(this.writerStackTrace != null)
			{
				if(includeRowDescription == true)
					retString = string.Format("{0}\r\n*W*\r\n{1}\r\n", this.GetRowDebugDescription(), writerStackTrace);
				else
					retString = string.Format("*W*\r\n{0}\r\n", writerStackTrace);
			}
			
			if(this._readersStackTrace != null &&
				this._readersStackTrace.Count > 0)
			{
				try
				{
					foreach(string curStackTrace in this._readersStackTrace)
					{
						if(retString == null && includeRowDescription == true)
							retString = string.Format("{0}\r\n{1}\r\n*R*{2}\r\n", this.GetRowDebugDescription(), retString, curStackTrace);
						else
							retString = string.Format("{0}\r\n*R*{1}\r\n", retString, curStackTrace);

					}
				}
				catch
				{
				}
			}

			return retString;
		}
	}
}

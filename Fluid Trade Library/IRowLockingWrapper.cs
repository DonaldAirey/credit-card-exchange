using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidTrade.Core
{
	public interface IRowLockingWrapper
	{
		/// <summary>
		/// Acquires a reader lock for this record if it is not locked
		/// </summary>
		void AcquireReaderLock();

		/// <summary>
		/// Acquires a writer lock for this record if it is not locked
		/// </summary>
		void AcquireWriterLock();

		/// <summary>
		/// Has this container acquired a reader lock
		/// </summary>
		bool HasEnteredReader { get; }

		/// <summary>
		/// Has this container acquired a writer lock
		/// </summary>
		bool HasEnteredWriter { get; }

		/// <summary>
		/// Releases the reader lock on this record.
		/// </summary>
		void ReleaseReaderLock();

		/// <summary>
		/// Releases the writer lock on this record.
		/// </summary>
		void ReleaseWriterLock();

		/// <summary>
		/// Get inner row
		/// </summary>
		IRow Row { get; }
	}

	/// <summary>
	/// Default impl for ILockingRowWrapper. 
	/// This will contain lock information about a row.
	/// In general a LockingRowContainer should be created and used on a single thread
	/// This object gives an easy way to pass a row along with its locking information
	/// to other methods. 
	/// There is also a less functional lightweight lock count so it is
	/// safe to call AcquireLock() more than once
	/// </summary>
	/// <typeparam name="TRow"></typeparam>
	public class RowLockingWrapper<TRow> : IRowLockingWrapper, IDisposable
		where TRow : IRow
	{
		private TRow row;
		private IDataModelTransaction dataModelTransaction;
		private int readerLockCount;
		private int writerLockCount;

		public RowLockingWrapper(TRow row, IDataModelTransaction dataModelTransaction)
		{
			this.row = row;
			this.dataModelTransaction = dataModelTransaction;
		}

		/// <summary>
		/// get the underlying strongly typed Row
		/// </summary>
		public TRow TypedRow
		{
			get
			{
				return this.row;
			}
		}

		/// <summary>
		/// Acquires a reader lock for this record if it is not locked
		/// </summary>
		public void AcquireReaderLock()
		{
			if(this.HasEnteredWriter)
				throw new NotSupportedException("cannot enter reader if writer is held");

			if(this.readerLockCount == 0)
			{
				this.row.AcquireReaderLock(this.dataModelTransaction.TransactionId, this.row.LockTimeout);
			}
			this.readerLockCount++;
		}

		/// <summary>
		/// Acquires a writer lock for this record if it is not locked
		/// </summary>
		public void AcquireWriterLock()
		{
			if(this.HasEnteredReader)
				throw new NotSupportedException("cannot enter writer if reader is held");

			if(this.writerLockCount == 0)
				this.row.AcquireWriterLock(this.dataModelTransaction.TransactionId, this.row.LockTimeout);
			
			this.writerLockCount++;

		}

		/// <summary>
		/// Has this container acquired a reader lock
		/// </summary>
		public bool HasEnteredReader
		{
			get { return this.readerLockCount >0; }
		}

		/// <summary>
		/// Has this container acquired a writer lock
		/// </summary>
		public bool HasEnteredWriter
		{
			get { return this.writerLockCount > 0; }
		}

		/// <summary>
		/// Releases the reader lock on this record.
		/// </summary>
		public void ReleaseReaderLock()
		{
			this.readerLockCount--;
			if(this.readerLockCount == 0 &&
				this.row.IsLockHeld(this.dataModelTransaction.TransactionId))
				this.row.ReleaseReaderLock(this.dataModelTransaction.TransactionId);

			if(this.readerLockCount < 0)
				this.readerLockCount = 0;
			
		}

		/// <summary>
		/// Releases the writer lock on this record.
		/// </summary>
		public void ReleaseWriterLock()
		{
			this.writerLockCount--;
			if(this.writerLockCount == 0 &&
				this.row.IsLockHeld(this.dataModelTransaction.TransactionId))
				this.row.ReleaseWriterLock(this.dataModelTransaction.TransactionId);

			if(this.writerLockCount < 0)
				this.writerLockCount = 0;
		}

		/// <summary>
		/// get the underlying Row
		/// </summary>
		public IRow Row
		{
			get { return this.row; }
		}

		/// <summary>
		/// destructor.. called by GC
		/// </summary>
		~RowLockingWrapper()
		{
		}

		/// <summary>
		/// clean up/ releases all locks
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			this.Dispose(true);
		}

		/// <summary>
		/// clean up/ releases all locks
		/// </summary>
		/// <param name="fromPublicDispose"></param>
		protected virtual void Dispose(bool fromPublicDispose)
		{
			if(this.readerLockCount > 0)
			{
				this.readerLockCount = 0;
				this.row.ReleaseReaderLock(this.dataModelTransaction.TransactionId);
			}

			if(this.writerLockCount > 0)
			{
				this.writerLockCount = 0;
				this.row.ReleaseWriterLock(this.dataModelTransaction.TransactionId);
			}
		}
	}
}

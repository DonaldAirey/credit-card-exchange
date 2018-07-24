namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;

	/// <summary>
	/// A synchronized queue. Specifically, allowing enqueuing on one thread and dequeuing on others.
	/// </summary>
	/// <typeparam name="T">The type of the elements of the queue.</typeparam>
	public class SynchronizedQueue<T> : Queue<T>
	{

		ManualResetEvent flag;
		Semaphore semaphore;
		object syncRoot;
		Int32 capacity;
		Boolean empty;

		/// <summary>
		/// Create a new queue.
		/// </summary>
		/// <param name="capacity">The capacity of the queue.</param>
		public SynchronizedQueue(Int32 capacity)
			: base(capacity)
		{

			this.flag = new ManualResetEvent(true);
			this.semaphore = new Semaphore(0, capacity);
			this.syncRoot = new object();
			this.capacity = capacity;
			this.empty = false;

		}

		/// <summary>
		/// Exception thrown when trying to enqueue on an Empty queue.
		/// </summary>
		public class QueueEmptyException : Exception
		{

			/// <summary>
			/// Create a new queue-empty exception.
			/// </summary>
			public QueueEmptyException()
				: base()
			{
			}

		}

		/// <summary>
		/// Gets the number of items in the queue.
		/// </summary>
		public new Int32 Count
		{
			get
			{

				lock (this.syncRoot)
					return base.Count;

			}
		}

		/// <summary>
		/// Gets or sets whether the source of the enqueuing items is exhausted.
		/// </summary>
		public Boolean Empty
		{
			get
			{

				lock (this.syncRoot)
					return this.empty;

			}
			set
			{

				lock (this.syncRoot)
					this.empty = value;

				if (this.empty)
					this.flag.Set();

			}
		}

		/// <summary>
		/// True if the queue is at or over capacity.
		/// </summary>
		public Boolean Full
		{
			get
			{

				lock (this.syncRoot)
					return base.Count >= this.capacity;

			}
		}

		/// <summary>
		/// The object used to synchronize on.
		/// </summary>
		public object SyncRoot
		{
			get { return this.syncRoot; }
		}

		/// <summary>
		/// Empties the queue.
		/// </summary>
		public new void Clear()
		{

			lock (this.syncRoot)
				base.Clear();

		}

		/// <summary>
		/// Determine whether a particular item is in the queue.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>True if the item is in the queue.</returns>
		public new Boolean Contains(T item)
		{

			lock (this.syncRoot)
				return base.Contains(item);

		}

		/// <summary>
		/// Copy the contents of the queue to an array.
		/// </summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="arrayIndex">The index into the array to start copying at.</param>
		public new void CopyTo(T[] array, Int32 arrayIndex)
		{

			lock (this.syncRoot)
				base.CopyTo(array, arrayIndex);

		}

		/// <summary>
		/// Remove an item from the queue.
		/// </summary>
		/// <returns>The item that was removed.</returns>
		public new T Dequeue()
		{

			if (this.Empty && this.Count == 0)
				throw new ArgumentOutOfRangeException("Empty");

			this.semaphore.WaitOne();

			lock (this.syncRoot)
			{

				this.flag.Set();
				return base.Dequeue();

			}

		}

		/// <summary>
		/// Add an item to the queue.
		/// </summary>
		/// <param name="item">The item.</param>
		public new void Enqueue(T item)
		{

			if (this.Empty)
				throw new QueueEmptyException();

			this.flag.WaitOne();

			lock (this.syncRoot)
			{

				base.Enqueue(item);
				if (base.Count >= this.capacity)
					this.flag.Reset();

				try
				{

					this.semaphore.Release();

				}
				catch (SemaphoreFullException)
				{

					if (this.Empty)
						throw new QueueEmptyException();
					else
						throw;

				}

			}

		}

		/// <summary>
		/// Enumerating over the queue is not supported.
		/// </summary>
		/// <returns>Nothing.</returns>
		public new Queue<T>.Enumerator GetEnumerator()
		{

			throw new NotImplementedException("SynchronizedQueue only supports queuing and dequeuing, not enumerating.");

		}

		/// <summary>
		/// Peek at the next item in the queue.
		/// </summary>
		/// <returns>The next item in the queue.</returns>
		public new T Peek()
		{

			lock (this.syncRoot)
				return base.Peek();

		}

		/// <summary>
		/// Create an array of the items in the queue.
		/// </summary>
		/// <returns>An array with all of the items in the queue.</returns>
		public new T[] ToArray()
		{

			lock (this.syncRoot)
				return base.ToArray();

		}

		/// <summary>
		/// Changing the capacity of the queue is not supported.
		/// </summary>
		public new void TrimExcess()
		{

			throw new NotImplementedException("SynchronizedQueue does not support changing its capacity.");

		}

	}

}

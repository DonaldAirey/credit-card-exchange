using System.Collections.Generic;
using System.Threading;
using System;

namespace FluidTrade.Core
{
	/// <summary>
	/// Thread-safe data cache 
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	[Serializable]
	public class CacheDictionary<TKey, TValue>
	{
		[NonSerialized]
		private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

		private Dictionary<TKey, TValue> cachedDictionary = new Dictionary<TKey, TValue>();

		/// <summary>
		/// Get a value in a thread safe
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TValue Get(TKey key)
		{
			TValue cachedItem;
			
			bool lockAcquired = false;
			while (!lockAcquired)
				lockAcquired = readerWriterLock.TryEnterReadLock(1);
			
			try
			{
				cachedDictionary.TryGetValue(key, out cachedItem);
			}
			finally
			{
				readerWriterLock.ExitReadLock();
			}

			return cachedItem;
		}
		
		/// <summary>
		/// Adds/Update a new item to the dictinary in a thread safe manner.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Upsert(TKey key, TValue value)
		{
			bool lockAcquired = false;
			while (!lockAcquired)
				lockAcquired = readerWriterLock.TryEnterWriteLock(1);

			try
			{			
				cachedDictionary[key] = value;
			}
			finally
			{
				readerWriterLock.ExitWriteLock();
			}
		}

	}
}

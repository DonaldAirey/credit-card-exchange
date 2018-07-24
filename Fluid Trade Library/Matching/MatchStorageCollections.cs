using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;

namespace FluidTrade.Core.Matching
{
	public class MatchStorageDictionary<KeyT>
	{
		private const int timeoutMSecs = 60000;
		private Dictionary<KeyT, MatchStorageRowList> innerDictionary;
		private ReaderWriterLockSlim innerDictionaryRWL;

		public MatchStorageDictionary(ReaderWriterLockSlim rwl)
		{
			this.innerDictionary = new Dictionary<KeyT, MatchStorageRowList>();
			this.innerDictionaryRWL = rwl;
		}

		private bool manuallyEnteredReadLock = false;
		public bool EnterReadLock()
		{
			bool retVal = innerDictionaryRWL.TryEnterReadLock(timeoutMSecs);
			if(retVal)
				manuallyEnteredReadLock = true;
			return retVal;
		}

		private bool manuallyEnteredWriteLock = false;
		public bool EnterWriteLock()
		{
			bool retVal = innerDictionaryRWL.TryEnterWriteLock(timeoutMSecs);
			if(retVal)
				manuallyEnteredWriteLock = true;
			return retVal;
		}

		public void ExitReadLock()
		{
			if(this.manuallyEnteredReadLock == false)
				return;

			innerDictionaryRWL.ExitReadLock();
			this.manuallyEnteredReadLock = false;
		}

		public void ExitWriteLock()
		{
			if(this.manuallyEnteredWriteLock == false)
				return;

			innerDictionaryRWL.ExitWriteLock();
			this.manuallyEnteredWriteLock = false;
		}


		public bool TryGetValue(KeyT key, out MatchStorageRowList rowList)
		{
			if(this.manuallyEnteredReadLock == true)
			{
				return innerDictionary.TryGetValue(key, out rowList);
			}

			if(innerDictionaryRWL.TryEnterReadLock(timeoutMSecs))
			{
				try
				{
					return innerDictionary.TryGetValue(key, out rowList);
				}
				finally
				{
					innerDictionaryRWL.ExitReadLock();
				}
			}
			else
			{
				throw new TimeoutException();
			}
		}

		public void Add(KeyT key, MatchStorageRowList rowList)
		{
			if(this.manuallyEnteredWriteLock == true)
			{
				innerDictionary.Add(key, rowList);
				return;
			}

			if(innerDictionaryRWL.TryEnterWriteLock(timeoutMSecs))
			{
				try
				{
					innerDictionary.Add(key, rowList);
				}
				finally
				{
					innerDictionaryRWL.ExitWriteLock();
				}
			}
			else
			{
				throw new TimeoutException();
			}
		}

		public bool Remove(KeyT key)
		{
			if(this.manuallyEnteredWriteLock == true)
			{
				return innerDictionary.Remove(key);
			}

			if(innerDictionaryRWL.TryEnterWriteLock(timeoutMSecs))
			{
				try
				{
					return innerDictionary.Remove(key);
				}
				finally
				{
					innerDictionaryRWL.ExitWriteLock();
				}
			}
			else
			{
				throw new TimeoutException();
			}
		}

		public ReaderWriterLockSlim InnerDictionaryRWL
		{
			get
			{
				return this.innerDictionaryRWL;
			}
		}

	}

	public class MatchStorageRowList : MatchStorageList<IRow>
	{
		public MatchStorageRowList(ReaderWriterLockSlim rwl)
			: base(rwl)
		{
		}
	}

	public class MatchStorageList<T> : System.Collections.IEnumerable
	{
		private const int timeoutMSecs = 60000;
		private List<T> innerList;
		private ReaderWriterLockSlim innerListRWL;

		public MatchStorageList(ReaderWriterLockSlim rwl)
		{
			this.innerList = new List<T>();
			this.innerListRWL = rwl;
		}

		private bool manuallyEnteredReadLock = false;
		public bool EnterReadLock()
		{
			bool retVal = innerListRWL.TryEnterReadLock(timeoutMSecs);
			if(retVal)
				manuallyEnteredReadLock = true;
			return retVal;
		}

		private bool manuallyEnteredWriteLock = false;
		public bool EnterWriteLock()
		{
			bool retVal = innerListRWL.TryEnterWriteLock(timeoutMSecs);
			if(retVal)
				manuallyEnteredWriteLock = true;
			return retVal;
		}

		public void ExitReadLock()
		{
			if(this.manuallyEnteredReadLock == false)
				return;

			innerListRWL.ExitReadLock();
			this.manuallyEnteredReadLock = false;
		}

		public void ExitWriteLock()
		{
			if(this.manuallyEnteredWriteLock == false)
				return;

			innerListRWL.ExitWriteLock();
			this.manuallyEnteredWriteLock = false;
		}

		public void Add(T item, bool needsLock)
		{
			if(this.manuallyEnteredWriteLock == true || needsLock == false)
			{
				innerList.Add(item);
				return;
			}

			if(innerListRWL.TryEnterWriteLock(timeoutMSecs))
			{
				try
				{
					innerList.Add(item);
				}
				finally
				{
					innerListRWL.ExitWriteLock();
				}
			}
			else
			{
				throw new TimeoutException();
			}
		}

		public bool Remove(T item, bool needsLock)
		{
			if(this.manuallyEnteredWriteLock == true || needsLock == false)
			{
				return innerList.Remove(item);
			}

			if(innerListRWL.TryEnterWriteLock(timeoutMSecs))
			{
				try
				{
					return innerList.Remove(item);
				}
				finally
				{
					innerListRWL.ExitWriteLock();
				}
			}
			else
			{
				throw new TimeoutException();
			}
		}


		public System.Collections.IEnumerable GetRawEnumerator()
		{
			if(this.manuallyEnteredReadLock == false)
				throw new NotSupportedException("must manaully lock collection before calling GetRawEnumerator");

			return this.innerList;
		}
		
		public System.Collections.IEnumerator GetEnumerator()
		{
			if(innerListRWL.IsReadLockHeld || 
				innerListRWL.IsWriteLockHeld || 
				this.manuallyEnteredReadLock == true)
			{
				return this.innerList.ToArray().GetEnumerator();
			}
			if(innerListRWL.TryEnterReadLock(timeoutMSecs))
			{
				try
				{
					return this.innerList.ToArray().GetEnumerator();
				}
				finally
				{
					innerListRWL.ExitReadLock();
				}
			}
			else
			{
				throw new TimeoutException();
			}
		}
	}
}

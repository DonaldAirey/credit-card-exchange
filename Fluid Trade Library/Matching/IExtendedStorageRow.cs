using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FluidTrade.Core.Matching
{
	/// <summary>
	/// IExtendedStorageRow
	/// allows for IExtendedStorage aware classes to put 
	/// objects into the row for retrieveal later.
	/// </summary>
	public interface IExtendedStorageRow
	{
		/// <summary>
		/// clear all the exteneded storage that is bound to the row
		/// </summary>
		void ClearContents();

		/// <summary>
		/// get exteneded storage by Index,  The index is the value
		/// that is returned from IExtendedStorageTable.GetNextExtendedStorageIndex()
		/// </summary>
		/// <param name="index">index of storage location in the IExtendedStorage. this should always be the 
		/// value that is returned by GetNextExtendedStorageIndex at init time</param>
		/// <returns></returns>
		object GetExtendedStorage(int index);

		/// <summary>
		/// make the extended storage size bigger
		/// </summary>
		/// <param name="newSize"></param>
		void GrowStorageSize(int newSize);

		/// <summary>
		/// set exteneded storage value by Index,  The index is the value
		/// that is returned from IExtendedStorageTable.GetNextExtendedStorageIndex()
		/// </summary>
		/// <param name="index">index of storage location in the IExtendedStorage. this should always be the 
		/// value that is returned by GetNextExtendedStorageIndex at init time</param>
		/// <param name="value">value to store in extended storage</param>
		void SetExtendedStorage(int index, object value);

		/// <summary>
		/// get the storage size
		/// </summary>
		int StorageSize { get; }
	}

	/// <summary>
	/// IExtendedStorageTable
	/// will return a unique index where IExtendedStorage aware classes can put their values
	/// </summary>
	public interface IExtendedStorageTable
	{
		/// <summary>
		/// get the next index (id) for extended storage. Caller should
		/// save this value and re-use it for all get and set operations
		/// on extended storage
		/// </summary>
		/// <returns></returns>
		int GetNextExtendedStorageIndex();
	}

	/// <summary>
	/// helper class that has default impl of IExtendedStorageRow
	/// </summary>
	public class ExtendedStorageRowHelper : IExtendedStorageRow
	{
		private object[] storageAr;
		public ExtendedStorageRowHelper(int defaultSize)
		{
			this.storageAr = new object[defaultSize];
		}

		/// <summary>
		/// clear all the exteneded storage that is bound to the row
		/// </summary>
		public void ClearContents()
		{
			Array.Clear(storageAr, 0, storageAr.Length);
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
			return storageAr[index];
		}

		/// <summary>
		/// set exteneded storage value by Index,  The index is the value
		/// that is returned from IExtendedStorageTable.GetNextExtendedStorageIndex()
		/// </summary>
		/// <param name="index">index of storage location in the IExtendedStorage. this should always be the 
		/// value that is returned by GetNextExtendedStorageIndex at init time</param>
		/// <param name="value">value to store in extended storage</param>
		public void SetExtendedStorage(int index, object value)
		{
			storageAr[index] = value;
		}

		/// <summary>
		/// make the extended storage size bigger
		/// </summary>
		/// <param name="newSize"></param>
		public void GrowStorageSize(int newSize)
		{
			if(newSize <= this.StorageSize)
				return;

			object[] newStorage = new object[newSize];
			Array.Copy(storageAr, newStorage, newSize);
			this.storageAr = newStorage;
		}

		/// <summary>
		/// get the storage size
		/// </summary>
		public int StorageSize
		{
			get
			{
				return this.storageAr.Length;
			}
		}
	}

	/// <summary>
	/// helper class that has a default impl of IExtendedStorageTable
	/// </summary>
	public class ExtendedStorageTableHelper : IExtendedStorageTable
	{
		private int nextIndex = -1;
		/// <summary>
		/// get the next index (id) for extended storage. Caller should
		/// save this value and re-use it for all get and set operations
		/// on extended storage
		/// </summary>
		/// <returns></returns>
		public int GetNextExtendedStorageIndex()
		{
			return Interlocked.Increment(ref this.nextIndex);
		}
	}
}

namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Collections.ObjectModel;
	using System.Collections;
	using System.Collections.Specialized;
	using FluidTrade.Guardian.Utilities;

	/// <summary>
	/// A list composed of several lists concatenated together.
	/// </summary>
	/// <typeparam name="T">A shared base type between the lists.</typeparam>
	public class MergedList<T> : IDataBoundList<T>
	{

		/// <summary>
		/// The event raised when the list changes.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		/// <summary>
		/// The event raised when the list is about to change.
		/// </summary>
		public event EventHandler CollectionChanging;
		/// <summary>
		/// Raised after the list has been initialized.
		/// </summary>
		public event EventHandler Initialized
		{
			add
			{

				if (this.IsInitialized)
					value(this, new EventArgs());
				this.initialized = Delegate.Combine(this.initialized, value) as EventHandler;

			}
			remove
			{

				this.initialized = Delegate.Remove(this.initialized, value) as EventHandler;

			}
		}

		// Private fields
		private List<IDataBoundList> lists = new List<IDataBoundList>();
		private Int32 initializedLists = 0;
		private object syncRoot = new object();

		// Private Events
		private EventHandler initialized;

		/// <summary>
		/// Create a new merged list.
		/// </summary>
		public MergedList()
		{


		}

		/// <summary>
		/// The total number of objects in the list.
		/// </summary>
		public Int32 Count
		{

			get
			{

				Int32 count = 0;

				foreach (IDataBoundList list in this.lists)
				{

					count += list.Count;

				}

				return count;

			}

		}

		/// <summary>
		/// True if the list has already been initialized.
		/// </summary>
		public Boolean IsInitialized
		{

			get;
			private set;

		}

		/// <summary>
		/// Determine whether the list is of a fixed size; always false.
		/// </summary>
		public bool IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Whether the list is read only. This is always true.
		/// </summary>
		public bool IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value indicating whether access to the list is synchronized (thread safe). This is always false.
		/// </summary>
		public bool IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the list.
		/// </summary>
		public object SyncRoot
		{
			get { return syncRoot; }
		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		/// <param name="item"></param>
		public void Add(T item)
		{

			this.Add(item as object);

		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		/// <param name="item"></param>
		public Int32 Add(object item)
		{

			throw new NotImplementedException("MergedList cannot be directly modified");

		}

		/// <summary>
		/// Add a list to the set of lists that compose this object.
		/// </summary>
		/// <param name="list">The list to add.</param>
		public void AddList(IDataBoundList list)
		{

			this.IsInitialized = false;
			this.lists.Add(list);
			list.CollectionChanged += OnCollectionChanged;
			list.CollectionChanging += OnCollectionChanging;
			list.Initialized += this.OnListInitialized;

		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		public void Clear()
		{
			throw new NotImplementedException("MergedList cannot be directly modified");
		}

		/// <summary>
		/// Determine whether an item is in one of the lists.
		/// </summary>
		/// <param name="item">The item to look for.</param>
		/// <returns>True if the item exists in one of the lists, false otherwise.</returns>
		public Boolean Contains(object item)
		{

			foreach (IDataBoundList list in this.lists)
				if (list.Contains(item))
					return true;

			return false;

		}

		/// <summary>
		/// Determine whether an object is in the list.
		/// </summary>
		/// <param name="item">The object to find.</param>
		/// <returns>True if the object is in the list, false otherwise.</returns>
		public bool Contains(T item)
		{

			foreach (IList<T> list in this.lists)
				if (list.Contains(item))
					return true;

			return false;

		}

		/// <summary>
		/// Copy the list into an array.
		/// </summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		public void CopyTo(T[] array, int arrayIndex)
		{

			foreach (IList<T> list in this.lists)
			{

				list.CopyTo(array, arrayIndex);
				arrayIndex += list.Count;

			}

		}

		/// <summary>
		/// Copy the list into an array.
		/// </summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		public void CopyTo(Array array, int arrayIndex)
		{

			foreach (IList<T> list in this.lists)
				foreach (T item in list)
					array.SetValue(item, arrayIndex++);

		}

		/// <summary>
		/// Dispose of the underlying lists.
		/// </summary>
		public void Dispose()
		{

			foreach (IDataBoundList list in this.lists)
				list.Dispose();

		}

		/// <summary>
		/// Get an enumerator over the contents of the list.
		/// </summary>
		/// <returns>The new enumerator.</returns>
		public IEnumerator<T> GetEnumerator()
		{

			List<IEnumerator> enumerators = new List<IEnumerator>();

			foreach (IList list in this.lists)
				enumerators.Add(list.GetEnumerator());

			return new MergedListEnumerator<T>(enumerators.GetEnumerator());

		}

		/// <summary>
		/// Get an enumerator over the contents of the list.
		/// </summary>
		/// <returns>The new enumerator.</returns>
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			List<IEnumerator> enumerators = new List<IEnumerator>();

			foreach (IDataBoundList list in this.lists)
				enumerators.Add(list.GetEnumerator());

			return new MergedListEnumerator<T>(enumerators.GetEnumerator());
		}

		/// <summary>
		/// The (first) index of an object in the list.
		/// </summary>
		/// <param name="item">The object to find.</param>
		/// <returns>The zero-based index into the list of the object, or -1 if not found.</returns>
		public Int32 IndexOf(T item)
		{

			Int32 index = 0;

			foreach (IList list in this.lists)
			{

				Int32 subIndex = list.IndexOf(item);

				if (subIndex >= 0)
					return index + subIndex;
				else
					index += list.Count;

			}

			return -1;

		}

		/// <summary>
		/// The (first) index of an object in the list.
		/// </summary>
		/// <param name="item">The object to find.</param>
		/// <returns>The zero-based index into the list of the object, or -1 if not found.</returns>
		public Int32 IndexOf(object item)
		{

			if (item is T)
				return this.IndexOf((T)item);
			else
				throw new ArgumentException("Object is of the wrong type", "item");

		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, object item)
		{

			throw new NotImplementedException("MergedList cannot be directly modified");

		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void  Insert(int index, T item)
		{

			this.Insert(index, item as object);
	
		}

		/// <summary>
		/// Handle changes to the underlying lists.
		/// </summary>
		/// <param name="sender">The list that sent the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{

			if (this.CollectionChanged != null)
			{

				NotifyCollectionChangedEventArgs eventArgs = e;

				if (e.Action == NotifyCollectionChangedAction.Add)
				{

					Int32 index = 0;

					foreach (object item in e.NewItems)
					{

						index = this.IndexOf(item);
						break;

					}

					eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, index);

				}
				else if (e.Action == NotifyCollectionChangedAction.Remove)
				{

					Int32 index = 0;

					foreach (object item in e.OldItems)
					{

						index = this.IndexOf(item);
						break;

					}

					eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, index);

				}

				this.CollectionChanged(this, eventArgs);

			}

		}

		/// <summary>
		/// Handle underlying lists that are about to change.
		/// </summary>
		/// <param name="sender">The list that sent the event.</param>
		/// <param name="e">The event arguments.</param>
		private void OnCollectionChanging(object sender, EventArgs e)
		{

			if (this.CollectionChanging != null)
				this.CollectionChanging(this, e);

		}

		/// <summary>
		/// Note the initialization of a list and notify our initialization listeners if necessary.
		/// </summary>
		/// <param name="sender">The list.</param>
		/// <param name="eventArgs">The event args.</param>
		private void OnListInitialized(object sender, EventArgs eventArgs)
		{

			this.initializedLists += 1;

			if (this.initializedLists == this.lists.Count && this.initialized != null)
			{

				this.IsInitialized = true;
				this.initialized(this, new EventArgs());

			}

		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{

			throw new NotImplementedException("MergedList cannot be directly modified");

		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		/// <param name="item"></param>
		public void Remove(object item)
		{

			throw new NotImplementedException("MergedList cannot be directly modified");

		}

		/// <summary>
		/// The exposed list cannot be modified directly.
		/// </summary>
		/// <param name="item"></param>
		public bool Remove(T item)
		{

			this.Remove(item as object);
			return false;

		}

		/// <summary>
		/// Get an object at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get
			{

				if (index < 0)
					throw new IndexOutOfRangeException("MergedList index must be greater than or equal to zero");

				foreach (IList list in this.lists)
				{

					if (list.Count > index)
						return (T)list[index];
					else
						index -= list.Count;

				}

				throw new IndexOutOfRangeException("MergedList index is too large");

			}
			set 
			{

				throw new NotImplementedException("MergedList cannot be directly modified");

			}
		}

		/// <summary>
		/// Get an object at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				this[index] = (T)value;
			}
		}

	}

	/// <summary>
	/// An enumerator of the items in a MergedList.
	/// </summary>
	/// <typeparam name="T">A shared base type between the lists.</typeparam>
	public class MergedListEnumerator<T> : IEnumerator<T>
	{

		private IEnumerator<IEnumerator> enumerators = null;

		/// <summary>
		/// Create a new enumerator.
		/// </summary>
		/// <param name="enumerators">The enumerators of the lists that are merged.</param>
		public MergedListEnumerator(IEnumerator<IEnumerator> enumerators)
		{

			enumerators.Reset();

			this.enumerators = enumerators;

		}

		/// <summary>
		/// Dispose of the enumerator.
		/// </summary>
		public void Dispose()
		{


		}

		/// <summary>
		/// The current object.
		/// </summary>
		public T Current
		{
			get { return (T)this.enumerators.Current.Current; }
		}

		/// <summary>
		/// The current object.
		/// </summary>
		object IEnumerator.Current
		{
			get { return this.enumerators.Current.Current; }
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the
		/// collection.</returns>
		public Boolean MoveNext()
		{

			Boolean advanced = true;

			if (this.enumerators.Current == null)
				this.enumerators.MoveNext();

			do
			{

				advanced = this.enumerators.Current.MoveNext();

				if (!advanced)
					if (this.enumerators.MoveNext())
					{

						this.enumerators.Current.Reset();
						advanced = this.enumerators.Current.MoveNext();

					}
					else
					{

						// If we can't advanced the enumerator point, call it quits.
						break;

					}

			} while (!advanced);

			return advanced;

		}

		/// <summary>
		/// Reset the enumerator to just before the first element in the list.
		/// </summary>
		public void Reset()
		{

			this.enumerators.Reset();
			this.enumerators.MoveNext();
			this.enumerators.Current.Reset();

		}

	}

}

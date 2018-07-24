namespace FluidTrade.Guardian
{

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.Data;
	using System.Threading;
	using System.Windows.Threading;
	using System.Collections;
	using FluidTrade.Core;

	/// <summary>
	/// The basic functionality of a list of objects bound to a data model.
	/// </summary>
	/// <typeparam name="T">The type of object the list contains.</typeparam>
	public abstract class DataBoundList<T> : List<T>, IDataBoundList<T>
	{

		// Private Delegates
		/// <summary>
		/// A method that takes an action on a list of objects.
		/// </summary>
		/// <param name="list"></param>
		protected delegate void ItemsDelegate(List<T> list);

		// Private Events
		private EventHandler initialized;

		// Private Fields
		private Dispatcher dispatcher;
		private Dictionary<DataRow, T> deleteList;

		// Public Events
		/// <summary>
		/// Raised when the contents of the collection changes.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		/// <summary>
		/// Raised before the contents of the collection changes.
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

		/// <summary>
		/// Creates a list that is bound to the data model.
		/// </summary>
		public DataBoundList()
		{

			DataTable parentTable = this.Table;

			// Set a default filter.
			this.FilterMethod = o => true;

			this.dispatcher = Dispatcher.CurrentDispatcher;
			deleteList = new Dictionary<DataRow, T>();

			// These delegates handle the updates that come from the background processing of the client data model.
			this.Table.RowChanged += this.HandleDataRowChanged;
			this.Table.RowDeleting += this.HandleDataRowDeleting;
			this.VisitParents(this.Table, table => table.RowChanged += this.HandleParentDataRowChanged);

		}

		/// <summary>
		/// Specific exception for row types not handled by New, Filter, and FilterMethod.
		/// </summary>
		protected class RowNotHandledException : Exception
		{

			/// <summary>
			/// Create a new row not handled exception.
			/// </summary>
			/// <param name="message">The exception message.</param>
			public RowNotHandledException(String message) : base (message)
			{
			}

		}

		/// <summary>
		/// The Comparer to use to compare to objects in the list.
		/// </summary>
		protected abstract IComparer<T> Comparer { get; }

		/// <summary>
		/// The foreground dispatcher used to create this list.
		/// </summary>
		protected Dispatcher Dispatcher
		{
			get { return this.dispatcher; }
		}

		/// <summary>
		/// Removes line items from the display.
		/// </summary>
		public Func<T, Boolean> FilterMethod { get; set; }

		/// <summary>
		/// True if the list has already been initialized.
		/// </summary>
		public Boolean IsInitialized
		{

			get;
			private set;

		}

		/// <summary>
		/// The table this list is based on.
		/// </summary>
		protected abstract DataTable Table { get; }

		/// <summary>
		/// Adds or removes items from the list.
		/// </summary>
		/// <param name="updateList">A list of modified items.</param>
		private void ChangeItems(List<T> updateList)
		{

			try
			{

				// When the list has changed beyond a simple add or update action, it must be reset.
				Boolean isReset = false;

				// Each of the modified items will be added to the list if the list doesn't already contain that item, or the existing
				// item will be updated if the list does contain the item.
				foreach (T item in updateList)
				{

					// If the item doesn't exist, it is added.  If it exists, it's updated.
					int currentItem = this.IndexOf(item);
					T insertItem = item;
					ReadOnlyCollection<T> readOnlyCollection;

					if (currentItem >= 0)
					{

						// Updating the elements here will drive the Property Notification to update any controls that are bound to 
						// these items.
						this.Update(this[currentItem], item);
						insertItem = this[currentItem];
						// Remove the item so we can re-add it in the right place in the list.
						this.RemoveAt(currentItem);

						isReset = true;

					}

					// Find where in the list the item should be displayed.
					currentItem = this.BinarySearch(insertItem, this.Comparer);

					if (currentItem < 0)
						currentItem = ~currentItem;

					this.NotifyChanging();

					// Add the element to the list.
					this.Insert(currentItem, insertItem);

					// Data views don't support updating entire lists, so each element sends out an individual notification.
					readOnlyCollection = new ReadOnlyCollection<T>(new T[] { item });
					this.NotifyChange(NotifyCollectionChangedAction.Add, readOnlyCollection, currentItem);

				}

				// The listener is asked to reset the list when the changes can't be handled as a single 'Add' operation.
				if (isReset)
					this.NotifyChange(NotifyCollectionChangedAction.Reset);

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Deletes items from the list.
		/// </summary>
		/// <param name="updateList">A list of deleted items.</param>
		private void DeleteItems(List<T> updateList)
		{

			try
			{

				// This will remove each of the items in the list and send a notification of the event to any listeners.  Note that
				// DataViews are not able to process collections, so each deletion is handled individually.  Additionally, the
				// notification message was reverse engineered to look like the events from an ObservableCollection.
				foreach (T item in updateList)
					if (this.Remove(item))
					{

						ReadOnlyCollection<T> readOnlyCollection = new ReadOnlyCollection<T>(new T[] { item });

						this.NotifyChange(NotifyCollectionChangedAction.Remove, readOnlyCollection);

					}

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Dispose of the object.
		/// </summary>
		public virtual void Dispose()
		{

			this.Table.RowChanged -= this.HandleDataRowChanged;
			this.Table.RowDeleting -= this.HandleDataRowDeleting;
			this.VisitParents(this.Table, table => table.RowChanged -= this.HandleParentDataRowChanged);

		}

		/// <summary>
		/// Filter what rows are included in the list.
		/// </summary>
		/// <param name="row">The row to examine.</param>
		/// <returns>True if the row is included in the list, false if not.</returns>
		protected virtual Boolean Filter(DataRow row)
		{

			return true;

		}
  
        /// <summary>
		/// Handles a change to a record in the data model.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		protected void HandleDataRowChanged(object sender, DataRowChangeEventArgs e)
		{

			try
			{

				// The list is for use in the foreground where the data model is updated in a background thread.  This will pass a copy
				// of the new or updated data to the foreground to be integrated with the list.
				if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached)
				{

					if (this.deleteList.ContainsKey(e.Row))
					{

						this.dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new DataBoundList<T>.ItemsDelegate(this.DeleteItems),
							new List<T> { this.deleteList[e.Row] });

						this.deleteList.Remove(e.Row);

					}
					else if (this.Filter(e.Row))
					{

						T item = this.New(e.Row);

						if (this.FilterMethod(item))
							this.dispatcher.BeginInvoke(
								DispatcherPriority.Normal,
								new DataBoundList<T>.ItemsDelegate(this.ChangeItems),
								new List<T> { item });

					}

				}
				else if (e.Action == DataRowAction.Rollback && this.deleteList.ContainsKey(e.Row))
				{

					this.deleteList.Remove(e.Row);

				}

			}
			catch (RowNotHandledException)
			{

				// Swallow unhandled row exceptions.

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Handles a change to a parent record in the data model.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		protected void HandleParentDataRowChanged(object sender, DataRowChangeEventArgs e)
		{

			try
			{

				// The list is for use in the foreground where the data model is updated in a background thread.  This will pass a copy
				// of the new or updated data to the foreground to be integrated with the list.
				if (e.Action == DataRowAction.Commit && e.Row.RowState != DataRowState.Detached && this.Filter(e.Row))
				{

					T item = this.New(e.Row);

					if (this.FilterMethod(item))
						this.dispatcher.BeginInvoke(
							DispatcherPriority.Normal,
							new DataBoundList<T>.ItemsDelegate(this.ChangeItems),
							new List<T> { item });

				}

			}
			catch (RowNotHandledException)
			{

				// Swallow unhandled row exceptions.

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Handles a change to a Blotter record in the data model.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		protected void HandleDataRowDeleting(object sender, DataRowChangeEventArgs e)
		{

			try
			{

				if (e.Action == DataRowAction.Delete)
				{

					List<T> updateList = new List<T>();
					T item = this.New(e.Row);

					if (this.Filter(e.Row))
						this.deleteList[e.Row] = item;

				}

			}
			catch (RowNotHandledException)
			{

				// Swallow unhandled row exceptions.

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

		}

		/// <summary>
		/// Adds or removes items from the list.
		/// </summary>
		private void Initialize()
		{

			this.IsInitialized = true;
			if (this.initialized != null)
				this.initialized(this, EventArgs.Empty);

		}

		/// <summary>
		/// Initializes the list from the data model.
		/// </summary>
		/// <param name="state">Unused thread start arguments.</param>
		private void InitializeList(object state)
		{

			try
			{

				// Exlusive access to the data model is required to read it.
				lock (DataModel.SyncRoot)
				{

					// This creates an item from each record in the table that can be passed to the foreground.
					foreach (DataRow dataRow in this.Table.Rows)
					{
						if (dataRow.RowState == DataRowState.Deleted ||
							dataRow.RowState == DataRowState.Detached)
							continue;

						Boolean include = this.Filter(dataRow);

						if (include)
						{

							List<T> updateList = new List<T>();
							T item = this.New(dataRow);

							if (this.FilterMethod(item))
							{

								updateList.Add(item);
								// Pass the list of items in the table to the foreground where it can be used without locking any resources.
								this.Dispatcher.BeginInvoke(new DataBoundList<T>.ItemsDelegate(this.ChangeItems), DispatcherPriority.Normal, updateList);

							}

						}

					}

				}

			}
			catch (RowNotHandledException)
			{

				// Swallow unhandled row exceptions.

			}
			catch (Exception exception)
			{

				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

			}

			// Announce initialization even on errors to (hopefully) make problems more obvious to the user.
			this.Dispatcher.BeginInvoke(new Action(this.Initialize), DispatcherPriority.Normal);

		}

		/// <summary>
		/// Initializes the list from the data model.
		/// </summary>
		protected void InitializeList()
		{

			// Initialization of the list requires access to the client data model which can only be peformed in a background 
			// thread.
			FluidTrade.Core.ThreadPoolHelper.QueueUserWorkItem(this.InitializeList);

		}

		/// <summary>
		/// Called to create a new item from a row in the data model.
		/// </summary>
		/// <param name="row">The row from the data model.</param>
		/// <returns>The object based on the row.</returns>
		protected abstract T New(DataRow row);

		/// <summary>
		/// Notify listeners that the collection changed.
		/// </summary>
		/// <param name="action">The particular action.</param>
		/// <param name="changeList">The items that changed.</param>
		/// <param name="index">The index where the changes start.</param>
		private void NotifyChange(NotifyCollectionChangedAction action, IList changeList, Int32 index)
		{

			// The event argments were reverse engineered to match the output of the ObservableCollection.
			this.NotifyChange(new NotifyCollectionChangedEventArgs(action, changeList, index));

		}

		/// <summary>
		/// Notify listeners that the collection changed.
		/// </summary>
		/// <param name="action">The particular action.</param>
		/// <param name="changeList">The items that changed.</param>
		private void NotifyChange(NotifyCollectionChangedAction action, IList changeList)
		{

			// The event argments were reverse engineered to match the output of the ObservableCollection.
			this.NotifyChange(new NotifyCollectionChangedEventArgs(action, changeList));

		}

		/// <summary>
		/// Notify listeners that the collection changed.
		/// </summary>
		/// <param name="action">The particular action.</param>
		private void NotifyChange(NotifyCollectionChangedAction action)
		{

			// The event argments were reverse engineered to match the output of the ObservableCollection.
			this.NotifyChange(new NotifyCollectionChangedEventArgs(action));

		}

		/// <summary>
		/// Notify listeners that the collection changed.
		/// </summary>
		/// <param name="eventArgs">The event arguments.</param>
		private void NotifyChange(NotifyCollectionChangedEventArgs eventArgs)
		{

			if (Thread.CurrentThread != this.Dispatcher.Thread)
				this.Dispatcher.BeginInvoke(new Action(() =>
					this.NotifyChange(eventArgs)),
				DispatcherPriority.Normal);
			else if (CollectionChanged != null)
				this.CollectionChanged(this, eventArgs);

		}

		/// <summary>
		/// Notify listeners that the collection is about to change.
		/// </summary>
		private void NotifyChanging()
		{

			if (Thread.CurrentThread != this.Dispatcher.Thread)
				this.Dispatcher.BeginInvoke(new Action(() =>
					this.NotifyChanging()),
				DispatcherPriority.Normal);
			else if (CollectionChanging != null)
				this.CollectionChanging(this, new EventArgs());

		}

		/// <summary>
		/// Called to update an object with a new object represented the same data.
		/// </summary>
		/// <param name="old">The original object.</param>
		/// <param name="update">The object with the new data.</param>
		protected abstract void Update(T old, T update);

		/// <summary>
		/// Visit all the parent tables of a table.
		/// </summary>
		/// <param name="table">The first table in the tree.</param>
		/// <param name="visitor">The method to visit with.</param>
		private void VisitParents(DataTable table, Action<DataTable> visitor)
		{

			foreach (DataRelation relation in table.ParentRelations)
			{

				visitor(table);
				this.VisitParents(relation.ParentTable, visitor);

			}

		}

	}

}

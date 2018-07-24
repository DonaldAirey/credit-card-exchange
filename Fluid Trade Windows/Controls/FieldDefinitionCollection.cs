namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections;
	using System.Collections.Generic;
    using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// An ordered and randomly accessed collection of MarkThee.Controls.ColumnDefinitions.
	/// </summary>
	public class ReportFieldCollection : FrameworkContentElement, IList
	{

		// Public Static Fields
		public static readonly System.Windows.RoutedEvent CollectionChangedEvent;

		// Private Instance Fields
		private System.Object syncRoot;
		private List<ReportField> list;
			private Dictionary<string, ReportField> fieldDefinitionMap;
		private Dictionary<Type, string> templateTypeColumnIdMap;

		/// <summary>
		/// Create the static resources for a collection of FieldDefinitions.
		/// </summary>
		static ReportFieldCollection()
		{

			// CollectionChanged Routed Event
			ReportFieldCollection.CollectionChangedEvent = EventManager.RegisterRoutedEvent("CollectionChanged",
				RoutingStrategy.Bubble, typeof(CollectionChangedEventHandler), typeof(ReportFieldCollection));

		}

		/// <summary>
		/// Creates a collection of column descriptions.
		/// </summary>
		internal ReportFieldCollection()
		{

			// This is used as a general stub for multithreading.
			this.syncRoot = new object();

			// The ordered list keeps track of the columns for user interfaces where it is important to control the
			// presentation of the columns.
			this.list = new List<ReportField>();

			// The dictionary allows for fast lookup of column definition values.
			this.fieldDefinitionMap = new Dictionary<string, ReportField>();

			this.templateTypeColumnIdMap = new Dictionary<Type, string>();

		}

		/// <summary>
		/// Gets a MarkThree.Windows.Controls.ColumnDefinition using the ColumnId.
		/// </summary>
		/// <param name="key">The unique identifier for a column.</param>
		/// <returns>The MarkThee.Controls.ColumnDefinition having the given key.</returns>
		public ReportField this[string key]
		{
			get { return this.fieldDefinitionMap[key]; }
		}

		public bool TryGetValue(string key, out ReportField value)
		{

			return this.fieldDefinitionMap.TryGetValue(key, out value);

		}

		/// <summary>
		/// Handles a modification to a member of this collection.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		void fieldDefinition_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{

			// Broadcast the event to any listeners.
			RaiseEvent(new CollectionChangedEventArgs(ReportFieldCollection.CollectionChangedEvent, UndoAction.Create,
				CollectionChangedAction.Reset));

		}

		#region IList Members

		/// <summary>
		/// Adds a MarkThree.Windows.Controls.ColumnDefinition to the end of the collection.
		/// </summary>
		/// <param name="value">The value to be added to the end of the collection.</param>
		/// <returns>0</returns>
		public int Add(object value)
		{

			// The XAML parser only recognizes the generic IList interfaces, so incoming values need to be qualified to be added to
			// this collection.
			if (value is ReportField)
			{

				// Column definitions are linked into the event handler for this collection so that any change to the individual
				// definition sets off a chain reaction of updates
				ReportField fieldDefinition = value as ReportField;
				fieldDefinition.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(fieldDefinition_PropertyChanged);

				// The list maintains the order for user interfaces and the dictionary is used for fast access to the column based 
				// on the ColumnId.
				this.list.Add(fieldDefinition);
				this.fieldDefinitionMap.Add(fieldDefinition.ColumnId, fieldDefinition);

				// This table maps the data type to a field that can display that data type.
				foreach (Type type in fieldDefinition.Types)
					if (this.templateTypeColumnIdMap.ContainsKey(type))
						this.templateTypeColumnIdMap[type] = fieldDefinition.ColumnId;
					else
						this.templateTypeColumnIdMap.Add(type, fieldDefinition.ColumnId);

				// This will broadcast a notification that the collection has been modified.  This event is typically used to 
				// apply the changes to a persistent version of the user preferences, such as an XAML document.
				RaiseEvent(new CollectionChangedEventArgs(ReportFieldCollection.CollectionChangedEvent, UndoAction.Create,
					CollectionChangedAction.Add, fieldDefinition));

			}

			// This is here to satisfy the interface specification.
			return 0;

		}

		public Dictionary<Type, string> ColumnIdMap
		{
			get
			{
				return this.templateTypeColumnIdMap;
			}
		}

		/// <summary>
		/// Clears the collection of MarkThree.Windows.Controls.ColumnDefinitions.
		/// </summary>
		public void Clear()
		{

			// This will broadcast an event to notify any listeners that the collection has been cleared.
			this.list.Clear();
			this.fieldDefinitionMap.Clear();

			// This will broadcast a notification that the collection has been modified.  This event is typically used to 
			// apply the changes to a persistent version of the user preferences, such as an XAML document.
			RaiseEvent(new CollectionChangedEventArgs(ReportFieldCollection.CollectionChangedEvent, UndoAction.Create,
				CollectionChangedAction.Reset));

		}

		/// <summary>
		/// Determins whether the MarkThree.Windows.Controls.ReportColumnCollection contains the specified key.
		/// </summary>
		/// <param name="value">A key to a ColumnDefinition item.</param>
		/// <returns>True if the collection contains an item with the specified key, false otherwise.</returns>
		public bool Contains(object value)
		{

			// The hashtable is used to indicate if the given key exists in the collection.
			if (value is ReportField)
			{
				ReportField fieldDefinition = value as ReportField;
				return this.fieldDefinitionMap.ContainsKey(fieldDefinition.ColumnId);
			}

			// The value can also be the key used to identify the field.
			if (value is string)
			{
				string columnId = value as string;
				return this.fieldDefinitionMap.ContainsKey(columnId);
			}

			// No other object type can be found in this collection.
			return false;

		}

		/// <summary>
		/// Searches for the specified object and returns the zero based index to the first occurrence in the collection.
		/// </summary>
		/// <param name="value">The value to be indexed.</param>
		/// <returns>The zero based index to the first occurence of the object in the collection.</returns>
		public int IndexOf(object value)
		{

			// The hashtable is used to indicate if the given key exists in the collection.
			if (value is ReportField)
				return this.list.IndexOf(value as ReportField);

			// No other object type can be found in this collection.
			return -1;

		}

		/// <summary>
		/// Inserts an element into the MarkThree.Windows.Controls.ReportColumnCollection at the specified index.
		/// </summary>
		/// <param name="index">The index of the new element.</param>
		/// <param name="value">The new element to be placed in the collection.</param>
		public void Insert(int index, object value)
		{

			// The XAML parser only recognizes the generic IList interfaces, so incoming values need to be qualified to be added to
			// this collection.
			if (value is ReportField)
			{

				// Column definitions are linked into the event handler for this collection so that any change to the individual
				// definition sets off a chain reaction of updates
				ReportField fieldDefinition = value as ReportField;
				fieldDefinition.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(fieldDefinition_PropertyChanged);

				// The list maintains the order for user interfaces and the dictionary is used for fast access to the column based
				// on the ColumnId.
				this.list.Insert(index, fieldDefinition);
				this.fieldDefinitionMap.Add(fieldDefinition.ColumnId, fieldDefinition);

				// This will broadcast a notification that the collection has been modified.  This event is typically used to 
				// apply the changes to a persistent version of the user preferences, such as an XAML document.
				RaiseEvent(new CollectionChangedEventArgs(ReportFieldCollection.CollectionChangedEvent, UndoAction.Create,
					CollectionChangedAction.Add, fieldDefinition, index));

			}

		}

		/// <summary>
		/// Gets an indication of whether the collection can change size or not.
		/// </summary>
		public bool IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an indication of whether the collection can be modified.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Removes the first occurence of the specified element from the MarkThree.Windows.Controls.ReportColumnCollection.
		/// </summary>
		/// <param name="value">The element to be removed.</param>
		public void Remove(object value)
		{

			// The XAML parser only recognizes the generic IList interfaces, so incoming values need to be qualified to be added to
			// this collection.
			if (value is ReportField)
			{

				// The ColumnDefinition events need to be unhooked from this collection when it is removed.
				ReportField fieldDefinition = value as ReportField;
				fieldDefinition.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(fieldDefinition_PropertyChanged);

				// The list and dictionary are updated at the same time to remove the element.
				this.list.Remove(fieldDefinition);
				this.fieldDefinitionMap.Remove(fieldDefinition.ColumnId);

				// This will broadcast a notification that the collection has been modified.  This event is typically used to 
				// apply the changes to a persistent version of the user preferences, such as an XAML document.
				RaiseEvent(new CollectionChangedEventArgs(ReportFieldCollection.CollectionChangedEvent, UndoAction.Create,
					CollectionChangedAction.Remove,	fieldDefinition));

			}
		}

		/// <summary>
		/// Removes the element at the specified index of the MarkThree.Windows.Controls.ReportColumnCollection.
		/// </summary>
		/// <param name="index">The location where the element will be removed.</param>
		public void RemoveAt(int index)
		{

			// The ColumnDefinition events need to be unhooked from this collection when it is removed.
			ReportField fieldDefinition = this.list[index];
			fieldDefinition.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(fieldDefinition_PropertyChanged);

			// The list and dictionary are updated at the same time to remove the element.
			this.list.RemoveAt(index);
			this.fieldDefinitionMap.Remove(fieldDefinition.ColumnId);

			// This will broadcast a notification that the collection has been modified.  This event is typically used to 
			// apply the changes to a persistent version of the user preferences, such as an XAML document.
			RaiseEvent(new CollectionChangedEventArgs(ReportFieldCollection.CollectionChangedEvent, UndoAction.Create,
				CollectionChangedAction.Remove, fieldDefinition));

		}

		/// <summary>
		/// Gets or sets the ColumnDefinition at the specified index.
		/// </summary>
		/// <param name="index">The index of the element.</param>
		/// <returns>The object at the given index.</returns>
		public object this[int index]
		{

			get
			{
				return this.list[index];
			}

			set
			{

				// The list and the dictionary must be udpated at the same time.
				ReportField fieldDefinition = this.list[index];
				this.list[index] = value as ReportField;
				this.fieldDefinitionMap[fieldDefinition.ColumnId] = value as ReportField;

				// This will broadcast a notification that the collection has been modified.  This event is typically used to 
				// apply the changes to a persistent version of the user preferences, such as an XAML document.
				RaiseEvent(new CollectionChangedEventArgs(ReportFieldCollection.CollectionChangedEvent, UndoAction.Create,
					CollectionChangedAction.Replace, value, fieldDefinition));

			}
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// Copies the entire collection to a compatible one dimensional array, starting at the specified index.
		/// </summary>
		/// <param name="array">The target array.</param>
		/// <param name="index">The starting index.</param>
		public void CopyTo(Array array, int index)
		{
			this.list.CopyTo(array as ReportField[], index);
		}

		/// <summary>
		/// Gets the number of elements actually contained in the collection.
		/// </summary>
		public int Count
		{
			get { return this.list.Count; }
		}

		/// <summary>
		/// This property is not supported by this class.
		/// </summary>
		public bool IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// This property is not supported by this class.
		/// </summary>
		public object SyncRoot
		{
			get { return this.syncRoot; }
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		#endregion

	}

}

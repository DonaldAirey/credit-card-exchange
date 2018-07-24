namespace FluidTrade.Core.Windows.Controls
{

	using System;
	using System.Collections;
	using System.Collections.Generic;

    public class RowTemplateCollection : IList<RowTemplate>, ICollection<RowTemplate>, IEnumerable<RowTemplate>, IList, ICollection, IEnumerable
	{

		// Private Instance Fields
		private List<RowTemplate> list;
		private Object syncRoot;

		/// <summary>
		/// Initializes a new instance of the MarkThree.Windows.Controls.RowTemplateCollection class that is empty and has the default initial capacity.
		/// </summary>
		internal RowTemplateCollection()
		{

			// Initialize the object.
			this.list = new List<RowTemplate>();
			this.syncRoot = new Object();

		}

		#region IList<RowTemplate> Members

		public int IndexOf(RowTemplate item)
		{
			return this.list.IndexOf(item);
		}

		public void Insert(int index, RowTemplate item)
		{
			this.list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			this.list.RemoveAt(index);
		}

		public RowTemplate this[int index]
		{
			get
			{
				return this.list[index];
			}
			set
			{
				this.list[index] = value;
			}
		}

		#endregion

		#region ICollection<RowTemplate> Members

		public void Add(RowTemplate item)
		{
			this.list.Add(item);
		}

		public void Clear()
		{
			this.list.Clear();
		}

		public bool Contains(RowTemplate item)
		{
			return this.list.Contains(item);
		}

		public void CopyTo(RowTemplate[] array, int arrayIndex)
		{
			this.list.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return this.list.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(RowTemplate item)
		{
			return this.list.Remove(item);
		}

		#endregion

		#region IEnumerable<RowTemplate> Members

		public IEnumerator<RowTemplate> GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		#endregion

		#region IList Members

		public int Add(object value)
		{
			this.list.Add(value as RowTemplate);
			return this.list.IndexOf(value as RowTemplate);
		}

		public bool Contains(object value)
		{
			return this.list.Contains(value as RowTemplate);
		}

		public int IndexOf(object value)
		{
			return this.list.IndexOf(value as RowTemplate);
		}

		public void Insert(int index, object value)
		{
			this.list.Insert(index, value as RowTemplate);
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		public void Remove(object value)
		{
			this.list.Remove(value as RowTemplate);
		}

		object IList.this[int index]
		{
			get
			{
				return this.list[index];
			}
			set
			{
				this.list[index] = value as RowTemplate;
			}
		}

		#endregion

		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			this.list.CopyTo(array as RowTemplate[], index);
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return this.syncRoot ; }
		}

		#endregion

	}

}

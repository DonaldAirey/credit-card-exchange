namespace FluidTrade.Core
{

    using System.Collections;

	/// <summary>
	/// Represents a collection of FluidTrade.Core.Index objects
	/// </summary>
	public class DataIndexCollection : System.Data.InternalDataCollectionBase
	{

		// Private Instance Fields
		private System.Collections.ArrayList arrayList;
		private System.Collections.Hashtable hashTable;

		/// <summary>
		/// Create a collection of FluidTrade.Core.Index objects.
		/// </summary>
		public DataIndexCollection()
		{

			// Initialize the object.
			this.arrayList = new ArrayList();
			this.hashTable = new Hashtable();

		}

		/// <summary>
		/// Adds a FluidTrade.Core.DataIndex to the collection.
		/// </summary>
		/// <param name="dataIndex">The data index to be added to the collection.</param>
		public void Add(DataIndex dataIndex)
		{

			// The list is used for iteration, the table is used for fast access.
			this.arrayList.Add(dataIndex);
			this.hashTable.Add(dataIndex.IndexName, dataIndex);

		}

		/// <summary>
		/// Removes a FluidTrade.Core.DataIndex from the collection.
		/// </summary>
		/// <param name="dataIndex">The index to be removed from the collection.</param>
		public void Remove(DataIndex dataIndex)
		{

			// The list is used for iteration, the table is used for fast access.
			this.arrayList.Remove(dataIndex);
			this.hashTable.Remove(dataIndex.IndexName);

		}

		/// <summary>
		/// Gets the number of elements actually contained in the FluidTrade.Core.DataIndexCollection.
		/// </summary>
		public override int Count
		{
			get
			{
				return arrayList.Count;
			}
		}

		/// <summary>
		/// Gets a list of FluidTrade.Core.DataIndex objects.
		/// </summary>
		protected override System.Collections.ArrayList List
		{
			get
			{
				return this.arrayList;
			}
		}

		/// <summary>
		/// Returns an enumerator for the entire FluidTrade.Core.DataIndexCollection.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public override System.Collections.IEnumerator GetEnumerator()
		{
			return this.arrayList.GetEnumerator();
		}

		/// <summary>
		/// Gets the FluidTrade.Core.DataIndex at the specified index.
		/// </summary>
		/// <param name="index">An index into the collection.</param>
		/// <returns>The FluidTrade.Core.DataIndex at the given index.</returns>
		public DataIndex this[int index]
		{
			get { return (DataIndex)this.arrayList[index]; }
		}

		/// <summary>
		/// Gets the FluidTrade.Core.DataIndex having the specified name.
		/// </summary>
		/// <param name="index">The name of the FluidTrade.Core.DataIndex.</param>
		/// <returns>The FluidTrade.Core.DataIndex at the given index.</returns>
		public DataIndex this[string index]
		{
			get { return (DataIndex)this.hashTable[index]; }
		}
	
	}

}

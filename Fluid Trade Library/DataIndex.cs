namespace FluidTrade.Core
{
    using System;
    using System.Data;    

	/// <summary>
	/// Used to find records in a table using one or more values as a key.
	/// </summary>
	/// <copyright>Copyright © 2002 - 2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public abstract class DataIndex
	{

		// Private Instance Fields
		private String indexName;

		public DataIndex(string indexName)
		{

			// Initialize the object
			this.indexName = indexName;

		}

		public override bool Equals(object obj)
		{
			if (obj is DataIndex)
			{
				DataIndex index = (DataIndex)obj;
				return this.indexName.Equals(index.indexName);
			}

			return false;

		}

		public override int GetHashCode()
		{
			return this.indexName.GetHashCode();
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public String IndexName
		{
			get { return this.indexName; }
		}

		/// <summary>
		/// Gets the row that contains the specified key values.
		/// </summary>
		/// <param name="key">A single key value.</param>
		/// <returns>The row containing the key value or null if there are no rows that have the key value.</returns>
		public abstract DataRow Find(object key);

		/// <summary>
		/// Gets the row that contains the specified key values.
		/// </summary>
		/// <param name="key">A array of key values.</param>
		/// <returns>The row containing the key values or null if there are no rows that have the key values.</returns>
		public abstract DataRow Find(object[] keys);

	}

}


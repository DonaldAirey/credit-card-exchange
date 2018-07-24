namespace FluidTrade.Core
{

	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Text;

	/// <summary>
	/// Used to find records in a table using one or more values as a key.
	/// </summary>
	/// <copyright>Copyright © 2002 - 2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class NonClusteredIndex : DataIndex
	{

		// Private Instance Fields
		/// <summary>
		/// map of data row to key used for lookup of key when a datarow changes
		/// might not be able to use the row to gen the key again because key
		/// values might have changed. or if the row is delete the values are not accessible
		/// </summary>
		private System.Collections.Generic.Dictionary<DataRow, CompositeKey> dataRowToKeyMap;

		/// <summary>
		/// map of keys to data rows. Used in Find()
		/// </summary>
		private System.Collections.Generic.Dictionary<CompositeKey, DataRow> keyToDataRowMap;

		/// <summary>
		/// dataColumns that define the key
		/// </summary>
		private DataColumn[] parentColumns;

		/// <summary>
		/// Create a non-clustered index on the table.
		/// </summary>
		/// <param name="name">The name of the index.</param>
		/// <param name="columns">The columns that describe a unique key.</param>
		public NonClusteredIndex(string indexName, DataColumn[] parentColumns)
			: base(indexName)
		{

			this.parentColumns = parentColumns;
			// This dictionary is used to quickly find the index row using the parent row as a key when the parent row is updated, 
			// committed, rolled back or deleted.
			this.dataRowToKeyMap = new Dictionary<DataRow, CompositeKey>();

			//dictionary used to lookup rows based on a key
			this.keyToDataRowMap = new Dictionary<CompositeKey, DataRow>();

			// The index is correlated to the parent table through event handlers that watch for changes to the parent table. When
			// a parent record is added, modified or deleted, these event handlers make sure that the index is reconciled to the
			// parent table.
			if ((parentColumns.Length != 0))
			{
				DataTable parentTable = parentColumns[0].Table;

				//if there are any rows in the table add them to the index
				foreach (DataRow row in parentTable.Rows)
				{
					if (row.RowState == DataRowState.Deleted ||
						row.RowState == DataRowState.Detached)
						continue;

					this.AddDataRowToIndex(row);
				}

				//subscribe to table change events to manage the index
				parentTable.TableCleared += new DataTableClearEventHandler(parentTable_TableCleared);
				parentTable.RowChanging += new DataRowChangeEventHandler(parentTable_RowChanging);
				parentTable.RowDeleted += new DataRowChangeEventHandler(parentTable_RowDeleted);
			}

		}

		/// <summary>
		/// Clears the index when the parent table is cleared.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void parentTable_TableCleared(object sender, DataTableClearEventArgs e)
		{

			// Clear the index when the parent table is cleared.
			this.dataRowToKeyMap.Clear();
			this.keyToDataRowMap.Clear();

		}

		/// <summary>
		/// adds a row to the index. Will throw if row or key is present
		/// </summary>
		/// <param name="row"></param>
		private void AddDataRowToIndex(DataRow row)
		{
			CompositeKey key = GetDataRowCompositeKey(row);
			if (key != null)
			{
				this.dataRowToKeyMap.Add(row, key);
				this.keyToDataRowMap.Add(key, row);
			}
		}

		/// <summary>
		/// deletes a datarow from the index. will no-op if row not in index
		/// </summary>
		/// <param name="row"></param>
		private void DeleteDataRowFromIndex(DataRow row)
		{
			CompositeKey key;
			if (this.dataRowToKeyMap.TryGetValue(row, out key))
			{
				this.dataRowToKeyMap.Remove(row);
				this.keyToDataRowMap.Remove(key);
			}
		}

		/// <summary>
		/// create a new CompositeKey from a dataRow using this.ParentColumns to define the key/values
		/// returns null if there is a dbnull in the key
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		private CompositeKey GetDataRowCompositeKey(DataRow row)
		{
			object[] rowValues = new object[this.parentColumns.Length];


			for (int i = 0; i < this.parentColumns.Length; i++)
			{
				rowValues[i] = row[parentColumns[i]];

				// An index is not created when the key elements are null.  This is ANSI standard, not to be confused with the
				// Microsoft SQL Server 2005 "Standard" which doesn't allow unique indices with nulls.
				// HACK: allow composite keys with DBNull.
				if (DBNull.Value.Equals(rowValues[i]))
					return null;
			}

			return new CompositeKey(rowValues);
		}


		/// <summary>
		/// updates the row in the index, if the index values have not
		/// changed then no-op. if values have changed old is removed
		/// and new is added
		/// </summary>
		/// <param name="row"></param>
		/// <param name="removedDetached"></param>
		private void UpdateDataRowInIndex(DataRow row, bool removedDetached, bool isRollback)
		{
			CompositeKey key;
			// This will commit any changes made when the parent row changed.  Note that the mapping element is removed when a
			// deleted row is committed.
			if(isRollback == true ||
				(removedDetached == true &&
				(row.RowState == DataRowState.Detached || 
				row.RowState == DataRowState.Deleted)))
			{
				//if the row is detached (probably from commit) and param set
				//just remove the row when it is detached
				this.DeleteDataRowFromIndex(row);
				return;
			}

			//check if key is is the same and in the map
			if (this.dataRowToKeyMap.TryGetValue(row, out key))
			{
				CompositeKey newKey = this.GetDataRowCompositeKey(row);
				if (newKey != null &&
					newKey.Equals(key))
					return;

				//created the key so set Key to it so dont need to create another one
				key = newKey;
			}
			else if (key == null)
			{
				//not in the map create a new key
				key = this.GetDataRowCompositeKey(row);
			}

			//get rid of the old
			this.DeleteDataRowFromIndex(row);

			//if there is a valid key add
			if (key != null)
			{
				this.dataRowToKeyMap.Add(row, key);
				this.keyToDataRowMap.Add(key, row);
			}
		}


		/// <summary>
		/// Handles changes to the parent row.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void parentTable_RowChanging(object sender, DataRowChangeEventArgs e)
		{

			// This will reconcile the ancillary index table to the parent table.
			switch (e.Action)
			{

				case DataRowAction.Add:

					this.AddDataRowToIndex(e.Row);

					break;

				case DataRowAction.Change:

					// Any changes made to the parent row are now reflected in the index row.
					this.UpdateDataRowInIndex(e.Row, false, false);
					break;

				case DataRowAction.Commit:

					this.UpdateDataRowInIndex(e.Row, true, false);

					break;

				case DataRowAction.Rollback:

					// Any changes made to the index are rolled back when the parent row is rolled back.
					this.UpdateDataRowInIndex(e.Row, true, true);
					break;

			}

		}

		/// <summary>
		/// Handles a deletion of a parent row.
		/// </summary>
		/// <param name="sender">The object that originated the event.</param>
		/// <param name="e">The event arguments.</param>
		private void parentTable_RowDeleted(object sender, DataRowChangeEventArgs e)
		{

			// This will reconcile the ancillary index table to the parent table.
			switch (e.Action)
			{

				case DataRowAction.Delete:

					// Delete the index row when the parent is deleted.  Note that the row will remain part of the index until the
					// changes are committed.  Also note that the mapping element is not removed here but when the deleted index row is
					// committed.
					this.DeleteDataRowFromIndex(e.Row);

					break;

			}

		}

		/// <summary>
		/// Finds a row in the table containing the key elements.
		/// </summary>
		/// <param name="key">The identifier element of the key.</param>
		/// <returns>A row that contains the key elements, or null if there is no match.</returns>
		public override DataRow Find(object key)
		{
			try
			{
				CompositeKey compositeKey = new CompositeKey(key);

				DataRow retRow;
				if (this.keyToDataRowMap.TryGetValue(compositeKey, out retRow) == false)
					return null;

				return retRow;
			}
			catch (ArgumentException exception)
			{
				// Rethrow the exception with a little more information about where the error occurred.
				throw new ArgumentException(string.Format("{0}: {1}", this.IndexName, exception.Message));
			}
			catch (FormatException formatException)
			{
				// Rethrow the exception with a little more information about where the error occurred.
				throw new FormatException(string.Format("{0}: {1}", this.IndexName, formatException.Message));
			}
			catch (NullReferenceException nullReferenceException)
			{
				// Translate null reference exceptions into argument exceptions.
				throw new ArgumentException(string.Format("{0}: {1}", this.IndexName, nullReferenceException.Message));
			}
		}

		/// <summary>
		/// Finds a row in the table containing the key elements.
		/// </summary>
		/// <param name="key">The identifier element of the key.</param>
		/// <returns>A row that contains the key elements, or null if there is no match.</returns>
		public override DataRow Find(object[] key)
		{
			try
			{
				// Use the non-clustered index to find the row based on the key values.
			
				CompositeKey compositeKey;

				for (int index = 0; index < key.Length; ++index)
					if (key[index] != null && key[index].GetType().IsEnum)
						key[index] = (Int32)key[index];

				compositeKey = new CompositeKey(key);

				DataRow retRow;
				if (this.keyToDataRowMap.TryGetValue(compositeKey, out retRow) == false)
					return null;

				return retRow;
			}
			catch (ArgumentException exception)
			{
				// Rethrow the exception with a little more information about where the error occurred.
				throw new ArgumentException(string.Format("{0}: {1}", this.IndexName, exception.Message));
			}
			catch (FormatException formatException)
			{
				// Rethrow the exception with a little more information about where the error occurred.
				throw new FormatException(string.Format("{0}: {1}", this.IndexName, formatException.Message));
			}
			catch (NullReferenceException nullReferenceException)
			{
				// Translate null reference exceptions into argument exceptions.
				throw new ArgumentException(string.Format("{0}: {1}", this.IndexName, nullReferenceException.Message));
			}
		}

		/// <summary>
		/// class that turns an array of values into a key
		/// each value in the array is hashed with the other values to create a hashcode
		/// the hashCode is returned by an overriden GetHashCode()
		/// the overriden Equals does an equality check for each item in the array
		/// </summary>
		protected class CompositeKey
		{
			/// <summary>
			/// value array. conatains the values of the key
			/// </summary>
			private object[] valueAr;

			/// <summary>
			/// hash of hashcodes of items in valuesAr
			/// </summary>
			private int hashCode;

			/// <summary>
			/// ctor
			/// </summary>
			/// <param name="values">values that define the key</param>
			public CompositeKey(params object[] values)
			{
				hashCode = 0;
				//if null values create an empty array
				if (values == null)
					this.valueAr = new object[0];

				this.valueAr = values;

				//create the hashcode
				foreach (object val in values)
				{
					if (val == null)
						continue;

					hashCode ^= val.GetHashCode();
				}
			}

			/// <summary>
			/// get hashcode of Key. Hashcode is a hash of value array entries hashcodes
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				return this.hashCode;
			}

			/// <summary>
			/// Determines whether two specified CompositeKey objects have the same values
			/// </summary>
			/// <param name="obj">other object to compare to this</param>
			/// <returns>true if values equal</returns>
			public override bool Equals(object obj)
			{
				if (!(obj is CompositeKey))
					return false;

				//ref check is quick so try that first
				if (object.ReferenceEquals(this, obj))
					return true;

				CompositeKey other = (CompositeKey)obj;
				if (other.hashCode != this.hashCode ||
					other.valueAr.Length != this.valueAr.Length)
					return false;

				for (int i = 0; i < this.valueAr.Length; i++)
				{
					object curVal = this.valueAr[i];
					object curOtherVal = other.valueAr[i];

					if (curVal == null)
					{
						if (curOtherVal != null)
							return false;

						continue;
					}
					if (curVal.Equals(curOtherVal) == false)
						return false;
				}

				return true;
			}


			/// <summary>
			/// Determines whether two specified CompositeKey objects have different values
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator !=(CompositeKey a, CompositeKey b)
			{
				if (object.ReferenceEquals(a, null))
				{
					if (object.ReferenceEquals(b, null))
						return false;

					return true;
				}

				return !a.Equals(b);
			}

			/// <summary>
			/// Determines whether two specified CompositeKey objects have the same values
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator ==(CompositeKey a, CompositeKey b)
			{
				if (object.ReferenceEquals(a, null))
				{
					if (object.ReferenceEquals(b, null))
						return true;

					return false;
				}

				return a.Equals(b);
			}

			/// <summary>
			/// overriden for debugging purposes
			/// </summary>
			/// <returns>delimited list of values in array converted to a string</returns>
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				foreach (object o in this.valueAr)
				{
					sb.Append(o);
					sb.Append(":");
				}
				return sb.ToString();
			}
		}

	}
}


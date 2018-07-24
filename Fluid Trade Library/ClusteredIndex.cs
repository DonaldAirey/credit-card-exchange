namespace FluidTrade.Core
{

	using System;
    using System.Data;

    /// <summary>
	/// Used to find records in a table using one or more values as a key.
	/// </summary>
	/// <copyright>Copyright © 2002 - 2008 - Fluid Trade, Inc.  All Rights Reserved.</copyright>
	public class ClusteredIndex : DataIndex
	{

		// Clustered indices use the primary key of the table to find the rows.
		private System.Data.DataTable dataTable;

		/// <summary>
		/// Create a primary, unique index on the Object table.
		/// </summary>
		/// <param name="columns">The columns that describe a unique key.</param>
		public ClusteredIndex(string indexName, DataColumn[] columns)
			: base(indexName)
		{
			// This will prevent an empty key from killing the application.
			if ((columns.Length == 0))
			{
				this.dataTable = new DataTable();
			}
			else
			{
				// The primary index uses the native 'Find' method of the base table to search for records.
				this.dataTable = columns[0].Table;
				this.dataTable.PrimaryKey = columns;
			}
		}

		/// <summary>
		/// Finds a row in the Object table containing the key elements.
		/// </summary>
		/// <param name="objectId">The EntityId element of the key.</param>
		/// <returns>A DepartmentRow that contains the key elements, or null if there is no match.</returns>
		public override DataRow Find(object key)
		{
			// Use the base table to find a row containing the key elements.
			try
			{
				return this.dataTable.Rows.Find(key);
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
		/// Finds a row in the Object table containing the key elements.
		/// </summary>
		/// <param name="objectId">The EntityId element of the key.</param>
		/// <returns>A DepartmentRow that contains the key elements, or null if there is no match.</returns>
		public override DataRow Find(object[] key)
		{
			// Use the base table to find a row containing the key elements.
			try
			{
				return this.dataTable.Rows.Find(key);
			}
			catch (ArgumentException argumentException)
			{
				// Rethrow the exception with a little more information about where the error occurred.
				throw new ArgumentException(string.Format("{0}: {1}", this.IndexName, argumentException.Message));
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

	}

}


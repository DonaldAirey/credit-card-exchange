namespace FluidTrade.Core
{

    using System.Data;
	using System;

	/// <summary>
	/// The address of an element of data in a DataTable.
	/// </summary>
	public class DataTableCoordinate
	{

		// Public Instance Fields
		public System.Data.DataRow DataRow;
		public System.Data.DataColumn DataColumn;
		public object associationObj;

		/// <summary>
		/// Create an address for an element in a DataTable.
		/// </summary>
		/// <param name="dataRow">The row where the element can be found.</param>
		/// <param name="dataColumn">The column where the element can be found.</param>
		public DataTableCoordinate(DataRow dataRow, DataColumn dataColumn)
		{

			// Initialize the object
			this.DataRow = dataRow;
			this.DataColumn = dataColumn;
		}

		/// <summary>
		/// Create an address for an element in a DataTable.
		/// </summary>
		/// <param name="dataRow">The row where the element can be found.</param>
		/// <param name="dataColumn">The column where the element can be found.</param>
		/// <param name="association">Any association</param>
		public DataTableCoordinate(DataRow dataRow, DataColumn dataColumn, Guid association)
		{
			// Initialize the object
			this.DataRow = dataRow;
			this.DataColumn = dataColumn;
			this.associationObj = association;
		}

		/// <summary>
		/// Any association
		/// </summary>
		public Guid Association
		{
			get
			{
				if (this.associationObj == null)
					return Guid.Empty;

				return (Guid)this.associationObj;
			}
		}
	}

}

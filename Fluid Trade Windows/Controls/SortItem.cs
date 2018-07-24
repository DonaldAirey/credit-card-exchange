namespace FluidTrade.Core.Windows.Controls
{


    /// <summary>
	/// Describes a column and direction (ascending or descending) in a collection.
	/// </summary>
	public class SortItem
	{

		/// <summary>
		/// The column containing the data.
		/// </summary>
		public ReportField Column;

		/// <summary>
		/// Describes whether the data is sorted in ascending or descending order.
		/// </summary>
		public SortOrder SortOrder;

		/// <summary>
		/// Creates an item used to describe the sort order of a report.
		/// </summary>
		/// <param name="column">The column containing the data.</param>
		/// <param name="sortOrder">The direction (ascending or descending) of the ordered data.</param>
		public SortItem(ReportField column, SortOrder sortOrder)
		{

			// Initialize the object
			this.Column = column;
			this.SortOrder = sortOrder;

		}

	}

}
